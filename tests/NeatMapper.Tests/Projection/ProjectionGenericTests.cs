using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.Tests.Projection {
	[TestClass]
	public class ProjectionGenericTests {
		public class Maps<T1, T2, T3> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#else
			IProjectionMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#endif
			{
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#else
				IProjectionMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#endif
				.Project(ProjectionContext context) {

				return source => source == null ? ValueTuple.Create(default(T1), default(T2), default(T3)) : ValueTuple.Create(source.Item1, source.Item2, default(T3));
			}
		}

		public class Maps<T1, T2> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<Tuple<T1, T2>, ValueTuple<T2, T1>>
#else
			IProjectionMap<Tuple<T1, T2>, ValueTuple<T2, T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Tuple<T1, T2>, (T2, T1)>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Tuple<T1, T2>, (T2, T1)>
#else
				IProjectionMap<Tuple<T1, T2>, (T2, T1)>
#endif
				.Project(ProjectionContext context) {

				return source => source == null ? ValueTuple.Create(default(T2), default(T1)) : ValueTuple.Create(source.Item2, source.Item1);
			}
		}

		public class Maps<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IEnumerable<T1>, IList<T1>>,
			IProjectionMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IProjectionMapStatic<IEnumerable<T1>, string>,
			IProjectionMapStatic<int, IList<T1>>,
			IProjectionMapStatic<Queue<T1>, string>,
			IProjectionMapStatic<T1[], IList<T1>>
#else
			IProjectionMap<IEnumerable<T1>, IList<T1>>,
			IProjectionMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IProjectionMap<IEnumerable<T1>, string>,
			IProjectionMap<int, IList<T1>>,
			IProjectionMap<Queue<T1>, string>,
			IProjectionMap<T1[], IList<T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IEnumerable<T1>, IList<T1>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IEnumerable<T1>, IList<T1>>
#else
				IProjectionMap<IEnumerable<T1>, IList<T1>>
#endif
				.Project(ProjectionContext context) {

				return source => source != null ? source.ToList() : null;
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#else
				IProjectionMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#endif
				.Project(ProjectionContext context) {

				return source => Enumerable.Empty<T1>();
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IEnumerable<T1>, string>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IEnumerable<T1>, string>
#else
				IProjectionMap<IEnumerable<T1>, string>
#endif
				.Project(ProjectionContext context) {
				
				return source => "Elements: " + (source != null ? (int?)source.Count() : null);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<int, IList<T1>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<int, IList<T1>>
#else
				IProjectionMap<int, IList<T1>>
#endif
				.Project(ProjectionContext context) {

				return source => new T1[source];
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Queue<T1>, string>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Queue<T1>, string>
#else
				IProjectionMap<Queue<T1>, string>
#endif
				.Project(ProjectionContext context) {

				throw new NotImplementedException();
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<T1[], IList<T1>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<T1[], IList<T1>>
#else
				IProjectionMap<T1[], IList<T1>>
#endif
				.Project(ProjectionContext context) {

				return source => context.Projector.Project<IEnumerable<T1>, IList<T1>>(source);
			}
		}

		// Avoid error CS0695
		public class Maps :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IEnumerable<bool>, IList<bool>>
#else
			IProjectionMap<IEnumerable<bool>, IList<bool>>
#endif
			{

			// Specific map
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IEnumerable<bool>, IList<bool>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IEnumerable<bool>, IList<bool>>
#else
				IProjectionMap<IEnumerable<bool>, IList<bool>>
#endif
				.Project(ProjectionContext context) {

				return source => new List<bool>(32);
			}
		}


		public class MapsWithClassType<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IEnumerable<T1>, int>,
			IProjectionMapStatic<IList<T1>, int>
#else
			IProjectionMap<IEnumerable<T1>, int>,
			IProjectionMap<IList<T1>, int>
#endif
			where T1 : class {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IEnumerable<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IEnumerable<T1>, int>
#else
				IProjectionMap<IEnumerable<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => source != null ? source.Count() : 0;
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, int>
#else
				IProjectionMap<IList<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => 42;
			}
		}

		public class MapsWithStructType<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, int>
#else
			IProjectionMap<IList<T1>, int>
#endif
			where T1 : struct {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, int>
#else
				IProjectionMap<IList<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => 36;
			}
		}

		public class MapsWithUnmanagedType<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, int>
#else
			IProjectionMap<IList<T1>, int>
#endif
			where T1 : unmanaged {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, int>
#else
				IProjectionMap<IList<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => 36;
			}
		}

		public struct UnmanagedTest {
			public int TestI;
			public bool TestB;
		}

		public struct ManagedTest {
			public int TestI;
			public bool TestB;
			public Product TestP;
		}

		public class MapsWithNewType<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, int>
#else
			IProjectionMap<IList<T1>, int>
#endif
			where T1 : new() {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, int>
#else
				IProjectionMap<IList<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => 36;
			}
		}

		public class MapsWithBaseClassType<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, int>
#else
			IProjectionMap<IList<T1>, int>
#endif
			where T1 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, int>
#else
				IProjectionMap<IList<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => 36;
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<T1, T2>
#else
			IProjectionMap<T1, T2>
#endif
			where T1 : List<T2> {
			public
#if NET7_0_OR_GREATER
				static 
#endif
				Expression<Func<T1, T2>> Project(ProjectionContext context) {

				return source => default(T2);
			}
		}

		public class BaseClassTest : CustomCollection<Category> { }

		public class MapsWithInterfaceType<T1> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, int>
#else
			IProjectionMap<IList<T1>, int>
#endif
			where T1 : IDisposable {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, int>
#else
				IProjectionMap<IList<T1>, int>
#endif
				.Project(ProjectionContext context) {

				return source => 36;
			}
		}

		public class DisposableTest : IDisposable {
			public void Dispose() {
				throw new NotImplementedException();
			}
		}

		public class MapsWithInterfaceType<T1, T2> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, T2>
#else
			IProjectionMap<IList<T1>, T2>
#endif
			where T1 : IEquatable<T2> {
			public
#if NET7_0_OR_GREATER
				static 
#endif
				Expression<Func<IList<T1>, T2>> Project(ProjectionContext context) {

				return source => default(T2);
			}
		}

		public class EquatableTest : IEquatable<Product> {
			public bool Equals(Product other) {
				return false;
			}
		}

		public class MapsWithGenericTypeParameterType<T1, T2> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, T2>
#else
			IProjectionMap<IList<T1>, T2>
#endif
			where T1 : T2 {
			public
#if NET7_0_OR_GREATER
				static 
#endif
				Expression<Func<IList<T1>, T2>> Project(ProjectionContext context) {
				
				return source => default(T2);
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<IList<T1>, Queue<T2>>
#else
			IProjectionMap<IList<T1>, Queue<T2>>
#endif
			where T1 : T2 where T2 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<IList<T1>, Queue<T2>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<IList<T1>, Queue<T2>>
#else
				IProjectionMap<IList<T1>, Queue<T2>>
#endif
				.Project(ProjectionContext context) {

				return source => new Queue<T2>();
			}
		}

		IProjector _projector = null;

		[TestInitialize]
		public void Initialize() {
			_projector = new CustomProjector(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			});
		}


		[TestMethod]
		public void ShouldProjectGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{
					Assert.IsTrue(_projector.CanProject<IEnumerable<string>, IList<string>>());

					Expression<Func<IEnumerable<string>, IList<string>>> expr = source => source != null ? source.ToList() : null;
					TestUtils.AssertExpressionsEqual(expr, _projector.Project<IEnumerable<string>, IList<string>>());
				}

				// Class constraint
				{
					var projector = new CustomProjector(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
					});

					Assert.IsTrue(projector.CanProject<IEnumerable<Product>, int>());

					Expression<Func<IEnumerable<Product>, int>> expr = source => source != null ? source.Count() : 0;
					TestUtils.AssertExpressionsEqual(expr, projector.Project<IEnumerable<Product>, int>());
				}
			}

			// 2 parameters
			{
				// Different
				{
					Assert.IsTrue(_projector.CanProject<Tuple<string, int>, ValueTuple<int, string>>());

					Expression<Func<Tuple<string, int>, ValueTuple<int, string>>> expr = source => source == null ? ValueTuple.Create(default(int), default(string)) : ValueTuple.Create(source.Item2, source.Item1);
					TestUtils.AssertExpressionsEqual(expr, _projector.Project<Tuple<string, int>, ValueTuple<int, string>>());
				}

				// Equal
				{
					Assert.IsTrue(_projector.CanProject<Tuple<string, string>, ValueTuple<string, string>>());

					Expression<Func<Tuple<string, string>, ValueTuple<string, string>>> expr = source => source == null ? ValueTuple.Create(default(string), default(string)) : ValueTuple.Create(source.Item2, source.Item1);
					TestUtils.AssertExpressionsEqual(expr, _projector.Project<Tuple<string, string>, ValueTuple<string, string>>());
				}
			}

			// 3 parameters
			{
				// Shared
				{
					Assert.IsTrue(_projector.CanProject<Tuple<string, int>, ValueTuple<string, int, bool>>());

					Expression<Func<Tuple<string, int>, ValueTuple<string, int, bool>>> expr = source => source == null ? ValueTuple.Create(default(string), default(int), default(bool)) : ValueTuple.Create(source.Item1, source.Item2, default(bool));
					TestUtils.AssertExpressionsEqual(expr, _projector.Project<Tuple<string, int>, ValueTuple<string, int, bool>>());
				}
			}
		}
	}
}
