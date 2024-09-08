﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class EquatableMatcherTests {
		public class EquatableClass : IEquatable<string>, IEquatable<int> {
			bool IEquatable<string>.Equals(string other) {
				return other.Length == 4;
			}

			bool IEquatable<int>.Equals(int other) {
				return other == 4;
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = EquatableMatcher.Instance;
		}


		[TestMethod]
		public void ShouldMatch() {
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, string>());
			Assert.IsTrue(_matcher.CanMatch<EquatableClass, int>());
			Assert.IsFalse(_matcher.CanMatch<string, EquatableClass>());
			Assert.IsFalse(_matcher.CanMatch<int, EquatableClass>());
			Assert.IsFalse(_matcher.CanMatch<EquatableClass, EquatableClass>());
			Assert.IsFalse(_matcher.CanMatch<string, int>());
			Assert.IsFalse(_matcher.CanMatch<int, string>());

			Assert.IsTrue(_matcher.CanMatch<string, string>());
			Assert.IsTrue(_matcher.CanMatch<int, int>());

			Assert.IsTrue(_matcher.Match(new EquatableClass(), "abcd"));
			Assert.IsFalse(_matcher.Match(new EquatableClass(), "abc"));
			Assert.IsTrue(_matcher.Match(new EquatableClass(), 4));
			Assert.IsFalse(_matcher.Match(new EquatableClass(), 3));

			Assert.IsTrue(_matcher.Match("abcd", "abcd"));
			Assert.IsFalse(_matcher.Match("abcd", "abc"));
			Assert.IsTrue(_matcher.Match(4, 4));
			Assert.IsFalse(_matcher.Match(4, 3));

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
	}
}