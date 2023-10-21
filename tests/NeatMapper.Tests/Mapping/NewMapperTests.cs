using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests {
	[TestClass]
	public class NewMapperTests {
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
						Categories = source.Categories?.Select(c => context.Mapper.Map<int>(c)).ToList() ?? new List<int>()
					};
				}
			}

			// Nested NewMap
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
						Categories = source.Categories?.Select((s, d) => context.Mapper.Map<int>(s)).ToList() ?? new List<int>(),
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
			_mapper = new NewMapper(new CustomMapsOptions {
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

		// DEV: find where to move
		/*[TestMethod]
		public void ShouldPreferNewMapIfBothAreDefined() {
			Assert.AreEqual("NewMap", _mapper.Map<string>(true));
		}*/

		[TestMethod]
		public void ShouldCatchExceptionsInMaps() {
			var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int>(2f));
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
		}

		[TestMethod]
		public void ShouldMapWithAdditionalMaps() {
			var options = new CustomNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => s?.Length ?? 0);
			var mapper = new NewMapper(null, options);

			Assert.AreEqual(4, mapper.Map<int>("Test"));
		}
	}
}
