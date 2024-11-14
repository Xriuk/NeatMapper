using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class MatcherExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IMatcher matcher =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable

			MappingOptions? options = null;
			IEnumerable? enumerable1 = null;
			IEnumerable<string>? enumerable2 = null;
			object? option1 = null;
			object? option2 = null;
			object? str = null;
			object strNonNull = null!;
#else
			MappingOptions options = null;
			IEnumerable enumerable1 = null;
			IEnumerable<string> enumerable2 = null;
			object option1 = null;
			object option2 = null;
			object str = "Test";
			object strNonNull = null;
#endif

			// Match
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					matcher.Match("Test", 2);
					matcher.Match<string, int>("Test", 2);

					// MappingOptions
					matcher.Match("Test", 2, options);
					matcher.Match<string, int>("Test", 2, options);

					// IEnumerable
					matcher.Match("Test", 2, enumerable1);
					matcher.Match<string, int>("Test", 2, enumerable1);
					matcher.Match("Test", 2, enumerable2);
					matcher.Match<string, int>("Test", 2, enumerable2);

					// Params (causes ambiguity with Runtime)
					//matcher.Match("Test", 2, option1);
					//matcher.Match<string, int>("Test", 2, option1);
					//matcher.Match("Test", 2, option1, option2);
					//matcher.Match<string, int>("Test", 2, option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.Match("Test", typeof(string), 2, typeof(int));
					matcher.Match(str, typeof(string), 2, typeof(int));
					matcher.Match(strNonNull, typeof(string), 2, typeof(int));

					// MappingOptions
					matcher.Match("Test", typeof(string), 2, typeof(int), options);
					matcher.Match(str, typeof(string), 2, typeof(int), options);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), options);

					// IEnumerable
					matcher.Match("Test", typeof(string), 2, typeof(int), enumerable1);
					matcher.Match(str, typeof(string), 2, typeof(int), enumerable1);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), enumerable1);
					matcher.Match("Test", typeof(string), 2, typeof(int), enumerable2);
					matcher.Match(str, typeof(string), 2, typeof(int), enumerable2);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), enumerable2);

					// Params
					matcher.Match("Test", typeof(string), 2, typeof(int), option1);
					matcher.Match(str, typeof(string), 2, typeof(int), option1);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), option1);
					matcher.Match("Test", typeof(string), 2, typeof(int), option1, option2);
					matcher.Match(str, typeof(string), 2, typeof(int), option1, option2);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), option1, option2);
				}
			}


			// CanMatch
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					matcher.CanMatch<string, int>();

					// MappingOptions
					matcher.CanMatch<string, int>(options);

					// IEnumerable
					matcher.CanMatch<string, int>(enumerable1);
					matcher.CanMatch<string, int>(enumerable2);

					// Params
					matcher.CanMatch<string, int>(option1);
					matcher.CanMatch<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.CanMatch(typeof(string), typeof(int));

					// MappingOptions
					matcher.CanMatch(typeof(string), typeof(int), options);

					// IEnumerable
					matcher.CanMatch(typeof(string), typeof(int), enumerable1);
					matcher.CanMatch(typeof(string), typeof(int), enumerable2);

					// Params
					matcher.CanMatch(typeof(string), typeof(int), option1);
					matcher.CanMatch(typeof(string), typeof(int), option1, option2);
				}
			}


			// MatchFactory
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					matcher.MatchFactory<string, int>();

					// MappingOptions
					matcher.MatchFactory<string, int>(options);

					// IEnumerable
					matcher.MatchFactory<string, int>(enumerable1);
					matcher.MatchFactory<string, int>(enumerable2);

					// Params
					matcher.MatchFactory<string, int>(option1);
					matcher.MatchFactory<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.MatchFactory(typeof(string), typeof(int));

					// MappingOptions
					matcher.MatchFactory(typeof(string), typeof(int), options);

					// IEnumerable
					matcher.MatchFactory(typeof(string), typeof(int), enumerable1);
					matcher.MatchFactory(typeof(string), typeof(int), enumerable2);

					// Params
					matcher.MatchFactory(typeof(string), typeof(int), option1);
					matcher.MatchFactory(typeof(string), typeof(int), option1, option2);
				}
			}


			// Predicate
			{
				// Explicit source and destination
				{
					// Source
					{ 
						// No parameters (MappingOptions with default value overload)
						matcher.Predicate<string, int>("Test");
						matcher.Predicate<int, string>(2);
						//matcher.Predicate<string, string>("Test"); // DEV: resolve ambiguity after breaking changes

						// MappingOptions
						matcher.Predicate<string, int>("Test", options);
						matcher.Predicate<int, string>(2, options);
						//matcher.Predicate<string, string>("Test", options); // DEV: resolve ambiguity after breaking changes

						// IEnumerable
						matcher.Predicate<string, int>("Test", enumerable1);
						matcher.Predicate<string, int>("Test", enumerable2);
						matcher.Predicate<int, string>(2, enumerable1);
						matcher.Predicate<int, string>(2, enumerable2);
						//matcher.Predicate<string, string>("Test", enumerable1); // DEV: resolve ambiguity after breaking changes
						//matcher.Predicate<string, string>("Test", enumerable2);

						// Params
						matcher.Predicate<string, int>("Test", option1);
						matcher.Predicate<string, int>("Test", option1, option2);
						matcher.Predicate<int, string>(2, option1);
						matcher.Predicate<int, string>(2, option1, option2);
						//matcher.Predicate<string, string>("Test", option1); // DEV: resolve ambiguity after breaking changes
						//matcher.Predicate<string, string>("Test", option1, option2);
					}

					// Destination
					{
						// No parameters (MappingOptions with default value overload)
						matcher.PredicateDestination<string, int>(2);
						matcher.PredicateDestination<int, string>("Test");
						matcher.PredicateDestination<string, string>("Test");

						// MappingOptions
						matcher.PredicateDestination<string, int>(2, options);
						matcher.PredicateDestination<int, string>("Test", options);
						matcher.PredicateDestination<string, string>("Test", options);

						// IEnumerable
						matcher.PredicateDestination<string, int>(2, enumerable1);
						matcher.PredicateDestination<string, int>(2, enumerable2);
						matcher.PredicateDestination<int, string>("Test", enumerable1);
						matcher.PredicateDestination<int, string>("Test", enumerable2);
						matcher.PredicateDestination<string, string>("Test", enumerable1);
						matcher.PredicateDestination<string, string>("Test", enumerable2);

						// Params
						matcher.PredicateDestination<string, int>(2, option1);
						matcher.PredicateDestination<string, int>(2, option1, option2);
						matcher.PredicateDestination<int, string>("Test", option1);
						matcher.PredicateDestination<int, string>("Test", option1, option2);
						matcher.PredicateDestination<string, string>("Test", option1);
						matcher.PredicateDestination<string, string>("Test", option1, option2);
					}
				}

				// Explicit destination, inferred source
				{
					// No parameters (MappingOptions with default value overload)
					matcher.Predicate<int>("Test");
					matcher.Predicate<int>(strNonNull);
					matcher.Predicate<string>(2);

					// MappingOptions
					matcher.Predicate<int>("Test", options);
					matcher.Predicate<int>(strNonNull, options);
					matcher.Predicate<string>(2, options);

					// IEnumerable
					matcher.Predicate<int>("Test", enumerable1);
					matcher.Predicate<int>(strNonNull, enumerable1);
					matcher.Predicate<int>("Test", enumerable2);
					matcher.Predicate<int>(strNonNull, enumerable2);
					matcher.Predicate<string>(2, enumerable1);
					matcher.Predicate<string>(2, enumerable2);

					// Params
					matcher.Predicate<int>("Test", option1);
					matcher.Predicate<int>(strNonNull, option1);
					matcher.Predicate<int>("Test", option1, option2);
					matcher.Predicate<int>(strNonNull, option1, option2);
					matcher.Predicate<string>(2, option1);
					matcher.Predicate<string>(2, option1, option2);
				}

				// Explicit source, inferred destinatinon
				{
					// No parameters (MappingOptions with default value overload)
					matcher.PredicateDestination<int>("Test");
					matcher.PredicateDestination<int>(strNonNull);
					matcher.PredicateDestination<string>(2);

					// MappingOptions
					matcher.PredicateDestination<int>("Test", options);
					matcher.PredicateDestination<int>(strNonNull, options);
					matcher.PredicateDestination<string>(2, options);

					// IEnumerable
					matcher.PredicateDestination<int>("Test", enumerable1);
					matcher.PredicateDestination<int>(strNonNull, enumerable1);
					matcher.PredicateDestination<int>("Test", enumerable2);
					matcher.PredicateDestination<int>(strNonNull, enumerable2);
					matcher.PredicateDestination<string>(2, enumerable1);
					matcher.PredicateDestination<string>(2, enumerable2);

					// Params
					matcher.PredicateDestination<int>("Test", option1);
					matcher.PredicateDestination<int>(strNonNull, option1);
					matcher.PredicateDestination<int>("Test", option1, option2);
					matcher.PredicateDestination<int>(strNonNull, option1, option2);
					matcher.PredicateDestination<string>(2, option1);
					matcher.PredicateDestination<string>(2, option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.Predicate("Test", typeof(string), typeof(int));
					matcher.Predicate(str, typeof(string), typeof(int));
					matcher.Predicate(strNonNull, typeof(string), typeof(int));
					matcher.Predicate(typeof(string), 2, typeof(int));

					// MappingOptions
					matcher.Predicate("Test", typeof(string), typeof(int), options);
					matcher.Predicate(str, typeof(string), typeof(int), options);
					matcher.Predicate(strNonNull, typeof(string), typeof(int), options);
					matcher.Predicate(typeof(string), 2, typeof(int), options);

					// IEnumerable
					matcher.Predicate("Test", typeof(string), typeof(int), enumerable1);
					matcher.Predicate(str, typeof(string), typeof(int), enumerable1);
					matcher.Predicate(strNonNull, typeof(string), typeof(int), enumerable1);
					matcher.Predicate("Test", typeof(string), typeof(int), enumerable2);
					matcher.Predicate(str, typeof(string), typeof(int), enumerable2);
					matcher.Predicate(strNonNull, typeof(string), typeof(int), enumerable2);
					matcher.Predicate(typeof(string), 2, typeof(int), enumerable1);
					matcher.Predicate(typeof(string), 2, typeof(int), enumerable2);

					// Params
					matcher.Predicate("Test", typeof(string), typeof(int), option1);
					matcher.Predicate(str, typeof(string), typeof(int), option1);
					matcher.Predicate(strNonNull, typeof(string), typeof(int), option1);
					matcher.Predicate("Test", typeof(string), typeof(int), option1, option2);
					matcher.Predicate(str, typeof(string), typeof(int), option1, option2);
					matcher.Predicate(strNonNull, typeof(string), typeof(int), option1, option2);
					matcher.Predicate(typeof(string), 2, typeof(int), option1);
					matcher.Predicate(typeof(string), 2, typeof(int), option1, option2);
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}


		[TestMethod]
		public void PredicateShouldWork() {
			var matcher = new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = new List<IMatcher> {
					new CustomMatcher(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(Matching.MatcherTests.Maps) }
					}),
					new HierarchyCustomMatcher(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(Matching.MatcherTests.Maps) }
					})
				}
			});

			var products1 = new List<Product>() {
				new Product {
					Code = "ABC"
				},
				new Product {
					Code = "DEF"
				},
				new Product {
					Code = "GHI"
				}
			};
			var match1 = new ProductDto {
				Code = "DEF"
			};

			var products2 = new List<ProductDto>() {
				new ProductDto {
					Code = "ABC"
				},
				new ProductDto {
					Code = "DEF"
				},
				new ProductDto {
					Code = "GHI"
				}
			};
			var match2 = new Product {
				Code = "DEF"
			};

			// Runtime
			using (var predicate = matcher.Predicate(match2, typeof(Product), typeof(ProductDto))) {
				var matchingProduct = products2.First(p => predicate.Invoke(p));

				Assert.AreEqual("DEF", matchingProduct.Code);
			}
			using (var predicate = matcher.Predicate(typeof(Product), match1, typeof(ProductDto))) {
				var matchingProduct = products1.First(p => predicate.Invoke(p));

				Assert.AreEqual("DEF", matchingProduct.Code);
			}

			// Explicit destination/source, inferred other
			using (var predicate = matcher.Predicate<Product>(match1)) {
				var matchingProduct = products1.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}
			using (var predicate = matcher.Predicate<ProductDto>(match2)) {
				var matchingProduct = products2.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}

			// Explicit source and destination (destination)
			using (var predicate = matcher.PredicateDestination<Product, ProductDto>(match1)) {
				var matchingProduct = products1.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}

			// Explicit source and destination (source)
			using (var predicate = matcher.Predicate<Product, ProductDto>(match2)) {
				var matchingProduct = products2.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}
		}
	}
}
