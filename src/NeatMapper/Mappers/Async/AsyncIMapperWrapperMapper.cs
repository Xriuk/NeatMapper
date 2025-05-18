using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IAsyncMapper"/> which maps types synchronously by using a <see cref="IMapper"/>.
	/// </summary>
	public sealed class AsyncIMapperWrapperMapper : IAsyncMapper, IAsyncMapperFactory, IAsyncMapperMaps {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map the types synchronously,
		/// will be also provided as a nested mapper in <see cref="MapperOverrideMappingOptions"/>
		/// (if not already present).
		/// </summary>
		private readonly IMapper _mapper;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;

		/// <summary>
		/// Creates a new instance of <see cref="AsyncIMapperWrapperMapper"/>.
		/// </summary>
		/// <param name="mapper">
		/// <see cref="IMapper"/> which is used to map the types synchronously.<br/>
		/// Can be overridden during mapping with <see cref="MapperOverrideMappingOptions"/>.
		/// </param>
		public AsyncIMapperWrapperMapper(IMapper mapper) {
			_mapper = mapper
				?? throw new ArgumentNullException(nameof(mapper));
			var nestedMappingContext = new AsyncNestedMappingContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<
				MapperOverrideMappingOptions,
				AsyncMapperOverrideMappingOptions,
				AsyncNestedMappingContext>(
					m => m?.Mapper != null ? m : new MapperOverrideMappingOptions(_mapper, m?.ServiceProvider),
					m => m?.Mapper != null ? m : new AsyncMapperOverrideMappingOptions(this, m?.ServiceProvider),
					n => n != null ? new AsyncNestedMappingContext(nestedMappingContext.ParentMapper, n) : nestedMappingContext, options.Cached));
		}


		#region IAsyncMapper methods
		public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return GetMapper(ref mappingOptions).CanMapNew(sourceType, destinationType, mappingOptions);
		}

		public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return GetMapper(ref mappingOptions).CanMapMerge(sourceType, destinationType, mappingOptions);
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));

			cancellationToken.ThrowIfCancellationRequested();

			return Task.FromResult(GetMapper(ref mappingOptions).Map(source, sourceType, destinationType, mappingOptions));
		}

		public Task<object?> MapAsync(
			object? source,
			Type sourceType,
			object? destination,
			Type destinationType,
			MappingOptions? mappingOptions = null,
			CancellationToken cancellationToken = default) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			cancellationToken.ThrowIfCancellationRequested();

			try { 
				return Task.FromResult(GetMapper(ref mappingOptions).Map(source, sourceType, destination, destinationType, mappingOptions));
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (MapNotFoundException) { // Do not wrap as types are the same
				throw;
			}
			catch (MappingException) { // Do not wrap as types are the same
				throw;
			}
			catch (Exception e) {
				throw new MappingException(e, (sourceType, destinationType));
			}
		}
		#endregion

		#region IAsyncMapperFactory methods
		public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Might throw MapNotFoundException and we're not wrapping it as the types are the same
			var factory = GetMapper(ref mappingOptions).MapNewFactory(sourceType, destinationType, mappingOptions);

			try {
				return new DisposableAsyncNewMapFactory(
					sourceType, destinationType,
					(source, cancellationToken) => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));

						cancellationToken.ThrowIfCancellationRequested();

						try { 
							return Task.FromResult(factory.Invoke(source));
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}
					}, factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
		}

		public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Might throw MapNotFoundException and we're not wrapping it as the types are the same
			var factory = GetMapper(ref mappingOptions).MapMergeFactory(sourceType, destinationType, mappingOptions);

			try {
				return new DisposableAsyncMergeMapFactory(
					sourceType, destinationType,
					(source, destination, cancellationToken) => {
						TypeUtils.CheckObjectType(source, sourceType, nameof(source));
						TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

						cancellationToken.ThrowIfCancellationRequested();

						try {
							return Task.FromResult(factory.Invoke(source, destination));
						}
						catch (OperationCanceledException) {
							throw;
						}
						catch (Exception e) {
							throw new MappingException(e, (sourceType, destinationType));
						}
					}, factory);
			}
			catch {
				factory.Dispose();
				throw;
			}
		}
		#endregion

		#region IAsyncMapperMaps methods
		public IEnumerable<(Type From, Type To)> GetAsyncNewMaps(MappingOptions? mappingOptions = null) {
			return GetMapper(ref mappingOptions).GetNewMaps(mappingOptions);
		}

		public IEnumerable<(Type From, Type To)> GetAsyncMergeMaps(MappingOptions? mappingOptions = null) {
			return GetMapper(ref mappingOptions).GetMergeMaps(mappingOptions);
		}
		#endregion


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private IMapper GetMapper([NotNull] ref MappingOptions? mappingOptions) {
			mappingOptions = _optionsCache.GetOrCreate(mappingOptions);

			return mappingOptions.GetOptions<MapperOverrideMappingOptions>()?.Mapper
				?? _mapper;
		}
	}
}
