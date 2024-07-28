using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	[TestClass]
	public class AsyncCompositeMapperTests {
		public class TestMapper1 : IAsyncMapper, IAsyncMapperFactory {
			public Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				if (sourceType != typeof(float) || destinationType != typeof(string))
					throw new MapNotFoundException((sourceType, destinationType));

				return new DefaultAsyncMergeMapFactory<float, string>((f, s, c) => Task.FromResult((f * 2f).ToString()));
			}

			public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}
		}


		[TestMethod]
		public async Task ShouldForwardNewMapToMergeMapIfNotFound() {
			var additionalMaps = new CustomAsyncMergeAdditionalMapsOptions();
			additionalMaps.AddMap<string, int>((s, d, c) => Task.FromResult(s?.Length ?? 0));
			var mapper = new AsyncMergeMapper(null, additionalMaps);

			var compositeMapper = new AsyncCompositeMapper(mapper);

			Assert.IsTrue(await compositeMapper.CanMapAsyncNew<string, int>());

			Assert.AreEqual(4, await compositeMapper.MapAsync<int>("Test"));
		}

		[TestMethod]
		public async Task ShouldFallbackToNextMapperIfMapRejectsItself() {
			// Rejects itself (not awaited)
			var additionalMaps1 = new CustomAsyncNewAdditionalMapsOptions();
			additionalMaps1.AddMap<string, int>((s, c) => throw new MapNotFoundException((typeof(string), typeof(int))));
			var mapper1 = new AsyncNewMapper(null, additionalMaps1);

			// Rejects itself (awaited)
			var additionalMaps2 = new CustomAsyncNewAdditionalMapsOptions();
			additionalMaps2.AddMap<string, int>(async (s, c) => {
				await Task.Delay(0);
				throw new MapNotFoundException((typeof(string), typeof(int)));
			});
			var mapper2 = new AsyncNewMapper(null, additionalMaps2);

			// Result
			var additionalMaps3 = new CustomAsyncNewAdditionalMapsOptions();
			additionalMaps3.AddMap<string, int>((s, c) => Task.FromResult(s?.Length ?? 0));
			var mapper3 = new AsyncNewMapper(null, additionalMaps3);

			var compositeMapper = new AsyncCompositeMapper(mapper1, mapper2, mapper3);

			Assert.IsTrue(await compositeMapper.CanMapAsyncNew<string, int>());

			Assert.AreEqual(4, await compositeMapper.MapAsync<int>("Test"));
		}

		[TestMethod]
		public async Task MapAsyncNewFactoryShouldReturnMergeToo() {
			var compositeMapper = new AsyncCompositeMapper(new TestMapper1());

			using (var factory = compositeMapper.MapAsyncNewFactory<float, string>()) {
				Assert.AreEqual("4", await factory.Invoke(2f));
			}
		}
	}
}
