using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	public class AsyncNewCollectionMapperTests {
		protected IAsyncMapper _mapper = null;

		[TestMethod]
		public async Task ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
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
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
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
				var strings = await _mapper.MapAsync<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = await _mapper.MapAsync<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = await _mapper.MapAsync<CustomCollection<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}
		}

		[TestMethod]
		public Task ShouldNotMapCollectionsIfCannotCreateDestination() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CustomCollectionWithoutParameterlessConstructor<string>>(new[] { 2, -3, 0 }));
		}

		[TestMethod]
		public Task ShouldNotMapCollectionsWithoutElementsMap() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<Category>>(new[] { 2 }));
		}

		[TestMethod]
		public async Task ShouldMapNullCollectionsOnlyIfElementsMapExists() {
			Assert.IsNull(await _mapper.MapAsync<int[], string[]>(null));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[], List<float>>(null));
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
			// Not awaited
			{ 
				// Normal collections
				var exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<int>>(new[] { 2f }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// Nested collections
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<int>>>(new[] { new[] { 2f } }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
			}

			// Awaited
			{
				// Normal collections
				var exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<decimal>>(new[] { 2f }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

				// Nested collections
				exc = await Assert.ThrowsExceptionAsync<CollectionMappingException>(() => _mapper.MapAsync<IEnumerable<IEnumerable<decimal>>>(new[] { new[] { 2f } }));
				Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
				Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
			}
		}

		[TestMethod]
		public async Task ShouldMapCollectionsOfCollections() {
			// No options
			{
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
				var merge = new MergeCollectionsMappingOptions {
					Matcher = (s, d, c) => false,
					RemoveNotMatchedDestinationElements = false
				};
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
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[]>(new[,] {
				{ 2, -3, 0 },
				{ 1, 2, 5 }
			}));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2 }
			}));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[,] {
				{ 2, -3, 0 },
				{ 1, 2, 5 }
			}));
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperWithAsyncNewMapperTests : AsyncNewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewCollectionMapper(new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapperTests.Maps) }
			}));
		}
	}

	[TestClass]
	public class AsyncNewCollectionMapperWithAsyncMergeMapperTests : AsyncNewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewCollectionMapper(new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncMergeMapperTests.Maps) }
			}));
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
		public Task ShouldNotFallbackToMergeMapInCollectionsIfCannotCreateElement() {
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}
	}
}
