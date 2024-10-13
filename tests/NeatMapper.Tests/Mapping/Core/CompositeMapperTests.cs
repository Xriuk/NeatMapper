using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class CompositeMapperTests {
		public class TestMapper1 : IMapper, IMapperFactory {
			public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return true;
			}

			public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			public object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			public object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return new DefaultNewMapFactory(sourceType, destinationType, source => throw new MapNotFoundException((sourceType, destinationType)));
			}

			public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}
		}

		public class TestMapper2 : IMapper, IMapperFactory {
			public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			public object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				if(sourceType != typeof(float) || destinationType != typeof(string))
					throw new MapNotFoundException((sourceType, destinationType));

				return new DefaultMergeMapFactory<float, string>((f, s) => (f * 2f).ToString());
			}

			public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}
		}


		/*[TestMethod]
		public void ShouldForwardNewMapToMergeMapIfNotFound() {
			var additionalMaps = new CustomMergeAdditionalMapsOptions();
			additionalMaps.AddMap<string, int>((s, d, c) => s?.Length ?? 0);
			var mapper = new MergeMapper(null, additionalMaps);

			var compositeMapper = new CompositeMapper(mapper);

			Assert.IsTrue(compositeMapper.CanMapNew<string, int>());

			Assert.AreEqual(4, compositeMapper.Map<int>("Test"));
		}*/

		[TestMethod]
		public void ShouldFallbackToNextMapperIfMapRejectsItself() {
			// Rejects itself
			var additionalMaps1 = new CustomNewAdditionalMapsOptions();
			additionalMaps1.AddMap<string, int>((s, c) => throw new MapNotFoundException((typeof(string), typeof(int))));
			var mapper1 = new NewMapper(null, additionalMaps1);

			// Result
			var additionalMaps2 = new CustomNewAdditionalMapsOptions();
			additionalMaps2.AddMap<string, int>((s, c) => s?.Length ?? 0);
			var mapper2 = new NewMapper(null, additionalMaps2);

			var compositeMapper = new CompositeMapper(mapper1, mapper2);

			Assert.IsTrue(compositeMapper.CanMapNew<string, int>());

			Assert.AreEqual(4, compositeMapper.Map<int>("Test"));
		}

		[TestMethod]
		public void ShouldFallbackToNextMapperIfMapFactoryRejectsItself() {
			// Rejects itself
			var mapper1 = new TestMapper1();

			// Result
			var additionalMaps2 = new CustomNewAdditionalMapsOptions();
			additionalMaps2.AddMap<string, int>((s, c) => s?.Length ?? 0);
			var mapper2 = new NewMapper(null, additionalMaps2);

			var compositeMapper = new CompositeMapper(mapper1, mapper2);

			Assert.IsTrue(compositeMapper.CanMapNew<string, int>());

			using(var factory = compositeMapper.MapNewFactory<string, int>()) { 
				Assert.AreEqual(4, factory.Invoke("Test"));
			}
		}

		[TestMethod]
		public void MapNewFactoryShouldReturnMergeToo() {
			var compositeMapper = new CompositeMapper(new TestMapper2());

			using(var factory = compositeMapper.MapNewFactory<float, string>()) {
				Assert.AreEqual("4", factory.Invoke(2f));
			}
		}
	}
}
