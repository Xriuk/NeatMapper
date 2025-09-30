using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps objects by copying all supported properties/fields between
	/// source and destination (can also copy private ones). Supports derived and base types (non-abstract),
	/// and deep copies. Same references are mapped to the same objects, to avoid duplicates and
	/// handle recursion.
	/// </summary>
	public sealed class CopyMapper : IMapper, IMapperFactory {
		private class CopyMapperInternalOptions {
			public readonly ConcurrentDictionary<(object Source, Type SourceType, object? Destination, Type DestinationType), object?> ResultsCache =
				new ConcurrentDictionary<(object, Type, object?, Type), object?>();
		}

		private class MemberInfoEqualityComparer<TMember> : IEqualityComparer<TMember> where TMember : MemberInfo {
			public static readonly MemberInfoEqualityComparer<TMember> Instance = new MemberInfoEqualityComparer<TMember>();

			public bool Equals(TMember? x, TMember? y) {
				return x?.MetadataToken == y?.MetadataToken &&
					object.Equals(x?.Module, y?.Module);
			}

			public int GetHashCode(TMember obj) {
				int hash = 17;
				hash = hash * 31 + obj.Module.GetHashCode();
				hash = hash * 31 + obj.MetadataToken.GetHashCode();
				return hash;
			}
		}


		/// <summary>
		/// <see cref="IMapper"/> which is used to deep-map objects when needed.
		/// </summary>
		private readonly CompositeMapper _deepMapper;

		/// <summary>
		/// Options to apply during mapping.
		/// </summary>
		private readonly CopyMapperOptions _copyMapperOptions;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> and output <see cref="IMapper"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<IMapper> _mappersCache;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Creates a new instance of <see cref="CopyMapper"/>.
		/// </summary>
		/// <param name="deepMapper">
		/// Optional <see cref="IMapper"/> used to deep-map objects when needed.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
		/// </param>
		/// <param name="copyMapperOptions">
		/// Options to apply during mapping, null to use default.<br/>
		/// Can be overridden during mapping with <see cref="CopyMapperMappingOptions"/>.
		/// </param>
		public CopyMapper(IMapper? deepMapper = null, CopyMapperOptions? copyMapperOptions = null) {
			var mappers = new List<IMapper>();
			if (deepMapper != null)
				mappers.Add(deepMapper);
			mappers.Add(this);
			mappers.Add(IdentityMapper.Instance);
			_deepMapper = new CompositeMapper(mappers);
			_copyMapperOptions = copyMapperOptions != null ? new CopyMapperOptions(copyMapperOptions) : new CopyMapperOptions();
			_mappersCache = new MappingOptionsFactoryCache<IMapper>(o => {
				var copyMapperOptions = o.GetOptions<CopyMapperMappingOptions>();
				if ((copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy) == DeepCopyFlags.None)
					return IdentityMapper.Instance;
				else {
					var mapper = o.GetOptions<MapperOverrideMappingOptions>()?.Mapper;
					if (mapper != null)
						return new CompositeMapper(mapper, this, IdentityMapper.Instance);
					else
						return _deepMapper;
				}
			});
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(o => o.ReplaceOrAdd<CopyMapperInternalOptions>(o => o ?? new CopyMapperInternalOptions()));
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			// We must be able to create the object
			return CanMapMerge(sourceType, destinationType, mappingOptions) && ObjectFactory.CanCreate(destinationType);
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// The destination type must be on the same hierarchy chain as the source type
			// (so either derived from it or its parent)
			return !sourceType.IsValueType && sourceType != typeof(string) &&
				!destinationType.IsValueType && destinationType != typeof(string) && 
				(sourceType.IsAssignableFrom(destinationType) || destinationType.IsAssignableFrom(sourceType));
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			return Map(source, sourceType, null, destinationType, mappingOptions);
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMerge(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			if (source == null)
				return null;

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
			var resultsCache = mappingOptions.GetOptions<CopyMapperInternalOptions>()!.ResultsCache;

			if (resultsCache.TryGetValue((source, sourceType, destination, destinationType), out var result))
				return result;

			// The bool var is needed because of race conditions on creation
			var created = false;
			destination = resultsCache.GetOrAdd((source, sourceType, destination, destinationType), r => {
				created = true;
				return r.Destination ?? ObjectFactory.Create(r.DestinationType);
			})!;
			if (!created)
				return destination;

			var deepMapper = _mappersCache.GetOrCreate(mappingOptions);

			var copyMapperOptions = mappingOptions.GetOptions<CopyMapperMappingOptions>();
			var useMerge = !(copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy).HasFlag(DeepCopyFlags.OverrideInstance);

			try { 
				// Retrieve the shared set of properties to copy, we do this before fields so that we can run code
				// in get/set functions if needed
				var propertyFlags = GetBindingFlags(copyMapperOptions?.PropertiesToMap ?? _copyMapperOptions.PropertiesToMap);
				var isNonPublic = propertyFlags.HasFlag(BindingFlags.NonPublic);
				var properties = sourceType.GetProperties(propertyFlags)
					.Intersect(destinationType.GetProperties(propertyFlags), MemberInfoEqualityComparer<PropertyInfo>.Instance)
					.Where(p => p.CanRead && p.CanWrite && p.GetAccessors(true).All(m => !m.IsStatic && (isNonPublic || m.IsPublic)))
					.ToList();
				foreach(var property in properties) {
					object? sourceValue;
					try {
						sourceValue = property.GetValue(source);
					}
					catch(Exception e) {
						throw new InvalidOperationException($"Could not retrieve value from property {property.Name} in the source object", e);
					}

					object? destinationValue;
					if (useMerge) {
						try {
							destinationValue = property.GetValue(destination);
						}
						catch (Exception e) {
							throw new InvalidOperationException($"Could not retrieve value from property {property.Name} in the destination object", e);
						}
					}
					else
						destinationValue = null;

					if (useMerge)
						sourceValue = deepMapper.Map(sourceValue, property.PropertyType, destinationValue, property.PropertyType, mappingOptions);
					else
						sourceValue = deepMapper.Map(sourceValue, property.PropertyType, property.PropertyType, mappingOptions);

					try {
						property.SetValue(destination, sourceValue);
					}
					catch (Exception e) {
						throw new InvalidOperationException($"Could not set value for property {property.Name} in the destination object", e);
					}
				}

				// Retrieve the shared set of fields to copy
				var fieldFlags = GetBindingFlags(copyMapperOptions?.FieldsToMap ?? _copyMapperOptions.FieldsToMap);
				var fields = sourceType.GetFields(fieldFlags)
					.Intersect(destinationType.GetFields(fieldFlags), MemberInfoEqualityComparer<FieldInfo>.Instance)
					.Where(f => !f.IsStatic)
					.ToList();
				foreach (var field in fields) {
					object? sourceValue;
					try {
						sourceValue = field.GetValue(source);
					}
					catch (Exception e) {
						throw new InvalidOperationException($"Could not retrieve value from field {field.Name} in the source object", e);
					}

					object? destinationValue;
					if (useMerge) {
						try {
							destinationValue = field.GetValue(destination);
						}
						catch (Exception e) {
							throw new InvalidOperationException($"Could not retrieve value from field {field.Name} in the destination object", e);
						}
					}
					else
						destinationValue = null;

					if (useMerge)
						sourceValue = deepMapper.Map(sourceValue, field.FieldType, destinationValue, field.FieldType, mappingOptions);
					else
						sourceValue = deepMapper.Map(sourceValue, field.FieldType, field.FieldType, mappingOptions);

					try {
						field.SetValue(destination, sourceValue);
					}
					catch (Exception e) {
						throw new InvalidOperationException($"Could not set value for field {field.Name} in the destination object", e);
					}
				}
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}

			return destination;
		}
		#endregion

		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapNew(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var factory = MapMergeFactory(sourceType, destinationType, mappingOptions);
			try {
				return new DisposableNewMapFactory(sourceType, destinationType,
					source => factory.Invoke(source, null),
					factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMapMerge(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
			var resultsCache = mappingOptions.GetOptions<CopyMapperInternalOptions>()!.ResultsCache;

			var deepMapper = _mappersCache.GetOrCreate(mappingOptions);

			var copyMapperOptions = mappingOptions.GetOptions<CopyMapperMappingOptions>();
			var useMerge = !(copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy).HasFlag(DeepCopyFlags.OverrideInstance);

			var destinationFactory = ObjectFactory.CreateFactory(destinationType);

			// Retrieve the shared set of properties to copy, we do this before fields so that we can run code
			// in get/set functions if needed
			var propertyFlags = GetBindingFlags(copyMapperOptions?.PropertiesToMap ?? _copyMapperOptions.PropertiesToMap);
			var isNonPublic = propertyFlags.HasFlag(BindingFlags.NonPublic);
			var properties = sourceType.GetProperties(propertyFlags)
				.Intersect(destinationType.GetProperties(propertyFlags), MemberInfoEqualityComparer<PropertyInfo>.Instance)
				.Where(p => p.CanRead && p.CanWrite && p.GetAccessors(true).All(m => !m.IsStatic && (isNonPublic || m.IsPublic)))
				.ToList();

			// Retrieve the shared set of fields to copy
			var fieldFlags = GetBindingFlags(copyMapperOptions?.FieldsToMap ?? _copyMapperOptions.FieldsToMap);
			var fields = sourceType.GetFields(fieldFlags)
				.Intersect(destinationType.GetFields(fieldFlags), MemberInfoEqualityComparer<FieldInfo>.Instance)
				.Where(f => !f.IsStatic)
				.ToList();

			return new DefaultMergeMapFactory(sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					if (source == null)
						return null;

					if (resultsCache.TryGetValue((source, sourceType, destination, destinationType), out var result))
						return result;

					// The bool var is needed because of race conditions on creation
					var created = false;
					destination = resultsCache.GetOrAdd((source, sourceType, destination, destinationType), r => {
						created = true;
						return r.Destination ?? destinationFactory.Invoke();
					})!;
					if (!created)
						return destination;

					try {
						// DEV: convert below to delegate/compiled expression to improve performance
						foreach (var property in properties) {
							object? sourceValue;
							try {
								sourceValue = property.GetValue(source);
							}
							catch (Exception e) {
								throw new InvalidOperationException($"Could not retrieve value from property {property.Name} in the source object", e);
							}

							object? destinationValue;
							if (useMerge) {
								try {
									destinationValue = property.GetValue(destination);
								}
								catch (Exception e) {
									throw new InvalidOperationException($"Could not retrieve value from property {property.Name} in the destination object", e);
								}
							}
							else
								destinationValue = null;

							if (useMerge)
								sourceValue = deepMapper.Map(sourceValue, property.PropertyType, destinationValue, property.PropertyType, mappingOptions);
							else
								sourceValue = deepMapper.Map(sourceValue, property.PropertyType, property.PropertyType, mappingOptions);

							try {
								property.SetValue(destination, sourceValue);
							}
							catch (Exception e) {
								throw new InvalidOperationException($"Could not set value for property {property.Name} in the destination object", e);
							}
						}

						foreach (var field in fields) {
							object? sourceValue;
							try {
								sourceValue = field.GetValue(source);
							}
							catch (Exception e) {
								throw new InvalidOperationException($"Could not retrieve value from field {field.Name} in the source object", e);
							}

							object? destinationValue;
							if (useMerge) {
								try {
									destinationValue = field.GetValue(destination);
								}
								catch (Exception e) {
									throw new InvalidOperationException($"Could not retrieve value from field {field.Name} in the destination object", e);
								}
							}
							else
								destinationValue = null;

							if (useMerge)
								sourceValue = deepMapper.Map(sourceValue, field.FieldType, destinationValue, field.FieldType, mappingOptions);
							else
								sourceValue = deepMapper.Map(sourceValue, field.FieldType, field.FieldType, mappingOptions);

							try {
								field.SetValue(destination, sourceValue);
							}
							catch (Exception e) {
								throw new InvalidOperationException($"Could not set value for field {field.Name} in the destination object", e);
							}
						}
					}
					catch (Exception e) {
						throw new MappingException(e, (sourceType, destinationType));
					}

					return destination;
				});
		}
		#endregion


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static BindingFlags GetBindingFlags(MemberVisibilityFlags flags) {
			return BindingFlags.Instance |
				(flags.HasFlag(MemberVisibilityFlags.Public) ? BindingFlags.Public : BindingFlags.Default) |
				(flags.HasFlag(MemberVisibilityFlags.NonPublic) ? BindingFlags.NonPublic : BindingFlags.Default);
		}
	}
}
