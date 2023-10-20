using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMapsTests {
		public class TestOptions { }

		public class Maps :
#if NET7_0_OR_GREATER
			INewMapStatic<int, string>,
			INewMapStatic<string, int>,
			INewMapStatic<bool, string>,
			IMergeMapStatic<bool, string>,
			IMergeMapStatic<float, string>,
			INewMapStatic<Price, decimal>,
			INewMapStatic<Price, PriceFloat>,
			INewMapStatic<Category, int>,
			INewMapStatic<Product, ProductDto>,
			INewMapStatic<LimitedProduct, LimitedProductDto>,
			INewMapStatic<string, KeyValuePair<string, int>>,
			INewMapStatic<float, int>,
			IMergeMapStatic<string, ClassWithoutParameterlessConstructor>,
			INewMapStatic<IEnumerable<decimal>, IList<string>>
#else
			INewMap<int, string>,
			INewMap<string, int>,
			INewMap<bool, string>,
			IMergeMap<bool, string>,
			IMergeMap<float, string>,
			INewMap<Price, decimal>,
			INewMap<Price, PriceFloat>,
			INewMap<Category, int>,
			INewMap<Product, ProductDto>,
			INewMap<LimitedProduct, LimitedProductDto>,
			INewMap<string, KeyValuePair<string, int>>,
			INewMap<float, int>,
			IMergeMap<string, ClassWithoutParameterlessConstructor>,
			INewMap<IEnumerable<decimal>, IList<string>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				INewMapStatic<int, string>
#else
				INewMap<int, string>
#endif
				.Map(int source, MappingContext context) {
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return (source * 2).ToString();
			}

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<string, int>
#else
				INewMap<string, int>
#endif
				.Map(string source, MappingContext context) {
				return source?.Length ?? -1;
			}

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				INewMapStatic<bool, string>
#else
				INewMap<bool, string>
#endif
				.Map(bool source, MappingContext context) {
				return "NewMap";
			}

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<bool, string>
#else
				IMergeMap<bool, string>
#endif
				.Map(bool source, string destination, MappingContext context) {
				return "MergeMap";
			}

			// MergeMap + options
			public static TestOptions options;
			public static MergeCollectionsMappingOptions mergeOptions;
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
				options = context.MappingOptions.GetOptions<TestOptions>();
				mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return (source * 3).ToString();
			}

#if NET7_0_OR_GREATER
			static
#endif
			decimal
#if NET7_0_OR_GREATER
				INewMapStatic<Price, decimal>
#else
				INewMap<Price, decimal>
#endif
				.Map(Price source, MappingContext context) {
				return source?.Amount ?? 0m;
			}

#if NET7_0_OR_GREATER
			static
#endif
			PriceFloat
#if NET7_0_OR_GREATER
				INewMapStatic<Price, PriceFloat>
#else
				INewMap<Price, PriceFloat>
#endif
				.Map(Price source, MappingContext context) {
				if(source == null)
					return null;
				else
					return new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					};
			}

#if NET7_0_OR_GREATER
				static
#endif
		int
#if NET7_0_OR_GREATER
				INewMapStatic<Category, int>
#else
				INewMap<Category, int>
#endif
				.Map(Category source, MappingContext context) {
				return source?.Id ?? 0;
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			ProductDto
#if NET7_0_OR_GREATER
				INewMapStatic<Product, ProductDto>
#else
				INewMap<Product, ProductDto>
#endif
				.Map(Product source, MappingContext context) {
				if(source == null)
					return null;
				else {
					return new ProductDto {
						Code = source.Code,
						Categories = context.Mapper.Map<ICollection<int>>(source.Categories) ?? new List<int>()
					};
				}
			}

			// Nested MergeMap
#if NET7_0_OR_GREATER
			static
#endif
			LimitedProductDto
#if NET7_0_OR_GREATER
				INewMapStatic<LimitedProduct, LimitedProductDto>
#else
				INewMap<LimitedProduct, LimitedProductDto>
#endif
				.Map(LimitedProduct source, MappingContext context) {
				if(source == null)
					return null;
				else {
					var categories = new List<int>();
					return new LimitedProductDto {
						Code = source.Code,
						Categories = context.Mapper.Map(source.Categories, categories) ?? new List<int>(),
						Copies = source.Copies
					};
				}
			}

#if NET7_0_OR_GREATER
			static
#endif
			KeyValuePair<string, int>
#if NET7_0_OR_GREATER
				INewMapStatic<string, KeyValuePair<string, int>>
#else
				INewMap<string, KeyValuePair<string, int>>
#endif
				.Map(string source, MappingContext context) {
				return new KeyValuePair<string, int>(source ?? "", context.Mapper.Map<string, int>(source));
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<float, int>
#else
				INewMap<float, int>
#endif
				.Map(float source, MappingContext context) {
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

#if NET7_0_OR_GREATER
			static
#endif
			IList<string>
#if NET7_0_OR_GREATER
				INewMapStatic<IEnumerable<decimal>, IList<string>>
#else
				INewMap<IEnumerable<decimal>, IList<string>>
#endif
				.Map(IEnumerable<decimal> source, MappingContext context) {
				return source?.Select(s => (s * 10m).ToString()).ToList();
			}
		}

		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper( new MapperConfigurationOptions {
				TypesToScan = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public void ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, _mapper.Map<string>(input));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			Assert.AreEqual(20.00m, _mapper.Map<decimal>(new Price {
				Amount = 20.00m,
				Currency = "EUR"
			}));

			var result = _mapper.Map<PriceFloat>(new Price {
				Amount = 40.00m,
				Currency = "EUR"
			});
			Assert.IsNotNull(result);
			Assert.AreEqual(40f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
		}

		[TestMethod]
		public void ShouldMapChildClassAsParent() {
			var result = _mapper.Map<Product, ProductDto>(new LimitedProduct {
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
		public void ShouldNotMapWithoutMap() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<Category>(2));
		}

		[TestMethod]
		public void ShouldMapNested() {
			// NewMap
			{ 
				var result = _mapper.Map<Product, ProductDto>(new Product {
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
				var result = _mapper.Map<LimitedProduct, LimitedProductDto>(new LimitedProduct {
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
		public void ShouldFallbackToMergeMapIfNewMapIsNotDefined() {
			// No Options
			{ 
				Maps.options = null;
				Maps.mergeOptions = null;

				Assert.AreEqual("6", _mapper.Map<string>(2f));

				Assert.IsNull(Maps.options);
				Assert.IsNull(Maps.mergeOptions);
			}

			// Options (without matcher)
			{ 
				Maps.options = null;
				Maps.mergeOptions = null;

				var opts = new TestOptions();
				_mapper.Map<string>(2f, opts);

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
				_mapper.Map<string>(2f, opts, merge);

				Assert.AreSame(opts, Maps.options);
				Assert.AreSame(merge, Maps.mergeOptions);
				Assert.IsNotNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldNotFallbackToMergeMapIfCannotCreateDestination() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<ClassWithoutParameterlessConstructor>(""));
		}

		[TestMethod]
		public void ShouldPreferNewMapIfBothAreDefined() {
			Assert.AreEqual("NewMap", _mapper.Map<string>(true));
		}

		[TestMethod]
		public void ShouldCatchExceptionsInMaps() {
			var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int>(2f));
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
		}


		[TestMethod]
		public void ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				Maps.options = null;
				Maps.mergeOptions = null;
				var strings = _mapper.Map<string[]>(new[] { 2, -3, 0 });

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
				var strings = _mapper.Map<IList<string>>(new[] { 2, -3, 0 }, opts);

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
				var strings = _mapper.Map<LinkedList<string>>(new[] { 2, -3, 0 }, opts, merge);

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
				var strings = _mapper.Map<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = _mapper.Map<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = _mapper.Map<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				var strings = _mapper.Map<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = _mapper.Map<CustomCollection<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}
		}

		[TestMethod]
		public void ShouldNotMapCollectionsIfCannotCreateDestination() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<CustomCollectionWithoutParameterlessConstructor<string>>(new[] { 2, -3, 0 }));
		}

		[TestMethod]
		public void ShouldNotMapCollectionsWithoutMap() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<Category>>(new[] { 2 }));
		}

		[TestMethod]
		public void ShouldMapNullCollections() {
			Assert.IsNull(_mapper.Map<int[], string[]>(null));
		}

		[TestMethod]
		public void ShouldMapNullElementsInCollections() {
			var ints = _mapper.Map<int[]>(new[] { "A", "BBB", "C", "", null });

			Assert.IsNotNull(ints);
			Assert.AreEqual(5, ints.Length);
			Assert.AreEqual(1, ints[0]);
			Assert.AreEqual(3, ints[1]);
			Assert.AreEqual(1, ints[2]);
			Assert.AreEqual(0, ints[3]);
			Assert.AreEqual(-1, ints[4]);
		}

		[TestMethod]
		public void ShouldFallbackToMergeMapInCollections() {
			// No Options
			{ 
				Maps.options = null;
				Maps.mergeOptions = null;

				var result = _mapper.Map<IList<string>>(new[] { 2f });
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
				_mapper.Map<IList<string>>(new[] { 2f }, new[]{ opts });

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
				_mapper.Map<IList<string>>(new[] { 2f }, new object[] { opts, merge });

				Assert.AreSame(opts, Maps.options);
				Assert.AreNotSame(merge, Maps.mergeOptions);
				Assert.IsNull(Maps.mergeOptions.Matcher);
				Assert.IsFalse(Maps.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldNotFallbackToMergeMapInCollectionsIfCannotCreateElement() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollections() {
			// No options
			{ 
				Maps.options = null;
				Maps.mergeOptions = null;

				var strings = _mapper.Map<IList<IEnumerable<string>>>(new[] {
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
				_mapper.Map<IList<IEnumerable<string>>>(new[] {
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
				_mapper.Map<IList<IEnumerable<string>>>(new[] {
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
		public void ShouldNotMapMultidimensionalArrays() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2, 5 }
			}));
		}

		[TestMethod]
		public void ShouldPreferExplicitCollectionMaps() {
			var result = _mapper.Map<IEnumerable<decimal>, IList<string>>(new[] { 4m });
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("40", result[0]);
		}

		[TestMethod]
		public void ShouldCatchExceptionsInCollectionMaps() {
			// Normal collections
			var exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map<IEnumerable<int>>(new[] { 2f }));
			Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
			Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

			// Nested collections
			exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map<IEnumerable<IEnumerable<int>>>(new[]{ new[] { 2f } }));
			Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
			Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
			Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
		}

		[TestMethod]
		public void ShouldMapWithAdditionalMaps() {
			var options = new CustomMapperOptions();
			options.AddNewMap<string, int>((s, _) => s?.Length ?? 0);
			var mapper = new Mapper(options);

			Assert.AreEqual(4, mapper.Map<int>("Test"));
		}
	}
}
