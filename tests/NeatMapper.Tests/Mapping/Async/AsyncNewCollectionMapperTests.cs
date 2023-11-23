using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	public class AsyncNewCollectionMapperTests {
		protected IAsyncMapper _mapper = null;


		[TestMethod]
		public async Task ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], string[]>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var strings = await _mapper.MapAsync<string[]>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (no merge)
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], IList<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var strings = await _mapper.MapAsync<IList<string>>(new[] { 2, -3, 0 }, new[] { opts });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge with matcher)
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], LinkedList<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				var strings = await _mapper.MapAsync<LinkedList<string>>(new[] { 2, -3, 0 }, new object[]{ opts, merge });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], Queue<string>>());

				var strings = await _mapper.MapAsync<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<string[], SortedList<string, int>>());

				var strings = await _mapper.MapAsync<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], Stack<string>>());

				var strings = await _mapper.MapAsync<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<string[], ReadOnlyDictionary<string, int>>());

				var strings = await _mapper.MapAsync<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], CustomCollection<string>>());

				var strings = await _mapper.MapAsync<CustomCollection<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<IList<int>, string>());

				var str = await _mapper.MapAsync<string>(new[] { 104, 101, 108, 108, 111 });

				Assert.AreEqual("hello", str);
			}

			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<string, float[]>());

				var result = await _mapper.MapAsync<float[]>("world");

				Assert.IsNotNull(result);
				Assert.AreEqual(5, result.Length);
				Assert.AreEqual(119f, result[0]);
				Assert.AreEqual(111f, result[1]);
				Assert.AreEqual(114f, result[2]);
				Assert.AreEqual(108f, result[3]);
				Assert.AreEqual(100f, result[4]);
			}
		}

		// This is mostly to check times
		[TestMethod]
		public Task ShouldMapBigCollections() {
			return _mapper.MapAsync<float[]>(Enumerable.Repeat(0, 100));
		}

		[TestMethod]
		public async Task ShouldNotMapCollectionsIfCannotCreateDestination() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<int[], CustomCollectionWithoutParameterlessConstructor<string>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CustomCollectionWithoutParameterlessConstructor<string>>(new[] { 2, -3, 0 }));
		}

		[TestMethod]
		public async Task ShouldNotMapCollectionsWithoutElementsMap() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<int[], IEnumerable<Category>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<Category>>(new[] { 2 }));
		}

		[TestMethod]
		public async Task ShouldMapNullCollectionsOnlyIfElementsMapExists() {
			Assert.IsNull(await _mapper.MapAsync<int[], string[]>(null));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<decimal>>(null));
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
		public async Task ShouldCatchExceptionsInCollectionMaps() {
			// Should wrap exceptions
			{
				// Not awaited
				{ 
					// Normal collections
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<IEnumerable<int>>(new[] { 2f }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

					// Nested collections
					exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<int>>>(new[] { new[] { 2f } }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
				}

				// Awaited
				{
					// Normal collections
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<IEnumerable<decimal>>(new[] { 2f }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

					// Nested collections
					exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<decimal>>>(new[] { new[] { 2f } }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
				}
			}

			// Should not wrap TaskCanceledException
			{
				// Not awaited
				{
					// Normal collections
					await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync<IEnumerable<float>>(new[] { 2m }));

					// Nested collections
					await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<float>>>(new[] { new[] { 2m } }));
				}

				// Awaited
				{
					// Normal collections
					await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync<IEnumerable<double>>(new[] { 2m }));

					// Nested collections
					await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<double>>>(new[] { new[] { 2m } }));
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapCollectionsOfCollections() {
			// No options
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[][], IList<IEnumerable<string>>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (no merge)
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int[][], string[][]>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge with matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public async Task ShouldNotMapMultidimensionalArrays() {
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<int[,], string[]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[]>(new[,] {
					{ 2, -3, 0 },
					{ 1, 2, 5 }
				}));
			}

			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<int[][], string[,]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}));
			}

			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<int[,], string[,]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[,] {
					{ 2, -3, 0 },
					{ 1, 2, 5 }
				}));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapIfMapRejectsItself() {
			// Not awaited
			{
				// CanMap returns true because the map does exist, even if it will fail
				Assert.IsTrue(await _mapper.CanMapAsyncNew<float[], double[]>());

				var exc = await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<double[]>(new[] { 1f }));
				Assert.AreEqual(typeof(float[]), exc.From);
				Assert.AreEqual(typeof(double[]), exc.To);
			}

			// Awaited
			{
				// CanMap returns true because the map does exist, even if it will fail
				Assert.IsTrue(await _mapper.CanMapAsyncNew<double[], float[]>());

				var exc = await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<float[]>(new[] { 1d }));
				Assert.AreEqual(typeof(double[]), exc.From);
				Assert.AreEqual(typeof(float[]), exc.To);
			}
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperNotParallelWithAsyncNewMapperTests : AsyncNewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewCollectionMapper(new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapperTests.Maps) }
			}), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 1
			});
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperNotParallelWithAsyncMergeMapperTests : AsyncNewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewCollectionMapper(new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncMergeMapperTests.Maps) }
			}), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 1
			});
		}


		// DEV: we have no way of checking the below atm
		/*
		[TestMethod]
		public async Task ShouldFallbackToMergeMapInCollections() {
			// No Options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var result = await _mapper.MapAsync<IList<string>>(new[] { 2f });
				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("MergeMap", result[0]);

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (without matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				await _mapper.MapAsync<IList<string>>(new[] { 2f }, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
				await _mapper.MapAsync<IList<string>>(new[] { 2f }, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}
		*/

		[TestMethod]
		public async Task ShouldNotFallbackToMergeMapInCollectionsIfCannotCreateElement() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<string[], IEnumerable<ClassWithoutParameterlessConstructor>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperParallelWithAsyncNewMapperTests : AsyncNewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewCollectionMapper(new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapperTests.Maps) }
			}), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 10
			});
		}

		private static int mapped = 0;

		[TestMethod]
		public async Task ShouldCancelParallelMappingsOnException() {
			var options = new CustomAsyncNewAdditionalMapsOptions();
			mapped = 0;
			options.AddMap<int, string>(async (s, c) => {
				await Task.Delay(s, c.CancellationToken);
				if(s % 2 == 0)
					throw new Exception();
				else
					Interlocked.Increment(ref mapped);
				return "";
			});

			var mapper = new AsyncNewCollectionMapper(new AsyncNewMapper(null, options), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 10
			});

			var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => mapper.MapAsync<IEnumerable<int>, IEnumerable<string>>(new[] { 1, 51, 100, 201 }));
			await Task.Delay(300);

			Assert.AreEqual(2, mapped);
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperParallelWithAsyncMergeMapperTests : AsyncNewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewCollectionMapper(new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncMergeMapperTests.Maps) }
			}), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 10
			});
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

			var mapper = new AsyncNewCollectionMapper(new AsyncMergeMapper(null, options), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 10
			});

			var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => mapper.MapAsync<IEnumerable<int>, IEnumerable<string>>(new[] { 1, 51, 100, 201 }));
			await Task.Delay(300);

			Assert.AreEqual(2, mapped);
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperCanMapTests {
		[TestMethod]
		public async Task ShouldUseMappingOptions() {
			var mapper = new AsyncNewCollectionMapper(new AsyncNewMapper());

			Assert.IsFalse(await mapper.CanMapAsyncNew<IEnumerable<string>, IEnumerable<int>>());

			var options = new CustomAsyncNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => Task.FromResult(0));
			var mapper2 = new AsyncNewMapper(null, options);

			Assert.IsTrue(await mapper.CanMapAsyncNew<IEnumerable<string>, IEnumerable<int>>(new[] { new AsyncMapperOverrideMappingOptions(mapper2) }));
		}
	}
}
