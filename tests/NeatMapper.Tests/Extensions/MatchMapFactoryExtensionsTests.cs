using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class MatchMapFactoryExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IMatchMapFactory factory =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif
			MatchMapFactory<string, int> genericFactory1 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif
#pragma warning disable CS0219
			MatchMapFactory<string, string> genericFactory2 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#pragma warning restore CS0219
#endif

			// Predicate
			{
				// Explicit source and destination
				{
					genericFactory1.Predicate("Test");
					genericFactory1.PredicateDestination(2);
					//genericFactory2.Predicate("Test"); // Ambiguous
				}

				// Explicit destination inferred source
				{
					factory.Predicate<int>("Test");
					factory.Predicate<string>(2);
				}

				// Runtime
				{
					factory.Predicate("Test");
					factory.Predicate(2);
					factory.Predicate(false);
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}


		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new CompositeMatcher(new CompositeMatcherOptions {
				Matchers = new List<IMatcher> {
					new CustomMatcher(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(Matching.MatcherTests.Maps) }
					}),
					new HierarchyCustomMatcher(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(Matching.MatcherTests.Maps) }
					})
				}
			});
		}

		[TestMethod]
		public void PredicateShouldWork() {
			var products = new List<Product>() {
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
			var match = new ProductDto {
				Code = "DEF"
			};

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
			using (var predicate = _matcher.MatchFactory(typeof(Product), typeof(ProductDto)).Predicate(match2)) {
				var matchingProduct = products2.First(p => predicate.Invoke(p));

				Assert.AreEqual("DEF", matchingProduct.Code);
			}

			// Explicit destination/source, inferred other
			using (var predicate = _matcher.MatchFactory(typeof(Product), typeof(ProductDto)).Predicate<ProductDto>(match2)) {
				var matchingProduct = products2.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}
			using (var predicate = _matcher.MatchFactory(typeof(Product), typeof(ProductDto)).Predicate<Product>(match1)) {
				var matchingProduct = products1.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}

			// Explicit source and destination (destination)
			using (var predicate = _matcher.MatchFactory<Product, ProductDto>().PredicateDestination<Product, ProductDto>(match1)) {
				var matchingProduct = products1.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}

			// Explicit source and destination (source)
			using (var predicate = _matcher.MatchFactory<Product, ProductDto>().Predicate<Product, ProductDto>(match2)) {
				var matchingProduct = products2.First(predicate);

				Assert.AreEqual("DEF", matchingProduct.Code);
			}
		}

		[TestMethod]
		public void PredicateShouldDisposeProvidedFactoryOnDispose() {
			var factory = _matcher.MatchFactory<int, string>();

			// Should not be disposed
			factory.Invoke(2, "");

			// Should dispose both
			using (factory.Predicate(2)) { }
			Assert.ThrowsException<ObjectDisposedException>(() => factory.Invoke(2, ""));


			factory = _matcher.MatchFactory<int, string>();

			// Should not be disposed
			factory.Invoke(2, "");

			// Should dispose only created
			using (factory.Predicate(2, false)) { }
			factory.Invoke(2, "");
		}

		[TestMethod]
		public void PredicateShouldDisposeProvidedFactoryOnException() {
			var factory = _matcher.MatchFactory(typeof(int), typeof(string));

			// Should not be disposed
			factory.Invoke(2, "");

			// Should fail and dispose
			Assert.ThrowsException<ArgumentException>(() => factory.Predicate(""));
			Assert.ThrowsException<ObjectDisposedException>(() => factory.Invoke(2, ""));


			factory = _matcher.MatchFactory(typeof(int), typeof(string));

			// Should not be disposed
			factory.Invoke(2, "");

			// Should fail and not dispose
			Assert.ThrowsException<ArgumentException>(() => factory.Predicate("", false));
			factory.Invoke(2, "");
		}
	}
}
