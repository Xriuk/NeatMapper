using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class CompositeMatcherTests {
		[TestMethod]
		public void ShouldFallbackToNextMapperIfMapRejectsItself() {
			// Rejects itself
			var additionalMaps1 = new CustomMatchAdditionalMapsOptions();
			additionalMaps1.AddMap<string, int>((s, d, c) => throw new MapNotFoundException((typeof(string), typeof(int))));
			var mapper1 = new CustomMatcher(null, additionalMaps1);

			// Result
			var additionalMaps2 = new CustomMatchAdditionalMapsOptions();
			additionalMaps2.AddMap<string, int>((s, d, c) => s?.Length == d);
			var mapper2 = new CustomMatcher(null, additionalMaps2);

			var compositeMatcher = new CompositeMatcher(mapper1, mapper2);

			Assert.IsTrue(compositeMatcher.CanMatch<string, int>());

			Assert.IsTrue(compositeMatcher.Match("Test", 4));
		}
	}
}
