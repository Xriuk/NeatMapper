using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;
using System.Collections.ObjectModel;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class MergeMapsTests {
		public class Maps :
			IMergeMap<int, string>,
			INewMap<float, string>,
			IMergeMap<float, string>,
			IMergeMap<Price, decimal>,
			IMergeMap<Price, PriceFloat>,
			IMergeMap<Category, int>,
			IMergeMap<Product, ProductDto>,
			IMergeMap<LimitedProduct, LimitedProductDto>,
			IMergeMap<Product, string>,
			IMergeMap<Category, CategoryProducts>,
			IMergeMap<LimitedProduct, string>,
			IMergeMap<decimal, Price>,
			IMergeMap<float, Price>,
			IMergeMap<Category, CategoryDto>,
			ICollectionElementComparer<Category, CategoryDto>,
			IMergeMap<IEnumerable<Category>, CategoryDto[]>,
			IMergeMap<float, int>,
			IMergeMap<decimal, int>,
			ICollectionElementComparer<decimal, int>,
			IMergeMap<string, ClassWithoutParameterlessConstructor>{

			static string? IMergeMap<int, string>.Map(int source, string? destination, MappingContext context) {
				return (source * 2).ToString();
			}

			static string? INewMap<float, string>.Map(float source, MappingContext context) {
				return "NewMap";
			}

			static string? IMergeMap<float, string>.Map(float source, string? destination, MappingContext context) {
				return "MergeMap";
			}

			static decimal IMergeMap<Price, decimal>.Map(Price? source, decimal destination, MappingContext context) {
				return source?.Amount ?? 0m;
			}

			static PriceFloat? IMergeMap<Price, PriceFloat>.Map(Price? source, PriceFloat? destination, MappingContext context) {
				if(source != null) {
					destination ??= new PriceFloat();
					destination.Amount = (float)source.Amount;
					destination.Currency = source.Currency;
				}
				return destination;
			}

			static int IMergeMap<Category, int>.Map(Category? source, int destination, MappingContext context) {
				return source?.Id ?? destination;
			}

			// Nested MergeMap
			static ProductDto? IMergeMap<Product, ProductDto>.Map(Product? source, ProductDto? destination, MappingContext context) {
				if (source != null) {
					destination ??= new ProductDto();
					destination.Code = source.Code;
					destination.Categories = context.Mapper.Map(source.Categories, destination.Categories) ?? new List<int>();
				}
				return destination;
			}

			// Nested NewMap
			static LimitedProductDto? IMergeMap<LimitedProduct, LimitedProductDto>.Map(LimitedProduct? source, LimitedProductDto? destination, MappingContext context) {
				if (source != null){
					destination ??= new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = context.Mapper.Map<ICollection<int>>(source.Categories) ?? new List<int>();
					destination.Copies = source.Copies;
				}
				return destination;
			}

			// Scope test
			public static IServiceProvider _sp1 = null!;
			static string? IMergeMap<Product, string>.Map(Product? source, string? destination, MappingContext context) {
				_sp1 = context.ServiceProvider;
				return source?.Code;
			}

			public static IServiceProvider _sp2 = null!;
			static CategoryProducts? IMergeMap<Category, CategoryProducts>.Map(Category? source, CategoryProducts? destination, MappingContext context) {
				_sp2 = context.ServiceProvider;
				if(source != null) {
					destination ??= new CategoryProducts();
					destination.Id = source.Id;
					destination.Products = context.Mapper.Map(source.Products, destination.Products) ?? new List<string>();
				}
				return destination;
			}

			public static List<IServiceProvider> _sp3 = new List<IServiceProvider>();
			static string? IMergeMap<LimitedProduct, string>.Map(LimitedProduct? source, string? destination, MappingContext context) {
				_sp3.Add(context.ServiceProvider);
				return context.Mapper.Map<Product, string>(source, destination);
			}

			// Returns new destination
			static Price? IMergeMap<decimal, Price>.Map(decimal source, Price? destination, MappingContext context) {
				return new Price {
					Amount = source,
					Currency = "EUR"
				};
			}

			// Returns passed destination (if not null)
			static Price? IMergeMap<float, Price>.Map(float source, Price? destination, MappingContext context) {
				destination ??= new Price();
				destination.Amount = (decimal)source;
				destination.Currency = "EUR";
				return destination;
			}

			static CategoryDto? IMergeMap<Category, CategoryDto>.Map(Category? source, CategoryDto? destination, MappingContext context) {
				if(source != null) {
					destination ??= new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = source.Parent?.Id;
				}
				return destination;
			}

			static bool ICollectionElementComparer<Category, CategoryDto>.Match(Category? source, CategoryDto? destination, MappingContext context) {
				return source?.Id == destination?.Id;
			}

			static CategoryDto[]? IMergeMap<IEnumerable<Category>, CategoryDto[]>.Map(IEnumerable<Category>? source, CategoryDto[]? destination, MappingContext context) {
				return source?.Select(s => context.Mapper.Map<Category, CategoryDto>(s)!).ToArray();
			}

			// Throws exception
			static int IMergeMap<float, int>.Map(float source, int destination, MappingContext context) {
				throw new NotImplementedException();
			}

			// Throws exception
			static int IMergeMap<decimal, int>.Map(decimal source, int destination, MappingContext context) {
				throw new NotImplementedException();
			}

			// Throws exception
			static bool ICollectionElementComparer<decimal, int>.Match(decimal source, int destination, MappingContext context) {
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
		public void ShouldCreateNewScopeForEachMap() {
			Maps._sp1 = null!;

			_mapper.Map<Product, string>(new Product {
				Code = "Test1"
			}, "");

			Assert.IsNotNull(Maps._sp1);
			var service = Maps._sp1;

			_mapper.Map<Product, string>(new Product {
				Code = "Test2"
			}, null);

			Assert.IsNotNull(Maps._sp1);
			Assert.AreNotSame(service, Maps._sp1);
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
		public void ShouldUseSameScopeInNestedMaps() {
			Maps._sp1 = null!;
			Maps._sp2 = null!;

			_mapper.Map<Category, CategoryProducts>(new Category {
				Id = 2,
				Products = new[] {
					new Product {
						Code = "Test"
					}
				}
			}, null);
			Assert.IsNotNull(Maps._sp1);
			Assert.IsNotNull(Maps._sp2);
			Assert.AreSame(Maps._sp1, Maps._sp2);
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
			var a = new Price();
			var b = new Price();
			var c = new Price();
			var destination = new List<Price> { a, b, c };
			var result = _mapper.Map(new[] { 20m, 15.25m, 0m }, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.IsTrue(result.All(v => v != a && v != b && v != c));
		}

		[TestMethod]
		public void ShouldMapCollectionsWithElementsComparer() {
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
		public void ShouldUseSameScopeInCollectionsMaps() {
			Maps._sp3.Clear();

			_mapper.Map(new[] {
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
			}, new List<string>());
			Assert.AreEqual(2, Maps._sp3.Count);
			Assert.AreSame(Maps._sp3[0], Maps._sp3[1]);
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
			var destination = new List<CategoryDto?> { a, b, null, c };
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
			Assert.IsNull(result[0]!.Parent);
			Assert.AreSame(b, result[1]);
			Assert.AreEqual(7, result[1]!.Parent);
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
					var destination = new List<Price?> { null };
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
					var destination = new List<Price?> { null };
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
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(Maps) },
					MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions {
						RemoveNotMatchedDestinationElements = false
					}
				}), new ServiceCollection().BuildServiceProvider());

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
		public void ShouldCatchExceptionsInCollectionMaps() {
			// Normal collections
			{ 
				// Without comparer
				var exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { 2f }, new List<int>()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// With comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { 2m }, new List<int>() { 3 }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionElementComparerException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// With custom comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { 2f }, new List<int>() { 3 }, (_, _, _) => throw new NotImplementedException()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionElementComparerException));
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
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { new[] { 2m } }, new List<List<int>>{ new List<int> { 3 } }, (_, _, _) => true));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(CollectionElementComparerException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

				// With custom comparer
				exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (_, _, _) => throw new NotImplementedException()));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionElementComparerException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
			}
		}
	}
}
