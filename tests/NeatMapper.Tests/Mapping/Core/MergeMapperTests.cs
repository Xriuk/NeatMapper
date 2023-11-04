using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class MergeMapperTests {
		public class Maps :
#if NET7_0_OR_GREATER
			IMergeMapStatic<int, string>,
			IMergeMapStatic<Price, decimal>,
			IMergeMapStatic<Price, PriceFloat>,
			IMergeMapStatic<Product, ProductDto>,
			IMergeMapStatic<LimitedProduct, LimitedProductDto>,
			IMergeMapStatic<Category, int?>,
			IMergeMapStatic<Category, CategoryDto>,
			IMatchMapStatic<Category, CategoryDto>,
			IMergeMapStatic<float, string>,
			IMergeMapStatic<string, ClassWithoutParameterlessConstructor>,
			IMergeMapStatic<decimal, Price>,
			IMergeMapStatic<float, Price>,
			IMergeMapStatic<float, int>,
			IMergeMapStatic<string, KeyValuePair<string, int>>,
			IMergeMapStatic<string, int>,
			IMergeMapStatic<decimal, int>,
			IMatchMapStatic<decimal, int>,
			IMergeMapStatic<decimal, string>,
			IMergeMapStatic<int, char>,
			IMergeMapStatic<char, float>
#else
			IMergeMap<int, string>,
			IMergeMap<Price, decimal>,
			IMergeMap<Price, PriceFloat>,
			IMergeMap<Product, ProductDto>,
			IMergeMap<LimitedProduct, LimitedProductDto>,
			IMergeMap<Category, int?>,
			IMergeMap<Category, CategoryDto>,
			IMatchMap<Category, CategoryDto>,
			IMergeMap<float, string>,
			IMergeMap<string, ClassWithoutParameterlessConstructor>,
			IMergeMap<decimal, Price>,
			IMergeMap<float, Price>,
			IMergeMap<float, int>,
			IMergeMap<string, KeyValuePair<string, int>>,
			IMergeMap<string, int>,
			IMergeMap<decimal, int>,
			IMatchMap<decimal, int>,
			IMergeMap<decimal, string>,
			IMergeMap<int, char>,
			IMergeMap<char, float>
#endif
			{

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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return (source * 2).ToString();
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
				if (source != null) {
					if (destination == null)
						destination = new PriceFloat();
					destination.Amount = (float)source.Amount;
					destination.Currency = source.Currency;
				}
				return destination;
			}

			// Nested NewMap
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
					destination.Categories = source.Categories?.Select(s => context.Mapper.Map<int?>(s)).Where(i => i != null).Cast<int>().ToList() ?? new List<int>();
				}
				return destination;
			}

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
				if (source != null) {
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = source.Categories?.Select(s => context.Mapper.Map<int?>(s)).Where(i => i != null).Cast<int>().ToList() ?? new List<int>();
					destination.Copies = source.Copies;
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			int?
#if NET7_0_OR_GREATER
				IMergeMapStatic<Category, int?>
#else
				IMergeMap<Category, int?>
#endif
				.Map(Category source, int? destination, MappingContext context) {
				return source?.Id ?? destination;
			}

			// Nested MergeMap
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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				if (source != null) {
					if (destination == null)
						destination = new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = context.Mapper.Map(source.Parent, destination.Parent);
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
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<float, string>
#else
				IMergeMap<float, string>
#endif
				.Map(float source, string destination, MappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return (source * 3).ToString();
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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
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

#if NET7_0_OR_GREATER
			static
#endif
			KeyValuePair<string, int>
#if NET7_0_OR_GREATER
				IMergeMapStatic<string, KeyValuePair<string, int>>
#else
				IMergeMap<string, KeyValuePair<string, int>>
#endif
				.Map(string source, KeyValuePair<string, int> destination, MappingContext context) {
				return new KeyValuePair<string, int>(source, source.Length);
			}

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<string, int>
#else
				IMergeMap<string, int>
#endif
				.Map(string source, int destination, MappingContext context) {
				return source?.Length ?? -1;
			}

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
				return 0;
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
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<decimal, string>
#else
				IMergeMap<decimal, string>
#endif
				.Map(decimal source, string destination, MappingContext context) {
				return "MergeMap";
			}

#if NET7_0_OR_GREATER
			static
#endif
			char
#if NET7_0_OR_GREATER
				IMergeMapStatic<int, char>
#else
				IMergeMap<int, char>
#endif
				.Map(int source, char destination, MappingContext context) {
				return (char)source;
			}


#if NET7_0_OR_GREATER
			static
#endif
			float
#if NET7_0_OR_GREATER
				IMergeMapStatic<char, float>
#else
				IMergeMap<char, float>
#endif
				.Map(char source, float destination, MappingContext context) {
				return (float)source;
			}
		}

		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new MergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps) }
			});
		}


		[TestMethod]
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapMerge<int, string>());

			Assert.AreEqual("4", _mapper.Map(2, ""));
			Assert.AreEqual("-6", _mapper.Map(-3, ""));
			Assert.AreEqual("0", _mapper.Map(0, ""));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			{
				Assert.IsTrue(_mapper.CanMapMerge<Price, decimal>());

				Assert.AreEqual(20.00m, _mapper.Map(new Price {
					Amount = 20.00m,
					Currency = "EUR"
				}, 21m));
			}

			Assert.IsTrue(_mapper.CanMapMerge<Price, PriceFloat>());

			// Null destination
			{
				var result = _mapper.Map(new Price {
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
				var result = _mapper.Map(new Price {
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
		public void ShouldMapChildClassesAsParents() {
			Assert.IsTrue(_mapper.CanMapMerge<Product, ProductDto>());

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
			Assert.IsFalse(_mapper.CanMapMerge<bool, int>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(false, 0));
		}

		[TestMethod]
		public void ShouldMapNested() {
			{
				var destination = new ProductDto();
				var result = _mapper.Map(new Product {
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
				var result = _mapper.Map<Category, CategoryDto>(new Category {
					Id = 2
				}, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(2, result.Id);
				Assert.IsNull(result.Parent);
			}

			{
				var destination = new CategoryDto();
				var result = _mapper.Map<Category, CategoryDto>(new Category {
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
				var result = _mapper.Map<Category, CategoryDto>(new Category {
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
				var result = _mapper.Map<Category, CategoryDto>(new Category {
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
		public void ShouldFallbackFromNewMapToMergeMapAndForwardOptions() {
			Assert.IsTrue(_mapper.CanMapNew<float, string>());

			// No Options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				Assert.AreEqual("6", _mapper.Map<string>(2f));

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (without matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				_mapper.Map<string>(2f, opts);

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, (s, d, c) => false);
				_mapper.Map<string>(2f, opts, merge);

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldNotFallbackFromNewMapToMergeMapIfCannotCreateDestination() {
			Assert.IsFalse(_mapper.CanMapNew<string, ClassWithoutParameterlessConstructor>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<ClassWithoutParameterlessConstructor>(""));
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
					var result = _mapper.Map(20m, (Price)null);
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
					var result = _mapper.Map(20f, (Price)null);
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
		public void ShouldMapWithAdditionalMaps() {
			var options = new CustomMergeAdditionalMapsOptions();
			options.AddMap<string, int>((s, d, _) => s?.Length ?? 0);
			var mapper = new MergeMapper(null, options);

			Assert.IsTrue(_mapper.CanMapMerge<string, int>());

			Assert.AreEqual(4, mapper.Map("Test", 2));
		}
	}
}
