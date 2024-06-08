using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

			var compositeMatcher = new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = new List<IMatcher> { mapper1, mapper2 }
			});

			Assert.IsTrue(compositeMatcher.CanMatch<string, int>());
			Assert.IsTrue(compositeMatcher.CanMatch<int, string>());

			Assert.IsTrue(compositeMatcher.Match("Test", 4));
			Assert.IsTrue(compositeMatcher.Match(4, "Test"));
		}
	}
}
