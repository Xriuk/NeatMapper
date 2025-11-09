using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps <see cref="Nullable{T}"/> and its underlying types by using another
	/// <see cref="IAsyncMapper"/>. Supports only new maps.
	/// <list type="bullet">
	/// <item>
	/// <b>Type -&gt; <see cref="Nullable{T}"/></b><br/>
	/// Maps the value to the underlying type and casts the result to <see cref="Nullable{T}"/>.
	/// </item>
	/// <item>
	/// <b><see cref="Nullable{T}"/> -&gt; Type</b>:
	/// <list type="bullet">
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is <see langword="null"/> and the Type is a reference type (nullable)
	/// <see langword="null"/> is returned.
	/// </item>
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is <see langword="null"/> and the Type is not a reference type
	/// <see cref="MappingException"/> is thrown.
	/// </item>
	/// <item>
	/// If <see cref="Nullable{T}.Value"/> is not <see langword="null"/> the underlying type is mapped.
	/// </item>
	/// </list>
	/// </item>
	/// <item>
	/// <b><see cref="Nullable{T}"/> -&gt; <see cref="Nullable{T}"/></b><br/>
	/// Tries to map the underlying types in this order:
	/// <list type="number">
	/// <item><see cref="Nullable{T}"/> -&gt; Type and casts the result to <see cref="Nullable{T}"/>.</item>
	/// <item>If <see cref="Nullable{T}.Value"/> is <see langword="null"/> returns <see langword="null"/>.</item>
	/// <item>Type -&gt; <see cref="Nullable{T}"/>.</item>
	/// <item>Type1 -&gt; Type2 and casts the result to <see cref="Nullable{T}"/>.</item>
	/// </list>
	/// </item>
	/// </list>
	/// </summary>
	public sealed class AsyncNullableMapper : IAsyncMapper, IAsyncMapperFactory, IAsyncMapperMaps {
		/// <summary>
		/// <see cref="IAsyncMapper"/> which is used to map concrete types of <see cref="Nullable{T}"/>,
		/// will be also provided as a nested mapper in <see cref="AsyncMapperOverrideMappingOptions"/>
		/// (if not already present).
		/// </summary>
		private readonly IAsyncMapper _concreteMapper;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Creates a new instance of <see cref="NullableMapper"/>.
		/// </summary>
		/// <param name="concreteMapper">
		/// <see cref="IAsyncMapper"/> to use to map underlying types.<br/>
		/// Can be overridden during mapping with <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </param>
		public AsyncNullableMapper(IAsyncMapper concreteMapper) {
			_concreteMapper = concreteMapper
				?? throw new ArgumentNullException(nameof(concreteMapper));
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<AsyncMapperOverrideMappingOptions, AsyncNestedMappingContext>(
				m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(_concreteMapper, m?.ServiceProvider),
				n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out _, out _);
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return false; // DEV: check
		}

		public async Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var concreteMapper, out var underlyingTypes) || concreteMapper == null)
				throw new MapNotFoundException(types);

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			if (sourceType != underlyingTypes.From) {
				if(source == null) { 
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
				result = await concreteMapper.MapAsync(source, underlyingTypes.From, underlyingTypes.To, mappingOptions, cancellationToken);
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

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			throw new MapNotFoundException((sourceType, destinationType)); // DEV: check
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanMapNewInternal(sourceType, destinationType, ref mappingOptions, out var concreteMapper, out var underlyingTypes) || concreteMapper == null)
				throw new MapNotFoundException(types);

			IAsyncNewMapFactory concreteFactory;
			try {
				concreteFactory = concreteMapper.MapAsyncNewFactory(underlyingTypes.From, underlyingTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}

			try {
				var converter = underlyingTypes.To != destinationType ? TypeDescriptor.GetConverter(destinationType) : null;

				return new DisposableAsyncNewMapFactory(
					sourceType, destinationType,
					async (source, cancellationToken) => {
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
							result = await concreteFactory.Invoke(source, cancellationToken);
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

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			throw new MapNotFoundException((sourceType, destinationType)); // DEV: check
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(MappingOptions? mappingOptions = null) {
			// If we are in a nested map retrieval we ignore ourselves
			if (mappingOptions?.GetOptions<AsyncNestedMappingContext>()?.CheckRecursive(c => c.ParentMapper == this) == true)
				return [];

			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			var concreteMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
				?? _concreteMapper;

			return concreteMapper.GetAsyncNewMaps(mappingOptions).SelectMany(NullifyType);


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

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(MappingOptions? mappingOptions = null) {
			return [];
		}
		#endregion


		private bool CanMapNewInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out IAsyncMapper concreteMapper,
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

				concreteMapper = mappingOptions.GetOptions<AsyncMapperOverrideMappingOptions>()?.Mapper
					?? _concreteMapper;

				Type? destinationNullableUnderlying;
				if (destinationType.IsNullable()) {
					destinationNullableUnderlying = Nullable.GetUnderlyingType(destinationType)!;
					// Type1? -> Type2
					if (concreteMapper.CanMapAsyncNew(sourceType, destinationNullableUnderlying, mappingOptions)) {
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
					if (concreteMapper.CanMapAsyncNew(sourceNullableUnderlying, destinationType, mappingOptions)) {
						underlyingTypes = (sourceNullableUnderlying, destinationType);
						return true;
					}
				}
				else
					sourceNullableUnderlying = null;


				// Type1 -> Type2
				if (sourceNullableUnderlying != null && destinationNullableUnderlying != null &&
					concreteMapper.CanMapAsyncNew(sourceNullableUnderlying, destinationNullableUnderlying, mappingOptions)) {

					underlyingTypes = (sourceNullableUnderlying, destinationNullableUnderlying);
					return true;
				}
				else {
					underlyingTypes = default;
					return false;
				}
			}
		}
	}
}
