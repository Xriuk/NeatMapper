#if NET7_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class EqualityOperatorsMatcherTests {
#pragma warning disable CS0660
#pragma warning disable CS0661
		public class EquatableClass :
#pragma warning restore CS0661
#pragma warning restore CS0660
			IEqualityOperators<EquatableClass, string, bool>,
			IEqualityOperators<EquatableClass, int, bool>,
			IEqualityOperators<EquatableClass, decimal, string>,
			IEqualityOperators<EquatableClass, float, bool>,
			IEqualityOperators<EquatableClass, float?, bool>,
			IEqualityOperators<EquatableClass, short, bool> {

			public int Member { get; set; }


			public static bool operator ==(EquatableClass? left, string? right) {
				return left?.Member == right?.Length;
			}
			public static bool operator !=(EquatableClass? left, string? right) {
				throw new NotImplementedException();
			}

			static bool IEqualityOperators<EquatableClass, int, bool>.operator ==(EquatableClass? left, int right) {
				return left?.Member == right;
			}
			static bool IEqualityOperators<EquatableClass, int, bool>.operator !=(EquatableClass? left, int right) {
				throw new NotImplementedException();
			}

			public static string operator ==(EquatableClass? left, decimal right) {
				throw new NotImplementedException();
			}
			public static string operator !=(EquatableClass? left, decimal right) {
				throw new NotImplementedException();
			}

			static bool IEqualityOperators<EquatableClass, float, bool>.operator ==(EquatableClass? left, float right) {
				return left?.Member == right;
			}
			static bool IEqualityOperators<EquatableClass, float, bool>.operator !=(EquatableClass? left, float right) {
				throw new NotImplementedException();
			}
			static bool IEqualityOperators<EquatableClass, float?, bool>.operator ==(EquatableClass? left, float? right) {
				return left?.Member * 2f == right;
			}
			static bool IEqualityOperators<EquatableClass, float?, bool>.operator !=(EquatableClass? left, float? right) {
				throw new NotImplementedException();
			}

			static bool IEqualityOperators<EquatableClass, short, bool>.operator ==(EquatableClass? left, short right) {
				throw new InvalidOperationException("Error");
			}
			static bool IEqualityOperators<EquatableClass, short, bool>.operator !=(EquatableClass? left, short right) {
				throw new NotImplementedException();
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = EqualityOperatorsMatcher.Instance;
		}


		[TestMethod]
		public void ShouldMatch() {
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, string>());
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, int>());
			Assert.IsFalse(_matcher.CanMatch<EquatableClass, decimal>());

			Assert.IsTrue(_matcher.Match(new EquatableClass{ Member = 4 }, "abcd"));
			Assert.IsFalse(_matcher.Match(new EquatableClass { Member = 3 }, "abcd"));
			Assert.IsFalse(_matcher.Match(new EquatableClass { Member = 4 }, "abc"));
			Assert.IsTrue(_matcher.Match(new EquatableClass { Member = 4 }, 4));
			Assert.IsFalse(_matcher.Match(new EquatableClass { Member = 3 }, 4));
			Assert.IsFalse(_matcher.Match(new EquatableClass { Member = 4 }, 3));

			using (var factory = _matcher.MatchFactory<EquatableClass, string>()) {
				Assert.IsTrue(factory.Invoke(new EquatableClass { Member = 4 }, "abcd"));
				Assert.IsFalse(factory.Invoke(new EquatableClass { Member = 3 }, "abcd"));
			}
			using (var factory = _matcher.MatchFactory<EquatableClass, int>()) {
				Assert.IsTrue(factory.Invoke(new EquatableClass { Member = 4 }, 4));
				Assert.IsFalse(factory.Invoke(new EquatableClass { Member = 3 }, 4));
			}
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
#endif
