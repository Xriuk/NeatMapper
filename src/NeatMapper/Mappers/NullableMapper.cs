using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMapper"/> which maps <see cref="Nullable{T}"/> and its underlying types
	/// by using another <see cref="IMapper"/> with the following rules:
	/// <list type="bullet">
	/// <item>
	///		<b>Type -&gt; <see cref="Nullable{T}"/></b>:<br/>
	///		Maps the source type to the underlying type and casts the result to
	///		<see cref="Nullable{T}"/>. For merge maps, if destination <see cref="Nullable{T}.Value"/>
	///		is <see langword="null"/> a new instance is created and passed to the map.
	/// </item>
	/// <item>
	///		<b><see cref="Nullable{T}"/> -&gt; Type</b>:
	///		<list type="bullet">
	///		<item>
	///			If source <see cref="Nullable{T}.Value"/> is <see langword="null"/>:
	///			<list type="bullet">
	///			<item>
	///				For new maps, if the destination Type is a reference type (nullable)
	///				returns <see langword="null"/>.
	///			</item>
	///			<item>
	///				For new maps, if the destination Type is not a reference type 
	///				throws <see cref="MappingException"/>.
	///			</item>
	///			<item>For merge maps, returns the destination value.</item>
	///			</list>
	///		</item>
	///		<item>
	///			If source <see cref="Nullable{T}.Value"/> is not <see langword="null"/>
	///			maps the underlying type with the destination Type.
	///		</item>
	///		</list>
	/// </item>
	/// <item>
	///		<b><see cref="Nullable{T}"/> -&gt; <see cref="Nullable{T}"/></b>:<br/>
	///		Maps the types as <see cref="Nullable{T}"/> -&gt; Type above and casts the result to
	///		<see cref="Nullable{T}"/>. For merge maps, if destination <see cref="Nullable{T}.Value"/>
	///		is <see langword="null"/> a new instance is created and passed to the map. 
	/// </item>
	/// </list>
	/// </summary>
	public sealed class NullableMapper : IMapper, IMapperFactory, IMapperMaps {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map concrete types of <see cref="Nullable{T}"/>,
		/// will be also provided as a nested mapper in <see cref="MapperOverrideMappingOptions"/>
		/// (if not already present).
		/// </summary>
		private readonly IMapper _concreteMapper;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Creates a new instance of <see cref="NullableMapper"/>.
		/// </summary>
		/// <param name="concreteMapper">
		/// <see cref="IMapper"/> to use to map underlying types.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
		/// </param>
		public NullableMapper(IMapper concreteMapper) {
			_concreteMapper = concreteMapper
				?? throw new ArgumentNullException(nameof(concreteMapper));
			var nestedMappingContext = new NestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<MapperOverrideMappingOptions, NestedMappingContext>(
				m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_concreteMapper, m?.ServiceProvider),
				n => n != null ? new NestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		#region IMapper methods
		public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapInternal(sourceType, destinationType, ref mappingOptions, m => m.CanMapNew, out _, out _);
		}

		public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapInternal(sourceType, destinationType, ref mappingOptions, m => m.CanMapMerge, out _, out _);
		}

		public object? Map(object? source, Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapInternal(sourceType, destinationType, ref mappingOptions, m => m.CanMapNew, out var concreteMapper, out var underlyingTypes) ||
				concreteMapper == null) { 

				throw new MapNotFoundException(types);
			}

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (sourceType != underlyingTypes.From) {
				if (source == null) {
					if (!destinationType.IsValueType || destinationType.IsNullable())
						return null;
					else
						throw new MappingException(new InvalidOperationException("Cannot map null value because the destination type is not nullable."), types);
				}
				else
					source = Convert.ChangeType(source, underlyingTypes.From);
			}

			object? result;
			try { 
				result = concreteMapper.Map(source, underlyingTypes.From, underlyingTypes.To, mappingOptions);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}
			catch (Exception e) {
				throw new MappingException(e, types);
			}

			if (underlyingTypes.To != destinationType)
				result = TypeDescriptor.GetConverter(destinationType).ConvertFrom(result!);

			return result;
		}

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapInternal(sourceType, destinationType, ref mappingOptions, m => m.CanMapNew, out var concreteMapper, out var underlyingTypes) ||
				concreteMapper == null) {

				throw new MapNotFoundException(types);
			}

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			if (sourceType != underlyingTypes.From) {
				if (source == null) 
					return destination;
				else
					source = Convert.ChangeType(source, underlyingTypes.From);
			}

			if (destinationType != underlyingTypes.To) {
				if (destination == null)
					destination = ObjectFactory.Create(underlyingTypes.To);
				else
					destination = Convert.ChangeType(destination, underlyingTypes.To);
			}

			object? result;
			try {
				result = concreteMapper.Map(source, underlyingTypes.From, destination, underlyingTypes.To, mappingOptions);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}
			catch (Exception e) {
				throw new MappingException(e, types);
			}

			if (underlyingTypes.To != destinationType)
				result = TypeDescriptor.GetConverter(destinationType).ConvertFrom(result!);

			return result;
		}
		#endregion
		
		#region IMapperFactory methods
		public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapInternal(sourceType, destinationType, ref mappingOptions, m => m.CanMapNew, out var concreteMapper, out var underlyingTypes) ||
				concreteMapper == null) {

				throw new MapNotFoundException(types);
			}

			INewMapFactory concreteFactory;
			try {
				concreteFactory = concreteMapper.MapNewFactory(underlyingTypes.From, underlyingTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}

			try { 
				var converter = underlyingTypes.To != destinationType ? TypeDescriptor.GetConverter(destinationType) : null;

				return new DisposableNewMapFactory(
					sourceType, destinationType,
					source => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));

						if (sourceType != underlyingTypes.From) {
							if (source == null) {
								if (!destinationType.IsValueType || destinationType.IsNullable())
									return null;
								else
									throw new MappingException(new InvalidOperationException("Cannot map null value because the destination type is not nullable."), types);
							}
							else
								source = Convert.ChangeType(source, underlyingTypes.From);
						}

						object? result;
						try {
							result = concreteFactory.Invoke(source);
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, types);
						}

						if (converter != null)
							result = converter.ConvertFrom(result!);

						return result;
					}, concreteFactory);
			}
			catch {
				concreteFactory.Dispose();
				throw;
			}
		}

		public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapInternal(sourceType, destinationType, ref mappingOptions, m => m.CanMapMerge, out var concreteMapper, out var underlyingTypes) ||
				concreteMapper == null) {

				throw new MapNotFoundException(types);
			}

			IMergeMapFactory concreteFactory;
			try {
				concreteFactory = concreteMapper.MapMergeFactory(underlyingTypes.From, underlyingTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}

			try {
				var converter = underlyingTypes.To != destinationType ? TypeDescriptor.GetConverter(destinationType) : null;

				return new DisposableMergeMapFactory(
					sourceType, destinationType,
					(source, destination) => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						if (sourceType != underlyingTypes.From) {
							if (source == null)
								return destination;
							else
								source = Convert.ChangeType(source, underlyingTypes.From);
						}

						if (destinationType != underlyingTypes.To) {
							if (destination == null)
								destination = ObjectFactory.Create(underlyingTypes.To);
							else
								destination = Convert.ChangeType(destination, underlyingTypes.To);
						}

						object? result;
						try {
							result = concreteFactory.Invoke(source, destination);
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, types);
						}

						if (converter != null)
							result = converter.ConvertFrom(result!);

						return result;
					}, concreteFactory);
			}
			catch {
				concreteFactory.Dispose();
				throw;
			}
		}
		#endregion

		#region IMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetNewMaps(MappingOptions? mappingOptions = null) {
			return GetMapsInternal(mappingOptions, m => m.GetNewMaps);
		}

		public IEnumerable<(Type From, Type To)> GetMergeMaps(MappingOptions? mappingOptions = null) {
			return GetMapsInternal(mappingOptions, m => m.GetMergeMaps);
		}
		#endregion


		private bool CanMapInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			Func<IMapper, Func<Type, Type, MappingOptions, bool>> canMapSelector,
			out IMapper? concreteMapper,
			out (Type From, Type To) underlyingTypes) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
				concreteMapper = null!;
				underlyingTypes = default;

				return sourceType == typeof(Nullable<>) || destinationType == typeof(Nullable<>);
			}
			else { 
				// DEV: disallow nested mapping if one of the most recent mappers is ourselves or a composite mapper containing us?

				mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

				concreteMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
					?? _concreteMapper;

				var canMap = canMapSelector.Invoke(concreteMapper);

				Type? destinationNullableUnderlying;
				if (destinationType.IsNullable()) {
					destinationNullableUnderlying = Nullable.GetUnderlyingType(destinationType)!;
					// Type1? -> Type2
					if(canMap.Invoke(sourceType, destinationNullableUnderlying, mappingOptions)) {
						underlyingTypes = (sourceType, destinationNullableUnderlying);
						return true;
					}
				}
				else
					destinationNullableUnderlying = null;

				Type? sourceNullableUnderlying;
				if (sourceType.IsNullable()) {
					sourceNullableUnderlying = Nullable.GetUnderlyingType(sourceType)!;
					// Type1 -> Type2?
					if (canMap.Invoke(sourceNullableUnderlying, destinationType, mappingOptions)) {
						underlyingTypes = (sourceNullableUnderlying, destinationType);
						return true;
					}
				}
				else
					sourceNullableUnderlying = null;


				// Type1 -> Type2
				if (sourceNullableUnderlying != null && destinationNullableUnderlying != null &&
					canMap.Invoke(sourceNullableUnderlying, destinationNullableUnderlying, mappingOptions)) {

					underlyingTypes = (sourceNullableUnderlying, destinationNullableUnderlying);
					return true;
				}
				else {
					underlyingTypes = default;
					return false;
				}
			}
		}

		private IEnumerable<(Type From, Type To)> GetMapsInternal(
			MappingOptions? mappingOptions,
			Func<IMapper, Func<MappingOptions, IEnumerable<(Type From, Type To)>>> getMapsSelector) {

			// If we are in a nested map retrieval we ignore ourselves
			if (mappingOptions?.GetOptions<NestedMappingContext>()?.CheckRecursive(c => c.ParentMapper == this) == true)
				return [];

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var concreteMapper = mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _concreteMapper;

			return getMapsSelector
				.Invoke(concreteMapper)
				.Invoke(mappingOptions)
				.SelectMany(NullifyType);


			static IEnumerable<(Type From, Type To)> NullifyType((Type From, Type To) type) {
				yield return type;

				var isFromNullable = type.From.IsValueType && !type.From.IsNullable();
				if (isFromNullable)
					yield return (typeof(Nullable<>).MakeGenericType(type.From), type.To);

				var isToNullable = type.To.IsValueType && !type.To.IsNullable();
				if (isToNullable)
					yield return (type.From, typeof(Nullable<>).MakeGenericType(type.To));

				if (isFromNullable && isToNullable)
					yield return (typeof(Nullable<>).MakeGenericType(type.From), typeof(Nullable<>).MakeGenericType(type.To));
			}
		}
	}
}
