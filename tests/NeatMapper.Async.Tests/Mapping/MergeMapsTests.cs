using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Async;
using NeatMapper.Configuration;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class MergeMapsTests {
		public class Maps :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<int, string>,
			IAsyncNewMapStatic<float, string>,
			IAsyncMergeMapStatic<float, string>,
			IAsyncMergeMapStatic<Price, decimal>,
			IAsyncMergeMapStatic<Price, PriceFloat>,
			IAsyncMergeMapStatic<Category, int>,
			IAsyncMergeMapStatic<Product, ProductDto>,
			IAsyncMergeMapStatic<LimitedProduct, LimitedProductDto>,
			IAsyncMergeMapStatic<decimal, Price>,
			IAsyncMergeMapStatic<float, Price>,
			IAsyncMergeMapStatic<Category, CategoryDto>,
			IMatchMapStatic<Category, CategoryDto>,
			IAsyncMergeMapStatic<IEnumerable<Category>, CategoryDto[]>,
			IAsyncMergeMapStatic<float, int>,
			IAsyncMergeMapStatic<double, int>,
			IAsyncMergeMapStatic<decimal, int>,
			IMatchMapStatic<decimal, int>,
			IAsyncMergeMapStatic<string, ClassWithoutParameterlessConstructor>
#else
			IAsyncMergeMap<int, string>,
			IAsyncNewMap<float, string>,
			IAsyncMergeMap<float, string>,
			IAsyncMergeMap<Price, decimal>,
			IAsyncMergeMap<Price, PriceFloat>,
			IAsyncMergeMap<Category, int>,
			IAsyncMergeMap<Product, ProductDto>,
			IAsyncMergeMap<LimitedProduct, LimitedProductDto>,
			IAsyncMergeMap<decimal, Price>,
			IAsyncMergeMap<float, Price>,
			IAsyncMergeMap<Category, CategoryDto>,
			IMatchMap<Category, CategoryDto>,
			IAsyncMergeMap<IEnumerable<Category>, CategoryDto[]>,
			IAsyncMergeMap<float, int>,
			IAsyncMergeMap<double, int>,
			IAsyncMergeMap<decimal, int>,
			IMatchMap<decimal, int>,
			IAsyncMergeMap<string, ClassWithoutParameterlessConstructor>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<int, string>
#else
				IAsyncMergeMap<int, string>
#endif
				.MapAsync(int source, string destination, AsyncMappingContext context) {
				return Task.FromResult((source * 2).ToString());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<float, string>
#else
				IAsyncNewMap<float, string>
#endif
				.MapAsync(float source, AsyncMappingContext context) {
				return Task.FromResult("NewMap");
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<float, string>
#else
				IAsyncMergeMap<float, string>
#endif
				.MapAsync(float source, string destination, AsyncMappingContext context) {
				return Task.FromResult("MergeMap");
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<decimal>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Price, decimal>
#else
				IAsyncMergeMap<Price, decimal>
#endif
				.MapAsync(Price source, decimal destination, AsyncMappingContext context) {
				return Task.FromResult(source?.Amount ?? 0m);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<PriceFloat>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Price, PriceFloat>
#else
				IAsyncMergeMap<Price, PriceFloat>
#endif
				.MapAsync(Price source, PriceFloat destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new PriceFloat();
					destination.Amount = (float)source.Amount;
					destination.Currency = source.Currency;
				}
				return Task.FromResult(destination);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Category, int>
#else
				IAsyncMergeMap<Category, int>
#endif
				.MapAsync(Category source, int destination, AsyncMappingContext context) {
				return Task.FromResult(source?.Id ?? destination);
			}

			// Nested MergeMap
#if NET7_0_OR_GREATER
			static
#endif
			async Task<ProductDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Product, ProductDto>
#else
				IAsyncMergeMap<Product, ProductDto>
#endif
				.MapAsync(Product source, ProductDto destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new ProductDto();
					destination.Code = source.Code;
					destination.Categories = await context.Mapper.MapAsync(source.Categories, destination.Categories) ?? new List<int>();
				}
				return destination;
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			async Task<LimitedProductDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<LimitedProduct, LimitedProductDto>
#else
				IAsyncMergeMap<LimitedProduct, LimitedProductDto>
#endif
				.MapAsync(LimitedProduct source, LimitedProductDto destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = await context.Mapper.MapAsync<ICollection<int>>(source.Categories) ?? new List<int>();
					destination.Copies = source.Copies;
				}
				return destination;
			}

			// Returns new destination
#if NET7_0_OR_GREATER
			static
#endif
			Task<Price>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<decimal, Price>
#else
				IAsyncMergeMap<decimal, Price>
#endif
				.MapAsync(decimal source, Price destination, AsyncMappingContext context) {
				return Task.FromResult(new Price {
					Amount = source,
					Currency = "EUR"
				});
			}

			// Returns passed destination (if not null)
#if NET7_0_OR_GREATER
			static
#endif
			Task<Price>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<float, Price>
#else
				IAsyncMergeMap<float, Price>
#endif
				.MapAsync(float source, Price destination, AsyncMappingContext context) {
				if (destination == null)
					destination = new Price();
				destination.Amount = (decimal)source;
				destination.Currency = "EUR";
				return Task.FromResult(destination);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<CategoryDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Category, CategoryDto>
#else
				IAsyncMergeMap<Category, CategoryDto>
#endif
				.MapAsync(Category source, CategoryDto destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = source.Parent?.Id;
				}
				return Task.FromResult(destination);
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
			async Task<CategoryDto[]>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IEnumerable<Category>, CategoryDto[]>
#else
				IAsyncMergeMap<IEnumerable<Category>, CategoryDto[]>
#endif
				.MapAsync(IEnumerable<Category> source, CategoryDto[] destination, AsyncMappingContext context) {
                if (source == null)
					return Array.Empty<CategoryDto>();
                var tasks = source.Select(s => context.Mapper.MapAsync<Category, CategoryDto>(s));
				await Task.WhenAll(tasks);

				return tasks.Select(t => t.Result).ToArray();
			}

			// Throws exception (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<float, int>
#else
				IAsyncMergeMap<float, int>
#endif
				.MapAsync(float source, int destination, AsyncMappingContext context) {
				throw new NotImplementedException();
			}

			// Throws exception (awaited)
#if NET7_0_OR_GREATER
			static
#endif
			async Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<double, int>
#else
				IAsyncMergeMap<double, int>
#endif
				.MapAsync(double source, int destination, AsyncMappingContext context) {
				await Task.Delay(1);
				throw new NotImplementedException();
			}

			// Throws exception (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<decimal, int>
#else
				IAsyncMergeMap<decimal, int>
#endif
				.MapAsync(decimal source, int destination, AsyncMappingContext context) {
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
			Task<ClassWithoutParameterlessConstructor>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<string, ClassWithoutParameterlessConstructor>
#else
				IAsyncMergeMap<string, ClassWithoutParameterlessConstructor>
#endif
				.MapAsync(string source, ClassWithoutParameterlessConstructor destination, AsyncMappingContext context) {
				return Task.FromResult(destination);
			}
		}

		public class HierarchyMatchers :
#if NET7_0_OR_GREATER
			IHierarchyMatchMapStatic<Product, ProductDto>,
			IAsyncMergeMapStatic<LimitedProduct, ProductDto>
#else
			IHierarchyMatchMap<Product, ProductDto>,
			IAsyncMergeMap<LimitedProduct, ProductDto>
#endif
			{


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
			Task<ProductDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<LimitedProduct, ProductDto>
#else
				IAsyncMergeMap<LimitedProduct, ProductDto>
#endif
				.MapAsync(LimitedProduct source, ProductDto destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = new List<int>();
				}
				return Task.FromResult(destination);
			}
		}

		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncMapper(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public async Task ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, await _mapper.MapAsync<int, string>(input, ""));
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			Assert.AreEqual(20.00m, await _mapper.MapAsync<Price, decimal>(new Price {
				Amount = 20.00m,
				Currency = "EUR"
			}, 21m));

			var result = await _mapper.MapAsync<Price, PriceFloat>(new Price {
				Amount = 40.00m,
				Currency = "EUR"
			}, null);
			Assert.IsNotNull(result);
			Assert.AreEqual(40f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);

			var destination = new PriceFloat();
			result = await _mapper.MapAsync<Price, PriceFloat>(new Price {
				Amount = 40.00m,
				Currency = "EUR"
			}, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(40f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
		}

		[TestMethod]
		public async Task ShouldMapChildClassesAsParents() {
			// Parent source
			{
				// Not null destination
				{
					var destination = new ProductDto();
					var result = await _mapper.MapAsync<Product, ProductDto>(new LimitedProduct {
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
					var result = await _mapper.MapAsync<Product, ProductDto>(new LimitedProduct {
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
					var result = await _mapper.MapAsync<Product, ProductDto>(new Product {
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
					var result = await _mapper.MapAsync<Product, ProductDto>(null, destination);
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
				var result = await _mapper.MapAsync<Product, ProductDto>(new LimitedProduct {
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
		public Task ShouldNotMapWithoutMap() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync(false, 0));
		}

		[TestMethod]
		public async Task ShouldMapNested() {
			{
				var destination = new ProductDto();
				var result = await _mapper.MapAsync<Product, ProductDto>(new Product {
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
				var result = await _mapper.MapAsync<LimitedProduct, LimitedProductDto>(new LimitedProduct {
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
		public async Task ShouldRespectReturnedValue() {
			// Returns new destination
			{
				// Not null
				{
					var destination = new Price();
					var result = await _mapper.MapAsync(20m, destination);
					Assert.IsNotNull(result);
					Assert.AreNotSame(destination, result);
				}

				// Null
				{
					var result = await _mapper.MapAsync<decimal, Price>(20m, null);
					Assert.IsNotNull(result);
				}
			}

			// Returns passed destination (if not null)
			{
				// Not null
				{
					var destination = new Price();
					var result = await _mapper.MapAsync(20f, destination);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
				}

				// Null
				{
					var result = await _mapper.MapAsync<float, Price>(20f, null);
					Assert.IsNotNull(result);
				}
			}
		}

		[TestMethod]
		public async Task ShouldCatchExceptionsInMaps() {
			// Not awaited
			{
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(2f, 2));
				Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
			}

			// Awaited
			{
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(2d, 2));
				Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
			}
		}


		[TestMethod]
		public async Task ShouldMapCollectionsWithoutElementsComparer() {
			var a = new Price();
			var b = new Price();
			var c = new Price();
			var destination = new List<Price> { a, b, c };
			var result = await _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.IsTrue(result.All(v => v != a && v != b && v != c));
		}

		[TestMethod]
		public async Task ShouldMapCollectionsWithElementsComparer() {
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
			var result = await _mapper.MapAsync(new[] {
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
		}

		[TestMethod]
		public async Task ShouldMapCollectionsWithHierarchyElementsComparer() {
			var mapper = new AsyncMapper(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(HierarchyMatchers) }
			});

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
			var result = await mapper.MapAsync(new[] {
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
		}

		[TestMethod]
		public async Task ShouldMapCollectionsWithCustomElementsComparer() {
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
			var result = await _mapper.MapAsync(new[] {
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
		public Task ShouldNotMapCollectionsWithoutMap() {
			var destination = new List<int>();
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { false }, destination));
		}

		[TestMethod]
		public async Task ShouldMapNullCollections() {
			// Null source
			Assert.IsNull(await _mapper.MapAsync<int[], List<string>>(null, null));
			Assert.IsNull(await _mapper.MapAsync<int[], List<string>>(null, new List<string>()));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<float>>(null, null));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<float>>(null, new List<float>()));

			// Null destination
			{
				var result = await _mapper.MapAsync<int[], List<string>>(new[] { 1, 4, 7 }, null);
				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Count);
			}

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<float>>(new[] { 1, 4, 7 }, null));
		}

		[TestMethod]
		public Task ShouldNotMapNullCollectionsIfCannotCreateDestination() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], CustomCollectionWithoutParameterlessConstructor<float>>(new[] { 1, 4, 7 }, null));
		}

		[TestMethod]
		public async Task ShouldMapNullElementsInCollections() {
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
			var result = await _mapper.MapAsync(new[] {
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
		public async Task ShouldNotMapReadonlyCollectionDestinationWithoutExplicitMap() {
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
				var destination = new Price[] { a, b, c };
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination));
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
				var destination = new ReadOnlyCollection<Price>(new[] { a, b, c });
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination));
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
		public async Task ShouldNotMapReadonlyCollectionDestinationNestedWithoutExplicitMap() {
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
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { source1, source3, null, source2 }, destination, (s, d, _) => (s == source1 && d == destination1) ||
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
		public async Task ShouldMapReadonlyCollectionDestinationWithExplicitMap() {
			var destination = new CategoryDto[3];
			var result = await _mapper.MapAsync<IEnumerable<Category>, CategoryDto[]>(new[] {
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
		public async Task ShouldMapCollectionsOfCollectionsWithoutElementsComparer() {
			var destination = new List<ICollection<string>>();
			var result = await _mapper.MapAsync(new[] {
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
		}

		[TestMethod]
		public async Task ShouldMapCollectionsOfCollectionsWithElementsComparer() {
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
			var result = await _mapper.MapAsync<IEnumerable<IEnumerable<Category>>, IList<ICollection<CategoryDto>>>(new[] { source1, source1 }, destination);

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
		public async Task ShouldRespectReturnedValueInCollections() {
			// Returns new destination
			{
				// Not null
				{
					var a1 = new Price {
						Amount = 20m
					};
					var destination = new List<Price> { a1 };
					var result = await _mapper.MapAsync(new[] { 20m }, destination, (s, d, _) => s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreNotSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
					var result = await _mapper.MapAsync(new[] { 20m }, destination, (s, d, _) => d == null);
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
					var result = await _mapper.MapAsync(new[] { 20f }, destination, (s, d, _) => (decimal)s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
					var result = await _mapper.MapAsync(new[] { 20f }, destination, (s, d, _) => d == null);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.IsNotNull(result.Single());
				}
			}
		}

		[TestMethod]
		public async Task ShouldPreferMergeMapForElementsToUpdateAndNewMapForElementsToAddInCollections() {
			var destination = new List<string> { "3", "7", "0" };
			var result = await _mapper.MapAsync(new[] { 7f, 4f, 3f }, destination, (s, d, _) => s.ToString() == d);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("MergeMap", result.ElementAt(0));
			Assert.AreEqual("NewMap", result.ElementAt(1));
			Assert.AreEqual("MergeMap", result.ElementAt(2));
		}

		[TestMethod]
		public Task ShouldNotMapCollectionsIfCannotCreateElement() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { "" }, new List<ClassWithoutParameterlessConstructor>()));
		}

		[TestMethod]
		public async Task ShouldNotRemoveUnmatchedElementsFromDestinationIfSpecified() {
			// Global settings
			{
				var mapper = new AsyncMapper(new MapperConfigurationOptions {
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
				var result = await mapper.MapAsync(new[] {
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
				var result = await _mapper.MapAsync(new[] {
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
				}, destination, new MappingOptions {
					CollectionRemoveNotMatchedDestinationElements = false
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
		public async Task ShouldCatchExceptionsInCollectionMaps() {
			// Normal collections
			{
				// Without comparer (not awaited)
				var exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { 2f }, new List<int>()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// Without comparer (awaited)
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { 2d }, new List<int>()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// With comparer
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { 2m }, new List<int>() { 3 }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// With custom comparer
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { 2f }, new List<int>() { 3 }, (a, b, c) => throw new NotImplementedException()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
			}

			// Nested collections
			{
				// Without comparer (not awaited)
				var exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { new[] { 2f } }, new List<List<int>> { new List<int>() }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

				// Without comparer (awaited)
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { new[] { 2d } }, new List<List<int>> { new List<int>() }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

				// With comparer
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { new[] { 2m } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => true));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

				// With custom comparer
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => throw new NotImplementedException()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
			}
		}
	}
}
