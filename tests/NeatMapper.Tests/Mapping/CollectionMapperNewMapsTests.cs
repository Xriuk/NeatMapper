using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class CollectionMapperNewMapsTests {
		protected IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new CollectionMapper(new CustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(NewMapsTests.Maps), typeof(MergeMapsTests.Maps) }
			}));
		}


		[TestMethod]
		public void ShouldMapCollections() {
			// Should forward options except merge.matcher

			// No options
			{
				Assert.IsTrue(_mapper.CanMapNew<int[], string[]>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.contexts.Clear();
				MappingOptionsUtils.mergeOptions = null;
				var strings = _mapper.Map<string[]>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);

				Assert.IsNull(MappingOptionsUtils.options);
				// Elements should share the same context
				Assert.AreEqual(3, MappingOptionsUtils.contexts.Count);
				Assert.AreEqual(1, MappingOptionsUtils.contexts.Distinct().Count());
				Assert.IsNull(MappingOptionsUtils.mergeOptions);

				using(var factory = _mapper.MapNewFactory<int[], string[]>()) { 
					var strings2 = factory.Invoke(new[] { 2, -3, 0 });

					Assert.IsNotNull(strings2);
					Assert.AreEqual(3, strings2.Length);
					Assert.AreEqual("4", strings2[0]);
					Assert.AreEqual("-6", strings2[1]);
					Assert.AreEqual("0", strings2[2]);
				}
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

				Assert.IsNotNull(MappingOptionsUtils.options);
				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge with matcher)
			{
				Assert.IsTrue(_mapper.CanMapNew<int[], LinkedList<string>>());

				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;
				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				var strings = _mapper.Map<LinkedList<string>>(new[] { 2, -3, 0 }, new object[] { opts, merge });

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
		public void ShouldCheckButNotMapOpenCollections() {
			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(int[]), typeof(Queue<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(new[] { 2, -3, 0 }, typeof(int[]), typeof(Queue<>)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(string[]), typeof(SortedList<,>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(new[] { "A", "BB", "CCC" }, typeof(string[]), typeof(SortedList<,>)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(IEnumerable<>), typeof(Stack<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(new[] { 2, -3, 0 }, typeof(IEnumerable<>), typeof(Stack<>)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(string[]), typeof(ReadOnlyDictionary<,>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(new[] { "A", "BB", "CCC" }, typeof(string[]), typeof(ReadOnlyDictionary<,>)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(IList<>), typeof(CustomCollection<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(new[] { 2, -3, 0 }, typeof(IList<>), typeof(CustomCollection<>)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(IList<>), typeof(string)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(new[] { 104, 101, 108, 108, 111 }, typeof(IList<>), typeof(string)));
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

				Assert.IsNotNull(MappingOptionsUtils.options);
				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (merge with matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				_mapper.Map<IList<IEnumerable<string>>>(new[] {
					new[]{ 2, -3, 0 },
					new[]{ 1, 2 }
				}, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
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
	public class CollectionMapperCanMapNewTests {
		[TestMethod]
		public void ShouldUseMappingOptions() {
			var mapper = new CollectionMapper(new CustomMapper());

			Assert.IsFalse(mapper.CanMapNew<IEnumerable<string>, IEnumerable<int>>());

			var options = new CustomNewAdditionalMapsOptions();
			options.AddMap<string, int>((s, _) => 0);
			var mapper2 = new CustomMapper(null, options);

			Assert.IsTrue(mapper.CanMapNew<IEnumerable<string>, IEnumerable<int>>(new []{ new MapperOverrideMappingOptions(mapper2) }));
		}
	}
}
