using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class GenericMatcherTests {
		public class Maps<T1, T2> :
#if NET7_0_OR_GREATER
			ICanMatchStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>,
			IMatchMapStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>
#else
			ICanMatch<IEnumerable<T1>, CustomCollectionComplex<T2>>,
			IMatchMap<IEnumerable<T1>, CustomCollectionComplex<T2>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				ICanMatchStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>
#else
				ICanMatch<IEnumerable<T1>, CustomCollectionComplex<T2>>
#endif
				.CanMatch(MatchingContext context) {

				return context.Matcher.CanMatch<T1, T2>(context.MappingOptions);
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>
#else
				IMatchMap<IEnumerable<T1>, CustomCollectionComplex<T2>>
#endif
				.Match(IEnumerable<T1> source, CustomCollectionComplex<T2> destination, MatchingContext context) {

				return source.Count() == destination.Elements.Count() &&
					source.Zip(destination.Elements, (s, d) => (s, d)).All(v => context.Matcher.Match(v.s, v.d, context.MappingOptions));
			}
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,>) }
			});
		}


		[TestMethod]
		public void ShouldCheckCanMatchIfPresent() {
			var nestedMatcher = new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MatcherTests.Maps) }
			});

			var options = new MappingOptions(new MatcherOverrideMappingOptions(nestedMatcher));

			Assert.IsTrue(_matcher.CanMatch<IEnumerable<int>, CustomCollectionComplex<string>>(options));
			Assert.IsFalse(_matcher.CanMatch<IEnumerable<int>, CustomCollectionComplex<float>>(options));

			var coll = new CustomCollectionComplex<string>();
			coll.Add("4");
			coll.Add("-6");
			coll.Add("0");
			Assert.IsTrue(_matcher.Match<IEnumerable<int>, CustomCollectionComplex<string>>(new[] { 2, -3, 0 }, coll, options));
			Assert.IsFalse(_matcher.Match<IEnumerable<int>, CustomCollectionComplex<string>>(new[] { 2, -3 }, coll, options));
			TestUtils.AssertMapNotFound(() => _matcher.Match<IEnumerable<int>, CustomCollectionComplex<float>>(new[] { 2, -3, 0 }, new CustomCollectionComplex<float>(), options));
		}
	}
}
