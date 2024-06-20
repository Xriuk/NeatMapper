using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper {
	public static class AsyncEnumerableExtensions {
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private class LazyAsyncEnumerable<TSource, TDestination> : IAsyncEnumerable<TDestination> {
			private class LazyAsyncEnumerator : IAsyncEnumerator<TDestination> {
				private readonly AsyncNewMapFactory<TSource, TDestination> _factory;
				private readonly IAsyncEnumerator<TSource> _enumerator;
				private readonly CancellationToken _cancellationToken;

				public TDestination Current { get; private set; }


				public LazyAsyncEnumerator(AsyncNewMapFactory<TSource, TDestination> factory, IAsyncEnumerator<TSource> enumerator, CancellationToken cancellationToken) {
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
			private readonly IAsyncEnumerable<TSource> _enumerable;
			private readonly MappingOptions _mappingOptions;


			public LazyAsyncEnumerable(IAsyncMapper mapper, IAsyncEnumerable<TSource> enumerable, MappingOptions mappingOptions) {
				_mapper = mapper;
				_enumerable = enumerable;
				_mappingOptions = mappingOptions;
			}


			public IAsyncEnumerator<TDestination> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
				return new LazyAsyncEnumerator(_mapper.MapAsyncNewFactory<TSource, TDestination>(_mappingOptions), _enumerable.GetAsyncEnumerator(cancellationToken), cancellationToken);
			}
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		#region Project
		#region Explicit source and destination
		/// <summary>
		/// Projects an async enumerable into another one lazily.
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TDestination"></typeparam>
		/// <param name="mapper">Mapper to use to map the elements.</param>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/param[@name='cancellationToken']"/>
		/// <returns>The projected enumerable, the actual elements may be null.</returns>
		/// <inheritdoc cref="IAsyncMapper.MapAsync(object, Type, Type, MappingOptions, CancellationToken)" path="/exception"/>
		public static IAsyncEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			> Project<TSource, TDestination>(this IAsyncEnumerable<
#pragma warning restore CS1712
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> enumerable,
			IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));

			return new LazyAsyncEnumerable<TSource, TDestination>(mapper, enumerable, mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IAsyncEnumerable{TSource}, IAsyncMapper, MappingOptions)"/>
		public static IAsyncEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			> Project<TSource, TDestination>(this IAsyncEnumerable<
#pragma warning restore CS1712
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> enumerable,
			IAsyncMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return enumerable.Project<TSource, TDestination>(mapper, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
