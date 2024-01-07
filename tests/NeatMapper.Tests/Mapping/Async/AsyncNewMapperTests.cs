using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	[TestClass]
	public class AsyncNewMapperTests {
		public class Maps :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<int, string>,
			IAsyncNewMapStatic<Price, decimal>,
			IAsyncNewMapStatic<Price, PriceFloat>,
			IAsyncNewMapStatic<Product, ProductDto>,
			IAsyncNewMapStatic<LimitedProduct, LimitedProductDto>,
			IAsyncNewMapStatic<Category, int?>,
			IAsyncNewMapStatic<Category, CategoryDto>,
			IAsyncNewMapStatic<float, string>,
			IAsyncNewMapStatic<string, ClassWithoutParameterlessConstructor>,
			IAsyncNewMapStatic<decimal, Price>,
			IAsyncNewMapStatic<float, Price>,
			IAsyncNewMapStatic<float, int>,
			IAsyncNewMapStatic<float, decimal>,
			IAsyncNewMapStatic<string, KeyValuePair<string, int>>,
			IAsyncNewMapStatic<string, int>,
			IAsyncNewMapStatic<decimal, int>,
			IAsyncNewMapStatic<decimal, string>,
			IAsyncNewMapStatic<int, float>,
			IAsyncNewMapStatic<int, char>,
			IAsyncNewMapStatic<char, float>,
			IAsyncNewMapStatic<decimal, float>,
			IAsyncNewMapStatic<decimal, double>,
			IAsyncNewMapStatic<decimal, bool>,
			IAsyncNewMapStatic<float, double>,
			IAsyncNewMapStatic<double, float>
#else
			IAsyncNewMap<int, string>,
			IAsyncNewMap<Price, decimal>,
			IAsyncNewMap<Price, PriceFloat>,
			IAsyncNewMap<Product, ProductDto>,
			IAsyncNewMap<LimitedProduct, LimitedProductDto>,
			IAsyncNewMap<Category, int?>,
			IAsyncNewMap<Category, CategoryDto>,
			IAsyncNewMap<float, string>,
			IAsyncNewMap<string, ClassWithoutParameterlessConstructor>,
			IAsyncNewMap<decimal, Price>,
			IAsyncNewMap<float, Price>,
			IAsyncNewMap<float, int>,
			IAsyncNewMap<float, decimal>,
			IAsyncNewMap<string, KeyValuePair<string, int>>,
			IAsyncNewMap<string, int>,
			IAsyncNewMap<decimal, int>,
			IAsyncNewMap<decimal, string>,
			IAsyncNewMap<int, float>,
			IAsyncNewMap<int, char>,
			IAsyncNewMap<char, float>,
			IAsyncNewMap<decimal, float>,
			IAsyncNewMap<decimal, double>,
			IAsyncNewMap<decimal, bool>,
			IAsyncNewMap<float, double>,
			IAsyncNewMap<double, float>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<int, string>
#else
				IAsyncNewMap<int, string>
#endif
				.MapAsync(int source, AsyncMappingContext context) {

				MappingOptionsUtils.asyncContext = context;
				MappingOptionsUtils.asyncContexts.Add(context);
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult((source * 2).ToString());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<decimal>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Price, decimal>
#else
				IAsyncNewMap<Price, decimal>
#endif
				.MapAsync(Price source, AsyncMappingContext context) {
				return Task.FromResult(source?.Amount ?? 0m);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<PriceFloat>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Price, PriceFloat>
#else
				IAsyncNewMap<Price, PriceFloat>
#endif
				.MapAsync(Price source, AsyncMappingContext context) {
				if (source == null)
					return Task.FromResult((PriceFloat)null);
				else { 
					return Task.FromResult(new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					});
				}
			}

			// Nested NewMap
			public static MappingOptions productOptions;
#if NET7_0_OR_GREATER
			static
#endif
			async Task<ProductDto>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Product, ProductDto>
#else
				IAsyncNewMap<Product, ProductDto>
#endif
				.MapAsync(Product source, AsyncMappingContext context) {

				productOptions = context.MappingOptions;
				if (source == null)
					return null;
				else {
					var tasks = source.Categories?.Select(c => context.Mapper.MapAsync<int?>(c)).ToArray();
					if(tasks != null)
						await Task.WhenAll(tasks);
					return new ProductDto {
						Code = source.Code,
						Categories = tasks?.Select(t => t.Result).Where(i => i != null).Cast<int>().ToList() ?? new List<int>()
					};
				}
			}

#if NET7_0_OR_GREATER
			static
#endif
			async Task<LimitedProductDto>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<LimitedProduct, LimitedProductDto>
#else
				IAsyncNewMap<LimitedProduct, LimitedProductDto>
#endif
				.MapAsync(LimitedProduct source, AsyncMappingContext context) {
				if (source == null)
					return null;
				else {
					var tasks = source.Categories?.Select((s, d) => context.Mapper.MapAsync<int?>(s)).ToArray();
					if(tasks != null)
						await Task.WhenAll(tasks);
					return new LimitedProductDto {
						Code = source.Code,
						Categories = tasks?.Select(t => t.Result).Where(i => i != null).Cast<int>().ToList() ?? new List<int>(),
						Copies = source.Copies
					};
				}
			}

			public static List<MappingOptions> categoryOptions = new List<MappingOptions>();
#if NET7_0_OR_GREATER
			static
#endif
			Task<int?>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Category, int?>
#else
				IAsyncNewMap<Category, int?>
#endif
			.MapAsync(Category source, AsyncMappingContext context) {

				categoryOptions.Add(context.MappingOptions);
				return Task.FromResult(source?.Id);
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			async Task<CategoryDto>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Category, CategoryDto>
#else
				IAsyncNewMap<Category, CategoryDto>
#endif
				.MapAsync(Category source, AsyncMappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				if (source == null)
					return null;
				else {
					return new CategoryDto {
						Id = source.Id,
						Parent = await context.Mapper.MapAsync<Category, int?>(source.Parent)
					};
				}
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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult((source * 3).ToString());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<ClassWithoutParameterlessConstructor>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<string, ClassWithoutParameterlessConstructor>
#else
				IAsyncNewMap<string, ClassWithoutParameterlessConstructor>
#endif
				.MapAsync(string source, AsyncMappingContext context) {
				return Task.FromResult(new ClassWithoutParameterlessConstructor(""));
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<Price>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, Price>
#else
				IAsyncNewMap<decimal, Price>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult(new Price {
					Amount = source,
					Currency = "EUR"
				});
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<Price>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<float, Price>
#else
				IAsyncNewMap<float, Price>
#endif
				.MapAsync(float source, AsyncMappingContext context) {
				return Task.FromResult(new Price {
					Amount = (decimal)source,
					Currency = "EUR"
				});
			}

			// Throws exception (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<float, int>
#else
				IAsyncNewMap<float, int>
#endif
				.MapAsync(float source, AsyncMappingContext context) {
				throw new NotImplementedException();
			}

			// Throws exception (awaited)
#if NET7_0_OR_GREATER
			static
#endif
			async Task<decimal>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<float, decimal>
#else
				IAsyncNewMap<float, decimal>
#endif
				.MapAsync(float source, AsyncMappingContext context) {
				await Task.Delay(0);
				throw new NotImplementedException();
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<KeyValuePair<string, int>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<string, KeyValuePair<string, int>>
#else
				IAsyncNewMap<string, KeyValuePair<string, int>>
#endif
				.MapAsync(string source, AsyncMappingContext context) {
				return Task.FromResult(new KeyValuePair<string, int>(source, source.Length));
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<string, int>
#else
				IAsyncNewMap<string, int>
#endif
				.MapAsync(string source, AsyncMappingContext context) {
				return Task.FromResult(source?.Length ?? -1);
			}

			// Throws exception (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, int>
#else
				IAsyncNewMap<decimal, int>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				throw new NotImplementedException();
			}

			// Different map
#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, string>
#else
				IAsyncNewMap<decimal, string>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				return Task.FromResult("NewMap");
			}

			// "long"-running task
#if NET7_0_OR_GREATER
			static
#endif
			async Task<float>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<int, float>
#else
				IAsyncNewMap<int, float>
#endif
				.MapAsync(int source, AsyncMappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				await Task.Delay(10);
				return (source * 2);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<char>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<int, char>
#else
				IAsyncNewMap<int, char>
#endif
				.MapAsync(int source, AsyncMappingContext context) {
				return Task.FromResult((char)source);
			}


#if NET7_0_OR_GREATER
			static
#endif
			Task<float>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<char, float>
#else
				IAsyncNewMap<char, float>
#endif
				.MapAsync(char source, AsyncMappingContext context) {
				return Task.FromResult((float)source);
			}

			// Throws task canceled (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<float>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, float>
#else
				IAsyncNewMap<decimal, float>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				throw new TaskCanceledException();
			}

			// Throws task canceled (awaited)
#if NET7_0_OR_GREATER
			static
#endif
			async Task<double>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, double>
#else
				IAsyncNewMap<decimal, double>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				await Task.Delay(0);
				throw new TaskCanceledException();
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<bool>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, bool>
#else
				IAsyncNewMap<decimal, bool>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				return Task.FromResult(true);
			}

			// Rejects itself (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<double>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<float, double>
#else
				IAsyncNewMap<float, double>
#endif
				.MapAsync(float source, AsyncMappingContext context) {

				throw new MapNotFoundException((typeof(float), typeof(double)));
			}

			// Rejects itself (awaited)
#if NET7_0_OR_GREATER
			static
#endif
			async Task<float>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<double, float>
#else
				IAsyncNewMap<double, float>
#endif
				.MapAsync(double source, AsyncMappingContext context) {

				await Task.Delay(0);
				throw new MapNotFoundException((typeof(double), typeof(float)));
			}
		}

		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		public async Task ShouldMapPrimitives() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<int, string>());

			Assert.AreEqual("4", await _mapper.MapAsync<string>(2));
			Assert.AreEqual("-6", await _mapper.MapAsync<string>(-3));
			Assert.AreEqual("0", await _mapper.MapAsync<string>(0));

			// Factories should share the same context
			var factory = _mapper.MapAsyncNewFactory<int, string>();
			MappingOptionsUtils.asyncContext = null;
			Assert.AreEqual("4", await factory.Invoke(2));
			var context1 = MappingOptionsUtils.asyncContext;
			Assert.IsNotNull(context1);
			MappingOptionsUtils.asyncContext = null;
			Assert.AreEqual("-6", await factory.Invoke(-3));
			var context2 = MappingOptionsUtils.asyncContext;
			Assert.IsNotNull(context2);
			Assert.AreSame(context1, context2);
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<Price, decimal>());

				Assert.AreEqual(20.00m, await _mapper.MapAsync<decimal>(new Price {
					Amount = 20.00m,
					Currency = "EUR"
				}));

				Assert.AreEqual(20.00m, await _mapper.MapAsyncNewFactory<Price, decimal>().Invoke(new Price {
					Amount = 20.00m,
					Currency = "EUR"
				}));
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<Price, PriceFloat>());

				var result = await _mapper.MapAsync<PriceFloat>(new Price {
					Amount = 40.00m,
					Currency = "EUR"
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(40f, result.Amount);
				Assert.AreEqual("EUR", result.Currency);

				var result2 = await _mapper.MapAsyncNewFactory<Price, PriceFloat>().Invoke(new Price {
					Amount = 40.00m,
					Currency = "EUR"
				});
				Assert.IsNotNull(result2);
				Assert.AreEqual(40f, result2.Amount);
				Assert.AreEqual("EUR", result2.Currency);
			}
		}

		[TestMethod]
		public async Task ShouldMapChildClassAsParent() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<Product, ProductDto>());

			var result = await _mapper.MapAsync<Product, ProductDto>(new LimitedProduct {
				Code = "Test",
				Categories = new List<Category> {
					new Category {
						Id = 2
					}
				},
				Copies = 3
			});
			Assert.IsNotNull(result);
			Assert.IsTrue(result.GetType() == typeof(ProductDto));
		}

		[TestMethod]
		public async Task ShouldNotMapWithoutMap() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<bool, int>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int>(false));
		}

		[TestMethod]
		public async Task ShouldMapNested() {
			{
				// Normal
				Maps.productOptions = null;
				Maps.categoryOptions.Clear();
				var result = await _mapper.MapAsync<Product, ProductDto>(new Product {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						},
						new Category {
							Id = 3
						}
					}
				});
				Assert.IsNotNull(result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(2, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.First());
				Assert.AreEqual(3, result.Categories.Last());

				Assert.IsNull(Maps.productOptions.GetOptions<AsyncNestedMappingContext>());
				// Should not use same context for nested maps
				Assert.AreEqual(2, Maps.categoryOptions.Count);
				Assert.AreEqual(2, Maps.categoryOptions.Distinct().Count());
				Assert.IsTrue(Maps.categoryOptions.All(o => o.GetOptions<AsyncNestedMappingContext>() != null));
				

				// Factory
				Maps.productOptions = null;
				Maps.categoryOptions.Clear();
				var result2 = await _mapper.MapAsyncNewFactory<Product, ProductDto>().Invoke(new Product {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						},
						new Category {
							Id = 3
						}
					}
				});
				Assert.IsNotNull(result2);
				Assert.AreEqual("Test", result2.Code);
				Assert.IsNotNull(result2.Categories);
				Assert.AreEqual(2, result2.Categories.Count());
				Assert.AreEqual(2, result2.Categories.First());
				Assert.AreEqual(3, result2.Categories.Last());

				Assert.IsNull(Maps.productOptions.GetOptions<AsyncNestedMappingContext>());
				// Should use same context for nested maps
				Assert.AreEqual(2, Maps.categoryOptions.Count);
				Assert.AreEqual(1, Maps.categoryOptions.Distinct().Count());
				Assert.IsNotNull(Maps.categoryOptions.First().GetOptions<AsyncNestedMappingContext>());
			}

			{ 
				var result = await _mapper.MapAsync<Category, CategoryDto>(new Category {
					Id = 2
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Id);
				Assert.IsNull(result.Parent);
			}

			{
				var result = await _mapper.MapAsync<Category, CategoryDto>(new Category {
					Id = 2,
					Parent = new Category { Id = 3 }
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Id);
				Assert.AreEqual(3, result.Parent);
			}
		}

		[TestMethod]
		public async Task ShouldCatchExceptionsInMaps() {
			// Should wrap exceptions
			{
				// Not awaited
				{ 
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int>(2f));
					Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
				}

				// Awaited
				{ 
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<decimal>(2f));
					Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
				}
			}

			// Should not wrap TaskCanceledException
			{
				// Not awaited
				await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync<float>(2m));

				// Awaited
				await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync<double>(2m));
			}
		}

		[TestMethod]
		public async Task ShouldMapWithAdditionalMaps() {
			var options = new CustomAsyncNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => Task.FromResult(s?.Length ?? 0));
			var mapper = new AsyncNewMapper(null, options);

			Assert.IsTrue(await mapper.CanMapAsyncNew<string, int>());

			Assert.AreEqual(4, await mapper.MapAsync<int>("Test"));
		}

		[TestMethod]
		public async Task ShouldNotMapIfMapRejectsItself() {
			// Not awaited
			{ 
				// CanMap returns true because the map does exist, even if it will fail
				Assert.IsTrue(await _mapper.CanMapAsyncNew<float, double>());

				var exc = await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<double>(1f));
				Assert.AreEqual(typeof(float), exc.From);
				Assert.AreEqual(typeof(double), exc.To);
			}

			// Awaited
			{
				// CanMap returns true because the map does exist, even if it will fail
				Assert.IsTrue(await _mapper.CanMapAsyncNew<double, float>());

				var exc = await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<float>(1d));
				Assert.AreEqual(typeof(double), exc.From);
				Assert.AreEqual(typeof(float), exc.To);
			}
		}
	}
}
