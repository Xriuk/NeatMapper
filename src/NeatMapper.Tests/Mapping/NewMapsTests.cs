using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;
using System.Collections.ObjectModel;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMapsTests {
		public class Maps :
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
			INewMap<Product, string>,
			INewMap<Category, CategoryProducts>,
			INewMap<LimitedProduct, string>,
			INewMap<string, KeyValuePair<string, int>>,
			INewMap<float, int>,
			IMergeMap<string, ClassWithoutParameterlessConstructor>{

			static string? INewMap<int, string>.Map(int source, MappingContext context) {
				return (source * 2).ToString();
			}

			static int INewMap<string, int>.Map(string? source, MappingContext context) {
				return source?.Length ?? -1;
			}

			static string? INewMap<bool, string>.Map(bool source, MappingContext context) {
				return "NewMap";
			}

			static string? IMergeMap<bool, string>.Map(bool source, string? destination, MappingContext context) {
				return "MergeMap";
			}

			static string? IMergeMap<float, string>.Map(float source, string? destination, MappingContext context) {
				return (source * 3).ToString();
			}

			static decimal INewMap<Price, decimal>.Map(Price? source, MappingContext context) {
				return source?.Amount ?? 0m;
			}

			static PriceFloat? INewMap<Price, PriceFloat>.Map(Price? source, MappingContext context) {
				if(source == null)
					return null;
				else
					return new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					};
			}

			static int INewMap<Category, int>.Map(Category? source, MappingContext context) {
				return source?.Id ?? 0;
			}

			// Nested NewMap
			static ProductDto? INewMap<Product, ProductDto>.Map(Product? source, MappingContext context) {
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
			static LimitedProductDto? INewMap<LimitedProduct, LimitedProductDto>.Map(LimitedProduct? source, MappingContext context) {
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

			// Scope test
			public static IServiceProvider _sp1 = null!;
			static string? INewMap<Product, string>.Map(Product? source, MappingContext context) {
				_sp1 = context.ServiceProvider;
				return source?.Code;
			}

			public static IServiceProvider _sp2 = null!;
			static CategoryProducts? INewMap<Category, CategoryProducts>.Map(Category? source, MappingContext context) {
				_sp2 = context.ServiceProvider;
				if (source == null)
					return null;
				else {
					return new CategoryProducts {
						Id = source.Id,
						Products = context.Mapper.Map<ICollection<string>>(source.Products) ?? new List<string>()
					};
				}
			}

			public static List<IServiceProvider> _sp3 = new List<IServiceProvider>();
			static string? INewMap<LimitedProduct, string>.Map(LimitedProduct? source, MappingContext context) {
				_sp3.Add(context.ServiceProvider);
				return context.Mapper.Map<Product, string>(source);
			}

			static KeyValuePair<string, int> INewMap<string, KeyValuePair<string, int>>.Map(string? source, MappingContext context) {
				return new KeyValuePair<string, int>(source ?? "", context.Mapper.Map<string, int>(source));
			}

			// Throws exception
			static int INewMap<float, int>.Map(float source, MappingContext context) {
				throw new NotImplementedException();
			}

			static ClassWithoutParameterlessConstructor? IMergeMap<string, ClassWithoutParameterlessConstructor>.Map(string? source, ClassWithoutParameterlessConstructor? destination, MappingContext context) {
				return destination;
			}
		}

		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions{
				ScanTypes = new List<Type> { typeof(Maps) }
			}), new ServiceCollection().BuildServiceProvider());
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
		public void ShouldCreateNewScopeForEachMap() {
			Maps._sp1 = null!;

			_mapper.Map<string>(new Product {
				Code = "Test1"
			});

			Assert.IsNotNull(Maps._sp1);
			var service = Maps._sp1;

			_mapper.Map<string>(new Product {
				Code = "Test2"
			});

			Assert.IsNotNull(Maps._sp1);
			Assert.AreNotSame(service, Maps._sp1);
		}

		[TestMethod]
		public void ShouldMapNested() {
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
		public void ShouldUseSameScopeInNestedMaps() {
			Maps._sp1 = null!;
			Maps._sp2 = null!;

			_mapper.Map<CategoryProducts>(new Category {
				Id = 2,
				Products = new[] {
					new Product {
						Code = "Test"
					}
				}
			});
			Assert.IsNotNull(Maps._sp1);
			Assert.IsNotNull(Maps._sp2);
			Assert.AreSame(Maps._sp1, Maps._sp2);
		}

		[TestMethod]
		public void ShouldFallbackToMergeMapIfNewMapIsNotDefined() {
			Assert.AreEqual("6", _mapper.Map<string>(2f));
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
			{
				var strings = _mapper.Map<string[]>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{ 
				var strings = _mapper.Map<IList<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{ 
				var strings = _mapper.Map<LinkedList<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
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
		public void ShouldUseSameScopeInCollectionsMaps() {
			Maps._sp3.Clear();

			_mapper.Map<IEnumerable<string>>(new[] {
				new LimitedProduct {
					Code = "Test1",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					},
					Copies = 3
				},
				new LimitedProduct {
					Code = "Test2",
					Categories = new List<Category>(),
					Copies = 1
				}
			});
			Assert.AreEqual(2, Maps._sp3.Count);
			Assert.AreSame(Maps._sp3[0], Maps._sp3[1]);
		}

		[TestMethod]
		public void ShouldMapNullCollectionsOnlyForDefinedMaps() {
			Assert.IsNull(_mapper.Map<int[]?, string[]?>(null));

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[]?, float[]?>(null));
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
			var result = _mapper.Map<IList<string>>(new[] { 2f });

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("6", result[0]);
		}

		[TestMethod]
		public void ShouldNotFallbackToMergeMapInCollectionsIfCannotCreateElement() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollections() {
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
		}

		[TestMethod]
		public void ShouldNotMapMultidimensionalArrays() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2, 5 }
			}));
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
	}
}
