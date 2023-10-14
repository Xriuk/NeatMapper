using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class MergeMapsTests {
		public class TestOptions { }

		public class Maps :
#if NET7_0_OR_GREATER
			IMergeMapStatic<int, string>,
			INewMapStatic<float, string>,
			IMergeMapStatic<float, string>,
			IMergeMapStatic<Price, decimal>,
			IMergeMapStatic<Price, PriceFloat>,
			IMergeMapStatic<Category, int>,
			IMergeMapStatic<Product, ProductDto>,
			IMergeMapStatic<LimitedProduct, LimitedProductDto>,
			IMergeMapStatic<decimal, Price>,
			IMergeMapStatic<float, Price>,
			IMergeMapStatic<Category, CategoryDto>,
			IMatchMapStatic<Category, CategoryDto>,
			IMergeMapStatic<IEnumerable<Category>, CategoryDto[]>,
			IMergeMapStatic<float, int>,
			IMergeMapStatic<decimal, int>,
			IMatchMapStatic<decimal, int>,
			IMergeMapStatic<string, ClassWithoutParameterlessConstructor>
#else
			IMergeMap<int, string>,
			INewMap<float, string>,
			IMergeMap<float, string>,
			IMergeMap<Price, decimal>,
			IMergeMap<Price, PriceFloat>,
			IMergeMap<Category, int>,
			IMergeMap<Product, ProductDto>,
			IMergeMap<LimitedProduct, LimitedProductDto>,
			IMergeMap<decimal, Price>,
			IMergeMap<float, Price>,
			IMergeMap<Category, CategoryDto>,
			IMatchMap<Category, CategoryDto>,
			IMergeMap<IEnumerable<Category>, CategoryDto[]>,
			IMergeMap<float, int>,
			IMergeMap<decimal, int>,
			IMatchMap<decimal, int>,
			IMergeMap<string, ClassWithoutParameterlessConstructor> 
#endif
			{

			public static TestOptions options;
			public static MergeMappingOptions mergeOptions;

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<int, string>
#else
				IMergeMap<int, string>
#endif
				.Map(int source, string destination, MappingContext context) {
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeMappingOptions>();
				return (source * 2).ToString();
			}

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				INewMapStatic<float, string>
#else
				INewMap<float, string>
#endif
				.Map(float source, MappingContext context) {
				return "NewMap";
			}

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<float, string>
#else
				IMergeMap<float, string>
#endif
				.Map(float source, string destination, MappingContext context) {
				return "MergeMap";
			}

#if NET7_0_OR_GREATER
			static
#endif
			decimal
#if NET7_0_OR_GREATER
				IMergeMapStatic<Price, decimal>
#else
				IMergeMap<Price, decimal>
#endif
				.Map(Price source, decimal destination, MappingContext context) {
				return source?.Amount ?? 0m;
			}

#if NET7_0_OR_GREATER
			static
#endif
			PriceFloat
#if NET7_0_OR_GREATER
				IMergeMapStatic<Price, PriceFloat>
#else
				IMergeMap<Price, PriceFloat>
#endif
				.Map(Price source, PriceFloat destination, MappingContext context) {
				if(source != null) {
					if (destination == null)
						destination = new PriceFloat();
					destination.Amount = (float)source.Amount;
					destination.Currency = source.Currency;
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<Category, int>
#else
				IMergeMap<Category, int>
#endif
				.Map(Category source, int destination, MappingContext context) {
				return source?.Id ?? destination;
			}

			// Nested MergeMap
#if NET7_0_OR_GREATER
			static
#endif
			ProductDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<Product, ProductDto>
#else
				IMergeMap<Product, ProductDto>
#endif
				.Map(Product source, ProductDto destination, MappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new ProductDto();
					destination.Code = source.Code;
					destination.Categories = context.Mapper.Map(source.Categories, destination.Categories) ?? new List<int>();
				}
				return destination;
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			LimitedProductDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<LimitedProduct, LimitedProductDto>
#else
				IMergeMap<LimitedProduct, LimitedProductDto>
#endif
				.Map(LimitedProduct source, LimitedProductDto destination, MappingContext context) {
				if (source != null){
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = context.Mapper.Map<ICollection<int>>(source.Categories) ?? new List<int>();
					destination.Copies = source.Copies;
				}
				return destination;
			}

			// Returns new destination
#if NET7_0_OR_GREATER
			static
#endif
			Price
#if NET7_0_OR_GREATER
				IMergeMapStatic<decimal, Price>
#else
				IMergeMap<decimal, Price>
#endif
				.Map(decimal source, Price destination, MappingContext context) {
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeMappingOptions>();
				return new Price {
					Amount = source,
					Currency = "EUR"
				};
			}

			// Returns passed destination (if not null)
#if NET7_0_OR_GREATER
			static
#endif
			Price
#if NET7_0_OR_GREATER
				IMergeMapStatic<float, Price>
#else
				IMergeMap<float, Price>
#endif
				.Map(float source, Price destination, MappingContext context) {
				if (destination == null)
					destination = new Price();
				destination.Amount = (decimal)source;
				destination.Currency = "EUR";
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			CategoryDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<Category, CategoryDto>
#else
				IMergeMap<Category, CategoryDto>
#endif
				.Map(Category source, CategoryDto destination, MappingContext context) {
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeMappingOptions>();
				if (source != null) {
					if (destination == null)
						destination = new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = source.Parent?.Id;
				}
				return destination;
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
				return source?.Id == destination?.Id;
			}

#if NET7_0_OR_GREATER
			static
#endif
			CategoryDto[]
#if NET7_0_OR_GREATER
				IMergeMapStatic<IEnumerable<Category>, CategoryDto[]>
#else
				IMergeMap<IEnumerable<Category>, CategoryDto[]>
#endif
				.Map(IEnumerable<Category> source, CategoryDto[] destination, MappingContext context) {
				return source?.Select(s => context.Mapper.Map<Category, CategoryDto>(s)).ToArray();
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<float, int>
#else
				IMergeMap<float, int>
#endif
				.Map(float source, int destination, MappingContext context) {
				throw new NotImplementedException();
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<decimal, int>
#else
				IMergeMap<decimal, int>
#endif
				.Map(decimal source, int destination, MappingContext context) {
				throw new NotImplementedException();
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<decimal, int>
#else
				IMatchMap<decimal, int>
#endif
				.Match(decimal source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}

#if NET7_0_OR_GREATER
			static
#endif
			ClassWithoutParameterlessConstructor
#if NET7_0_OR_GREATER
				IMergeMapStatic<string, ClassWithoutParameterlessConstructor>
#else
				IMergeMap<string, ClassWithoutParameterlessConstructor>
#endif
				.Map(string source, ClassWithoutParameterlessConstructor destination, MappingContext context) {
				return destination;
			}
		}

		public class HierarchyMatchers :
#if NET7_0_OR_GREATER
			IHierarchyMatchMapStatic<Product, ProductDto>,
			IMergeMapStatic<LimitedProduct, ProductDto>
#else
			IHierarchyMatchMap<Product, ProductDto>,
			IMergeMap<LimitedProduct, ProductDto>
#endif
			{

			public static TestOptions options;
			public static MergeMappingOptions mergeOptions;

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
			ProductDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<LimitedProduct, ProductDto>
#else
				IMergeMap<LimitedProduct, ProductDto>
#endif
				.Map(LimitedProduct source, ProductDto destination, MappingContext context) {
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeMappingOptions>();
				if (source != null) {
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = new List<int>();
				}
				return destination;
			}
		}

		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfigurationOptions{
				ScanTypes = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public void ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, _mapper.Map<int, string>(input, ""));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			Assert.AreEqual(20.00m, _mapper.Map<Price, decimal>(new Price {
				Amount = 20.00m,
				Currency = "EUR"
			}, 21m));

			var result = _mapper.Map<Price, PriceFloat>(new Price {
				Amount = 40.00m,
				Currency = "EUR"
			}, null);
			Assert.IsNotNull(result);
			Assert.AreEqual(40f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);

			var destination = new PriceFloat();
			result = _mapper.Map<Price, PriceFloat>(new Price {
				Amount = 40.00m,
				Currency = "EUR"
			}, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(40f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
		}

		[TestMethod]
		public void ShouldMapChildClassesAsParents() {
			// Parent source
			{
				// Not null destination
				{
					var destination = new ProductDto();
					var result = _mapper.Map<Product, ProductDto>(new LimitedProduct {
						Code = "Test",
						Categories = new List<Category> {
							new Category {
								Id = 2
							}
						},
						Copies = 3
					}, destination);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.IsTrue(result.GetType() == typeof(ProductDto));
				}

				// Null destination
				{
					var result = _mapper.Map<Product, ProductDto>(new LimitedProduct {
						Code = "Test",
						Categories = new List<Category> {
							new Category {
								Id = 2
							}
						},
						Copies = 3
					}, null);
					Assert.IsNotNull(result);
					Assert.IsTrue(result.GetType() == typeof(ProductDto));
				}
			}

			// Parent destination
			{
				// Not null source
				{
					var destination = new LimitedProductDto {
						Code = "AAA",
						Categories = new List<int>(),
						Copies = 3
					};
					var result = _mapper.Map<Product, ProductDto>(new Product {
						Code = "Test",
							Categories = new List<Category> {
							new Category {
								Id = 2
							}
						}
					}, destination);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.IsTrue(result.GetType() == typeof(LimitedProductDto));
					Assert.AreEqual("Test", destination.Code);
					Assert.AreEqual(1, destination.Categories.Count());
					Assert.AreEqual(2, destination.Categories.Single());
					Assert.AreEqual(3, destination.Copies);
				}

				// Null source
				{
					var destination = new LimitedProductDto {
						Code = "AAA",
						Categories = new List<int>(),
						Copies = 3
					};
					var result = _mapper.Map<Product, ProductDto>(null, destination);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.IsTrue(result.GetType() == typeof(LimitedProductDto));
					Assert.AreEqual("AAA", destination.Code);
					Assert.AreEqual(0, destination.Categories.Count());
					Assert.AreEqual(3, destination.Copies);
				}
			}

			// Parent source and destination
			{
				var destination = new LimitedProductDto {
					Code = "AAA",
					Categories = new List<int>(),
					Copies = 3
				};
				var result = _mapper.Map<Product, ProductDto>(new LimitedProduct {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					},
					Copies = 3
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.IsTrue(result.GetType() == typeof(LimitedProductDto));
				Assert.AreEqual("Test", destination.Code);
				Assert.AreEqual(1, destination.Categories.Count());
				Assert.AreEqual(2, destination.Categories.Single());
				Assert.AreEqual(3, destination.Copies);
			}
		}

		[TestMethod]
		public void ShouldNotMapWithoutMap() {
			TestUtils.AssertMapNotFound(() => _mapper.Map(false, 0));
		}

		[TestMethod]
		public void ShouldMapNested() {
			{
				var destination = new ProductDto();
				var result = _mapper.Map<Product, ProductDto>(new Product {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					}
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(1, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.Single());
			}

			{
				var result = _mapper.Map<LimitedProduct, LimitedProductDto>(new LimitedProduct {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					},
					Copies = 3
				}, null);
				Assert.IsNotNull(result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(1, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.Single());
				Assert.AreEqual(3, result.Copies);
			}
		}

		[TestMethod]
		public void ShouldRespectReturnedValue() {
			// Returns new destination
			{
				// Not null
				{
					var destination = new Price();
					var result = _mapper.Map(20m, destination);
					Assert.IsNotNull(result);
					Assert.AreNotSame(destination, result);
				}

				// Null
				{
					var result = _mapper.Map<decimal, Price>(20m, null);
					Assert.IsNotNull(result);
				}
			}

			// Returns passed destination (if not null)
			{
				// Not null
				{
					var destination = new Price();
					var result = _mapper.Map(20f, destination);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
				}

				// Null
				{
					var result = _mapper.Map<float, Price>(20f, null);
					Assert.IsNotNull(result);
				}
			}
		}

		[TestMethod]
		public void ShouldCatchExceptionsInMaps() {
			var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(2f, 2));
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
		}


		[TestMethod]
		public void ShouldMapCollectionsWithoutElementsComparer() {
			// No options
			{ 
				Maps.options = null;
				Maps.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var result = _mapper.Map(new[] { 20m, 15.25m, 0m }, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.IsTrue(result.All(v => v != a && v != b && v != c));

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (no merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var opts = new TestOptions();
				_mapper.Map(new[] { 20m, 15.25m, 0m }, destination, new[] { opts } );

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var opts = new TestOptions();
				var merge = new MergeMappingOptions {
					Matcher = (s, d, _) => false,
					CollectionRemoveNotMatchedDestinationElements = false
				};
				_mapper.Map(new[] { 20m, 15.25m, 0m }, destination, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNotNull(Maps.mergeOptions);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.CollectionRemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsWithElementsComparer() {
			// No options
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var a = new CategoryDto {
					Id = 2,
					Parent = 2
				};
				var b = new CategoryDto {
					Id = 3
				};
				var c = new CategoryDto {
					Id = 5
				};
				var destination = new CustomCollection<CategoryDto> { a, b, c };
				var result = _mapper.Map(new[] { 
					new Category {
						Id = 3,
						Parent = new Category {
							Id = 7
						}
					},
					new Category {
						Id = 2
					},
					new Category {
						Id = 6
					}
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.AreSame(a, result[0]);
				Assert.IsNull(result[0].Parent);
				Assert.AreSame(b, result[1]);
				Assert.AreEqual(7, result[1].Parent);
				Assert.AreEqual(6, result[2].Id);

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (no merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var a = new CategoryDto {
					Id = 2,
					Parent = 2
				};
				var b = new CategoryDto {
					Id = 3
				};
				var c = new CategoryDto {
					Id = 5
				};
				var destination = new CustomCollection<CategoryDto> { a, b, c };
				var opts = new TestOptions();
				_mapper.Map(new[] {
					new Category {
						Id = 3,
						Parent = new Category {
							Id = 7
						}
					},
					new Category {
						Id = 2
					},
					new Category {
						Id = 6
					}
				}, destination, new[] { opts });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var a = new CategoryDto {
					Id = 2,
					Parent = 2
				};
				var b = new CategoryDto {
					Id = 3
				};
				var c = new CategoryDto {
					Id = 5
				};
				var destination = new CustomCollection<CategoryDto> { a, b, c };
				var opts = new TestOptions();
				var merge = new MergeMappingOptions {
					Matcher = (s, d, _) => false,
					CollectionRemoveNotMatchedDestinationElements = false
				};
				_mapper.Map(new[] {
					new Category {
						Id = 3,
						Parent = new Category {
							Id = 7
						}
					},
					new Category {
						Id = 2
					},
					new Category {
						Id = 6
					}
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNotNull(Maps.mergeOptions);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.CollectionRemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsWithHierarchyElementsComparer() {
			var mapper = new Mapper(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(HierarchyMatchers) }
			});

			// No options
			{
				HierarchyMatchers.options = null;
				HierarchyMatchers.mergeOptions = null;

				var a = new ProductDto {
					Code = "Test1",
					Categories = new List<int>()
				};
				var b = new ProductDto {
					Code = "Test2",
					Categories = new List<int>()
				};
				var c = new ProductDto {
					Code = "Test4",
					Categories = new List<int>()
				};
				var destination = new CustomCollection<ProductDto> { a, b, c };
				var result = mapper.Map(new[] {
					new LimitedProduct {
						Code = "Test4",
						Categories = new List<Category>()
					},
					new LimitedProduct {
						Code = "Test1",
						Categories = new List<Category>()
					},
					new LimitedProduct {
						Code = "Test5",
						Categories = new List<Category>()
					}
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.AreSame(a, result[0]);
				Assert.AreSame(c, result[1]);
				Assert.AreEqual("Test5", result[2].Code);

				Assert.IsNull(HierarchyMatchers.options);
				Assert.IsNull(HierarchyMatchers.mergeOptions);
			}

			// Options (no merge)
			{
				HierarchyMatchers.options = null;
				HierarchyMatchers.mergeOptions = null;

				var a = new ProductDto {
					Code = "Test1",
					Categories = new List<int>()
				};
				var b = new ProductDto {
					Code = "Test2",
					Categories = new List<int>()
				};
				var c = new ProductDto {
					Code = "Test4",
					Categories = new List<int>()
				};
				var destination = new CustomCollection<ProductDto> { a, b, c };
				var opts = new TestOptions();
				mapper.Map(new[] {
					new LimitedProduct {
						Code = "Test4",
						Categories = new List<Category>()
					},
					new LimitedProduct {
						Code = "Test1",
						Categories = new List<Category>()
					},
					new LimitedProduct {
						Code = "Test5",
						Categories = new List<Category>()
					}
				}, destination, new[] { opts });

				Assert.AreSame(opts, HierarchyMatchers.options);
				Assert.IsNull(HierarchyMatchers.mergeOptions);
			}

			// Options (merge)
			{
				HierarchyMatchers.options = null;
				HierarchyMatchers.mergeOptions = null;

				var a = new ProductDto {
					Code = "Test1",
					Categories = new List<int>()
				};
				var b = new ProductDto {
					Code = "Test2",
					Categories = new List<int>()
				};
				var c = new ProductDto {
					Code = "Test4",
					Categories = new List<int>()
				};
				var destination = new CustomCollection<ProductDto> { a, b, c };
				var opts = new TestOptions();
				var merge = new MergeMappingOptions {
					Matcher = (s, d, _) => false,
					CollectionRemoveNotMatchedDestinationElements = false
				};
				mapper.Map(new[] {
					new LimitedProduct {
						Code = "Test4",
						Categories = new List<Category>()
					},
					new LimitedProduct {
						Code = "Test1",
						Categories = new List<Category>()
					},
					new LimitedProduct {
						Code = "Test5",
						Categories = new List<Category>()
					}
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, HierarchyMatchers.options);
				Assert.IsNotNull(HierarchyMatchers.mergeOptions);
				Assert.AreNotSame(merge, HierarchyMatchers.mergeOptions);
				Assert.IsNull(HierarchyMatchers.mergeOptions.Matcher);
				Assert.IsFalse(HierarchyMatchers.mergeOptions.CollectionRemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsWithCustomElementsComparer() {
			var a = new CategoryDto {
				Id = 2,
				Parent = 2
			};
			var b = new CategoryDto {
				Id = 3
			};
			var c = new CategoryDto {
				Id = 5
			};
			var destination = new List<CategoryDto> { a, b, c };
			// Just to override the elements comparer we are going to replace odd ids instead of merging them
			var result = _mapper.Map(new[] {
				new Category {
					Id = 3,
					Parent = new Category {
						Id = 7
					}
				},
				new Category {
					Id = 2
				},
				new Category {
					Id = 6
				}
			}, destination, (s, d, _) => s?.Id == d?.Id && s?.Id % 2 == 0);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreSame(a, result.ElementAt(0));
			Assert.IsNull(result.ElementAt(0).Parent);
			Assert.AreEqual(3, result.ElementAt(1).Id);
			Assert.AreEqual(7, result.ElementAt(1).Parent);
			Assert.AreEqual(6, result.ElementAt(2).Id);
		}

		[TestMethod]
		public void ShouldNotMapCollectionsWithoutMap() {
			var destination = new List<int>();
			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { false }, destination));
		}

		[TestMethod]
		public void ShouldMapNullCollections() {
			// Null source
			Assert.IsNull(_mapper.Map<int[], List<string>>(null, null));
			Assert.IsNull(_mapper.Map<int[], List<string>>(null, new List<string>()));

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(null, null));
			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(null, new List<float>()));

			// Null destination
			{
				var result = _mapper.Map<int[], List<string>>(new[] { 1, 4, 7 }, null);
				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Count);
			}

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(new[] { 1, 4, 7 }, null));
		}

		[TestMethod]
		public void ShouldNotMapNullCollectionsIfCannotCreateDestination() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], CustomCollectionWithoutParameterlessConstructor<float>>(new[] { 1, 4, 7 }, null));
		}

		[TestMethod]
		public void ShouldMapNullElementsInCollections() {
			var a = new CategoryDto {
				Id = 2,
				Parent = 2
			};
			var b = new CategoryDto {
				Id = 3
			};
			var c = new CategoryDto {
				Id = 5
			};
			var destination = new List<CategoryDto> { a, b, null, c };
			var result = _mapper.Map(new[] {
				new Category {
					Id = 3,
					Parent = new Category {
						Id = 7
					}
				},
				null,
				new Category {
					Id = 2
				},
				new Category {
					Id = 6
				}
			}, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(4, result.Count());
			Assert.AreSame(a, result[0]);
			Assert.IsNull(result[0].Parent);
			Assert.AreSame(b, result[1]);
			Assert.AreEqual(7, result[1].Parent);
			Assert.IsNull(result[2]);
			Assert.AreEqual(6, result[3]?.Id);
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestinationWithoutExplicitMap() {
			{ 
				var a = new Price{
					Amount = 12m,
					Currency = "EUR"
				};
				var b = new Price {
					Amount = 34m,
					Currency = "EUR"
				};
				var c = new Price {
					Amount = 56m,
					Currency = "EUR"
				};
				var destination = new Price[]{ a, b, c };
				TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { 20m, 15.25m, 0m }, destination));
				// Should not alter destination
				Assert.AreSame(a, destination[0]);
				Assert.AreEqual(12m, a.Amount);
				Assert.AreSame(b, destination[1]);
				Assert.AreEqual(34m, b.Amount);
				Assert.AreSame(c, destination[2]);
				Assert.AreEqual(56m, c.Amount);
			}

			{
				var a = new Price {
					Amount = 12m,
					Currency = "EUR"
				};
				var b = new Price {
					Amount = 34m,
					Currency = "EUR"
				};
				var c = new Price {
					Amount = 56m,
					Currency = "EUR"
				};
				var destination = new ReadOnlyCollection<Price>(new []{ a, b, c });
				TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { 20m, 15.25m, 0m }, destination));
				// Should not alter destination
				Assert.AreSame(a, destination[0]);
				Assert.AreEqual(12m, a.Amount);
				Assert.AreSame(b, destination[1]);
				Assert.AreEqual(34m, b.Amount);
				Assert.AreSame(c, destination[2]);
				Assert.AreEqual(56m, c.Amount);
			}
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestinationNestedWithoutExplicitMap() {
			var a1 = new CategoryDto {
				Id = 2,
				Parent = 2
			};
			var b1 = new CategoryDto {
				Id = 3
			};
			var c1 = new CategoryDto {
				Id = 5
			};
			var destination1 = new List<CategoryDto> { a1, b1, c1 };
			var a2 = new CategoryDto {
				Id = 6
			};
			var b2 = new CategoryDto {
				Id = 7,
				Parent = 2
			};
			var c2 = new CategoryDto {
				Id = 8
			};
			var destination2 = new CategoryDto[] { a2, b2, c2 };
			var destination = new List<IList<CategoryDto>> { destination1, destination2 };

			// Custom element comparer just to merge the collections
			// We'll start with the list since it is actually mappable, but in this case it shouldn't be because it is inside a collection with an array
			var source1 = new[] {
				new Category {
					Id = 3,
					Parent = new Category {
						Id = 7
					}
				},
				new Category {
					Id = 2
				},
				new Category {
					Id = 6
				}
			};
			var source2 = new[] {
				new Category {
					Id = 8,
					Parent = new Category {
						Id = 7
					}
				},
				new Category {
					Id = 5
				},
				new Category {
					Id = 3
				}
			};
			var source3 = new[] {
				new Category {
					Id = 4,
					Parent = new Category {
						Id = 7
					}
				},
				new Category {
					Id = 9
				},
				new Category {
					Id = 1
				}
			};
			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { source1, source3, null, source2 }, destination, (s, d, _) => (s == source1 && d == destination1) ||
				(s == source2 && d == destination2)));

			// Should not alter destination
			Assert.AreEqual(2, destination.Count);
			Assert.AreSame(destination1, destination[0]);
			Assert.AreSame(a1, destination[0][0]);
			Assert.AreEqual(2, a1.Parent);
			Assert.AreSame(b1, destination[0][1]);
			Assert.IsNull(b1.Parent);
			Assert.AreSame(c1, destination[0][2]);
			Assert.IsNull(c1.Parent);
			Assert.AreSame(destination2, destination[1]);
			Assert.AreSame(a2, destination[1][0]);
			Assert.IsNull(a2.Parent);
			Assert.AreSame(b2, destination[1][1]);
			Assert.AreEqual(2, b2.Parent);
			Assert.AreSame(c2, destination[1][2]);
			Assert.IsNull(c2.Parent);
		}

		[TestMethod]
		public void ShouldMapReadonlyCollectionDestinationWithExplicitMap() {
			var destination = new CategoryDto[3];
			var result = _mapper.Map<IEnumerable<Category>, CategoryDto[]>(new[] {
				new Category {
					Id = 2
				},
				new Category {
					Id = 3,
					Parent = new Category {
						Id = 6
					}
				}
			});
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Length);
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollectionsWithoutElementsComparer() {
			// No options
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var destination = new List<ICollection<string>>();
				var result = _mapper.Map(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, destination);

				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(2, result.Count);
				Assert.AreEqual(3, result[0].Count());
				Assert.AreEqual(2, result[1].Count());
				Assert.AreEqual("4", result[0].ElementAt(0));
				Assert.AreEqual("-6", result[0].ElementAt(1));
				Assert.AreEqual("0", result[0].ElementAt(2));
				Assert.AreEqual("2", result[1].ElementAt(0));
				Assert.AreEqual("4", result[1].ElementAt(1));

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (no merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var destination = new List<ICollection<string>>();
				var opts = new TestOptions();
				_mapper.Map(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, destination, new[] { opts });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var destination = new List<ICollection<string>>();
				var opts = new TestOptions();
				var merge = new MergeMappingOptions {
					Matcher = (s, d, _) => false,
					CollectionRemoveNotMatchedDestinationElements = false
				};
				_mapper.Map(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNotNull(Maps.mergeOptions);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.CollectionRemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollectionsWithElementsComparer() {
			var a1 = new CategoryDto {
				Id = 2,
				Parent = 2
			};
			var b1 = new CategoryDto {
				Id = 3
			};
			var c1 = new CategoryDto {
				Id = 5
			};
			var destination1 = new List<CategoryDto> { a1, b1, c1 };
			var a2 = new CategoryDto {
				Id = 6
			};
			var b2 = new CategoryDto {
				Id = 7,
				Parent = 2
			};
			var c2 = new CategoryDto {
				Id = 8
			};
			var destination2 = new HashSet<CategoryDto>() { a2, b2, c2 };
			var destination = new List<ICollection<CategoryDto>> { destination1, destination2 };

			var source1 = new[] {
				new Category {
					Id = 3,
					Parent = new Category {
						Id = 7
					}
				},
				new Category {
					Id = 2
				},
				new Category {
					Id = 6
				}
			};
			var source2 = new[] {
				new Category {
					Id = 8,
					Parent = new Category {
						Id = 7
					}
				},
				new Category {
					Id = 5
				},
				new Category {
					Id = 3
				}
			};
			var result = _mapper.Map<IEnumerable<IEnumerable<Category>>, IList<ICollection<CategoryDto>>>(new[] { source1, source1 }, destination);

			// A collection of collections, even though the innermost has an elements comparer the outer ones could not be matched
			// so they will be recreated, and also the children will be recreated regardless
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(3, result[0].Count());
			Assert.IsTrue(result[0].All(e => e != a1 && e != b1 && e != c1 & e != a2 && e != b2 && e != c2));
			Assert.AreEqual(3, result[1].Count());
			Assert.IsTrue(result[1].All(e => e != a1 && e != b1 && e != c1 & e != a2 && e != b2 && e != c2));
		}

		[TestMethod]
		public void ShouldRespectReturnedValueInCollections() {
			// Returns new destination
			{
				// Not null
				{
					var a1 = new Price {
						Amount = 20m
					};
					var destination = new List<Price>{ a1 };
					var result = _mapper.Map(new[] { 20m }, destination, (s, d, _) => s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreNotSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
					var result = _mapper.Map(new[] { 20m }, destination, (s, d, _) => d == null);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.IsNotNull(result.Single());
				}
			}

			// Returns passed destination (if not null)
			{
				// Not null
				{
					var a1 = new Price {
						Amount = 20m
					};
					var destination = new List<Price> { a1 };
					var result = _mapper.Map(new[] { 20f }, destination, (s, d, _) => (decimal)s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
					var result = _mapper.Map(new[] { 20f }, destination, (s, d, _) => d == null);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.IsNotNull(result.Single());
				}
			}
		}

		[TestMethod]
		public void ShouldPreferMergeMapForElementsToUpdateAndNewMapForElementsToAddInCollections() {
			var destination = new List<string> { "3", "7", "0" };
			var result = _mapper.Map(new[] { 7f, 4f, 3f }, destination, (s, d, _) => s.ToString() == d);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("MergeMap", result.ElementAt(0));
			Assert.AreEqual("NewMap", result.ElementAt(1));
			Assert.AreEqual("MergeMap", result.ElementAt(2));
		}

		[TestMethod]
		public void ShouldNotMapCollectionsIfCannotCreateElement() {
			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { "" }, new List<ClassWithoutParameterlessConstructor>()));
		}

		[TestMethod]
		public void ShouldNotRemoveUnmatchedElementsFromDestinationIfSpecified() {
			// Global settings
			{ 
				var mapper = new Mapper(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(Maps) },
					MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions {
						RemoveNotMatchedDestinationElements = false
					}
				});

				var a = new CategoryDto {
					Id = 2,
					Parent = 2
				};
				var b = new CategoryDto {
					Id = 3
				};
				var c = new CategoryDto {
					Id = 5
				};
				var destination = new List<CategoryDto> { a, b, c };
				var result = mapper.Map(new[] {
					new Category {
						Id = 3,
						Parent = new Category {
							Id = 7
						}
					},
					new Category {
						Id = 2
					},
					new Category {
						Id = 6
					}
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(4, result.Count());
				Assert.AreSame(a, result[0]);
				Assert.IsNull(result[0].Parent);
				Assert.AreSame(b, result[1]);
				Assert.AreEqual(7, result[1].Parent);
				Assert.AreSame(c, result[2]);
				Assert.AreEqual(6, result[3].Id);
			}

			// Override
			{
				var a = new CategoryDto {
					Id = 2,
					Parent = 2
				};
				var b = new CategoryDto {
					Id = 3
				};
				var c = new CategoryDto {
					Id = 5
				};
				var destination = new List<CategoryDto> { a, b, c };
				var result = _mapper.Map(new[] {
					new Category {
						Id = 3,
						Parent = new Category {
							Id = 7
						}
					},
					new Category {
						Id = 2
					},
					new Category {
						Id = 6
					}
				}, destination, new[]{
					new MergeMappingOptions {
						CollectionRemoveNotMatchedDestinationElements = false
					} 
				});
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(4, result.Count());
				Assert.AreSame(a, result[0]);
				Assert.IsNull(result[0].Parent);
				Assert.AreSame(b, result[1]);
				Assert.AreEqual(7, result[1].Parent);
				Assert.AreSame(c, result[2]);
				Assert.AreEqual(6, result[3].Id);
			}
		}

		[TestMethod]
		public void ShouldCatchExceptionsInCollectionMaps() {
			// Normal collections
			{ 
				// Without comparer
				var exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { 2f }, new List<int>()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// With comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { 2m }, new List<int>() { 3 }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// With custom comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { 2f }, new List<int>() { 3 }, (a, b, c) => throw new NotImplementedException()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
			}

			// Nested collections
			{
				// Without comparer
				var exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { new[] { 2f } }, new List<List<int>> { new List<int>() }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

				// With comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { new[] { 2m } }, new List<List<int>>{ new List<int> { 3 } }, (a, b, c) => true));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

				// With custom comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => throw new NotImplementedException()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
			}
		}

		[TestMethod]
		public void ShouldMapWithAdditionalMaps() {
			var options = new MapperOptions();
			options.AddMergeMap<string, int>((s, d, _) => (s?.Length ?? 0) + 2);
			var mapper = new Mapper(options);

			Assert.AreEqual(6, mapper.Map("Test", 2));
		}
	}
}
