using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;

namespace NeatMapper.Tests.Matching{
	[TestClass]
	public class StructuralEquatableMatcherTests{
		public class StructuralEquatableClass : IStructuralEquatable {
			public bool Equals(object other, IEqualityComparer comparer) {
				throw new InvalidOperationException("Error");
			}

			public int GetHashCode(IEqualityComparer comparer) {
				throw new NotImplementedException();
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = StructuralEquatableMatcher.Instance;
		}

		[TestMethod]
		public void ShouldMatch() {
			Assert.IsTrue(_matcher.CanMatch<(int, bool), (int, bool)>());
			Assert.IsFalse(_matcher.CanMatch<(int, bool), (string, bool)>());
			Assert.IsFalse(_matcher.CanMatch<(int, bool), string>());
			Assert.IsTrue(_matcher.CanMatch<string[], string[]>());
			Assert.IsFalse(_matcher.CanMatch<string[], int[]>());
			Assert.IsTrue(_matcher.CanMatch<StructuralEquatableClass, StructuralEquatableClass>());

			Assert.IsTrue(_matcher.Match((2, false), (2, false)));
			Assert.IsFalse(_matcher.Match((2, true), (2, false)));
			Assert.IsTrue(_matcher.Match(new string[] { "Test", "Prova" }, new string[] { "Test", "Prova" }));
			Assert.IsFalse(_matcher.Match(new string[] { "Test1", "Prova2" }, new string[] { "Test", "Prova" }));

			using (var factory = _matcher.MatchFactory<(int, bool), (int, bool)>()) {
				Assert.IsTrue(factory.Invoke((2, false), (2, false)));
				Assert.IsFalse(factory.Invoke((2, true), (2, false)));
			}
			using (var factory = _matcher.MatchFactory<string[], string[]>()) {
				Assert.IsTrue(factory.Invoke(new string[] { "Test", "Prova" }, new string[] { "Test", "Prova" }));
				Assert.IsFalse(factory.Invoke(new string[] { "Test1", "Prova2" }, new string[] { "Test", "Prova" }));
			}
		}

		[TestMethod]
		public void ShouldThrowExceptionsCorrectly() {
			var exc = Assert.ThrowsException<MatcherException>(() => _matcher.Match<StructuralEquatableClass, StructuralEquatableClass>(new StructuralEquatableClass(), new StructuralEquatableClass()));
			Assert.IsTrue(exc.InnerException is InvalidOperationException);
			Assert.AreEqual("Error", exc.InnerException.Message);

			using (var factory = _matcher.MatchFactory<StructuralEquatableClass, StructuralEquatableClass>()) {
				exc = Assert.ThrowsException<MatcherException>(() => factory.Invoke(new StructuralEquatableClass(), new StructuralEquatableClass()));
				Assert.IsTrue(exc.InnerException is InvalidOperationException);
				Assert.AreEqual("Error", exc.InnerException.Message);
			}
		}
	}
}
