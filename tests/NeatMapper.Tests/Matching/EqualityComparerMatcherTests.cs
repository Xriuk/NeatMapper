using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class EqualityComparerMatcherTests {
		public class EqualityComparerClass :
			IEqualityComparer<string>,
			IEqualityComparer<int>,
			IEqualityComparer<short>{

			public bool Equals(string x, string y) {
				return x.Length == y.Length;
			}

			public int GetHashCode(string obj) {
				throw new NotImplementedException();
			}


			public bool Equals(int x, int y) {
				return x == y;
			}

			public int GetHashCode(int obj) {
				throw new NotImplementedException();
			}


			public bool Equals(short x, short y) {
				throw new InvalidOperationException("Error");
			}

			public int GetHashCode(short obj) {
				throw new NotImplementedException();
			}
		}


		[TestMethod]
		public void ShouldMatch() {
			IMatcher matcher = EqualityComparerMatcher.Create<string>(new EqualityComparerClass());

			Assert.IsTrue(matcher.CanMatch<string, string>());
			Assert.IsFalse(matcher.CanMatch<string, int>());
			Assert.IsFalse(matcher.CanMatch<int, string>());
			Assert.IsFalse(matcher.CanMatch<int, int>());

			Assert.IsTrue(matcher.Match("abcd", "efgh"));
			Assert.IsFalse(matcher.Match("abc", "efgh"));
			Assert.IsFalse(matcher.Match("abcd", "efg"));

			using (var factory = matcher.MatchFactory<string, string>()) {
				Assert.IsTrue(factory.Invoke("abcd", "efgh"));
				Assert.IsFalse(factory.Invoke("abc", "efgh"));
				Assert.IsFalse(factory.Invoke("abcd", "efg"));
			}
		}

		[TestMethod]
		public void ShouldMatchCompatibleNullableIfSupported() {
			IMatcher matcher = EqualityComparerMatcher.Create<int>(new EqualityComparerClass(), new NullableTypesMatchingOptions {
				SupportNullableTypes = true
			});

			Assert.IsFalse(matcher.CanMatch<string, int?>());
			Assert.IsFalse(matcher.CanMatch<int?, string>());

			Assert.IsTrue(matcher.CanMatch<int, int?>());
			Assert.IsTrue(matcher.CanMatch<int?, int>());
			Assert.IsTrue(matcher.CanMatch<int?, int?>());

			Assert.IsTrue(matcher.Match<int, int?>(4, 4));
			Assert.IsFalse(matcher.Match<int, int?>(4, 3));
			Assert.IsTrue(matcher.Match<int, int?>(3, 3));
			Assert.IsFalse(matcher.Match<int, int?>(4, null));

			Assert.IsTrue(matcher.Match<int?, int>(4, 4));
			Assert.IsFalse(matcher.Match<int?, int>(4, 3));
			Assert.IsFalse(matcher.Match<int?, int>(null, 3));

			Assert.IsTrue(matcher.Match<int?, int?>(4, 4));
			Assert.IsFalse(matcher.Match<int?, int?>(4, 3));
			Assert.IsFalse(matcher.Match<int?, int?>(4, null));
			Assert.IsFalse(matcher.Match<int?, int?>(null, 3));
			Assert.IsTrue(matcher.Match<int?, int?>(null, null));

			using (var factory = matcher.MatchFactory<int, int?>()) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
				Assert.IsFalse(factory.Invoke(4, null));
			}
			using (var factory = matcher.MatchFactory<int?, int>()) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
				Assert.IsFalse(factory.Invoke(null, 3));
			}
			using (var factory = matcher.MatchFactory<int?, int?>()) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
				Assert.IsFalse(factory.Invoke(4, null));
				Assert.IsFalse(factory.Invoke(null, 3));
				Assert.IsTrue(factory.Invoke(null, null));
			}
		}

		[TestMethod]
		public void ShouldThrowExceptionsCorrectly() {
			IMatcher matcher = EqualityComparerMatcher.Create<short>(new EqualityComparerClass());

			var exc = Assert.ThrowsException<MatcherException>(() => matcher.Match<short, short>(2, 2));
			Assert.IsTrue(exc.InnerException is InvalidOperationException);
			Assert.AreEqual("Error", exc.InnerException.Message);
		}
	}
}
