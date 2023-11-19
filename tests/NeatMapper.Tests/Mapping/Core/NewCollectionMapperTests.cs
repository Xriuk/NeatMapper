using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping {
	public class NewCollectionMapperTests {
		protected IMapper _mapper = null;

		[TestMethod]
		public void ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				Assert.IsTrue(_mapper.CanMapNew<int[], string[]>());

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
				Assert.IsTrue(_mapper.CanMapNew<int[], IList<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var strings = _mapper.Map<IList<string>>(new[] { 2, -3, 0 }, new object[] { opts });

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
				Assert.IsTrue(_mapper.CanMapNew<int[], LinkedList<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, (s, d, _) => false);
				var strings = _mapper.Map<LinkedList<string>>(new[] { 2, -3, 0 }, new object[] { opts, merge });

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
				Assert.IsTrue(_mapper.CanMapNew<int[], Queue<string>>());

				var strings = _mapper.Map<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<string[], SortedList<string, int>>());

				var strings = _mapper.Map<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<IEnumerable<int>, Stack<string>>());

				var strings = _mapper.Map<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<string[], ReadOnlyDictionary<string, int>>());

				var strings = _mapper.Map<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<IList<int>, CustomCollection<string>>());

				var strings = _mapper.Map<CustomCollection<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<IList<int>, string>());

				var str = _mapper.Map<string>(new[] { 104, 101, 108, 108, 111 });

				Assert.AreEqual("hello", str);
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<string, float[]>());

				var result = _mapper.Map<float[]>("world");

				Assert.IsNotNull(result);
				Assert.AreEqual(5, result.Length);
				Assert.AreEqual(119f, result[0]);
				Assert.AreEqual(111f, result[1]);
				Assert.AreEqual(114f, result[2]);
				Assert.AreEqual(108f, result[3]);
				Assert.AreEqual(100f, result[4]);
			}
		}

		[TestMethod]
		public void ShouldNotMapCollectionsIfCannotCreateDestination() {
			Assert.IsFalse(_mapper.CanMapNew<int[], CustomCollectionWithoutParameterlessConstructor<string>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<CustomCollectionWithoutParameterlessConstructor<string>>(new[] { 2, -3, 0 }));
		}

		[TestMethod]
		public void ShouldNotMapCollectionsWithoutElementsMap() {
			Assert.IsFalse(_mapper.CanMapNew<int[], IEnumerable<Category>>());

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
			// Should wrap exceptions
			{
				// Normal collections
				{ 
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<IEnumerable<int>>(new[] { 2f }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(NotImplementedException));
				}

				// Nested collections
				{ 
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<IEnumerable<IEnumerable<int>>>(new[] { new[] { 2f } }));
					Assert.IsInstanceOfType(exc.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException, typeof(MappingException));
					Assert.IsInstanceOfType(exc.InnerException?.InnerException?.InnerException, typeof(NotImplementedException));
				}
			}

			// Should not wrap TaskCanceledException
			{
				// Normal collections
				Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map<IEnumerable<float>>(new[] { 2m }));

				// Nested collections
				Assert.ThrowsException<TaskCanceledException>(() => _mapper.Map<IEnumerable<IEnumerable<float>>>(new[] { new[] { 2m } }));
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollections() {
			// No options
			{
				Assert.IsTrue(_mapper.CanMapNew<int[][], IList<IEnumerable<string>>>());

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
				Assert.IsTrue(_mapper.CanMapNew<int[][], string[][]>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				_mapper.Map<string[][]>(new[] {
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
			{ 
				Assert.IsFalse(_mapper.CanMapNew<int[,], string[]>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<string[][]>(new[,] {
					{ 2, -3, 0 },
					{ 1, 2, 5 }
				}));
			}

			{ 
				Assert.IsFalse(_mapper.CanMapNew<int[][], string[,]>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}));
			}

			{ 
				Assert.IsFalse(_mapper.CanMapNew<int[,], string[,]>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[,] {
					{ 2, -3, 0 },
					{ 1, 2, 5 }
				}));
			}
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
			Assert.IsFalse(_mapper.CanMapNew<string[], IEnumerable<ClassWithoutParameterlessConstructor>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<ClassWithoutParameterlessConstructor>>(new[] { "" }));
		}
	}

	[TestClass]
	public class NewCollectionMapperCanMapTests {
		[TestMethod]
		public void ShouldUseMappingOptions() {
			var mapper = new NewCollectionMapper(new NewMapper());

			Assert.IsFalse(mapper.CanMapNew<IEnumerable<string>, IEnumerable<int>>());

			var options = new CustomNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => 0);
			var mapper2 = new NewMapper(null, options);

			Assert.IsTrue(mapper.CanMapNew<IEnumerable<string>, IEnumerable<int>>(new []{ new MapperOverrideMappingOptions(mapper2) }));
		}
	}
}
