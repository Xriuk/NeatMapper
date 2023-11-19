using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	public abstract class AsyncMergeCollectionMapperTests {
		public class HierarchyMatchers :
#if NET7_0_OR_GREATER
			IHierarchyMatchMapStatic<Product, ProductDto>,
			IAsyncMergeMapStatic<LimitedProduct, ProductDto>
#else
			IHierarchyMatchMap<Product, ProductDto>,
			IAsyncMergeMap<LimitedProduct, ProductDto>
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
			Task<ProductDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<LimitedProduct, ProductDto>
#else
				IAsyncMergeMap<LimitedProduct, ProductDto>
#endif
				.MapAsync(LimitedProduct source, ProductDto destination, AsyncMappingContext context) {
				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				MappingOptionsUtils.mergeOptions = context.MappingOptions.GetOptions<MergeCollectionsMappingOptions>();
				if (source != null) {
					if (destination == null)
						destination = new LimitedProductDto();
					destination.Code = source.Code;
					destination.Categories = new List<int>();
				}
				return Task.FromResult(destination);
			}
		}

		protected IAsyncMapper _mapper = null;

		protected abstract IAsyncMapper GetElementsMapper(params Type[] additionalTypes);

		protected abstract int GetParallelMappings();

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncMergeCollectionMapper(GetElementsMapper(), new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncMergeMapperTests.Maps) }
			}), new AsyncCollectionMappersOptions {
				MaxParallelMappings = GetParallelMappings()
			});
		}


		[TestMethod]
		public async Task ShouldMapCollectionsWithoutElementsComparer() {
			Assert.IsTrue(await _mapper.CanMapAsyncMerge<decimal[], List<Price>>());

			// Should forward options except merge.matcher

			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var a = new Price();
				var b = new Price();
				var c = new Price();
				var destination = new List<Price> { a, b, c };
				var result = await _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination);
				Assert.IsNotNull(result);
				Assert.AreSame(destination, result);
				Assert.AreEqual(3, result.Count());
				Assert.IsTrue(result.All(v => v != a && v != b && v != c));

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
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
				await _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination, new[] { opts });

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
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				await _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncMerge<string, List<float>>());

				var result = await _mapper.MapAsync("world", new List<float>());

				Assert.IsNotNull(result);
				Assert.AreEqual(5, result.Count);
				Assert.AreEqual(119f, result[0]);
				Assert.AreEqual(111f, result[1]);
				Assert.AreEqual(114f, result[2]);
				Assert.AreEqual(108f, result[3]);
				Assert.AreEqual(100f, result[4]);
			}
		}

		// This is mostly to check times
		[TestMethod]
		public Task ShouldMapBigCollectionsWithoutElementsComparer() {
			return _mapper.MapAsync(Enumerable.Repeat(0, 100), new List<float>());
		}

		[TestMethod]
		public async Task ShouldNotMapCollectionsWithoutElementsMap() {
			Assert.IsFalse(await _mapper.CanMapAsyncMerge<bool[], List<int>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { false }, new List<int>()));
		}

		[TestMethod]
		public async Task ShouldMapNullCollectionsOnlyIfElementsMapExists() {
			// Null source
			Assert.IsNull(await _mapper.MapAsync<int[], List<string>>(null, (List<string>)null));
			Assert.IsNull(await _mapper.MapAsync<int[], List<string>>(null, new List<string>()));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<decimal>>(null, (List<decimal>)null));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<decimal>>(null, new List<decimal>()));

			// Null destination
			{
				var result = await _mapper.MapAsync<int[], List<string>>(new[] { 1, 4, 7 }, (List<string>)null);
				Assert.IsNotNull(result);
				Assert.AreEqual(3, result.Count);
			}

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<decimal>>(new[] { 1, 4, 7 }, (List<decimal>)null));
		}

		[TestMethod]
		public Task ShouldNotMapNullCollectionsIfCannotCreateDestination() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], CustomCollectionWithoutParameterlessConstructor<float>>(new[] { 1, 4, 7 }, (CustomCollectionWithoutParameterlessConstructor<float>)null));
		}

		[TestMethod]
		public async Task ShouldNotMapReadonlyCollectionDestinationWithoutExplicitMap() {
			{
				Assert.IsFalse(await _mapper.CanMapAsyncMerge<decimal[], Price[]>());

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
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination));
				// Should not alter destination
				Assert.AreSame(a, destination[0]);
				Assert.AreEqual(12m, a.Amount);
				Assert.AreSame(b, destination[1]);
				Assert.AreEqual(34m, b.Amount);
				Assert.AreSame(c, destination[2]);
				Assert.AreEqual(56m, c.Amount);
			}

			{
				// Cannot determine
				await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _mapper.CanMapAsyncMerge<IEnumerable<decimal>, ICollection<Price>>());

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
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<decimal>, ICollection<Price>>(new[] { 20m, 15.25m, 0m }, (ICollection<Price>)destination));
				// Should not alter destination
				Assert.AreSame(a, destination[0]);
				Assert.AreEqual(12m, a.Amount);
				Assert.AreSame(b, destination[1]);
				Assert.AreEqual(34m, b.Amount);
				Assert.AreSame(c, destination[2]);
				Assert.AreEqual(56m, c.Amount);
			}

			{
				Assert.IsFalse(await _mapper.CanMapAsyncMerge<decimal[], ReadOnlyCollection<Price>>());

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
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { 20m, 15.25m, 0m }, destination));
				// Should not alter destination
				Assert.AreSame(a, destination[0]);
				Assert.AreEqual(12m, a.Amount);
				Assert.AreSame(b, destination[1]);
				Assert.AreEqual(34m, b.Amount);
				Assert.AreSame(c, destination[2]);
				Assert.AreEqual(56m, c.Amount);
			}

			{
				Assert.IsFalse(await _mapper.CanMapAsyncMerge<IList<int>, string>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { 104, 101, 108, 108, 111 }, ""));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapReadonlyCollectionDestinationNestedWithoutExplicitMap() {
			Assert.IsTrue(await _mapper.CanMapAsyncMerge<IEnumerable<Category[]>, List<List<CategoryDto>>>());

			// Cannot determine if mappable
			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _mapper.CanMapAsyncMerge<IEnumerable<Category[]>, ICollection<IList<CategoryDto>>>());
			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _mapper.CanMapAsyncMerge<IEnumerable<Category[]>, List<IList<CategoryDto>>>());

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
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { source1, source3, null, source2 }, destination, (s, d, _) => (s == source1 && d == destination1) ||
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
		public async Task ShouldMapCollectionsOfCollectionsWithoutElementsComparer() {
			// Cannot determine
			await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _mapper.CanMapAsyncMerge<int[][], List<ICollection<string>>>());

			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var destination = new List<ICollection<string>>();
				var result = await _mapper.MapAsync(new[] {
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
				await _mapper.MapAsync(new[] {
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
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				await _mapper.MapAsync(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, destination, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public async Task ShouldCatchExceptionsInCollectionMaps() {
			// Should wrap exceptions
			{
				// Not awaited
				{ 
					// Normal collections
					{
						// Without comparer
						var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { 2f }, new List<int>()));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

						// Exception in comparer
						exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { 2m }, new List<int>() { 3 }));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

						// Exception in custom comparer
						exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { 2f }, new List<int>() { 3 }, (a, b, c) => throw new NotImplementedException()));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
					}

					// Nested collections
					{
						// Without comparer
						var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { new[] { 2f } }, new List<List<int>> { new List<int>() }));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

						// Exception in comparer
						exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { new[] { 2m } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => true));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MatcherException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));

						// Exception in custom comparer
						exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => throw new NotImplementedException()));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MatcherException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
					}
				}

				// Awaited
				{
					// Normal collections
					{
						// Without comparer
						var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { 2f }, new List<decimal>()));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
					}

					// Nested collections
					{
						// Without comparer
						var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { new[] { 2f } }, new List<List<decimal>> { new List<decimal>() }));
						Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
						Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
					}
				}
			}

			// Should not wrap TaskCanceledException
			{
				// Not awaited
				{
					// Normal collections
					{
						// Without comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { 2m }, new List<float>()));

						// Exception in comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { 2m }, new List<bool>() { true }));

						// Exception in custom comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { 2f }, new List<int>() { 3 }, (a, b, c) => throw new TaskCanceledException()));
					}

					// Nested collections
					{
						// Without comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { new[] { 2m } }, new List<List<float>> { new List<float>() }));

						// Exception in comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { new[] { 2m } }, new List<List<bool>> { new List<bool> { true } }, (a, b, c) => true));

						// Exception in custom comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { new[] { 2f } }, new List<List<int>> { new List<int> { 3 } }, (a, b, c) => throw new TaskCanceledException()));
					}
				}

				// Awaited
				{
					// Normal collections
					{
						// Without comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { 2m }, new List<double>()));
					}

					// Nested collections
					{
						// Without comparer
						await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync(new[] { new[] { 2m } }, new List<List<double>> { new List<double>() }));
					}
				}
			}
		}
	}

	public abstract class AsyncMergeMapperMergeCollectionMapperTests : AsyncMergeCollectionMapperTests {
		[TestMethod]
		public async Task ShouldMapCollectionsWithElementsComparer() {
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
				var result = await _mapper.MapAsync(new[] {
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
				await _mapper.MapAsync(new[] {
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
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				await _mapper.MapAsync(new[] {
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
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public async Task ShouldMapCollectionsWithCustomElementsComparer() {
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
			var result = await _mapper.MapAsync(new[] {
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
		public async Task ShouldMapCollectionsWithHierarchyElementsComparer() {
			var mapper = new AsyncMergeCollectionMapper(GetElementsMapper(typeof(HierarchyMatchers)), new HierarchyCustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(HierarchyMatchers) }
			}));

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
				var result = await mapper.MapAsync(new[] {
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
				await mapper.MapAsync(new[] {
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
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				await mapper.MapAsync(new[] {
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
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public async Task ShouldMapNullElementsInCollections() {
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
			var result = await _mapper.MapAsync(new[] {
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
		public async Task ShouldRespectReturnedValueInCollections() {
			// Returns new destination
			{
				// Not null
				{
					var a1 = new Price {
						Amount = 20m
					};
					var destination = new List<Price> { a1 };
					var result = await _mapper.MapAsync(new[] { 20m }, destination, (s, d, _) => s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreNotSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
					var result = await _mapper.MapAsync(new[] { 20m }, destination, (s, d, _) => d == null);
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
					var result = await _mapper.MapAsync(new[] { 20f }, destination, (s, d, _) => (decimal)s == d?.Amount);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.AreSame(a1, result.Single());
				}

				// Null
				{
					var destination = new List<Price> { null };
					var result = await _mapper.MapAsync(new[] { 20f }, destination, (s, d, _) => d == null);
					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count());
					Assert.IsNotNull(result.Single());
				}
			}
		}

		[TestMethod]
		public async Task ShouldNotRemoveUnmatchedElementsFromDestinationIfSpecified() {
			// Global settings
			{
				var mapper = new AsyncMergeCollectionMapper(GetElementsMapper(), new CustomMatcher(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MergeMapperTests.Maps) }
				}), new AsyncCollectionMappersOptions {
					MaxParallelMappings = GetParallelMappings()
				}, new MergeCollectionsOptions {
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
				var result = await mapper.MapAsync(new[] {
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
				var result = await _mapper.MapAsync(new[] {
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
		public async Task ShouldMapCollectionsOfCollectionsWithElementsComparer() {
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
			var result = await _mapper.MapAsync<IEnumerable<IEnumerable<Category>>, IList<ICollection<CategoryDto>>>(new[] { source1, source1 }, destination);

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
	}

	[TestClass]
	public class AsyncMergeCollectionMapperNotParallelWithAsyncNewMapperTests : AsyncMergeCollectionMapperTests {
		protected override IAsyncMapper GetElementsMapper(params Type[] additionalTypes) {
			return new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new [] { typeof(AsyncNewMapperTests.Maps) }).ToList()
			});
		}

		protected override int GetParallelMappings() {
			return 1;
		}
	}

	[TestClass]
	public class AsyncMergeCollectionMapperNotParallelWithAsyncMergeMapperTests : AsyncMergeMapperMergeCollectionMapperTests {
		protected override IAsyncMapper GetElementsMapper(params Type[] additionalTypes) {
			return new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new [] { typeof(AsyncMergeMapperTests.Maps) }).ToList()
			});
		}

		protected override int GetParallelMappings() {
			return 1;
		}


		[TestMethod]
		public async Task ShouldNotMapCollectionsIfCannotCreateElement() {
			Assert.IsFalse(await _mapper.CanMapAsyncMerge<string[], List<ClassWithoutParameterlessConstructor>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { "" }, new List<ClassWithoutParameterlessConstructor>()));
		}
	}

	[TestClass]
	public class AsyncMergeCollectionMapperNotParallelWithAsyncNewAndMergeMappersTests : AsyncMergeMapperMergeCollectionMapperTests {
		protected override IAsyncMapper GetElementsMapper(params Type[] additionalTypes) {
			return new AsyncCompositeMapper(new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new[] { typeof(AsyncNewMapperTests.Maps) }).ToList()
			}), new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new[] { typeof(AsyncMergeMapperTests.Maps) }).ToList() 
			}));
		}

		protected override int GetParallelMappings() {
			return 1;
		}


		[TestMethod]
		public async Task ShouldPreferMergeMapForElementsToUpdateAndNewMapForElementsToAddInCollections() {
			var destination = new List<string> { "3", "7", "0" };
			var result = await _mapper.MapAsync(new[] { 7m, 4m, 3m }, destination, (s, d, _) => s.ToString() == d);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("MergeMap", result.ElementAt(0));
			Assert.AreEqual("NewMap", result.ElementAt(1));
			Assert.AreEqual("MergeMap", result.ElementAt(2));
		}
	}

	[TestClass]
	public class AsyncMergeCollectionMapperParallelWithAsyncNewMapperTests : AsyncMergeCollectionMapperTests {
		protected override IAsyncMapper GetElementsMapper(params Type[] additionalTypes) {
			return new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new[] { typeof(AsyncNewMapperTests.Maps) }).ToList()
			});
		}

		protected override int GetParallelMappings() {
			return 10;
		}

		private static int mapped = 0;

		[TestMethod]
		public async Task ShouldCancelParallelMappingsOnException() {
			var options = new CustomAsyncNewAdditionalMapsOptions();
			mapped = 0;
			options.AddMap<int, string>(async (s, c) => {
				await Task.Delay(s, c.CancellationToken);
				if (s % 2 == 0)
					throw new Exception();
				else
					Interlocked.Increment(ref mapped);
				return "";
			});

			var mapper = new AsyncMergeCollectionMapper(new AsyncNewMapper(null, options), null, new AsyncCollectionMappersOptions {
				MaxParallelMappings = 10
			});

			var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => mapper.MapAsync<IEnumerable<int>, ICollection<string>>(new[] { 1, 51, 100, 201 }, new List<string>()));
			await Task.Delay(300);

			Assert.AreEqual(2, mapped);
		}
	}

	[TestClass]
	public class AsyncMergeCollectionMapperParallelWithAsyncMergeMapperTests : AsyncMergeMapperMergeCollectionMapperTests {
		protected override IAsyncMapper GetElementsMapper(params Type[] additionalTypes) {
			return new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new[] { typeof(AsyncMergeMapperTests.Maps) }).ToList()
			});
		}

		protected override int GetParallelMappings() {
			return 10;
		}


		[TestMethod]
		public async Task ShouldNotMapCollectionsIfCannotCreateElement() {
			Assert.IsFalse(await _mapper.CanMapAsyncMerge<string[], List<ClassWithoutParameterlessConstructor>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { "" }, new List<ClassWithoutParameterlessConstructor>()));
		}

		private static int mapped = 0;

		[TestMethod]
		public async Task ShouldCancelParallelMappingsOnException() {
			var options = new CustomAsyncMergeAdditionalMapsOptions();
			mapped = 0;
			options.AddMap<int, string>(async (s, d, c) => {
				await Task.Delay(s, c.CancellationToken);
				if (s % 2 == 0)
					throw new Exception();
				else
					Interlocked.Increment(ref mapped);
				return "";
			});

			var mapper = new AsyncMergeCollectionMapper(new AsyncMergeMapper(null, options), null, new AsyncCollectionMappersOptions {
				MaxParallelMappings = 10
			});

			var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => mapper.MapAsync<IEnumerable<int>, ICollection<string>>(new[] { 1, 51, 100, 201 }, new List<string>()));
			await Task.Delay(300);

			Assert.AreEqual(2, mapped);
		}
	}

	[TestClass]
	public class AsyncMergeCollectionMapperParallelWithAsyncNewAndMergeMappersTests : AsyncMergeMapperMergeCollectionMapperTests {
		protected override IAsyncMapper GetElementsMapper(params Type[] additionalTypes) {
			return new AsyncCompositeMapper(new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new[] { typeof(AsyncNewMapperTests.Maps) }).ToList()
			}), new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = additionalTypes.Concat(new[] { typeof(AsyncMergeMapperTests.Maps) }).ToList()
			}));
		}

		protected override int GetParallelMappings() {
			return 10;
		}


		[TestMethod]
		public async Task ShouldPreferMergeMapForElementsToUpdateAndNewMapForElementsToAddInCollections() {
			var destination = new List<string> { "3", "7", "0" };
			var result = await _mapper.MapAsync(new[] { 7m, 4m, 3m }, destination, (s, d, _) => s.ToString() == d);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("MergeMap", result.ElementAt(0));
			Assert.AreEqual("NewMap", result.ElementAt(1));
			Assert.AreEqual("MergeMap", result.ElementAt(2));
		}
	}

	[TestClass]
	public class AsyncMergeCollectionMapperCanMapTests {
		[TestMethod]
		public async Task ShouldUseMappingOptions() {
			var mapper = new AsyncMergeCollectionMapper(new AsyncNewMapper());

			Assert.IsFalse(await mapper.CanMapAsyncMerge<IEnumerable<string>, List<int>>());

			var options = new CustomAsyncNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => Task.FromResult(0));
			var mapper2 = new AsyncNewMapper(null, options);

			Assert.IsTrue(await mapper.CanMapAsyncMerge<IEnumerable<string>, List<int>>(new[] { new AsyncMapperOverrideMappingOptions(mapper2) }));
		}
	}
}
