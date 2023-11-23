using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class CompositeMapperTests {
		[TestMethod]
		public void ShouldForwardNewMapToMergeMapIfNotFound() {
			var additionalMaps = new CustomMergeAdditionalMapsOptions();
			additionalMaps.AddMap<string, int>((s, d, c) => s?.Length ?? 0);
			var mapper = new MergeMapper(null, additionalMaps);

			var compositeMapper = new CompositeMapper(mapper);

			Assert.IsTrue(compositeMapper.CanMapNew<string, int>());

			Assert.AreEqual(4, compositeMapper.Map<int>("Test"));
		}

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
	}
}
