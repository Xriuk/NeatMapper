﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	public class NewCollectionMapperTests {
		protected IMapper _mapper = null;

		[TestMethod]
		public void ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var strings = _mapper.Map<string[]>(new[] { 2, -3, 0 });

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
				var strings = _mapper.Map<IList<string>>(new[] { 2, -3, 0 }, opts);

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
				var strings = _mapper.Map<LinkedList<string>>(new[] { 2, -3, 0 }, opts, merge);

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
		public void ShouldNotMapCollectionsWithouElementsMap() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<Category>>(new[] { 2 }));
		}

		[TestMethod]
		public void ShouldMapNullCollectionsOnlyIfElementsMapExists() {
			Assert.IsNull(_mapper.Map<int[], string[]>(null));

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[], List<float>>(null));
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
		public void ShouldCatchExceptionsInCollectionMaps() {
			// Normal collections
			var exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map<IEnumerable<int>>(new[] { 2f }));
			Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
			Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));

			// Nested collections
			exc = Assert.ThrowsException<CollectionMappingException>(() => _mapper.Map<IEnumerable<IEnumerable<int>>>(new[] { new[] { 2f } }));
			Assert.IsInstanceOfType(exc.InnerException, typeof(CollectionMappingException));
			Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
			Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollections() {
			// No options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

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

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (no merge)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				_mapper.Map<IList<IEnumerable<string>>>(new[] {
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
				_mapper.Map<IList<IEnumerable<string>>>(new[] {
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
		public void ShouldNotMapMultidimensionalArrays() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<string[]>(new[,] {
				{ 2, -3, 0 },
				{ 1, 2, 5 }
			}));

			TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2 }
			}));

			TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[,] {
				{ 2, -3, 0 },
				{ 1, 2, 5 }
			}));
		}
	}

	[TestClass]
	public class NewCollectionMapperWithNewMapperTests : NewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new NewCollectionMapper(new NewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(NewMapperTests.Maps) }
			}));
		}
	}

	[TestClass]
	public class NewCollectionMapperWithMergeMapperTests : NewCollectionMapperTests {
		[TestInitialize]
		public void Initialize() {
			_mapper = new NewCollectionMapper(new MergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MergeMapperTests.Maps) }
			}));
		}


		// DEV: we have no way of checking the below atm
		/*
		[TestMethod]
		public void ShouldFallbackToMergeMapInCollections() {
			// No Options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var result = _mapper.Map<IList<string>>(new[] { 2f });
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
				_mapper.Map<IList<string>>(new[] { 2f }, new[] { opts });

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
				_mapper.Map<IList<string>>(new[] { 2f }, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.AreNotSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}
		*/

		[TestMethod]
		public void ShouldNotFallbackToMergeMapInCollectionsIfCannotCreateElement() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}
	}
}
