using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
			// Normal maps and factories
			public readonly ConcurrentDictionary<(object Source, Type SourceType, object? Destination, Type DestinationType), object?> ResultsCache =
				new ConcurrentDictionary<(object, Type, object?, Type), object?>();


			// Only factories
			// Owner which will dispose of the factories
			public CopyMapper Owner = null!;

			public readonly ConcurrentDictionary<Type, Lazy<IDisposable>> TypeFactories = new ConcurrentDictionary<Type, Lazy<IDisposable>>();
		}

		private class MemberInfoEqualityComparer<TMember> : IEqualityComparer<TMember> where TMember : MemberInfo {
			public static readonly MemberInfoEqualityComparer<TMember> Instance = new MemberInfoEqualityComparer<TMember>();

			public bool Equals(TMember? x, TMember? y) {
				return x?.MetadataToken == y?.MetadataToken &&
					object.Equals(x?.Module, y?.Module);
			}

			public int GetHashCode(TMember obj) {
				return HashUtils.Combine(obj.Module, obj.MetadataToken);
			}
		}
		
		private static readonly PropertyInfo IDictionary_MemberInfo_IDisposable_indexer =
			typeof(IDictionary<MemberInfo, Lazy<IDisposable>>)
				.GetProperties()
				.Single(p => p.GetIndexParameters().Length > 0);
		private static readonly MethodInfo INewMapFactory_Invoke = TypeUtils.GetMethod(() => default(INewMapFactory)!.Invoke(default));
		private static readonly MethodInfo IMergeMapFactory_Invoke = TypeUtils.GetMethod(() => default(IMergeMapFactory)!.Invoke(default, default));
		private static readonly MethodInfo IMapper_MapNew = TypeUtils.GetMethod(() => default(IMapper)!.Map(default, default!, default!, default));
		private static readonly MethodInfo IMapper_MapMerge = TypeUtils.GetMethod(() => default(IMapper)!.Map(default, default!, default, default!, default));


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

		private readonly ConcurrentDictionary<(Type From, Type To, int OptionsHash), IEnumerable<MemberInfo>> _membersCache =
			new ConcurrentDictionary<(Type, Type, int), IEnumerable<MemberInfo>>();
		private readonly ConcurrentDictionary<(Type From, Type To, int OptionsHash), Action<object, object, IDictionary<MemberInfo, Lazy<IDisposable>>>> _factoriesDelegatesCache =
			new ConcurrentDictionary<(Type, Type, int), Action<object, object, IDictionary<MemberInfo, Lazy<IDisposable>>>>();
		private readonly ConcurrentDictionary<(Type From, Type To, int OptionsHash), Action<object, object, IMapper, MappingOptions>> _mapperDelegatesCache =
			new ConcurrentDictionary<(Type, Type, int), Action<object, object, IMapper, MappingOptions>>();

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
			_mappersCache = new MappingOptionsFactoryCache<IMapper>(options => {
				var copyMapperOptions = options.GetOptions<CopyMapperMappingOptions>();
				if ((copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy) == DeepCopyFlags.None)
					return IdentityMapper.Instance;
				else {
					var mapper = options.GetOptions<MapperOverrideMappingOptions>()?.Mapper;
					if (mapper != null)
						return new CompositeMapper(mapper, this, IdentityMapper.Instance);
					else
						return _deepMapper;
				}
			});
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<NestedMappingContext>(
				n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
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

			if (source == destination)
				return destination;

			mappingOptions = GetOrCreateOptions(mappingOptions);
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

			// Create the delegate
			var copyMapperOptions = mappingOptions.GetOptions<CopyMapperMappingOptions>();
			var hash = HashUtils.Combine(
				copyMapperOptions?.PropertiesToMap ?? _copyMapperOptions.PropertiesToMap,
				copyMapperOptions?.FieldsToMap ?? _copyMapperOptions.FieldsToMap,
				copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy);
			var deleg = _mapperDelegatesCache.GetOrAdd((sourceType, destinationType, hash), _ => {
				var members = GetMembers(sourceType, destinationType,
					copyMapperOptions?.PropertiesToMap ?? _copyMapperOptions.PropertiesToMap,
					copyMapperOptions?.FieldsToMap ?? _copyMapperOptions.FieldsToMap);

				var useMerge = (copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy) == DeepCopyFlags.DeepMap;

				var sourceParam = Expression.Parameter(typeof(object), "source");
				var destinationParam = Expression.Parameter(typeof(object), "destination");
				var mapperParam = Expression.Parameter(typeof(IMapper), "mapper");
				var mappingOptionsParam = Expression.Parameter(typeof(MappingOptions), "mappingOptions");
				var propertyAssignments = members.Select(m => {
					// (Type)
					var memberType = m is PropertyInfo p ? p.PropertyType : ((FieldInfo)m).FieldType;
					var memberTypeConst = Expression.Constant(memberType);

					// ((TDestination)destination).Prop
					var destinationProp = Expression.MakeMemberAccess(Expression.Convert(destinationParam, destinationType), m);

					// (object)((TSource)source).Prop
					Expression body = Expression.Convert(Expression.MakeMemberAccess(Expression.Convert(sourceParam, sourceType), m), typeof(object));

					if (useMerge) {
						// mapper.Map(..., (Type), (object)((TDestination)destination).Prop, (Type), mappingOptions)
						body = Expression.Call(
							mapperParam,
							IMapper_MapMerge,
							body, memberTypeConst, Expression.Convert(destinationProp, typeof(object)), memberTypeConst, mappingOptionsParam);
					}
					else {
						// mapper.Map(..., (Type), (Type), mappingOptions)
						body = Expression.Call(
							mapperParam,
							IMapper_MapNew,
							body, memberTypeConst, memberTypeConst, mappingOptionsParam);
					}

					// ((TDestination)destination).Prop = (Type)...
					return Expression.Assign(destinationProp, Expression.Convert(body, memberType));
				});
				return Expression.Lambda<Action<object, object, IMapper, MappingOptions>>(
					members.Any() ? Expression.Block(propertyAssignments) : Expression.Empty(),
					sourceParam, destinationParam, mapperParam, mappingOptionsParam).Compile();
			});

			try {
				deleg.Invoke(source, destination, deepMapper, mappingOptions);
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

			mappingOptions = GetOrCreateOptions(mappingOptions);
			var copyMapperInternalOptions = mappingOptions.GetOptions<CopyMapperInternalOptions>()!;
			var resultsCache = copyMapperInternalOptions.ResultsCache;

			var copyMapperOptions = mappingOptions.GetOptions<CopyMapperMappingOptions>();
			var deepCopy = copyMapperOptions?.DeepCopy ?? _copyMapperOptions.DeepCopy;
			var useMerge = deepCopy == DeepCopyFlags.DeepMap;

			var destinationFactory = ObjectFactory.CreateFactory(destinationType);

			// Retrieve the shared set of properties and fields
			var members = GetMembers(sourceType, destinationType,
				copyMapperOptions?.PropertiesToMap ?? _copyMapperOptions.PropertiesToMap,
				copyMapperOptions?.FieldsToMap ?? _copyMapperOptions.FieldsToMap);

			var deepMapper = _mappersCache.GetOrCreate(mappingOptions);

			// Create the factories for all property/field types, factories are lazy because of recursive types initialization
			// Property/field type -> NewMapFactory/MergeMapFactory (lazy)
			// DEV: does it make sense to cache type factories across all the maps when mapping options could be different for nested maps?
			var factories = members.ToDictionary(m => m, m => {
				var type = m is PropertyInfo p ? p.PropertyType : ((FieldInfo)m).FieldType;
				return copyMapperInternalOptions.TypeFactories.GetOrAdd(type, _ => new Lazy<IDisposable>(() => {
					if (useMerge)
						return deepMapper.MapMergeFactory(type, type, mappingOptions);
					else
						return deepMapper.MapNewFactory(type, type, mappingOptions);
				}, true));
			}, MemberInfoEqualityComparer<MemberInfo>.Instance);
			var disposed = false; // Needed to avoid looping forever
			try { 
				// Create the delegate
				var hash = HashUtils.Combine(
					copyMapperOptions?.PropertiesToMap ?? _copyMapperOptions.PropertiesToMap,
					copyMapperOptions?.FieldsToMap ?? _copyMapperOptions.FieldsToMap,
					deepCopy);
				var deleg = _factoriesDelegatesCache.GetOrAdd((sourceType, destinationType, hash), _ => {
					var sourceParam = Expression.Parameter(typeof(object), "source");
					var destinationParam = Expression.Parameter(typeof(object), "destination");
					var factoriesParam = Expression.Parameter(typeof(IDictionary<MemberInfo, Lazy<IDisposable>>), "factories");
					var propertyAssignments = members.Select(m => {
						// (Type)
						var memberType = m is PropertyInfo p ? p.PropertyType : ((FieldInfo)m).FieldType;

						// ((TDestination)destination).Prop
						var destinationProp = Expression.MakeMemberAccess(Expression.Convert(destinationParam, destinationType), m);

						// (object)((TSource)source).Prop
						Expression body = Expression.Convert(Expression.MakeMemberAccess(Expression.Convert(sourceParam, sourceType), m), typeof(object));

						// factories[m].Value
						var factory = Expression.Property(Expression.MakeIndex(factoriesParam, IDictionary_MemberInfo_IDisposable_indexer, [Expression.Constant(m)]), nameof(Lazy<object>.Value));

						if (useMerge) {
							// ((IMergeMapFactory)factories[m].Value).Invoke(..., (object)((TDestination)destination).Prop)
							body = Expression.Call(
								Expression.Convert(factory, typeof(IMergeMapFactory)),
								IMergeMapFactory_Invoke,
								body, Expression.Convert(destinationProp, typeof(object)));
						}
						else {
							// ((INewMapFactory)factories[m].Value).Invoke(...)
							body = Expression.Call(
								Expression.Convert(factory, typeof(INewMapFactory)),
								INewMapFactory_Invoke,
								body);
						}

						// destination.Prop = (Type)...
						return Expression.Assign(destinationProp, Expression.Convert(body, memberType));
					});
					return Expression.Lambda<Action<object, object, IDictionary<MemberInfo, Lazy<IDisposable>>>>(
						members.Any() ? Expression.Block(propertyAssignments) : Expression.Empty(),
						sourceParam, destinationParam, factoriesParam).Compile();
				});

				return new DisposableMergeMapFactory(sourceType, destinationType,
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
							deleg.Invoke(source, destination, factories);
						}
						catch (Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}

						return destination;
					}, copyMapperInternalOptions.Owner == this ? [ new LazyDisposable(DisposeFactories) ] : []);
			}
			catch {
				DisposeFactories();

				throw;
			}


			void DisposeFactories() {
				lock (copyMapperInternalOptions.TypeFactories) {
					if(disposed)
						return;

					disposed = true;

					foreach (var factory in copyMapperInternalOptions.TypeFactories.Values) {
						if (factory.IsValueCreated)
							factory.Value.Dispose();
					}
				}
			}
		}
		#endregion


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static BindingFlags GetBindingFlags(MemberVisibilityFlags flags) {
			return BindingFlags.Instance |
				(flags.HasFlag(MemberVisibilityFlags.Public) ? BindingFlags.Public : BindingFlags.Default) |
				(flags.HasFlag(MemberVisibilityFlags.NonPublic) ? BindingFlags.NonPublic : BindingFlags.Default);
		}

		private IEnumerable<MemberInfo> GetMembers(Type sourceType, Type destinationType,
			MemberVisibilityFlags propertiesToMap, MemberVisibilityFlags fieldsToMap) {
			
			var hash = HashUtils.Combine(propertiesToMap, fieldsToMap);
			return _membersCache.GetOrAdd((sourceType, destinationType, hash), _ => {
				// Retrieve the shared set of properties to copy and create factories for them
				var propertyBindingFlags = GetBindingFlags(propertiesToMap);
				var isNonPublic = propertyBindingFlags.HasFlag(BindingFlags.NonPublic);
				var properties = sourceType.GetProperties(propertyBindingFlags)
					.Intersect(destinationType.GetProperties(propertyBindingFlags), MemberInfoEqualityComparer<PropertyInfo>.Instance)
					.Where(p => p.CanRead && p.CanWrite && p.GetAccessors(true).All(m => !m.IsStatic && (isNonPublic || m.IsPublic)));

				// Retrieve the shared set of fields to copy
				var fieldBindingFlags = GetBindingFlags(fieldsToMap);
				var fields = sourceType.GetFields(fieldBindingFlags)
					.Intersect(destinationType.GetFields(fieldBindingFlags), MemberInfoEqualityComparer<FieldInfo>.Instance)
					.Where(f => !f.IsStatic);

				return properties
					.Cast<MemberInfo>()
					.Concat(fields)
					.ToList();
			});
		}

		// Internal options contain cached results lookup and we don't want to save it, so we recreate it for each different map
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions GetOrCreateOptions(MappingOptions? mappingOptions) {
			return _optionsCache.GetOrCreate(mappingOptions)
				.ReplaceOrAdd<CopyMapperInternalOptions>(o => o ?? new CopyMapperInternalOptions {
					Owner = this
				});
		}
	}
}
