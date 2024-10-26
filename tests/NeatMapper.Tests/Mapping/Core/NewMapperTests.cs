using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMapperTests {
		public class Maps :
#if NET7_0_OR_GREATER
			INewMapStatic<int, string>,
			INewMapStatic<Price, decimal>,
			INewMapStatic<Price, PriceFloat>,
			INewMapStatic<Product, ProductDto>,
			INewMapStatic<LimitedProduct, LimitedProductDto>,
			INewMapStatic<Category, int?>,
			INewMapStatic<Category, CategoryDto>,
			INewMapStatic<float, string>,
			INewMapStatic<string, ClassWithoutParameterlessConstructor>,
			INewMapStatic<decimal, Price>,
			INewMapStatic<float, Price>,
			INewMapStatic<float, int>,
			INewMapStatic<string, KeyValuePair<string, int>>,
			INewMapStatic<string, int>,
			INewMapStatic<decimal, int>,
			INewMapStatic<decimal, string>,
			INewMapStatic<int, char>,
			INewMapStatic<char, float>,
			INewMapStatic<decimal, float>,
			INewMapStatic<decimal, bool>,
			INewMapStatic<float, double>
#else
			INewMap<int, string>,
			INewMap<Price, decimal>,
			INewMap<Price, PriceFloat>,
			INewMap<Product, ProductDto>,
			INewMap<LimitedProduct, LimitedProductDto>,
			INewMap<Category, int?>,
			INewMap<Category, CategoryDto>,
			INewMap<float, string>,
			INewMap<string, ClassWithoutParameterlessConstructor>,
			INewMap<decimal, Price>,
			INewMap<float, Price>,
			INewMap<float, int>,
			INewMap<string, KeyValuePair<string, int>>,
			INewMap<string, int>,
			INewMap<decimal, int>,
			INewMap<decimal, string>,
			INewMap<int, char>,
			INewMap<char, float>,
			INewMap<decimal, float>,
			INewMap<decimal, bool>,
			INewMap<float, double>
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

				MappingOptionsUtils.context = context;
				MappingOptionsUtils.contexts.Add(context);
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return (source * 2).ToString();
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
				if (source == null)
					return null;
				else { 
					return new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					};
				}
			}

			// Nested NewMap
			public static MappingOptions productOptions;
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

				productOptions = context.MappingOptions;
				if (source == null)
					return null;
				else {
					return new ProductDto {
						Code = source.Code,
						Categories = source.Categories?.Select(c => context.Mapper.Map<int?>(c)).Where(i => i != null).Cast<int>().ToList() ?? new List<int>()
					};
				}
			}

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

				if (source == null)
					return null;
				else {
					return new LimitedProductDto {
						Code = source.Code,
						Categories = source.Categories?.Select(s => context.Mapper.Map<int?>(s)).Where(i => i != null).Cast<int>().ToList() ?? new List<int>(),
						Copies = source.Copies
					};
				}
			}

			public static List<MappingOptions> categoryOptions = new List<MappingOptions>();
#if NET7_0_OR_GREATER
			static
#endif
			int?
#if NET7_0_OR_GREATER
				INewMapStatic<Category, int?>
#else
				INewMap<Category, int?>
#endif
			.Map(Category source, MappingContext context) {

				categoryOptions.Add(context.MappingOptions);
				return source?.Id;
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			CategoryDto
#if NET7_0_OR_GREATER
				INewMapStatic<Category, CategoryDto>
#else
				INewMap<Category, CategoryDto>
#endif
				.Map(Category source, MappingContext context) {

				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();

				if (source == null)
					return null;
				else {
					return new CategoryDto {
						Id = source.Id,
						Parent = context.Mapper.Map<Category, int?>(source.Parent)
					};
				}
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
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return (source * 3).ToString();
			}

#if NET7_0_OR_GREATER
			static
#endif
			ClassWithoutParameterlessConstructor
#if NET7_0_OR_GREATER
				INewMapStatic<string, ClassWithoutParameterlessConstructor>
#else
				INewMap<string, ClassWithoutParameterlessConstructor>
#endif
				.Map(string source, MappingContext context) {
				return new ClassWithoutParameterlessConstructor("");
			}

#if NET7_0_OR_GREATER
			static
#endif
			Price
#if NET7_0_OR_GREATER
				INewMapStatic<decimal, Price>
#else
				INewMap<decimal, Price>
#endif
				.Map(decimal source, MappingContext context) {

				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				return new Price {
					Amount = source,
					Currency = "EUR"
				};
			}

#if NET7_0_OR_GREATER
			static
#endif
			Price
#if NET7_0_OR_GREATER
				INewMapStatic<float, Price>
#else
				INewMap<float, Price>
#endif
				.Map(float source, MappingContext context) {

				return new Price {
					Amount = (decimal)source,
					Currency = "EUR"
				};
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
			KeyValuePair<string, int>
#if NET7_0_OR_GREATER
				INewMapStatic<string, KeyValuePair<string, int>>
#else
				INewMap<string, KeyValuePair<string, int>>
#endif
				.Map(string source, MappingContext context) {

				return new KeyValuePair<string, int>(source, source.Length);
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

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<decimal, int>
#else
				INewMap<decimal, int>
#endif
				.Map(decimal source, MappingContext context) {

				throw new NotImplementedException();
			}

			// Different map
#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				INewMapStatic<decimal, string>
#else
				INewMap<decimal, string>
#endif
				.Map(decimal source, MappingContext context) {

				return "NewMap";
			}

#if NET7_0_OR_GREATER
			static
#endif
			char
#if NET7_0_OR_GREATER
				INewMapStatic<int, char>
#else
				INewMap<int, char>
#endif
				.Map(int source, MappingContext context) {

				return (char)source;
			}


#if NET7_0_OR_GREATER
			static
#endif
			float
#if NET7_0_OR_GREATER
				INewMapStatic<char, float>
#else
				INewMap<char, float>
#endif
				.Map(char source, MappingContext context) {

				return (float)source;
			}

			// Throws task canceled
#if NET7_0_OR_GREATER
			static
#endif
			float
#if NET7_0_OR_GREATER
				INewMapStatic<decimal, float>
#else
				INewMap<decimal, float>
#endif
				.Map(decimal source, MappingContext context) {

				throw new TaskCanceledException();
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				INewMapStatic<decimal, bool>
#else
				INewMap<decimal, bool>
#endif
				.Map(decimal source, MappingContext context) {
				return true;
			}

			// Rejects itself
#if NET7_0_OR_GREATER
			static
#endif
			double
#if NET7_0_OR_GREATER
				INewMapStatic<float, double>
#else
				INewMap<float, double>
#endif
				.Map(float source, MappingContext context) {
				
				throw new MapNotFoundException((typeof(float), typeof(double)));
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
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapNew<int, string>());
			Assert.IsTrue(_mapper.CanMapNew(typeof(int), typeof(string)));

			MappingOptionsUtils.context = null;
			Assert.AreEqual("4", _mapper.Map<string>(2));
			Assert.IsNull(MappingOptionsUtils.context.MappingOptions.GetOptions<NestedMappingContext>());
			Assert.AreEqual("-6", (string)_mapper.Map(-3, typeof(int), typeof(string)));
			Assert.AreEqual("0", _mapper.Map<int, string>(0));

			// Factories should share the same context
			using(var factory = _mapper.MapNewFactory<int, string>()) { 
				MappingOptionsUtils.context = null;
				Assert.AreEqual("4", factory.Invoke(2));
				var context1 = MappingOptionsUtils.context;
				Assert.IsNotNull(context1);
				MappingOptionsUtils.context = null;
				Assert.AreEqual("-6", factory.Invoke(-3));
				var context2 = MappingOptionsUtils.context;
				Assert.IsNotNull(context2);
				Assert.AreSame(context1, context2);
			}
		}

		[TestMethod]
		public void ShouldMapClasses() {
			{
				Assert.IsTrue(_mapper.CanMapNew<Price, decimal>());

				Assert.AreEqual(20.00m, _mapper.Map<decimal>(new Price {
					Amount = 20.00m,
					Currency = "EUR"
				}));

				using(var factory = _mapper.MapNewFactory<Price, decimal>()) { 
					Assert.AreEqual(20.00m, factory.Invoke(new Price {
						Amount = 20.00m,
						Currency = "EUR"
					}));
				}
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<Price, PriceFloat>());

				var result = _mapper.Map<PriceFloat>(new Price {
					Amount = 40.00m,
					Currency = "EUR"
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(40f, result.Amount);
				Assert.AreEqual("EUR", result.Currency);

				using(var factory = _mapper.MapNewFactory<Price, PriceFloat>()) { 
					var result2 = factory.Invoke(new Price {
						Amount = 40.00m,
						Currency = "EUR"
					});
					Assert.IsNotNull(result2);
					Assert.AreEqual(40f, result2.Amount);
					Assert.AreEqual("EUR", result2.Currency);
				}
			}
		}

		[TestMethod]
		public void ShouldMapChildClassAsParent() {
			Assert.IsTrue(_mapper.CanMapNew<Product, ProductDto>());

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
			Assert.IsFalse(_mapper.CanMapNew<bool, int>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(false));

			TestUtils.AssertMapNotFound(() => _mapper.MapNewFactory<bool, int>());
		}

		[TestMethod]
		public void ShouldMapNested() {
			{ 
				// Normal
				Maps.productOptions = null;
				Maps.categoryOptions.Clear();
				var result = _mapper.Map<Product, ProductDto>(new Product {
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

				Assert.IsNull(Maps.productOptions.GetOptions<NestedMappingContext>());
				// Should use same context for nested maps
				Assert.AreEqual(2, Maps.categoryOptions.Count);
				Assert.AreEqual(1, Maps.categoryOptions.Distinct().Count());
				Assert.IsTrue(Maps.categoryOptions.All(o => o.GetOptions<NestedMappingContext>() != null));


				// Factory
				Maps.productOptions = null;
				Maps.categoryOptions.Clear();
				using (var factory = _mapper.MapNewFactory<Product, ProductDto>()) { 
					var result2 = factory.Invoke(new Product {
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

					Assert.IsNull(Maps.productOptions.GetOptions<NestedMappingContext>());
					// Should use same context for nested maps
					Assert.AreEqual(2, Maps.categoryOptions.Count);
					Assert.AreEqual(1, Maps.categoryOptions.Distinct().Count());
					Assert.IsNotNull(Maps.categoryOptions.First().GetOptions<NestedMappingContext>());
				}
			}

			{ 
				var result = _mapper.Map<Category, CategoryDto>(new Category {
					Id = 2
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Id);
				Assert.IsNull(result.Parent);
			}

			{
				var result = _mapper.Map<Category, CategoryDto>(new Category {
					Id = 2,
					Parent = new Category { Id = 3 }
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Id);
				Assert.AreEqual(3, result.Parent);
			}
		}

		[TestMethod]
		public void ShouldCatchExceptionsInMaps() {
			// Should wrap exceptions
			{ 
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int>(2f));
				Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
			}

			// Should not wrap TaskCanceledException
			Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map<float>(0m));
		}

		[TestMethod]
		public void ShouldMapWithAdditionalMaps() {
			var options = new CustomNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => s?.Length ?? 0);
			var mapper = new NewMapper(null, options);

			Assert.IsTrue(_mapper.CanMapNew<string, int>());

			Assert.AreEqual(4, mapper.Map<int>("Test"));
		}

		// DEV: factories should not affect each other, create 2 factories and invoke them in reverse order
	}
}
