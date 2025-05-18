using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class CollectionMatcherTests {
		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new CollectionMatcher(new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MatcherTests.Maps) }
			}));
		}


		[TestMethod]
		public void ShouldMatchNotOrdered() {
			var options = new MappingOptions(new CollectionMatchersMappingOptions(null, CollectionMatchingOrder.NotOrdered));

			Assert.IsTrue(_matcher.CanMatch<IEnumerable<int>, ICollection<string>>(options));

			Assert.IsTrue(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new []{ "4", "-6", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0 }, new[] { "4", "-6", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0 }, new[] { "4", "0", "0" }, options));
		}

		[TestMethod]
		public void ShouldMatchOrdered() {
			var options = new MappingOptions(new CollectionMatchersMappingOptions(null, CollectionMatchingOrder.Ordered));

			Assert.IsTrue(_matcher.CanMatch<IEnumerable<int>, ICollection<string>>(options));

			Assert.IsTrue(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0", "-6" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "-6", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0 }, new[] { "4", "-6", "0" }, options));
		}

		[TestMethod]
		public void ShouldMatchDefault() {
			var options = new MappingOptions(new CollectionMatchersMappingOptions(null, CollectionMatchingOrder.Default));

			Assert.IsTrue(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0", "-6" }, options));
			Assert.IsTrue(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "-6", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, ICollection<string>>(new[] { 2, 0 }, new[] { "4", "-6", "0" }, options));

			Assert.IsTrue(_matcher.Match<IList<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0", "-6" }, options));
			Assert.IsFalse(_matcher.Match<IList<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "-6", "0" }, options));
			Assert.IsFalse(_matcher.Match<IList<int>, ICollection<string>>(new[] { 2, 0, -3 }, new[] { "4", "0" }, options));
			Assert.IsFalse(_matcher.Match<IList<int>, ICollection<string>>(new[] { 2, 0 }, new[] { "4", "-6", "0" }, options));

			Assert.IsTrue(_matcher.Match<IEnumerable<int>, string[]>(new[] { 2, 0, -3 }, new[] { "4", "0", "-6" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, string[]>(new[] { 2, 0, -3 }, new[] { "4", "-6", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, string[]>(new[] { 2, 0, -3 }, new[] { "4", "0" }, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, string[]>(new[] { 2, 0 }, new[] { "4", "-6", "0" }, options));
		}
	}
}
