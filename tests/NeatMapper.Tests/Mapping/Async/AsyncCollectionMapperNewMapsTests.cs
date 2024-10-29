using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	public class AsyncCollectionMapperNewMapsTests {
		protected IAsyncMapper _mapper = null;


		[TestMethod]
		public async Task ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], string[]>());

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
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], IList<string>>());

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
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], LinkedList<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				var strings = await _mapper.MapAsync<LinkedList<string>>(new[] { 2, -3, 0 }, new object[]{ opts, merge });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], Queue<string>>());

				var strings = await _mapper.MapAsync<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<string[], SortedList<string, int>>());

				var strings = await _mapper.MapAsync<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], Stack<string>>());

				var strings = await _mapper.MapAsync<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<string[], ReadOnlyDictionary<string, int>>());

				var strings = await _mapper.MapAsync<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], CustomCollection<string>>());

				var strings = await _mapper.MapAsync<CustomCollection<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<IList<int>, string>());

				var str = await _mapper.MapAsync<string>(new[] { 104, 101, 108, 108, 111 });

				Assert.AreEqual("hello", str);
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<string, float[]>());

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
		public async Task ShouldMapBigCollections() {
			var start = DateTime.UtcNow;
			await _mapper.MapAsync<float[]>(Enumerable.Repeat(0, 100));
			var duration = DateTime.UtcNow - start;
			Console.WriteLine("Duration: " + duration.ToString("ss\\.fff"));
		}

		[TestMethod]
		public async Task ShouldNotMapCollectionsIfCannotCreateDestination() {
			Assert.IsFalse(_mapper.CanMapAsyncNew<int[], CustomCollectionWithoutParameterlessConstructor<string>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CustomCollectionWithoutParameterlessConstructor<string>>(new[] { 2, -3, 0 }));
		}

		[TestMethod]
		public async Task ShouldNotMapCollectionsWithoutElementsMap() {
			Assert.IsFalse(_mapper.CanMapAsyncNew<int[], IEnumerable<Category>>());

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
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[][], IList<IEnumerable<string>>>());

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
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[][], string[][]>());

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
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
			}
		}

		[TestMethod]
		public async Task ShouldNotMapMultidimensionalArrays() {
			{
				Assert.IsFalse(_mapper.CanMapAsyncNew<int[,], string[]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[]>(new[,] {
					{ 2, -3, 0 },
					{ 1, 2, 5 }
				}));
			}

			{
				Assert.IsFalse(_mapper.CanMapAsyncNew<int[][], string[,]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}));
			}

			{
				Assert.IsFalse(_mapper.CanMapAsyncNew<int[,], string[,]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[,] {
					{ 2, -3, 0 },
					{ 1, 2, 5 }
				}));
			}
		}


		[TestMethod]
		public async Task ShouldMapAsyncEnumerable() {
			// From
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<IAsyncEnumerable<int>, string[]>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var strings = await _mapper.MapAsync<string[]>(new DefaultAsyncEnumerable<int>(new[] { 2, -3, 0 }));

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// To
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<int[], IAsyncEnumerable<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var strings = await _mapper.MapAsync<IAsyncEnumerable<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				var asyncEnumerator = strings.GetAsyncEnumerator();
				try { 
					Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
					Assert.AreEqual("4", asyncEnumerator.Current);
					Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
					Assert.AreEqual("-6", asyncEnumerator.Current);
					Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
					Assert.AreEqual("0", asyncEnumerator.Current);
					Assert.IsFalse(await asyncEnumerator.MoveNextAsync());

					Assert.IsNull(MappingOptionsUtils.options);
					Assert.IsNull(MappingOptionsUtils.mergeOptions);
				}
				finally {
					await asyncEnumerator.DisposeAsync();
				}
			}

			// Both
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<IAsyncEnumerable<int>, IAsyncEnumerable<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var strings = await _mapper.MapAsync<IAsyncEnumerable<string>>(new DefaultAsyncEnumerable<int>(new[] { 2, -3, 0 }));

				Assert.IsNotNull(strings);
				var asyncEnumerator = strings.GetAsyncEnumerator();
				try {
					Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
					Assert.AreEqual("4", asyncEnumerator.Current);
					Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
					Assert.AreEqual("-6", asyncEnumerator.Current);
					Assert.IsTrue(await asyncEnumerator.MoveNextAsync());
					Assert.AreEqual("0", asyncEnumerator.Current);
					Assert.IsFalse(await asyncEnumerator.MoveNextAsync());

					Assert.IsNull(MappingOptionsUtils.options);
					Assert.IsNull(MappingOptionsUtils.mergeOptions);
				}
				finally {
					await asyncEnumerator.DisposeAsync();
				}
			}
		}
	}

	[TestClass]
	public class AsyncCollectionMapperNewMapsNotParallelTests : AsyncCollectionMapperNewMapsTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncCollectionMapper(new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps), typeof(AsyncMergeMapsTests.Maps) }
			}), new AsyncCollectionMappersOptions {
				MaxParallelMappings = 1
			});
		}
	}

	[TestClass]
	public class AsyncCollectionMapperNewMapsParallelTests : AsyncCollectionMapperNewMapsTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncCollectionMapper(new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps), typeof(AsyncMergeMapsTests.Maps) }
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

			var mapper = new AsyncCollectionMapper(new AsyncCustomMapper(null, options), new AsyncCollectionMappersOptions {
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
		public void ShouldUseMappingOptions() {
			var mapper = new AsyncCollectionMapper(new AsyncCustomMapper());

			Assert.IsFalse(mapper.CanMapAsyncNew<IEnumerable<string>, IEnumerable<int>>());

			var options = new CustomAsyncNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => Task.FromResult(0));
			var mapper2 = new AsyncCustomMapper(null, options);

			Assert.IsTrue(mapper.CanMapAsyncNew<IEnumerable<string>, IEnumerable<int>>(new[] { new AsyncMapperOverrideMappingOptions(mapper2) }));
		}
	}
}
