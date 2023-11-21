using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMapperGenericTests {
		public class Maps<T1, T2, T3> :
#if NET7_0_OR_GREATER
			INewMapStatic<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#else
			INewMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#endif
			{
#if NET7_0_OR_GREATER
			static
#endif
			(T1, T2, T3)
#if NET7_0_OR_GREATER
				INewMapStatic<Tuple<T1, T2>, (T1, T2, T3)>
#else
				INewMap<Tuple<T1, T2>, (T1, T2, T3)>
#endif
				.Map(Tuple<T1, T2> source, MappingContext context) {
				if (source == null)
					return (default(T1), default(T2), default(T3));
				return (source.Item1, source.Item2, default(T3));
			}
		}

		public class Maps<T1, T2> :
#if NET7_0_OR_GREATER
			INewMapStatic<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			INewMapStatic<T1[], T2[]>
#else
			INewMap<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			INewMap<T1[], T2[]>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			(T2, T1)
#if NET7_0_OR_GREATER
				INewMapStatic<Tuple<T1, T2>, (T2, T1)>
#else
				INewMap<Tuple<T1, T2>, (T2, T1)>
#endif
				.Map(Tuple<T1, T2> source, MappingContext context) {
				if(source == null)
					return (default(T2), default(T1));
				return (source.Item2, source.Item1);
			}

			// Rejects itself
#if NET7_0_OR_GREATER
			static
#endif
			T2[]
#if NET7_0_OR_GREATER
				INewMapStatic<T1[], T2[]>
#else
				INewMap<T1[], T2[]>
#endif
				.Map(T1[] source, MappingContext context) {
				
				throw new MapNotFoundException((typeof(T1[]), typeof(T2[])));
			}
		}

		public class Maps<T1> :
#if NET7_0_OR_GREATER
			INewMapStatic<IEnumerable<T1>, IList<T1>>,
			INewMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			INewMapStatic<IEnumerable<T1>, string>,
			INewMapStatic<int, IList<T1>>,
			INewMapStatic<Queue<T1>, string>,
			INewMapStatic<T1[], IList<T1>>
#else
			INewMap<IEnumerable<T1>, IList<T1>>,
			INewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			INewMap<IEnumerable<T1>, string>,
			INewMap<int, IList<T1>>,
			INewMap<Queue<T1>, string>,
			INewMap<T1[], IList<T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			IList<T1>
#if NET7_0_OR_GREATER
				INewMapStatic<IEnumerable<T1>, IList<T1>>
#else
				INewMap<IEnumerable<T1>, IList<T1>>
#endif
				.Map(IEnumerable<T1> source, MappingContext context) {
				return source?.ToList();
			}

#if NET7_0_OR_GREATER
			static
#endif
			IEnumerable<T1>
#if NET7_0_OR_GREATER
				INewMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#else
				INewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#endif
				.Map(IDictionary<string, IDictionary<int, IList<T1>>> source, MappingContext context) {
				return Enumerable.Empty<T1>();
			}

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				INewMapStatic<IEnumerable<T1>, string>
#else
				INewMap<IEnumerable<T1>, string>
#endif
				.Map(IEnumerable<T1> source, MappingContext context) {
				return "Elements: " + source?.Count();
			}

#if NET7_0_OR_GREATER
			static
#endif
			IList<T1>
#if NET7_0_OR_GREATER
				INewMapStatic<int, IList<T1>>
#else
				INewMap<int, IList<T1>>
#endif
				.Map(int source, MappingContext context) {
				return new T1[source];
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				INewMapStatic<Queue<T1>, string>
#else
				INewMap<Queue<T1>, string>
#endif
				.Map(Queue<T1> source, MappingContext context) {
				throw new NotImplementedException();
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			IList<T1>
#if NET7_0_OR_GREATER
				INewMapStatic<T1[], IList<T1>>
#else
				INewMap<T1[], IList<T1>>
#endif
				.Map(T1[] source, MappingContext context) {
				return context.Mapper.Map<IEnumerable<T1>, IList<T1>>(source);
			}
		}

		// Avoid error CS0695
		public class Maps :
#if NET7_0_OR_GREATER
			INewMapStatic<IEnumerable<bool>, IList<bool>>
#else
			INewMap<IEnumerable<bool>, IList<bool>>
#endif
			{

			// Specific map
#if NET7_0_OR_GREATER
			static
#endif
			IList<bool>
#if NET7_0_OR_GREATER
				INewMapStatic<IEnumerable<bool>, IList<bool>>
#else
				INewMap<IEnumerable<bool>, IList<bool>>
#endif
				.Map(IEnumerable<bool> source, MappingContext context) {
				return new List<bool>(32);
			}
		}


		public class MapsWithClassType<T1> :
#if NET7_0_OR_GREATER
			INewMapStatic<IEnumerable<T1>, int>,
			INewMapStatic<IList<T1>, int>
#else
			INewMap<IEnumerable<T1>, int>,
			INewMap<IList<T1>, int>
#endif
			where T1 : class {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IEnumerable<T1>, int>
#else
				INewMap<IEnumerable<T1>, int>
#endif
				.Map(IEnumerable<T1> source, MappingContext context) {
				return source?.Count() ?? 0;
			}

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, int>
#else
				INewMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return 42;
			}
		}

		public class MapsWithStructType<T1> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, int>
#else
			INewMap<IList<T1>, int>
#endif
			where T1 : struct {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, int>
#else
				INewMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return 36;
			}
		}
		
		public class MapsWithUnmanagedType<T1> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, int>
#else
			INewMap<IList<T1>, int>
#endif
			where T1 : unmanaged {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, int>
#else
				INewMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return 36;
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
			INewMapStatic<IList<T1>, int>
#else
			INewMap<IList<T1>, int>
#endif
			where T1 : new() {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, int>
#else
				INewMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, int>
#else
			INewMap<IList<T1>, int>
#endif
			where T1 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, int>
#else
				INewMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
#if NET7_0_OR_GREATER
			INewMapStatic<T1, T2>
#else
			INewMap<T1, T2>
#endif
			where T1 : List<T2> {
			public 
#if NET7_0_OR_GREATER
				static 
#endif
				T2 Map(T1 source, MappingContext context) {
				return default(T2);
			}
		}

		public class BaseClassTest : CustomCollection<Category>{}

		public class MapsWithInterfaceType<T1> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, int>
#else
			INewMap<IList<T1>, int>
#endif
			where T1 : IDisposable {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, int>
#else
				INewMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return 36;
			}
		}

		public class DisposableTest : IDisposable {
			public void Dispose() {
				throw new NotImplementedException();
			}
		}

		public class MapsWithInterfaceType<T1, T2> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, T2>
#else
			INewMap<IList<T1>, T2>
#endif
			where T1 : IEquatable<T2> {
			public 
#if NET7_0_OR_GREATER
				static 
#endif
				T2 Map(IList<T1> source, MappingContext context) {
				return default(T2);
			}
		}

		public class EquatableTest : IEquatable<Product> {
			public bool Equals(Product other) {
				return false;
			}
		}

		public class MapsWithGenericTypeParameterType<T1, T2> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, T2>
#else
			INewMap<IList<T1>, T2>
#endif
			where T1 : T2 {
			public 
#if NET7_0_OR_GREATER
				static 
#endif
				T2 Map(IList<T1> source, MappingContext context) {
				return default(T2);
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
#if NET7_0_OR_GREATER
			INewMapStatic<IList<T1>, Queue<T2>>
#else
			INewMap<IList<T1>, Queue<T2>>
#endif
			where T1 : T2 where T2 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Queue<T2>
#if NET7_0_OR_GREATER
				INewMapStatic<IList<T1>, Queue<T2>>
#else
				INewMap<IList<T1>, Queue<T2>>
#endif
				.Map(IList<T1> source, MappingContext context) {
				return new Queue<T2>();
			}
		}


		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new NewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			});
		}


		[TestMethod]
		public void ShouldMapGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{ 
					Assert.IsTrue(_mapper.CanMapNew<IEnumerable<string>, IList<string>>());

					var source = new[] { "Test" };
					var result = _mapper.Map<IEnumerable<string>, IList<string>>(source);

					Assert.IsNotNull(result);
					Assert.AreNotSame(source, result);
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("Test", result[0]);
				}

				// Class constraint
				{
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
					});

					Assert.IsTrue(mapper.CanMapNew<IEnumerable<Product>, int>());

					Assert.AreEqual(1, mapper.Map<IEnumerable<Product>, int>(new[] { new Product() }));
				}
			}

			// 2 parameters
			{
				// Different
				{
					Assert.IsTrue(_mapper.CanMapNew<Tuple<string, int>, ValueTuple<int, string>>());

					var result = _mapper.Map<Tuple<string, int>, ValueTuple<int, string>>(new Tuple<string, int>("Test", 4));
					Assert.AreEqual(4, result.Item1);
					Assert.AreEqual("Test", result.Item2);
				}

				// Equal
				{
					Assert.IsTrue(_mapper.CanMapNew<Tuple<string, string>, ValueTuple<string, string>>());

					var result = _mapper.Map<Tuple<string, string>, ValueTuple<string, string>>(new Tuple<string, string>("Test1", "Test2"));
					Assert.AreEqual("Test2", result.Item1);
					Assert.AreEqual("Test1", result.Item2);
				}
			}

			// 3 parameters
			{
				// Shared
				{
					Assert.IsTrue(_mapper.CanMapNew<Tuple<string, int>, ValueTuple<string, int, bool>>());

					var result = _mapper.Map<Tuple<string, int>, ValueTuple<string, int, bool>>(new Tuple<string, int>("Test", 2));
					Assert.AreEqual("Test", result.Item1);
					Assert.AreEqual(2, result.Item2);
					Assert.IsFalse(result.Item3);
				}
			}
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenericTypes() {
			// Types should be the same
			Assert.IsFalse(_mapper.CanMapNew<IEnumerable<string>, IList<int>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<string>, IList<int>>(new[] { "Test" }));
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenericConstraints() {
			// struct
			{
				var mapper = new NewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithStructType<>) }
				});

				Assert.IsFalse(mapper.CanMapNew<IList<Product>, int>());
				Assert.IsTrue(mapper.CanMapNew<IList<Guid>, int>());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>()));
				mapper.Map<IList<Guid>, int>(new List<Guid>());
				mapper.Map<IList<ManagedTest>, int>(new List<ManagedTest>());
				mapper.Map<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
			}

			// class
			{
				var mapper = new NewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
				});

				Assert.IsFalse(mapper.CanMapNew<IList<Guid>, int>());
				Assert.IsTrue(mapper.CanMapNew<IList<Product>, int>());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Guid>, int>(new List<Guid>()));
				mapper.Map<IList<Product>, int>(new List<Product>());
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new NewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				});

				Assert.IsFalse(mapper.CanMapNew<IList<Product>, int>());
				Assert.IsFalse(mapper.CanMapNew<IList<ManagedTest>, int>());
				Assert.IsTrue(mapper.CanMapNew<IList<UnmanagedTest>, int>());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>()));
				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ManagedTest>, int>(new List<ManagedTest>()));
				mapper.Map<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
				mapper.Map<IList<Guid>, int>(new List<Guid>());
				mapper.Map<IList<int>, int>(new List<int>());
			}

			// new()
			{
				var mapper = new NewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithNewType<>) }
				});

				Assert.IsFalse(mapper.CanMapNew<IList<ClassWithoutParameterlessConstructor>, int>());
				Assert.IsTrue(mapper.CanMapNew<IList<Product>, int>());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>()));
				mapper.Map<IList<Product>, int>(new List<Product>());
			}

			// base class
			{
				// Not generic
				{ 
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<>) }
					});

					Assert.IsFalse(mapper.CanMapNew<IList<Category>, int>());
					Assert.IsTrue(mapper.CanMapNew<IList<Product>, int>());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<List<Product>, int>(new List<Product>()));
					mapper.Map<IList<Product>, int>(new List<Product>());
					mapper.Map<IList<LimitedProduct>, int>(new List<LimitedProduct>());
				}

				// Generic
				{
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					});

					Assert.IsFalse(mapper.CanMapNew<Queue<Category>, Category>());
					Assert.IsTrue(mapper.CanMapNew<CustomCollection<Category>, Category>());
					Assert.IsTrue(mapper.CanMapNew<BaseClassTest, Category>());

					TestUtils.AssertMapNotFound(() => mapper.Map<Queue<Category>, Category>(new Queue<Category>()));
					mapper.Map<CustomCollection<Category>, Category>(new CustomCollection<Category>());
					mapper.Map<BaseClassTest, Category>(new BaseClassTest());
				}
			}

			// interface
			{
				// Not generic
				{ 
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<>) }
					});

					Assert.IsFalse(mapper.CanMapNew<IList<Category>, int>());
					Assert.IsTrue(mapper.CanMapNew<IList<DisposableTest>, int>());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>()));
					mapper.Map<IList<DisposableTest>, int>(new List<DisposableTest>());
				}

				// Generic
				{
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					});

					Assert.IsFalse(mapper.CanMapNew<IList<CustomCollection<Category>>, Category>());
					Assert.IsTrue(mapper.CanMapNew<IList<EquatableTest>, Product>());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Queue<Category>>, Category>(new List<Queue<Category>>()));
					mapper.Map<IList<EquatableTest>, Product>(new List<EquatableTest>());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					});

					Assert.IsFalse(mapper.CanMapNew<IList<Category>, Product>());
					Assert.IsTrue(mapper.CanMapNew<IList<CustomCollection<int>>, List<int>>());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Product>(new List<Category>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, LimitedProduct>(new List<Product>()));
					mapper.Map<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>());
					mapper.Map<IList<BaseClassTest>, List<Category>>(new List<BaseClassTest>());
					mapper.Map<IList<LimitedProduct>, Product>(new List<LimitedProduct>());
				}

				// Complex
				{
					var mapper = new NewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterComplexType<,>) }
					});

					Assert.IsFalse(mapper.CanMapNew<IList<Category>, Queue<Product>>());
					Assert.IsTrue(mapper.CanMapNew<IList<LimitedProduct>, Queue<Product>>());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Queue<Product>>(new List<Category>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, Queue<LimitedProduct>>(new List<Product>()));
					mapper.Map<IList<LimitedProduct>, Queue<Product>>(new List<LimitedProduct>());
					mapper.Map<IList<LimitedProduct>, Queue<LimitedProduct>>(new List<LimitedProduct>());
					mapper.Map<IList<Product>, Queue<Product>>(new List<Product>());
				}
			}
		}

		[TestMethod]
		public void ShouldMapSingleGenericType() {
			// Generic source
			{
				Assert.IsTrue(_mapper.CanMapNew<IEnumerable<int>, string>());

				Assert.AreEqual("Elements: 2", _mapper.Map<IEnumerable<int>, string>(new[] { 1, 2 }));
			}

			// Generic destination
			{
				Assert.IsTrue(_mapper.CanMapNew<int, IList<string>>());

				var list = _mapper.Map<int, IList<string>>(3);
				Assert.IsNotNull(list);
				Assert.AreEqual(3, list.Count);
				Assert.IsTrue(list.IsReadOnly);
				Assert.IsTrue(list.All(e => e == default));
			}
		}

		[TestMethod]
		public void ShouldMapDeepGenerics() {
			Assert.IsTrue(_mapper.CanMapNew<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>());

			// Does not throw
			_mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(new Dictionary<string, IDictionary<int, IList<bool>>>());
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingDeepGenerics() {
			// Types should be the same
			Assert.IsFalse(_mapper.CanMapNew<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(new Dictionary<string, IDictionary<int, IList<bool>>>()));
		}

		[TestMethod]
		public void ShouldRespectConstraints() {
			var mapper = new NewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MapsWithClassType<>), typeof(MapsWithStructType<>) }
			});

			Assert.AreEqual(42, mapper.Map<IList<Product>, int>(new List<Product>()));

			Assert.AreEqual(36, mapper.Map<IList<Guid>, int>(new List<Guid>()));
		}

		[TestMethod]
		public void ShouldPreferSpecificMaps() {
			var boolArray = new bool[] { true };
			var boolList = _mapper.Map<IEnumerable<bool>, IList<bool>>(boolArray);

			Assert.IsNotNull(boolList);
			Assert.AreNotSame(boolArray, boolList);
			Assert.AreEqual(0, boolList.Count);
			Assert.AreEqual(32, (boolList as List<bool>)?.Capacity);
		}

		[TestMethod]
		public void ShouldNotMapIfMapRejectsItself() {
			// CanMap returns true because the map does exist, even if it will fail
			Assert.IsTrue(_mapper.CanMapNew<float[], double[]>());

			var exc = TestUtils.AssertMapNotFound(() => _mapper.Map<double[]>(new []{ 1f }));
			Assert.AreEqual(exc.From, typeof(float[]));
			Assert.AreEqual(exc.To, typeof(double[]));
		}


		[TestMethod]
		public void ShouldMapCollections() {
			var mapper = new NewCollectionMapper(_mapper);

			{
				Assert.IsTrue(mapper.CanMapNew<Tuple<string, int>[], ValueTuple<int, string>[]>());

				var tuples = mapper.Map<ValueTuple<int, string>[]>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Length);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapNew<Tuple<string, int>[], IList<ValueTuple<int, string>>>());

				var tuples = mapper.Map<IList<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapNew<Tuple<string, int>[], LinkedList<ValueTuple<int, string>>>());

				var tuples = mapper.Map<LinkedList<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapNew<Tuple<string, int>[], Queue<ValueTuple<int, string>>>());

				var tuples = mapper.Map<Queue<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapNew<Tuple<string, int>[], Stack<ValueTuple<int, string>>>());

				var tuples = mapper.Map<Stack<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				// Order is inverted
				Assert.AreEqual(5, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(0).Item2);
				Assert.AreEqual(4, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(1).Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapNew<IEnumerable<Tuple<string, int>>, CustomCollection<ValueTuple<int, string>>>());

				var tuples = mapper.Map<CustomCollection<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}
		}
	}
}
