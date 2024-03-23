using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.Tests.Projection {
	[TestClass]
	public class CollectionProjectionTests {
		protected IProjector _projector = null;

		[TestInitialize]
		public void Initialize() {
			_projector = new CollectionProjector(new CustomProjector(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(ProjectionTests.Maps) }
			}));
		}


		[TestMethod]
		public void ShouldProjectCollections() {
			// Should forward options

			// No options
			{
				Assert.IsTrue(_projector.CanProject<int[], string[]>());

				MappingOptionsUtils.options = null;

				Expression<Func<int[], string[]>> expr = source => source == null ? null : source.Select(s => (s * 2).ToString()).ToArray();
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[], string[]>());

				Assert.IsNull(MappingOptionsUtils.options);
			}

			// Options
			{
				Assert.IsTrue(_projector.CanProject<int[], IList<string>>());

				MappingOptionsUtils.options = null;
				var opts = new TestOptions();

				Expression<Func<int[], IList<string>>> expr = source => source == null ? null : source.Select(s => (s * 2).ToString()).ToList();
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[], IList<string>>(opts));

				Assert.AreSame(opts, MappingOptionsUtils.options);
			}

			{
				Assert.IsTrue(_projector.CanProject<int[], LinkedList<string>>());

				Expression<Func<int[], LinkedList<string>>> expr = source => source == null ? null : new LinkedList<string>(source.Select(s => (s * 2).ToString()));
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[], LinkedList<string>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<int[], Queue<string>>());

				Expression<Func<int[], Queue<string>>> expr = source => source == null ? null : new Queue<string>(source.Select(s => (s * 2).ToString()));
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[], Queue<string>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<string[], SortedList<string, int>>());

				Expression<Func<string[], SortedList<string, int>>> expr = source => source == null ? null : new SortedList<string, int>(
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
					new Dictionary<string, int>(
#endif
					source.Select(s => new KeyValuePair<string, int>(s, s.Length))
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
					)
#else
					.ToDictionary(p => p.Key, p => p.Value)
#endif
					);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<string[], SortedList<string, int>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<int[], Stack<string>>());

				Expression<Func<int[], Stack<string>>> expr = source => source == null ? null : new Stack<string>(source.Select(s => (s * 2).ToString()));
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[], Stack<string>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<string[], ReadOnlyDictionary<string, int>>());

				Expression<Func<string[], ReadOnlyDictionary<string, int>>> expr = source => source == null ? null : new ReadOnlyDictionary<string, int>(
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
					new Dictionary<string, int>(
#endif
					source.Select(s => new KeyValuePair<string, int>(s, s.Length))
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
					)
#else
					.ToDictionary(p => p.Key, p => p.Value)
#endif
					);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<string[], ReadOnlyDictionary<string, int>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<string[], Dictionary<string, int>>());

				Expression<Func<string[], Dictionary<string, int>>> expr = source => source == null ? null : 
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
					new Dictionary<string, int>(
#endif
					source.Select(s => new KeyValuePair<string, int>(s, s.Length))
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
					)
#else
					.ToDictionary(p => p.Key, p => p.Value)
#endif
					;
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<string[], Dictionary<string, int>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<int[], CustomCollectionWithEnumerableConstructor<string>>());

				Expression<Func<int[], CustomCollectionWithEnumerableConstructor<string>>> expr = source => source == null ? null : new CustomCollectionWithEnumerableConstructor<string>(source.Select(s => (s * 2).ToString()));
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[], CustomCollectionWithEnumerableConstructor<string>>());
			}

			{
				Assert.IsTrue(_projector.CanProject<IList<int>, string>());

				Expression<Func<IList<int>, string>> expr = source => source == null ? null : new string(source.Select(s => (char)s).ToArray());
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<IList<int>, string>());
			}

			{
				Assert.IsTrue(_projector.CanProject<string, float[]>());

				Expression<Func<string, float[]>> expr = source => source == null ? null : source.Select(s => (float)s).ToArray();
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<string, float[]>());
			}
		}

		[TestMethod]
		public void ShouldNotProjectCustomCollectionsWithoutAppropriateConstructor() {
			{
				Assert.IsFalse(_projector.CanProject<int[], CustomCollection<string>>());

				TestUtils.AssertMapNotFound(() => _projector.Project<int[], CustomCollection<string>>());
			}
		}

		[TestMethod]
		public void ShouldProjectQueryable() {
			Assert.IsTrue(_projector.CanProject<IQueryable<int>, IQueryable<string>>());

			MappingOptionsUtils.options = null;
			Expression<Func<IQueryable<int>, IQueryable<string>>> expr = source => source.Select(s => (s * 2).ToString());
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<IQueryable<int>, IQueryable<string>>());
		}

		[TestMethod]
		public void ShouldNotProjectCollectionsWithoutElementsMap() {
			Assert.IsFalse(_projector.CanProject<int[], IEnumerable<Category>>());

			TestUtils.AssertMapNotFound(() => _projector.Project<int[], IEnumerable<Category>>());
		}

		[TestMethod]
		public void ShouldProjectCollectionsOfCollections() {
			// No options
			{
				Assert.IsTrue(_projector.CanProject<int[][], IList<IEnumerable<string>>>());

				MappingOptionsUtils.options = null;

				Expression<Func<int[][], IList<IEnumerable<string>>>> expr =
					source => source == null ? null : source.Select(s => s == null ? null : s.Select(ss => (ss * 2).ToString())).ToList();
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[][], IList<IEnumerable<string>>>());

				Assert.IsNull(MappingOptionsUtils.options);
			}

			// Options
			{
				Assert.IsTrue(_projector.CanProject<int[][], string[][]>());

				MappingOptionsUtils.options = null;
				var opts = new TestOptions();

				Expression<Func<int[][], string[][]>> expr =
					source => source == null ? null : source.Select(s => s == null ? null : s.Select(ss => (ss * 2).ToString()).ToArray()).ToArray();
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int[][], string[][]>(opts));

				Assert.AreSame(opts, MappingOptionsUtils.options);
			}
		}

		[TestMethod]
		public void ShouldNotProjectMultidimensionalArrays() {
			{
				Assert.IsFalse(_projector.CanProject<int[,], string[]>());

				TestUtils.AssertMapNotFound(() => _projector.Project<int[,], string[]>());
			}

			{
				Assert.IsFalse(_projector.CanProject<int[][], string[,]>());

				TestUtils.AssertMapNotFound(() => _projector.Project<int[][], string[,]>());
			}

			{
				Assert.IsFalse(_projector.CanProject<int[,], string[,]>());

				TestUtils.AssertMapNotFound(() => _projector.Project<int[,], string[,]>());
			}
		}

		[TestMethod]
		public void ShouldUseMappingOptions() {
			var projector = new CollectionProjector(new CustomProjector());

			Assert.IsFalse(projector.CanProject<IEnumerable<string>, IEnumerable<int>>());

			var options = new CustomProjectionAdditionalMapsOptions();
			options.AddMap<string, int>(c => s => 0);
			var projector2 = new CustomProjector(null, options);

			Assert.IsTrue(projector.CanProject<IEnumerable<string>, IEnumerable<int>>(new[] { new ProjectorOverrideMappingOptions(projector2) }));
		}

		[TestMethod]
		public void ShouldNotMapIfMapRejectsItself() {
			// CanProject returns true because the map does exist, even if it will fail
			Assert.IsTrue(_projector.CanProject<float[], double[]>());

			var exc = TestUtils.AssertMapNotFound(() => _projector.Project<float[], double[]>());
			Assert.AreEqual(typeof(float[]), exc.From);
			Assert.AreEqual(typeof(double[]), exc.To);
		}
	}
}
