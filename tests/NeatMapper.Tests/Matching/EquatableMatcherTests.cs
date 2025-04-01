using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class EquatableMatcherTests {
		public class EquatableClass :
			IEquatable<string>,
			IEquatable<int>,
			IEquatable<decimal>,
			IEquatable<decimal?>,
			IEquatable<short>{

			bool IEquatable<string>.Equals(string other) {
				return other.Length == 4;
			}

			bool IEquatable<int>.Equals(int other) {
				return other == 4;
			}

			bool IEquatable<decimal>.Equals(decimal other) {
				return other == 8;
			}
			bool IEquatable<decimal?>.Equals(decimal? other) {
				return other == 12;
			}

			bool IEquatable<short>.Equals(short other) {
				throw new InvalidOperationException("Error");
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new EquatableMatcher();
		}


		[TestMethod]
		public void ShouldMatch() {
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, string>());
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, int>());
			Assert.IsFalse(_matcher.CanMatch<EquatableClass, int?>());
			Assert.IsFalse(_matcher.CanMatch<string, EquatableClass>());
			Assert.IsFalse(_matcher.CanMatch<int, EquatableClass>());
			Assert.IsFalse(_matcher.CanMatch<EquatableClass, EquatableClass>());
			Assert.IsFalse(_matcher.CanMatch<string, int>());
			Assert.IsFalse(_matcher.CanMatch<string, int?>());
			Assert.IsFalse(_matcher.CanMatch<int, string>());
			Assert.IsFalse(_matcher.CanMatch<int?, string>());

			Assert.IsTrue(_matcher.CanMatch<string, string>());
			Assert.IsTrue(_matcher.CanMatch<int, int>());
			Assert.IsFalse(_matcher.CanMatch<int, int?>());
			Assert.IsFalse(_matcher.CanMatch<int?, int>());
			Assert.IsFalse(_matcher.CanMatch<int?, int?>());

			Assert.IsTrue(_matcher.Match(new EquatableClass(), "abcd"));
			Assert.IsFalse(_matcher.Match(new EquatableClass(), "abc"));
			Assert.IsTrue(_matcher.Match(new EquatableClass(), 4));
			Assert.IsFalse(_matcher.Match(new EquatableClass(), 3));


			Assert.IsTrue(_matcher.Match("abcd", "abcd"));
			Assert.IsFalse(_matcher.Match("abcd", "abc"));

			Assert.IsTrue(_matcher.Match<int, int>(4, 4));
			Assert.IsFalse(_matcher.Match<int, int>(4, 3));


			using (var factory = _matcher.MatchFactory<EquatableClass, string>()) {
				Assert.IsTrue(factory.Invoke(new EquatableClass(), "abcd"));
				Assert.IsFalse(factory.Invoke(new EquatableClass(), "abc"));
			}
			using (var factory = _matcher.MatchFactory<EquatableClass, int>()) {
				Assert.IsTrue(factory.Invoke(new EquatableClass(), 4));
				Assert.IsFalse(factory.Invoke(new EquatableClass(), 3));
			}

			using (var factory = _matcher.MatchFactory<string, string>()) {
				Assert.IsTrue(factory.Invoke("abcd", "abcd"));
				Assert.IsFalse(factory.Invoke("abcd", "abc"));
			}
			using (var factory = _matcher.MatchFactory<int, int>()) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
			}
		}

		[TestMethod]
		public void ShouldMatchCompatibleNullableIfSupported() {
			var options = new MappingOptions(new NullableTypesMatchingMappingOptions(true));

			Assert.IsTrue(_matcher.CanMatch<EquatableClass, int?>(options));

			Assert.IsFalse(_matcher.CanMatch<string, int?>(options));
			Assert.IsFalse(_matcher.CanMatch<int?, string>(options));

			Assert.IsTrue(_matcher.CanMatch<int, int?>(options));
			Assert.IsTrue(_matcher.CanMatch<int?, int>(options));
			Assert.IsTrue(_matcher.CanMatch<int?, int?>(options));

			Assert.IsTrue(_matcher.Match<EquatableClass, int?>(new EquatableClass(), 4, options));
			Assert.IsFalse(_matcher.Match<EquatableClass, int?>(new EquatableClass(), 3, options));
			Assert.IsFalse(_matcher.Match<EquatableClass, int?>(new EquatableClass(), null, options));
			Assert.IsFalse(_matcher.Match<EquatableClass, int?>(null, 3, options));
			Assert.IsTrue(_matcher.Match<EquatableClass, int?>(null, null, options));

			Assert.IsTrue(_matcher.Match<int, int?>(4, 4, options));
			Assert.IsFalse(_matcher.Match<int, int?>(4, 3, options));
			Assert.IsFalse(_matcher.Match<int, int?>(4, null, options));

			Assert.IsTrue(_matcher.Match<int?, int>(4, 4, options));
			Assert.IsFalse(_matcher.Match<int?, int>(4, 3, options));
			Assert.IsFalse(_matcher.Match<int?, int>(null, 3, options));

			Assert.IsTrue(_matcher.Match<int?, int?>(4, 4, options));
			Assert.IsFalse(_matcher.Match<int?, int?>(4, 3, options));
			Assert.IsFalse(_matcher.Match<int?, int?>(4, null, options));
			Assert.IsFalse(_matcher.Match<int?, int?>(null, 3, options));
			Assert.IsTrue(_matcher.Match<int?, int?>(null, null, options));

			using (var factory = _matcher.MatchFactory<EquatableClass, int?>(options)) {
				Assert.IsTrue(factory.Invoke(new EquatableClass(), 4));
				Assert.IsFalse(factory.Invoke(new EquatableClass(), 3));
				Assert.IsTrue(factory.Invoke(null, null));
				Assert.IsFalse(factory.Invoke(null, 4));
				Assert.IsFalse(factory.Invoke(new EquatableClass(), null));
			}

			using (var factory = _matcher.MatchFactory<int, int?>(options)) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
				Assert.IsFalse(factory.Invoke(4, null));
			}
			using (var factory = _matcher.MatchFactory<int?, int>(options)) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
				Assert.IsFalse(factory.Invoke(null, 3));
			}
			using (var factory = _matcher.MatchFactory<int?, int?>(options)) {
				Assert.IsTrue(factory.Invoke(4, 4));
				Assert.IsFalse(factory.Invoke(4, 3));
				Assert.IsFalse(factory.Invoke(4, null));
				Assert.IsFalse(factory.Invoke(null, 3));
				Assert.IsTrue(factory.Invoke(null, null));
			}
		}

		[TestMethod]
		public void ShouldPreferExplicitNullableEquatableIfImplemented() {
			var options = new MappingOptions(new NullableTypesMatchingMappingOptions(true));

			Assert.IsTrue(_matcher.CanMatch<EquatableClass, decimal>(options));
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, decimal?>(options));

			Assert.IsTrue(_matcher.Match<EquatableClass, decimal>(new EquatableClass(), 8m, options));
			Assert.IsFalse(_matcher.Match<EquatableClass, decimal>(new EquatableClass(), 4m, options));

			Assert.IsTrue(_matcher.Match<EquatableClass, decimal?>(new EquatableClass(), 12m, options));
			Assert.IsFalse(_matcher.Match<EquatableClass, decimal?>(new EquatableClass(), 8m, options));
		}

		[TestMethod]
		public void ShouldThrowExceptionsCorrectly() {
			var exc = Assert.ThrowsException<MatcherException>(() => _matcher.Match<EquatableClass, short>(new EquatableClass(), 2));
			Assert.IsTrue(exc.InnerException is InvalidOperationException);
			Assert.AreEqual("Error", exc.InnerException.Message);

			using(var factory = _matcher.MatchFactory<EquatableClass, short>()) {
				exc = Assert.ThrowsException<MatcherException>(() => factory.Invoke(new EquatableClass(), 2));
				Assert.IsTrue(exc.InnerException is InvalidOperationException);
				Assert.AreEqual("Error", exc.InnerException.Message);
			}
		}
	}
}
