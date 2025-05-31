using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class NullableMatcherTests {
		public class Maps :
			IMatchMap<int, char> {

			bool IMatchMap<int, char>.Match(int source, char destination, MatchingContext context) {
				return source == (int)destination;
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new NullableMatcher(new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps), typeof(MatcherTests.Maps) }
			}));
		}


		[TestMethod]
		public void ShouldMatchValueTypeWithNullable() {
			{ 
				Assert.IsTrue(_matcher.CanMatch<int, char?>());

				Assert.IsTrue(_matcher.Match<int, char?>(122, 'z'));
				Assert.IsFalse(_matcher.Match<int, char?>(0, null));

				using(var factory = _matcher.MatchFactory<int, char?>()){
					Assert.IsTrue(factory.Invoke(122, 'z'));
					Assert.IsFalse(factory.Invoke(0, null));
				}
			}

			{
				Assert.IsTrue(_matcher.CanMatch<int?, char>());

				Assert.IsTrue(_matcher.Match<int?, char>(122, 'z'));
				Assert.IsFalse(_matcher.Match<int?, char>(null, '\0'));

				using (var factory = _matcher.MatchFactory<int?, char>()) {
					Assert.IsTrue(factory.Invoke(122, 'z'));
					Assert.IsFalse(factory.Invoke(null, '\0'));
				}
			}
		}

		[TestMethod]
		public void ShouldMatchReferenceTypeToNullable() {
			{ 
				Assert.IsTrue(_matcher.CanMatch<int?, string>());

				Assert.IsTrue(_matcher.Match<int?, string>(2, "4"));
				Assert.IsTrue(_matcher.Match<int?, string>(null, null));
				Assert.IsFalse(_matcher.Match<int?, string>(null, "4"));

				using (var factory = _matcher.MatchFactory<int?, string>()) {
					Assert.IsTrue(factory.Invoke(2, "4"));
					Assert.IsTrue(factory.Invoke(null, null));
					Assert.IsFalse(factory.Invoke(null, "4"));
				}
			}
		}


		[TestMethod]
		public void ShouldMatchNullableToNullable() {
			// int? -> char
			{
				var additionalMaps = new CustomMatchAdditionalMapsOptions();
				additionalMaps.AddMap<int?, char>((s, d, c) => s != null ? s.Value + 2 == (int)d : d == '\0');
				var matcher = new NullableMatcher(new CustomMatcher(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}, additionalMaps));


				Assert.IsTrue(matcher.CanMatch<int?, char?>());

				Assert.IsTrue(matcher.Match<int?, char?>(null, null)); // Shortcut
				Assert.IsTrue(matcher.Match<int?, char?>(122, '|'));
				Assert.IsFalse(matcher.Match<int?, char?>(122, 'z'));
				Assert.IsFalse(matcher.Match<int?, char?>(122, null));
				Assert.IsFalse(matcher.Match<int?, char?>(null, '|'));
				Assert.IsTrue(matcher.Match<int?, char?>(null, '\0')); // Map

				using (var factory = matcher.MatchFactory<int?, char?>()) {
					Assert.IsTrue(factory.Invoke(null, null)); // Shortcut
					Assert.IsTrue(factory.Invoke(122, '|'));
					Assert.IsFalse(factory.Invoke(122, 'z'));
					Assert.IsFalse(factory.Invoke(122, null));
					Assert.IsFalse(factory.Invoke(null, '|'));
					Assert.IsTrue(factory.Invoke(null, '\0')); // Map
				}
			}

			// int -> char?
			{
				var additionalMaps = new CustomMatchAdditionalMapsOptions();
				additionalMaps.AddMap<int, char?>((s, d, c) => d != null ? s - 2 == (int)d.Value : s == 0);
				var matcher = new NullableMatcher(new CustomMatcher(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}, additionalMaps));


				Assert.IsTrue(matcher.CanMatch<int?, char?>());

				Assert.IsTrue(matcher.Match<int?, char?>(null, null)); // Shortcut
				Assert.IsTrue(matcher.Match<int?, char?>(122, 'x'));
				Assert.IsFalse(matcher.Match<int?, char?>(122, 'z'));
				Assert.IsFalse(matcher.Match<int?, char?>(122, null));
				Assert.IsFalse(matcher.Match<int?, char?>(null, 'x'));
				Assert.IsTrue(matcher.Match<int?, char?>(0, null)); // Map

				using (var factory = matcher.MatchFactory<int?, char?>()) {
					Assert.IsTrue(factory.Invoke(null, null)); // Shortcut
					Assert.IsTrue(factory.Invoke(122, 'x'));
					Assert.IsFalse(factory.Invoke(122, 'z'));
					Assert.IsFalse(factory.Invoke(122, null));
					Assert.IsFalse(factory.Invoke(null, 'x'));
					Assert.IsTrue(factory.Invoke(0, null)); // Map
				}
			}

			// int -> char
			{ 
				Assert.IsTrue(_matcher.CanMatch<int?, char?>());

				Assert.IsTrue(_matcher.Match<int?, char?>(null, null));
				Assert.IsTrue(_matcher.Match<int?, char?>(122, 'z'));
				Assert.IsFalse(_matcher.Match<int?, char?>(122, null));
				Assert.IsFalse(_matcher.Match<int?, char?>(null, 'z'));

				using (var factory = _matcher.MatchFactory<int?, char?>()) {
					Assert.IsTrue(factory.Invoke(null, null));
					Assert.IsTrue(factory.Invoke(122, 'z'));
					Assert.IsFalse(factory.Invoke(122, null));
					Assert.IsFalse(factory.Invoke(null, 'z'));
				}
			}
		}


		[TestMethod]
		public void ShouldCheckButNotMapOpenNullable() {
			{
				Assert.IsTrue(_matcher.CanMatch(typeof(int), typeof(Nullable<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _matcher.Match(1, typeof(int), 2, typeof(Nullable<>)));
			}

			{
				Assert.IsTrue(_matcher.CanMatch(typeof(Nullable<>), typeof(int)));

				Assert.ThrowsException<MapNotFoundException>(() => _matcher.Match(1, typeof(Nullable<>), 2, typeof(int)));
			}

			{
				Assert.IsTrue(_matcher.CanMatch(typeof(Nullable<>), typeof(Nullable<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _matcher.Match(1, typeof(Nullable<>), 2, typeof(Nullable<>)));
			}
		}
	}
}
