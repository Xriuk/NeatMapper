using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	[TestClass]
	public class AsyncMergeMapperTests {
		public class Maps :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<int, string>,
			IAsyncMergeMapStatic<Price, decimal>,
			IAsyncMergeMapStatic<Price, PriceFloat>,
			IAsyncMergeMapStatic<Product, ProductDto>,
			IAsyncMergeMapStatic<LimitedProduct, LimitedProductDto>,
			IAsyncMergeMapStatic<Category, int?>,
			IAsyncMergeMapStatic<Category, CategoryDto>,
			IMatchMapStatic<Category, CategoryDto>,
			IAsyncMergeMapStatic<float, string>,
			IAsyncMergeMapStatic<string, ClassWithoutParameterlessConstructor>,
			IAsyncMergeMapStatic<decimal, Price>,
			IAsyncMergeMapStatic<float, Price>,
			IAsyncMergeMapStatic<float, int>,
			IAsyncMergeMapStatic<float, decimal>,
			IAsyncMergeMapStatic<string, KeyValuePair<string, int>>,
			IAsyncMergeMapStatic<string, int>,
			IAsyncMergeMapStatic<decimal, int>,
			IMatchMapStatic<decimal, int>,
			IAsyncMergeMapStatic<decimal, string>,
			IAsyncMergeMapStatic<int, float>,
			IAsyncMergeMapStatic<int, char>,
			IAsyncMergeMapStatic<char, float>
#else
			IAsyncMergeMap<int, string>,
			IAsyncMergeMap<Price, decimal>,
			IAsyncMergeMap<Price, PriceFloat>,
			IAsyncMergeMap<Product, ProductDto>,
			IAsyncMergeMap<LimitedProduct, LimitedProductDto>,
			IAsyncMergeMap<Category, int?>,
			IAsyncMergeMap<Category, CategoryDto>,
			IMatchMap<Category, CategoryDto>,
			IAsyncMergeMap<float, string>,
			IAsyncMergeMap<string, ClassWithoutParameterlessConstructor>,
			IAsyncMergeMap<decimal, Price>,
			IAsyncMergeMap<float, Price>,
			IAsyncMergeMap<float, int>,
			IAsyncMergeMap<float, decimal>,
			IAsyncMergeMap<string, KeyValuePair<string, int>>,
			IAsyncMergeMap<string, int>,
			IAsyncMergeMap<decimal, int>,
			IMatchMap<decimal, int>,
			IAsyncMergeMap<decimal, string>,
			IAsyncMergeMap<int, float>,
			IAsyncMergeMap<int, char>,
			IAsyncMergeMap<char, float>
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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult((source * 2).ToString());
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

			// Nested NewMap
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
					var tasks = source.Categories?.Select(s => context.Mapper.MapAsync<int?>(s));
					if(tasks != null)
						await Task.WhenAll(tasks);
					destination.Categories = tasks?.Select(t => t.Result).Where(i => i != null).Cast<int>().ToList() ?? new List<int>();
				}
				return destination;
			}

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
					var tasks = source.Categories?.Select(s => context.Mapper.MapAsync<int?>(s));
					if(tasks != null)
						await Task.WhenAll(tasks);
					destination.Categories = tasks?.Select(t => t.Result).Where(i => i != null).Cast<int>().ToList() ?? new List<int>();
					destination.Copies = source.Copies;
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<int?>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Category, int?>
#else
				IAsyncMergeMap<Category, int?>
#endif
				.MapAsync(Category source, int? destination, AsyncMappingContext context) {
				return Task.FromResult(source?.Id ?? destination);
			}

			// Nested MergeMap
#if NET7_0_OR_GREATER
			static
#endif
			async Task<CategoryDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Category, CategoryDto>
#else
				IAsyncMergeMap<Category, CategoryDto>
#endif
				.MapAsync(Category source, CategoryDto destination, AsyncMappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				if (source != null) {
					if (destination == null)
						destination = new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = await context.Mapper.MapAsync(source.Parent, destination.Parent);
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
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<float, string>
#else
				IAsyncMergeMap<float, string>
#endif
				.MapAsync(float source, string destination, AsyncMappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult((source * 3).ToString());
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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
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
			async Task<decimal>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<float, decimal>
#else
				IAsyncMergeMap<float, decimal>
#endif
				.MapAsync(float source, decimal destination, AsyncMappingContext context) {
				await Task.Delay(0);
				throw new NotImplementedException();
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<KeyValuePair<string, int>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<string, KeyValuePair<string, int>>
#else
				IAsyncMergeMap<string, KeyValuePair<string, int>>
#endif
				.MapAsync(string source, KeyValuePair<string, int> destination, AsyncMappingContext context) {
				return Task.FromResult(new KeyValuePair<string, int>(source, source.Length));
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<string, int>
#else
				IAsyncMergeMap<string, int>
#endif
				.MapAsync(string source, int destination, AsyncMappingContext context) {
				return Task.FromResult(source?.Length ?? -1);
			}

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
				return Task.FromResult(0);
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

			// Different map
#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<decimal, string>
#else
				IAsyncMergeMap<decimal, string>
#endif
				.MapAsync(decimal source, string destination, AsyncMappingContext context) {
				return Task.FromResult("MergeMap");
			}

			// "long"-running task
#if NET7_0_OR_GREATER
			static
#endif
			async Task<float>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<int, float>
#else
				IAsyncMergeMap<int, float>
#endif
				.MapAsync(int source, float destination, AsyncMappingContext context) {
				await Task.Delay(10);
				return (source * 2);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<char>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<int, char>
#else
				IAsyncMergeMap<int, char>
#endif
				.MapAsync(int source, char destination, AsyncMappingContext context) {
				return Task.FromResult((char)source);
			}


#if NET7_0_OR_GREATER
			static
#endif
			Task<float>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<char, float>
#else
				IAsyncMergeMap<char, float>
#endif
				.MapAsync(char source, float destination, AsyncMappingContext context) {
				return Task.FromResult((float)source);
			}
		}

		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		public async Task ShouldMapPrimitives() {
			Assert.IsTrue(await _mapper.CanMapAsyncMerge<int, string>());

			Assert.AreEqual("4", await _mapper.MapAsync(2, ""));
			Assert.AreEqual("-6", await _mapper.MapAsync(-3, ""));
			Assert.AreEqual("0", await _mapper.MapAsync(0, ""));
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			{
				Assert.IsTrue(await _mapper.CanMapAsyncMerge<Price, decimal>());

				Assert.AreEqual(20.00m, await _mapper.MapAsync(new Price {
					Amount = 20.00m,
					Currency = "EUR"
				}, 21m));
			}

			Assert.IsTrue(await _mapper.CanMapAsyncMerge<Price, PriceFloat>());

			// Null destination
			{
				var result = await _mapper.MapAsync(new Price {
					Amount = 40.00m,
					Currency = "EUR"
				}, (PriceFloat)null);
				Assert.IsNotNull(result);
				Assert.AreEqual(40f, result.Amount);
				Assert.AreEqual("EUR", result.Currency);
			}

			// Not null destination
			{ 
				var destination = new PriceFloat();
				var result = await _mapper.MapAsync(new Price {
					Amount = 40.00m,
					Currency = "EUR"
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(40f, result.Amount);
				Assert.AreEqual("EUR", result.Currency);
			}
		}

		[TestMethod]
		public async Task ShouldMapChildClassesAsParents() {
			Assert.IsTrue(await _mapper.CanMapAsyncMerge<Product, ProductDto>());

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
					}, (ProductDto)null);
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
		public async Task ShouldNotMapWithoutMap() {
			Assert.IsFalse(await _mapper.CanMapAsyncMerge<bool, int>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(false, 0));
		}

		[TestMethod]
		public async Task ShouldMapNested() {
			{
				var destination = new ProductDto();
				var result = await _mapper.MapAsync(new Product {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					}
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(1, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.Single());
			}

			{
				var destination = new CategoryDto();
				var result = await _mapper.MapAsync<Category, CategoryDto>(new Category {
					Id = 2
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(2, result.Id);
				Assert.IsNull(result.Parent);
			}

			{
				var destination = new CategoryDto();
				var result = await _mapper.MapAsync<Category, CategoryDto>(new Category {
					Id = 2,
					Parent = new Category { Id = 3 }
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(2, result.Id);
				Assert.AreEqual(3, result.Parent);
			}

			{
				var destination = new CategoryDto {
					Parent = 4
				};
				var result = await _mapper.MapAsync<Category, CategoryDto>(new Category {
					Id = 2
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(2, result.Id);
				Assert.AreEqual(4, result.Parent);
			}

			{
				var destination = new CategoryDto{
					Parent = 4
				};
				var result = await _mapper.MapAsync<Category, CategoryDto>(new Category {
					Id = 2,
					Parent = new Category { Id = 3 }
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(2, result.Id);
				Assert.AreEqual(3, result.Parent);
			}
		}

		[TestMethod]
		public async Task ShouldFallbackFromNewMapToMergeMapAndForwardOptions() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<float, string>());

			// No Options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				Assert.AreEqual("6", await _mapper.MapAsync<string>(2f));

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (without matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				await _mapper.MapAsync<string>(2f, new[]{ opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				await _mapper.MapAsync<string>(2f, new object[]{ opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public async Task ShouldNotFallbackFromNewMapToMergeMapIfCannotCreateDestination() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<string, ClassWithoutParameterlessConstructor>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<ClassWithoutParameterlessConstructor>(""));
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
					var result = await _mapper.MapAsync(20m, (Price)null);
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
					var result = await _mapper.MapAsync(20f, (Price)null);
					Assert.IsNotNull(result);
				}
			}
		}

		[TestMethod]
		public async Task ShouldCatchExceptionsInMaps() {
			// Not awaited
			var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(2f, 2));
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));

			// Awaited
			exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(2f, 2m));
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
		}

		[TestMethod]
		public async Task ShouldMapWithAdditionalMaps() {
			var options = new CustomAsyncMergeAdditionalMapsOptions();
			options.AddMap<string, int>((s, d, _) => Task.FromResult(s?.Length ?? 0));
			var mapper = new AsyncMergeMapper(null, options);

			Assert.IsTrue(await mapper.CanMapAsyncMerge<string, int>());

			Assert.AreEqual(4, await mapper.MapAsync("Test", 2));
		}
	}
}
