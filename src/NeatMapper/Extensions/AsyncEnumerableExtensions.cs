using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	public static class AsyncEnumerableExtensions {
		private class LazyAsyncEnumerable<TSource, TDestination> : IAsyncEnumerable<TDestination?> {
			private class LazyAsyncEnumerator : IAsyncEnumerator<TDestination?> {
				private readonly AsyncNewMapFactory<TSource, TDestination> _factory;
				private readonly IAsyncEnumerator<TSource?> _enumerator;
				private readonly CancellationToken _cancellationToken;

				public TDestination? Current { get; private set; } = default;


				public LazyAsyncEnumerator(AsyncNewMapFactory<TSource, TDestination> factory, IAsyncEnumerator<TSource?> enumerator, CancellationToken cancellationToken) {
					_factory = factory;
					_enumerator = enumerator;
					_cancellationToken = cancellationToken;
				}


				public ValueTask DisposeAsync() {
					_factory.Dispose();
					return _enumerator.DisposeAsync();
				}

				public async ValueTask<bool> MoveNextAsync() {
					if(await _enumerator.MoveNextAsync()) {
						Current = await _factory.Invoke(_enumerator.Current, _cancellationToken);
						return true;
					}
					else {
						Current = default;
						return false;
					}
				}
			}


			private readonly IAsyncMapper _mapper;
			private readonly IAsyncEnumerable<TSource?> _enumerable;
			private readonly MappingOptions? _mappingOptions;


			public LazyAsyncEnumerable(IAsyncMapper mapper, IAsyncEnumerable<TSource?> enumerable, MappingOptions? mappingOptions) {
				_mapper = mapper;
				_enumerable = enumerable;
				_mappingOptions = mappingOptions;
			}


			public IAsyncEnumerator<TDestination?> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
				return new LazyAsyncEnumerator(_mapper.MapAsyncNewFactory<TSource, TDestination>(_mappingOptions), _enumerable.GetAsyncEnumerator(cancellationToken), cancellationToken);
			}
		}


		#region Project
		#region Explicit source and destination
		/// <summary>
		/// Projects an async enumerable into another one lazily.
		/// </summary>
		/// <typeparam name="TSource">Type of the source element, used to retrieve the available maps.</typeparam>
		/// <typeparam name="TDestination">
		/// Type of the destination element, used to retrieve the available maps.
		/// </typeparam>
		/// <param name="mapper">Mapper to use to map the elements.</param>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object?, Type, Type, MappingOptions?, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object?, Type, Type, MappingOptions?, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>The projected enumerable, the actual elements may be null.</returns>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object?, Type, Type, MappingOptions?, CancellationToken)" path="/exception"/>
		public static IAsyncEnumerable<TDestination?> Project<TSource, TDestination>(this IAsyncEnumerable<TSource?> enumerable,
			IAsyncMapper mapper,
			MappingOptions? mappingOptions = null) {

			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			if(!mapper.CanMapAsyncNew<TSource, TDestination>(mappingOptions))
				throw new MapNotFoundException((typeof(TSource), typeof(TDestination)));

			return new LazyAsyncEnumerable<TSource, TDestination>(mapper, enumerable, mappingOptions);
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IAsyncEnumerable{TSource}, IAsyncMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncEnumerable<TDestination?> Project<TSource, TDestination>(this IAsyncEnumerable<TSource?> enumerable,
			IAsyncMapper mapper,
			IEnumerable? mappingOptions) {

			return enumerable.Project<TSource, TDestination>(mapper, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IAsyncEnumerable{TSource}, IAsyncMapper, MappingOptions?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IAsyncEnumerable<TDestination?> Project<TSource, TDestination>(this IAsyncEnumerable<TSource?> enumerable,
			IAsyncMapper mapper,
			params object?[]? mappingOptions) {

			return enumerable.Project<TSource, TDestination>(mapper, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
