using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.AsyncMapping {
	[TestClass]
	public class AsyncCompositeMapperTests {
		public class TestMapper1 : IAsyncMapper, IAsyncMapperFactory {
			public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return sourceType == typeof(float) && destinationType == typeof(string);
			}

			public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return false;
			}

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
		public async Task MapAsyncNewFactoryShouldReturnMergeToo() {
			var compositeMapper = new AsyncCompositeMapper(new TestMapper1());

			using (var factory = compositeMapper.MapAsyncNewFactory<float, string>()) {
				Assert.AreEqual("4", await factory.Invoke(2f));
			}
		}
	}
}
