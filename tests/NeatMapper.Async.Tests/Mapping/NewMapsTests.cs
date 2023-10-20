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
	public class NewMapsTests {
		public class TestOptions { }

		public class Maps :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<int, string>,
			IAsyncNewMapStatic<string, int>,
			IAsyncNewMapStatic<bool, string>,
			IAsyncMergeMapStatic<bool, string>,
			IAsyncMergeMapStatic<float, string>,
			IAsyncNewMapStatic<Price, decimal>,
			IAsyncNewMapStatic<Price, PriceFloat>,
			IAsyncNewMapStatic<Category, int>,
			IAsyncNewMapStatic<Product, ProductDto>,
			IAsyncNewMapStatic<LimitedProduct, LimitedProductDto>,
			IAsyncNewMapStatic<string, KeyValuePair<string, int>>,
			IAsyncNewMapStatic<float, int>,
			IAsyncNewMapStatic<decimal, int>,
			IAsyncMergeMapStatic<string, ClassWithoutParameterlessConstructor>,
			IAsyncNewMapStatic<IEnumerable<decimal>, IList<string>>
#else
			IAsyncNewMap<int, string>,
			IAsyncNewMap<string, int>,
			IAsyncNewMap<bool, string>,
			IAsyncMergeMap<bool, string>,
			IAsyncMergeMap<float, string>,
			IAsyncNewMap<Price, decimal>,
			IAsyncNewMap<Price, PriceFloat>,
			IAsyncNewMap<Category, int>,
			IAsyncNewMap<Product, ProductDto>,
			IAsyncNewMap<LimitedProduct, LimitedProductDto>,
			IAsyncNewMap<string, KeyValuePair<string, int>>,
			IAsyncNewMap<float, int>,
			IAsyncNewMap<decimal, int>,
			IAsyncMergeMap<string, ClassWithoutParameterlessConstructor>,
			IAsyncNewMap<IEnumerable<decimal>, IList<string>>
#endif
			{

			public static TestOptions options;
			public static MergeCollectionsMappingOptions mergeOptions;

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
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult((source * 2).ToString());
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

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<bool, string>
#else
				IAsyncNewMap<bool, string>
#endif
				.MapAsync(bool source, AsyncMappingContext context) {
				return Task.FromResult("NewMap");
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<bool, string>
#else
				IAsyncMergeMap<bool, string>
#endif
				.MapAsync(bool source, string destination, AsyncMappingContext context) {
				return Task.FromResult("MergeMap");
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
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return Task.FromResult((source * 3).ToString());
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
					return Task.FromResult<PriceFloat>(null);
				else
					return Task.FromResult(new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					});
			}

#if NET7_0_OR_GREATER
				static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Category, int>
#else
					IAsyncNewMap<Category, int>
#endif
					.MapAsync(Category source, AsyncMappingContext context) {
				return Task.FromResult(source?.Id ?? 0);
			}

			// Nested NewMap
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
				if (source == null)
					return null;
				else {
					return new ProductDto {
						Code = source.Code,
						Categories = await context.Mapper.MapAsync<ICollection<int>>(source.Categories) ?? new List<int>()
					};
				}
			}

			// Nested MergeMap
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
					var categories = new List<int>();
					return new LimitedProductDto {
						Code = source.Code,
						Categories = await context.Mapper.MapAsync(source.Categories, categories) ?? new List<int>(),
						Copies = source.Copies
					};
				}
			}

#if NET7_0_OR_GREATER
			static
#endif
			async Task<KeyValuePair<string, int>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<string, KeyValuePair<string, int>>
#else
				IAsyncNewMap<string, KeyValuePair<string, int>>
#endif
				.MapAsync(string source, AsyncMappingContext context) {
				return new KeyValuePair<string, int>(source ?? "", await context.Mapper.MapAsync<string, int>(source));
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
			async Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<decimal, int>
#else
				IAsyncNewMap<decimal, int>
#endif
				.MapAsync(decimal source, AsyncMappingContext context) {
				await Task.Delay(1);
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

#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<string>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IEnumerable<decimal>, IList<string>>
#else
				IAsyncNewMap<IEnumerable<decimal>, IList<string>>
#endif
				.MapAsync(IEnumerable<decimal> source, AsyncMappingContext context) {
				return Task.FromResult<IList<string>>(source?.Select(s => (s * 10m).ToString()).ToList());
			}
		}

		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncMapper(new MapperConfigurationOptions {
				TypesToScan = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public async Task ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, await _mapper.MapAsync<string>(input));
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			Assert.AreEqual(20.00m, await _mapper.MapAsync<decimal>(new Price {
				Amount = 20.00m,
				Currency = "EUR"
			}));

			var result = await _mapper.MapAsync<PriceFloat>(new Price {
				Amount = 40.00m,
				Currency = "EUR"
			});
			Assert.IsNotNull(result);
			Assert.AreEqual(40f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
		}

		[TestMethod]
		public async Task ShouldMapChildClassAsParent() {
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
		public Task ShouldNotMapWithoutMap() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Category>(2));
		}

		[TestMethod]
		public async Task ShouldMapNested() {
			// NewMap
			{
				var result = await _mapper.MapAsync<Product, ProductDto>(new Product {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					}
				});
				Assert.IsNotNull(result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(1, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.Single());
			}

			// MergeMap
			{
				var result = await _mapper.MapAsync<LimitedProduct, LimitedProductDto>(new LimitedProduct {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					},
					Copies = 3
				});
				Assert.IsNotNull(result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(1, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.Single());
				Assert.AreEqual(3, result.Copies);
			}
		}

		[TestMethod]
		public async Task ShouldFallbackToMergeMapIfNewMapIsNotDefined() {
			// No Options
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				Assert.AreEqual("6", await _mapper.MapAsync<string>(2f));

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (without matcher)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				await _mapper.MapAsync<string>(2f, new[] { opts });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
				await _mapper.MapAsync<string>(2f, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.AreSame(merge, Maps.mergeOptions);
				Assert.IsNotNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public Task ShouldNotFallbackToMergeMapIfCannotCreateDestination() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<ClassWithoutParameterlessConstructor>(""));
		}

		[TestMethod]
		public async Task ShouldPreferNewMapIfBothAreDefined() {
			Assert.AreEqual("NewMap", await _mapper.MapAsync<string>(true));
		}

		[TestMethod]
		public async Task ShouldCatchExceptionsInMaps() {
			// Not awaited
			{ 
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int>(2f));
				Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
			}

			// Awaited
			{
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int>(2m));
				Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
			}
		}


		[TestMethod]
		public async Task ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				Maps.options = null;
				Maps.mergeOptions = null;
				var strings = await _mapper.MapAsync<string[]>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (no merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;
				var opts = new TestOptions();
				var strings = await _mapper.MapAsync<IList<string>>(new[] { 2, -3, 0 }, new[] { opts });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (merge with matcher)
			{
				Maps.options = null;
				Maps.mergeOptions = null;
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
				var strings = await _mapper.MapAsync<LinkedList<string>>(new[] { 2, -3, 0 }, new object[] { opts, merge });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));

				Assert.AreSame(opts, Maps.options);
				Assert.IsNotNull(Maps.mergeOptions);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.RemoveNotMatchedDestinationElements);
			}

			{
				var strings = await _mapper.MapAsync<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = await _mapper.MapAsync<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = await _mapper.MapAsync<CustomCollection<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}
		}

		[TestMethod]
		public Task ShouldNotMapCollectionsIfCannotCreateDestination() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CustomCollectionWithoutParameterlessConstructor<string>>(new[] { 2, -3, 0 }));
		}

		[TestMethod]
		public Task ShouldNotMapCollectionsWithoutMap() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<Category>>(new[] { 2 }));
		}

		[TestMethod]
		public async Task ShouldMapNullCollections() {
			Assert.IsNull(await _mapper.MapAsync<int[], string[]>(null));
		}

		[TestMethod]
		public async Task ShouldMapNullElementsInCollections() {
			var ints = await _mapper.MapAsync<int[]>(new[] { "A", "BBB", "C", "", null });

			Assert.IsNotNull(ints);
			Assert.AreEqual(5, ints.Length);
			Assert.AreEqual(1, ints[0]);
			Assert.AreEqual(3, ints[1]);
			Assert.AreEqual(1, ints[2]);
			Assert.AreEqual(0, ints[3]);
			Assert.AreEqual(-1, ints[4]);
		}

		[TestMethod]
		public async Task ShouldFallbackToMergeMapInCollections() {
			// No Options
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var result = await _mapper.MapAsync<IList<string>>(new[] { 2f });
				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("6", result[0]);

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (without matcher)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				await _mapper.MapAsync<IList<string>>(new[] { 2f }, new[] { opts });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
				await _mapper.MapAsync<IList<string>>(new[] { 2f }, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public Task ShouldNotFallbackToMergeMapInCollectionsIfCannotCreateElement() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}

		[TestMethod]
		public async Task ShouldMapCollectionsOfCollections() {
			// No options
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var strings = await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				});

				Assert.IsNotNull(strings);
				Assert.AreEqual(2, strings.Count);
				Assert.AreEqual(3, strings[0].Count());
				Assert.AreEqual(2, strings[1].Count());
				Assert.AreEqual("4", strings[0].ElementAt(0));
				Assert.AreEqual("-6", strings[0].ElementAt(1));
				Assert.AreEqual("0", strings[0].ElementAt(2));
				Assert.AreEqual("2", strings[1].ElementAt(0));
				Assert.AreEqual("4", strings[1].ElementAt(1));

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (no merge)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, new[] { opts });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (merge with matcher)
			{
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
				await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.IsNotNull(Maps.mergeOptions);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public Task ShouldNotMapMultidimensionalArrays() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2, 5 }
			}));
		}

		[TestMethod]
		public async Task ShouldPreferExplicitCollectionMaps() {
			var result = await _mapper.MapAsync<IEnumerable<decimal>, IList<string>>(new[] { 4m });
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("40", result[0]);
		}

		[TestMethod]
		public async Task ShouldCatchExceptionsInCollectionMaps() {
			// Not awaited
			{ 
				// Normal collections
				var exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<int>>(new[] { 2f }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// Nested collections
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<int>>>(new[] { new[] { 2f } }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
			}

			// Awaited
			{
				// Normal collections
				var exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<int>>(new[] { 2m }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// Nested collections
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<int>>>(new[] { new[] { 2m } }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
			}
		}

		[TestMethod]
		public async Task ShouldMapWithAdditionalMaps() {
			var options = new AsyncMapperOptions();
			options.AddNewMap<string, int>((s, _) => Task.FromResult(s?.Length ?? 0));
			var mapper = new AsyncMapper(options);

			Assert.AreEqual(4, await mapper.MapAsync<int>("Test"));
		}
	}
}
