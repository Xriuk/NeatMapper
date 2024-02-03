using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class MatcherTests {
		public class TestMappingOptions { }


		public class Maps :
#if NET7_0_OR_GREATER
			IMatchMapStatic<int, string>,
			IMatchMapStatic<float, string>,
			IMatchMapStatic<Category, CategoryDto>,
			IHierarchyMatchMapStatic<Product, ProductDto>,
			IMatchMapStatic<float, double>
#else
			IMatchMap<int, string>,
			IMatchMap<float, string>,
			IMatchMap<Category, CategoryDto>,
			IHierarchyMatchMap<Product, ProductDto>,
			IMatchMap<float, double>
#endif
		{
#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<int, string>
#else
				IMatchMap<int, string>
#endif
				.Match(int source, string destination, MatchingContext context) {

				MappingOptionsUtils.matchingContext = context;
				return (source * 2).ToString() == destination;
			}
			
			// Options
			public static TestMappingOptions options;
#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<float, string>
#else
				IMatchMap<float, string>
#endif
				.Match(float source, string destination, MatchingContext context) {
				options = context.MappingOptions.GetOptions<TestMappingOptions>();
				return (source * 2).ToString() == destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<Category, CategoryDto>
#else
				IMatchMap<Category, CategoryDto>
#endif
				.Match(Category source, CategoryDto destination, MatchingContext context) {
				return source.Id == destination.Id;
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IHierarchyMatchMapStatic<Product, ProductDto>
#else
				IHierarchyMatchMap<Product, ProductDto>
#endif
				.Match(Product source, ProductDto destination, MatchingContext context) {
				return source.Code == destination.Code;
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<float, double>
#else
				IMatchMap<float, double>
#endif
				.Match(float source, double destination, MatchingContext context) {

				throw new MapNotFoundException((typeof(float), typeof(double)));
			}
		}

		public class Maps2 :
#if NET7_0_OR_GREATER
			IMatchMapStatic<Product, ProductDto>
#else
			IMatchMap<Product, ProductDto>
#endif
		{
#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<Product, ProductDto>
#else
				IMatchMap<Product, ProductDto>
#endif
				.Match(Product source, ProductDto destination, MatchingContext context) {
				return source.Code == destination.Code;
			}
		}

		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new CompositeMatcher(
				new CustomMatcher(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new HierarchyCustomMatcher(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				})
			);
		}


		[TestMethod]
		public void ShouldMatchPrimitives() {
			Assert.IsTrue(_matcher.CanMatch<int, string>());

			Assert.IsTrue(_matcher.Match(2, "4"));
			Assert.IsFalse(_matcher.Match(-3, "-5"));
			Assert.IsTrue(_matcher.Match(0, "0"));

			// Factories should share the same context
			var factory = _matcher.MatchFactory<int, string>();
			MappingOptionsUtils.matchingContext = null;
			Assert.IsTrue(factory.Invoke(2, "4"));
			var context1 = MappingOptionsUtils.matchingContext;
			Assert.IsNotNull(context1);
			MappingOptionsUtils.matchingContext = null;
			Assert.IsTrue(factory.Invoke(-3, "-6"));
			var context2 = MappingOptionsUtils.matchingContext;
			Assert.IsNotNull(context2);
			Assert.AreSame(context1, context2);
		}

		[TestMethod]
		public void ShouldMatchClasses() {
			Assert.IsTrue(_matcher.CanMatch<Product, ProductDto>());

			Assert.IsTrue(_matcher.Match(new Product {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test1"
			}));
			Assert.IsFalse(_matcher.Match(new Product {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test2"
			}));

			Assert.IsTrue(_matcher.MatchFactory<Product, ProductDto>().Invoke(new Product {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test1"
			}));
			Assert.IsFalse(_matcher.MatchFactory<Product, ProductDto>().Invoke(new Product {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test2"
			}));
		}

		[TestMethod]
		public void ShouldMatchChildClassAsParent() {
			var matcher = new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps2) }
			});

			Assert.IsTrue(matcher.Match<Product, ProductDto>(new LimitedProduct {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test1"
			}));

			Assert.IsTrue(matcher.Match<Product, ProductDto>(new Product {
				Code = "Test1"
			}, new LimitedProductDto {
				Code = "Test1"
			}));

			Assert.IsFalse(matcher.Match<Product, ProductDto>(new LimitedProduct {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test2"
			}));
		}

		[TestMethod]
		public void ShouldMatchHierarchies() {
			Assert.IsTrue(_matcher.CanMatch<LimitedProduct, ProductDto>());

			Assert.IsTrue(_matcher.Match(new LimitedProduct {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test1"
			}));

			Assert.IsTrue(_matcher.CanMatch<Product, LimitedProductDto>());

			Assert.IsTrue(_matcher.Match(new Product {
				Code = "Test1"
			}, new LimitedProductDto {
				Code = "Test1"
			}));

			Assert.IsFalse(_matcher.Match(new LimitedProduct {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test2"
			}));
		}

		[TestMethod]
		public void ShouldSetOptions() {
			// No options
			Maps.options = null;
			_matcher.Match(2f, "4");
			Assert.IsNull(Maps.options);

			// Options
			Maps.options = null;
			var opts = new TestMappingOptions();
			_matcher.Match(2f, "4", new[] { opts });
			Assert.AreSame(opts, Maps.options);
		}

		[TestMethod]
		public void ShouldMatchWithAdditionalMaps() {
			var options = new CustomMatchAdditionalMapsOptions();
			options.AddMap<string, int>((s, d, _) => s?.Length == d);
			var matcher = new CustomMatcher(null, options);

			Assert.IsTrue(matcher.CanMatch<string, int>());

			Assert.IsTrue(matcher.Match("Test", 4));
			Assert.IsFalse(matcher.Match("Test", 2));
		}

		[TestMethod]
		public void ShouldNotMatchIfMapRejectsItself() {
			// CanMatch returns true because the map does exist, even if it will fail
			Assert.IsTrue(_matcher.CanMatch<float, double>());

			var exc = TestUtils.AssertMapNotFound(() => _matcher.Match(1f, 2d));
			Assert.AreEqual(typeof(float), exc.From);
			Assert.AreEqual(typeof(double), exc.To);
		}
	}
}
