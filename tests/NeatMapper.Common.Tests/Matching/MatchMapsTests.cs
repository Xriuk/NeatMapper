using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Matching {
	[TestClass]
	public class MatchMapsTests {
		public class TestMappingOptions { }


		public class Maps :
#if NET7_0_OR_GREATER
			IMatchMapStatic<int, string>,
			IMatchMapStatic<float, string>,
			IMatchMapStatic<Category, CategoryDto>,
			IHierarchyMatchMapStatic<Product, ProductDto>
#else
			IMatchMap<int, string>,
			IMatchMap<float, string>,
			IMatchMap<Category, CategoryDto>,
			IHierarchyMatchMap<Product, ProductDto>
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

		public class Matcher : BaseMapper {
			public Matcher(MapperConfigurationOptions configuration) :
				base(_ => false, _ => false, configuration) {}
		}

		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			_matcher = new Matcher(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		[DataRow(2, "4", true)]
		[DataRow(-3, "-5", false)]
		[DataRow(0, "0", true)]
		public void ShouldMatchPrimitives(int num, string str, bool result) {
			Assert.AreEqual(result, _matcher.Match(num, str));
		}

		[TestMethod]
		public void ShouldMatchClasses() {
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
		}

		[TestMethod]
		public void ShouldMatchChildClassAsParent() {
			var matcher = new Matcher(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Maps2) }
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
			Assert.IsTrue(_matcher.Match(new LimitedProduct {
				Code = "Test1"
			}, new ProductDto {
				Code = "Test1"
			}));

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
	}
}
