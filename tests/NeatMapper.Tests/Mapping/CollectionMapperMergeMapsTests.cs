﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class CollectionMapperMergeMapsTests {
		public class HierarchyMatchers :
#if NET7_0_OR_GREATER
			IHierarchyMatchMapStatic<Product, ProductDto>,
			IMergeMapStatic<LimitedProduct, ProductDto>
#else
			IHierarchyMatchMap<Product, ProductDto>,
			IMergeMap<LimitedProduct, ProductDto>
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
			ProductDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<LimitedProduct, ProductDto>
#else
				IMergeMap<LimitedProduct, ProductDto>
#endif
				.Map(LimitedProduct source, ProductDto destination, MappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				if (source != null) {
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = new List<int>();
				}
				return destination;
			}
		}

		protected IMapper _mapper = null;


		[TestInitialize]
		public void Initialize() {
			var options = new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(NewMapsTests.Maps), typeof(MergeMapsTests.Maps) }
			};
			_mapper = new CollectionMapper(new CustomMapper(options), new CustomMatcher(options));
		}


		[TestMethod]
		public void ShouldMapCollectionsWithoutElementsComparer() {
			Assert.IsTrue(_mapper.CanMapMerge<decimal[], List<Price>>());

			// Should forward options except merge.matcher

			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var result = _mapper.Map(new[] { 20m, 15.25m, 0m }, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.IsTrue(result.All(v => v != a && v != b && v != c));

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);

				var a2 = new Price();
				var b2 = new Price();
				var c2 = new Price();
				var destination2 = new List<Price> { a2, b2, c2 };
				var result2 = _mapper.MapMergeFactory<decimal[], List<Price>>().Invoke(new[] { 20m, 15.25m, 0m }, destination2);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.IsTrue(result.All(v => v != a && v != b && v != c));
			}

			// Options (no merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var opts = new TestOptions();
				_mapper.Map(new[] { 20m, 15.25m, 0m }, destination, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				_mapper.Map(new[] { 20m, 15.25m, 0m }, destination, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
			}

			{
				Assert.IsTrue(_mapper.CanMapMerge<string, List<float>>());

				var result = _mapper.Map("world", new List<float>());

				Assert.IsNotNull(result);
				Assert.AreEqual(5, result.Count);
				Assert.AreEqual(119f, result[0]);
				Assert.AreEqual(111f, result[1]);
				Assert.AreEqual(114f, result[2]);
				Assert.AreEqual(108f, result[3]);
				Assert.AreEqual(100f, result[4]);
			}
		}

		[TestMethod]
		public void ShouldCheckButNotMapOpenCollections() {
			Assert.IsTrue(_mapper.CanMapMerge(typeof(decimal[]), typeof(List<>)));

			{
				Assert.IsTrue(_mapper.CanMapMerge(typeof(string), typeof(List<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map("world", typeof(string), new List<float>(), typeof(List<>)));
			}
		}

		[TestMethod]
		public void ShouldNotMapCollectionsWithoutElementsMap() {
			Assert.IsFalse(_mapper.CanMapMerge<bool[], List<int>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { false }, new List<int>()));
		}

		[TestMethod]
		public void ShouldMapNullCollectionsOnlyIfElementsMapExists() {
			// Null source
			Assert.IsNull(_mapper.Map<int[], List<string>>(null, (List<string>)null));
			var dest = new List<string>();
			Assert.AreSame(dest, _mapper.Map<int[], List<string>>(null, dest));

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(null, (List<float>)null));
			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(null, new List<float>()));

			// Null destination
			{
				var result = _mapper.Map<int[], List<string>>(new[] { 1, 4, 7 }, (List<string>)null);
				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Count);
			}

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(new[] { 1, 4, 7 }, (List<float>)null));
		}

		[TestMethod]
		public void ShouldNotMapNullCollectionsIfCannotCreateDestination() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], CustomCollectionWithoutParameterlessConstructor<float>>(new[] { 1, 4, 7 }, (CustomCollectionWithoutParameterlessConstructor<float>)null));
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestinationWithoutExplicitMap() {
			{
				Assert.IsFalse(_mapper.CanMapMerge<decimal[], Price[]>());

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
				Assert.IsTrue(_mapper.CanMapMerge<IEnumerable<decimal>, ICollection<Price>>());

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
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<IEnumerable<decimal>, ICollection<Price>>(new[] { 20m, 15.25m, 0m }, (ICollection<Price>)destination));
				Assert.IsInstanceOfType(exc.InnerException, typeof(InvalidOperationException));
				Assert.IsTrue(exc.InnerException.Message.StartsWith("Cannot merge map to a readonly destination collection"));
				// Should not alter destination
				Assert.AreSame(a, destination[0]);
				Assert.AreEqual(12m, a.Amount);
				Assert.AreSame(b, destination[1]);
				Assert.AreEqual(34m, b.Amount);
				Assert.AreSame(c, destination[2]);
				Assert.AreEqual(56m, c.Amount);
			}

			{
				Assert.IsFalse(_mapper.CanMapMerge<decimal[], ReadOnlyCollection<Price>>());

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
				Assert.IsFalse(_mapper.CanMapMerge<IList<int>, string>());

				TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { 104, 101, 108, 108, 111 }, ""));
			}
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestinationNestedWithoutExplicitMap() {
			Assert.IsTrue(_mapper.CanMapMerge<IEnumerable<Category[]>, List<List<CategoryDto>>>());

			Assert.IsTrue(_mapper.CanMapMerge<IEnumerable<Category[]>, ICollection<IList<CategoryDto>>>());
			Assert.IsTrue(_mapper.CanMapMerge<IEnumerable<Category[]>, List<IList<CategoryDto>>>());

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
			var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { source1, source3, null, source2 }, destination, (s, d, _) => (s == source1 && d == destination1) ||
				(s == source2 && d == destination2)));
			Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
			Assert.IsInstanceOfType(exc.InnerException.InnerException, typeof(InvalidOperationException));
			Assert.IsTrue(exc.InnerException.InnerException.Message.StartsWith("Cannot merge map to a readonly destination collection"));

			// Unfortunately alters destination
		}

		[TestMethod]
		public void ShouldMapReadonlyCollectionDestinationIfRecreateIsTrue() {
			var options = new MappingOptions(new MergeCollectionsMappingOptions(recreateReadonlyDestination: true));

			// Without elements comparer
			{
				Assert.IsTrue(_mapper.CanMapMerge<decimal[], Price[]>(options));

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
				var result = _mapper.Map(new[] { 20m, 15.25m, 0m }, destination, options);
				Assert.IsNotNull(result);
				Assert.AreNotSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.IsTrue(result.All(v => v != a && v != b && v != c));
			}

			// With elements comparer
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
				var destination = new CategoryDto[] { a, b, c };
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
				}, destination, options);
				Assert.IsNotNull(result);
				Assert.AreNotSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.AreSame(a, result[0]);
				Assert.AreEqual(2, result[0].Parent);
				Assert.AreSame(b, result[1]);
				Assert.AreEqual(7, result[1].Parent);
				Assert.AreEqual(6, result[2].Id);
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollectionsWithoutElementsComparer() {
			// Cannot determine
			Assert.IsTrue(_mapper.CanMapMerge<int[][], List<ICollection<string>>>());

			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (no merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var destination = new List<ICollection<string>>();
				var opts = new TestOptions();
				_mapper.Map(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, destination, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var destination = new List<ICollection<string>>();
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions (false, EmptyMatcher.Instance);
				_mapper.Map(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
			}
		}

		[TestMethod]
		public void ShouldCatchExceptionsInCollectionMaps() {
			// Should wrap exceptions
			{
				// Normal collections
				{
					// Without comparer
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { 2f }, new List<int>()));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

					// Exception in comparer
					exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { 2m }, new List<int>() { 3 }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

					// Exception in custom comparer
					exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { 2f }, new List<int>() { 3 }, (a, b, c) => throw new NotImplementedException()));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
				}

				// Nested collections
				{
					// Without comparer
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { new[] { 2f } }, new List<List<int>> { new List<int>() }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

					// Exception in comparer
					exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { new[] { 2m } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => true));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MatcherException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

					// Exception in custom comparer
					exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => throw new NotImplementedException()));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
				}
			}

			// Should not wrap TaskCanceledException
			{
				// Normal collections
				{
					// Without comparer
					Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map(new[] { 2m }, new List<float>()));

					// Exception in comparer
					Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map(new[] { 2m }, new List<bool>() { true }));

					// Exception in custom comparer
					Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map(new[] { 2f }, new List<int>() { 3 }, (a, b, c) => throw new TaskCanceledException()));
				}

				// Nested collections
				{
					// Without comparer
					Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map(new[] { new[] { 2m } }, new List<List<float>> { new List<float>() }));

					// Exception in comparer
					Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map(new[] { new[] { 2m } }, new List<List<bool>> { new List<bool> { true } }, (a, b, c) => true));

					// Exception in custom comparer
					Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => throw new TaskCanceledException()));
				}
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsWithElementsComparer() {
			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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
				Assert.AreEqual(2, result[0].Parent);
				Assert.AreSame(b, result[1]);
				Assert.AreEqual(7, result[1].Parent);
				Assert.AreEqual(6, result[2].Id);

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (no merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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
				var opts = new TestOptions();
				_mapper.Map(new[] {
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
				}, destination, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				_mapper.Map(new[] {
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
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
			}
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
			Assert.AreEqual(2, result.ElementAt(0).Parent);
			Assert.AreEqual(3, result.ElementAt(1).Id);
			Assert.AreEqual(7, result.ElementAt(1).Parent);
			Assert.AreEqual(6, result.ElementAt(2).Id);
		}

		[TestMethod]
		public void ShouldMapCollectionsWithHierarchyElementsComparer() {
			var options = new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(HierarchyMatchers) }
			};
			var mapper = new CollectionMapper(new CustomMapper(options), new HierarchyCustomMatcher(options));

			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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
				var result = mapper.Map(new[] {
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

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (no merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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
				var opts = new TestOptions();
				mapper.Map(new[] {
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
				}, destination, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				mapper.Map(new[] {
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
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
			}
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
			var destination = new List<CategoryDto> { a, b, null, c };
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
			Assert.AreEqual(2, result[0].Parent);
			Assert.AreSame(b, result[1]);
			Assert.AreEqual(7, result[1].Parent);
			Assert.IsNull(result[2]);
			Assert.AreEqual(6, result[3]?.Id);
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
					var destination = new List<Price> { a1 };
					var result = _mapper.Map(new[] { 20m }, destination, (s, d, _) => s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreNotSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
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
					var destination = new List<Price> { null };
					var result = _mapper.Map(new[] { 20f }, destination, (s, d, _) => d == null);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.IsNotNull(result.Single());
				}
			}
		}

		[TestMethod]
		public void ShouldNotRemoveUnmatchedElementsFromDestinationIfSpecified() {
			// Global settings
			{
				var options = new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MergeMapsTests.Maps) }
				};
				var mapper = new CollectionMapper(new CustomMapper(options), new CustomMatcher(options), new MergeCollectionsOptions {
					RemoveNotMatchedDestinationElements = false
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
				Assert.AreEqual(2, result[0].Parent);
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
				}, destination, new[]{
					new MergeCollectionsMappingOptions(false)
				});
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(4, result.Count());
				Assert.AreSame(a, result[0]);
				Assert.AreEqual(2, result[0].Parent);
				Assert.AreSame(b, result[1]);
				Assert.AreEqual(7, result[1].Parent);
				Assert.AreSame(c, result[2]);
				Assert.AreEqual(6, result[3].Id);
			}
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
		public void ShouldPreferMergeMapForElementsToUpdateAndNewMapForElementsToAddInCollections() {
			var destination = new List<string> { "3", "7", "0" };
			var result = _mapper.Map(new[] { 7m, 4m, 3m }, destination, (s, d, _) => s.ToString() == d);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("MergeMap", result.ElementAt(0));
			Assert.AreEqual("NewMap", result.ElementAt(1));
			Assert.AreEqual("MergeMap", result.ElementAt(2));
		}
	}

	[TestClass]
	public class CollectionMapperCanMapMergeTests {
		[TestMethod]
		public void ShouldUseMappingOptions() {
			var mapper = new CollectionMapper(new CustomMapper());

			Assert.IsFalse(mapper.CanMapMerge<IEnumerable<string>, List<int>>());

			var options = new CustomNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => 0);
			var mapper2 = new CustomMapper(null, options);

			Assert.IsTrue(mapper.CanMapMerge<IEnumerable<string>, List<int>>(new[] { new MapperOverrideMappingOptions(mapper2) }));
		}
	}

	// DEV: test matching to edited elements (via previous elements mapping) and newly added elements
}
