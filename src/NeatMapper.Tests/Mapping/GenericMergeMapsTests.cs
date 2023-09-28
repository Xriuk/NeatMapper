using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class GenericMergeMapsTests {
		public class Maps<T1, T2, T3> :
			IMergeMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>> {
			static (T1, T2, T3) IMergeMap<Tuple<T1, T2>, (T1, T2, T3)>.Map(Tuple<T1, T2>? source, (T1, T2, T3) destination, MappingContext context) {
				if (source == null)
					return (default(T1), default(T2), default(T3))!;
				return (source.Item1, source.Item2, default(T3))!;
			}
		}

		public class Maps<T1, T2> :
			IMergeMap<Tuple<T1, T2>, ValueTuple<T2, T1>>{

			static (T2, T1) IMergeMap<Tuple<T1, T2>, (T2, T1)>.Map(Tuple<T1, T2>? source, (T2, T1) destination, MappingContext context) {
				if (source == null)
					return (default(T2), default(T1))!;
				return (source.Item2, source.Item1)!;
			}
		}

		public class Maps<T1> :
			IMergeMap<IEnumerable<T1>, IList<T1>>,
			IMergeMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IMergeMap<IEnumerable<T1>, string>,
			IMergeMap<int, IList<T1>>,
			IMergeMap<Queue<T1>, string>,
			IMergeMap<T1[], IList<T1>> {

			static IList<T1>? IMergeMap<IEnumerable<T1>, IList<T1>>.Map(IEnumerable<T1>? source, IList<T1>? destination, MappingContext context) {
				destination ??= new List<T1>();
				destination.Clear();
				if (source != null) { 
					foreach(var el in source) {
						destination.Add(el);
					}
				}
				return destination;
			}

			static IEnumerable<T1>? IMergeMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>.Map(IDictionary<string, IDictionary<int, IList<T1>>>? source, IEnumerable<T1>? destination, MappingContext context) {
				return Enumerable.Empty<T1>();
			}

			static string? IMergeMap<IEnumerable<T1>, string>.Map(IEnumerable<T1>? source, string? destination, MappingContext context) {
				return "Elements: " + source?.Count();
			}

			static IList<T1>? IMergeMap<int, IList<T1>>.Map(int source, IList<T1>? destination, MappingContext context) {
				return new T1[source];
			}

			// Throws exception
			static string? IMergeMap<Queue<T1>, string>.Map(Queue<T1>? source, string? destination, MappingContext context) {
				throw new NotImplementedException();
			}

			// Nested NewMap
			static IList<T1>? IMergeMap<T1[], IList<T1>>.Map(T1[]? source, IList<T1>? destination, MappingContext context) {
				return context.Mapper.Map<IEnumerable<T1>, IList<T1>>(source);
			}
		}

		// Avoid error CS0695
		public class Maps :
			IMergeMap<IEnumerable<bool>, IList<bool>> {

			// Specific map
			static IList<bool>? IMergeMap<IEnumerable<bool>, IList<bool>>.Map(IEnumerable<bool>? source, IList<bool>? destination, MappingContext context) {
				return new List<bool>(32);
			}
		}


		public class MapsWithClassType<T1> :
			IMergeMap<IEnumerable<T1>, int>,
			IMergeMap<IList<T1>, int> where T1 : class {

			static int IMergeMap<IEnumerable<T1>, int>.Map(IEnumerable<T1>? source, int destination, MappingContext context) {
				return source?.Count() ?? 0;
			}

			static int IMergeMap<IList<T1>, int>.Map(IList<T1>? source, int destination, MappingContext context) {
				return 42;
			}
		}

		public class MapsWithStructType<T1> :
			IMergeMap<IList<T1>, int> where T1 : struct {

			static int IMergeMap<IList<T1>, int>.Map(IList<T1>? source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithUnmanagedType<T1> :
			IMergeMap<IList<T1>, int> where T1 : unmanaged {

			static int IMergeMap<IList<T1>, int>.Map(IList<T1>? source, int destination, MappingContext context) {
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
			IMergeMap<IList<T1>, int> where T1 : new() {

			static int IMergeMap<IList<T1>, int>.Map(IList<T1>? source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1> :
			IMergeMap<IList<T1>, int> where T1 : Product {

			static int IMergeMap<IList<T1>, int>.Map(IList<T1>? source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
			IMergeMap<T1, T2> where T1 : List<T2> {
			public static T2? Map(T1? source, T2? destination, MappingContext context) {
				return default(T2);
			}
		}

		public class BaseClassTest : CustomCollection<Category> { }

		public class MapsWithInterfaceType<T1> :
			IMergeMap<IList<T1>, int> where T1 : IDisposable {

			static int IMergeMap<IList<T1>, int>.Map(IList<T1>? source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class DisposableTest : IDisposable {
			public void Dispose() {
				throw new NotImplementedException();
			}
		}

		public class MapsWithInterfaceType<T1, T2> :
			IMergeMap<IList<T1>, T2> where T1 : IEquatable<T2> {
			public static T2? Map(IList<T1>? source, T2? destination, MappingContext context) {
				return default(T2);
			}
		}

		public class EquatableTest : IEquatable<Product> {
			public bool Equals(Product? other) {
				return false;
			}
		}

		public class MapsWithGenericTypeParameterType<T1, T2> :
			IMergeMap<IList<T1>, T2> where T1 : T2 {
			public static T2? Map(IList<T1>? source, T2? destination, MappingContext context) {
				return default(T2);
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
			IMergeMap<IList<T1>, Queue<T2>> where T1 : T2 where T2 : Product {

			static Queue<T2>? IMergeMap<IList<T1>, Queue<T2>>.Map(IList<T1>? source, Queue<T2>? destination, MappingContext context) {
				return new Queue<T2>();
			}
		}


		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			}), new ServiceCollection().BuildServiceProvider());
		}


		[TestMethod]
		public void ShouldMapGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{
					var source = new[] { "Test" };
					var destination = new List<string>();
					var result = _mapper.Map<IEnumerable<string>, IList<string>>(source, destination);

					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("Test", result[0]);
				}

				// Class constraint
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithClassType<>) }
					}), new ServiceCollection().BuildServiceProvider());
					Assert.AreEqual(1, mapper.Map<IEnumerable<Product>, int>(new[] { new Product() }, 0));
				}
			}

			// 2 parameters
			{
				// Different
				{
					var result = _mapper.Map<Tuple<string, int>, ValueTuple<int, string>>(new Tuple<string, int>("Test", 4), (0, "AAA"));
					Assert.AreEqual(4, result.Item1);
					Assert.AreEqual("Test", result.Item2);
				}

				// Equal
				{
					var result = _mapper.Map<Tuple<string, string>, ValueTuple<string, string>>(new Tuple<string, string>("Test1", "Test2"), ("AAA", "BBB"));
					Assert.AreEqual("Test2", result.Item1);
					Assert.AreEqual("Test1", result.Item2);
				}
			}

			// 3 parameters
			{
				// Shared
				{
					var result = _mapper.Map<Tuple<string, int>, ValueTuple<string, int, bool>>(new Tuple<string, int>("Test", 2), ("AAA", 0, true));
					Assert.AreEqual("Test", result.Item1);
					Assert.AreEqual(2, result.Item2);
					Assert.IsFalse(result.Item3);
				}
			}
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenericTypes() {
			// Types should be the same
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<string>, IList<int>>(new[] { "Test" }, new List<int>()));
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenericConstraints() {
			// struct
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithStructType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>(), 0));
				mapper.Map<IList<Guid>, int>(new List<Guid>(), 0);
			}

			// class
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithClassType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Guid>, int>(new List<Guid>(), 0));
				mapper.Map<IList<Product>, int>(new List<Product>(), 0);
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>(), 2));
				TestUtils.AssertMapNotFound(() => mapper.Map<IList<int?>, int>(new List<int?>(), 2));
				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ManagedTest>, int>(new List<ManagedTest>(), 2));
				mapper.Map<IList<UnmanagedTest>, int>(new List<UnmanagedTest>(), 2);
				mapper.Map<IList<Guid>, int>(new List<Guid>(), 2);
				mapper.Map<IList<int>, int>(new List<int>(), 2);
			}

			// new()
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithNewType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>(), 42));
				mapper.Map<IList<Product>, int>(new List<Product>(), 42);
			}

			// base class
			{
				// Not generic
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithBaseClassType<>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>(), 42));
					TestUtils.AssertMapNotFound(() => mapper.Map<List<Product>, int>(new List<Product>(), 42));
					mapper.Map<IList<Product>, int>(new List<Product>(), 42);
					mapper.Map<IList<LimitedProduct>, int>(new List<LimitedProduct>(), 42);
				}

				// Generic
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<Queue<Category>, Category>(new Queue<Category>(), new Category()));
					mapper.Map<CustomCollection<Category>, Category>(new CustomCollection<Category>(), new Category());
					mapper.Map<BaseClassTest, Category>(new BaseClassTest(), new Category());
				}
			}

			// interface
			{
				// Not generic
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithInterfaceType<>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>(), 36));
					mapper.Map<IList<DisposableTest>, int>(new List<DisposableTest>(), 36);
				}

				// Generic
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>(), new Category()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Queue<Category>>, Category>(new List<Queue<Category>>(), new Category()));
					mapper.Map<IList<EquatableTest>, Product>(new List<EquatableTest>(), new Product());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Product>(new List<Category>(), new Product()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, LimitedProduct>(new List<Product>(), new LimitedProduct()));
					mapper.Map<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>(), new List<int>());
					mapper.Map<IList<BaseClassTest>, List<Category>>(new List<BaseClassTest>(), new List<Category>());
					mapper.Map<IList<LimitedProduct>, Product>(new List<LimitedProduct>(), new Product());
				}

				// Complex
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithGenericTypeParameterComplexType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Queue<Product>>(new List<Category>(), new Queue<Product>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, Queue<LimitedProduct>>(new List<Product>(), new Queue<LimitedProduct>()));
					mapper.Map<IList<LimitedProduct>, Queue<Product>>(new List<LimitedProduct>(), new Queue<Product>());
					mapper.Map<IList<LimitedProduct>, Queue<LimitedProduct>>(new List<LimitedProduct>(), new Queue<LimitedProduct>());
					mapper.Map<IList<Product>, Queue<Product>>(new List<Product>(), new Queue<Product>());
				}
			}
		}

		[TestMethod]
		public void ShouldMapSingleGenericType() {
			// Generic source
			Assert.AreEqual("Elements: 2", _mapper.Map<IEnumerable<int>, string>(new[] { 1, 2 }, "a"));

			// Generic destination
			var list = _mapper.Map<int, IList<string>>(3, new List<string>());
			Assert.IsNotNull(list);
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.IsReadOnly);
			Assert.IsTrue(list.All(e => e == default));
		}

		[TestMethod]
		public void ShouldMapDeepGenerics() {
			var objectToMap = new Dictionary<string, IDictionary<int, IList<bool>>>();

			// Does not throw
			_mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(objectToMap, new List<bool>());
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingDeepGenerics() {
			// Types should be the same
			TestUtils.AssertMapNotFound(() => _mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(new Dictionary<string, IDictionary<int, IList<bool>>>(), new List<float>()));
		}

		[TestMethod]
		public void ShouldRespectConstraints() {
			var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(MapsWithClassType<>), typeof(MapsWithStructType<>) }
			}), new ServiceCollection().BuildServiceProvider());

			Assert.AreEqual(42, mapper.Map<IList<Product>, int>(new List<Product>(), 2));

			Assert.AreEqual(36, mapper.Map<IList<Guid>, int>(new List<Guid>(), 3));
		}

		[TestMethod]
		public void ShouldPreferSpecificMaps() {
			var boolArray = new bool[] { true };
			var boolList = _mapper.Map<IEnumerable<bool>, IList<bool>>(boolArray, new List<bool>());

			Assert.IsNotNull(boolList);
			Assert.AreNotSame(boolArray, boolList);
			Assert.AreEqual(0, boolList.Count);
			Assert.AreEqual(32, (boolList as List<bool>)?.Capacity);
		}


		[TestMethod]
		public void ShouldMapCollectionsWithoutElementsComparer() {
			{
				var tuples = _mapper.Map(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) }, new List<ValueTuple<int, string>>());

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				var tuples = _mapper.Map(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) }, new LinkedList<ValueTuple<int, string>>());

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				var tuples = _mapper.Map(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) }, new CustomCollection<ValueTuple<int, string>>());

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}
		}

		[TestMethod]
		public void ShouldMapCollectionsWithNormalElementsComparer() {
			throw new NotImplementedException();
		}

		[TestMethod]
		public void ShouldMapCollectionsWithGenericElementsComparer() {
			throw new NotImplementedException();
		}

		[TestMethod]
		public void ShouldMapCollectionsWithCustomElementsComparer() {
			throw new NotImplementedException();
		}
	}
}
