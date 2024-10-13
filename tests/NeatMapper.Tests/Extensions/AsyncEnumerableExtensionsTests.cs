using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class AsyncEnumerableExtensionsTests {
		private class NewFactory : IAsyncNewMapFactory {
			Type IAsyncNewMapFactory.SourceType => throw new NotImplementedException();

			Type IAsyncNewMapFactory.DestinationType => throw new NotImplementedException();

			public static bool Disposed = false;

			void IDisposable.Dispose() {
				Disposed = true;
			}

			Task<object> IAsyncNewMapFactory.Invoke(object source, CancellationToken cancellationToken) {
				return Task.FromResult<object>(1);
			}
		}

		private class CustomMapper : IAsyncMapper, IAsyncMapperFactory {
			public Task<bool> CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default) {
				throw new NotImplementedException();
			}

			public Task<bool> CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default) {
				throw new NotImplementedException();
			}

			Task<object> IAsyncMapper.MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken) {
				throw new NotImplementedException();
			}

			Task<object> IAsyncMapper.MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions, CancellationToken cancellationToken) {
				throw new NotImplementedException();
			}

			IAsyncMergeMapFactory IAsyncMapperFactory.MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions) {
				throw new NotImplementedException();
			}

			IAsyncNewMapFactory IAsyncMapperFactory.MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions) {
				return new NewFactory();
			}
		}

		public class Maps :
			IAsyncNewMap<string, int>{

			public static int maps = 0;

			Task<int> IAsyncNewMap<string, int>.MapAsync(string source, AsyncMappingContext context) {
				maps++;
				return Task.FromResult(source?.Length ?? 0);
			}
		}


		IAsyncMapper _mapper;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => o.TypesToScan.Add(typeof(Maps)));
			using(var services = serviceCollection.BuildServiceProvider()) { 
				_mapper = services.GetRequiredService<IAsyncMapper>();
			}
		}

		[TestMethod]
		public async Task ShouldProjectLazily() {
			var mappedEnumerable = new DefaultAsyncEnumerable<string>(new[] { "AAA", "BB", "C" }).Project<string, int>(_mapper);
			var enumerator = mappedEnumerable.GetAsyncEnumerator();
			try { 
				Assert.IsTrue(await enumerator.MoveNextAsync());
				Assert.AreEqual(3, enumerator.Current);
				Assert.IsTrue(await enumerator.MoveNextAsync());
				Assert.AreEqual(2, enumerator.Current);
			}
			finally {
				await enumerator.DisposeAsync();
			}
			Assert.AreEqual(2, Maps.maps); // Only 2 elements actually mapped, not 3
		}
	}
}
