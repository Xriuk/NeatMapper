using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class EqualityComparerMatcherTests {
		public class EqualityComparerClass : IEqualityComparer<string> {
			public bool Equals(string x, string y) {
				return x.Length == y.Length;
			}

			public int GetHashCode(string obj) {
				throw new NotImplementedException();
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = EqualityComparerMatcher.Create(new EqualityComparerClass());
		}


		[TestMethod]
		public void ShouldMatch() {
			Assert.IsTrue(_matcher.CanMatch<string, string>());
			Assert.IsFalse(_matcher.CanMatch<string, int>());
			Assert.IsFalse(_matcher.CanMatch<int, string>());
			Assert.IsFalse(_matcher.CanMatch<int, int>());

			Assert.IsTrue(_matcher.Match("abcd", "efgh"));
			Assert.IsFalse(_matcher.Match("abc", "efgh"));
			Assert.IsFalse(_matcher.Match("abcd", "efg"));

			using (var factory = _matcher.MatchFactory<string, string>()) {
				Assert.IsTrue(factory.Invoke("abcd", "efgh"));
				Assert.IsFalse(factory.Invoke("abc", "efgh"));
				Assert.IsFalse(factory.Invoke("abcd", "efg"));
			}
		}
	}
}
