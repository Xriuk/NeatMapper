using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;
using static NeatMapper.Tests.Mapping.GenericNewMapsTests;
using static System.Net.Mime.MediaTypeNames;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class GenericNewMapsTests {
		public class Maps<T1, T2> :
			INewMap<Tuple<T1, T2>, ValueTuple<T2, T1>> {
			static (T2, T1) INewMap<Tuple<T1, T2>, (T2, T1)>.Map(Tuple<T1, T2>? source, MappingContext context) {
				if(source == null)
					return (default(T2), default(T1))!;
				return (source.Item2, source.Item1)!;
			}
		}

		public class Maps<T1> :
			INewMap<IEnumerable<T1>, IList<T1>>,
			INewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			INewMap<IEnumerable<T1>, string>,
			INewMap<int, IList<T1>>{

			static IList<T1>? INewMap<IEnumerable<T1>, IList<T1>>.Map(IEnumerable<T1>? source, MappingContext context) {
				return source?.ToList();
			}

			static IEnumerable<T1>? INewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>.Map(IDictionary<string, IDictionary<int, IList<T1>>>? source, MappingContext context) {
				return Enumerable.Empty<T1>();
			}

			static string? INewMap<IEnumerable<T1>, string>.Map(IEnumerable<T1>? source, MappingContext context) {
				return "Elements: " + source?.Count();
			}

			static IList<T1>? INewMap<int, IList<T1>>.Map(int source, MappingContext context) {
				return new T1[source];
			}
		}

		// Avoid error CS0695
		public class Maps :
			INewMap<IEnumerable<bool>, IList<bool>> {

			// Specific map
			static IList<bool>? INewMap<IEnumerable<bool>, IList<bool>>.Map(IEnumerable<bool>? source, MappingContext context) {
				return new List<bool>(32);
			}
		}

		public class MapsWithClassType<T1> :
			INewMap<IEnumerable<T1>, int>,
			INewMap<IList<T1>, int> where T1 : class {

			static int INewMap<IEnumerable<T1>, int>.Map(IEnumerable<T1>? source, MappingContext context) {
				return source?.Count() ?? 0;
			}

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
				return 42;
			}
		}

		public class MapsWithStructType<T1> :
			INewMap<IList<T1>, int> where T1 : struct {

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithUnmanagedType<T1> :
			INewMap<IList<T1>, int> where T1 : unmanaged {

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
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
			INewMap<IList<T1>, int> where T1 : new() {

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1> :
			INewMap<IList<T1>, int> where T1 : Product {

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
			INewMap<T1, T2> where T1 : List<T2> {
			public static T2? Map(T1? source, MappingContext context) {
				return default(T2);
			}
		}

		public class MapsWithInterfaceType<T1> :
			INewMap<IList<T1>, int> where T1 : IDisposable {

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
				return 36;
			}
		}

		public class DisposableTest : IDisposable {
			public void Dispose() {
				throw new NotImplementedException();
			}
		}

		public class MapsWithInterfaceType<T1, T2> :
			INewMap<IList<T1>, T2> where T1 : IEquatable<T2> {
			public static T2? Map(IList<T1>? source, MappingContext context) {
				return default(T2);
			}
		}

		public class EquatableTest : IEquatable<Product> {
			public bool Equals(Product? other) {
				return false;
			}
		}

		public class MapsWithGenericTypeParameterType<T1, T2> :
			INewMap<IList<T1>, T2> where T1 : T2 {
			public static T2? Map(IList<T1>? source, MappingContext context) {
				return default(T2);
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
			INewMap<IList<T1>, int> where T1 : T2 where T2 : Product {

			static int INewMap<IList<T1>, int>.Map(IList<T1>? source, MappingContext context) {
				return 36;
			}
		}


		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			}), new ServiceCollection().BuildServiceProvider());
		}


		[TestMethod]
		public void ShouldMapGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{ 
					var source = new[] { "Test" };
					var result = _mapper.Map<IEnumerable<string>, IList<string>>(source);

					Assert.IsNotNull(result);
					Assert.AreNotSame(source, result);
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("Test", result[0]);
				}

				// Class constraint
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithClassType<>) }
					}), new ServiceCollection().BuildServiceProvider());
					Assert.AreEqual(1, mapper.Map<IEnumerable<Product>, int>(new[] { new Product() }));
				}
			}

			// 2 parameters
			{
				// Different
				{
					var result = _mapper.Map<Tuple<string, int>, ValueTuple<int, string>>(new Tuple<string, int>("Test", 4));
					Assert.AreEqual(4, result.Item1);
					Assert.AreEqual("Test", result.Item2);
				}

				// Equal
				{
					var result = _mapper.Map<Tuple<string, string>, ValueTuple<string, string>>(new Tuple<string, string>("Test1", "Test2"));
					Assert.AreEqual("Test2", result.Item1);
					Assert.AreEqual("Test1", result.Item2);
				}
			}
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenericTypes() {
			// Types should be the same
			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<string>, IList<int>>(new[] { "Test" }));
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenericConstraints() {
			// struct
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithStructType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>()));
				mapper.Map<IList<Guid>, int>(new List<Guid>());
			}

			// class
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithClassType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Guid>, int>(new List<Guid>()));
				mapper.Map<IList<Product>, int>(new List<Product>());
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>()));
				TestUtils.AssertMapNotFound(() => mapper.Map<IList<int?>, int>(new List<int?>()));
				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ManagedTest>, int>(new List<ManagedTest>()));
				mapper.Map<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
				mapper.Map<IList<Guid>, int>(new List<Guid>());
				mapper.Map<IList<int>, int>(new List<int>());
			}

			// new()
			{
				var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithNewType<>) }
				}), new ServiceCollection().BuildServiceProvider());

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>()));
				mapper.Map<IList<Product>, int>(new List<Product>());
			}

			// base class
			{
				// Not generic
				{ 
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithBaseClassType<>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>()));
					mapper.Map<IList<Product>, int>(new List<Product>());
					mapper.Map<IList<LimitedProduct>, int>(new List<LimitedProduct>());
				}

				// Generic
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<CustomCollection<Category>, Category>(new CustomCollection<Category>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<Queue<Category>, Category>(new Queue<Category>()));
					mapper.Map<CustomCollection<Category>, Category>(new CustomCollection<Category>());
				}
			}

			// interface
			{
				// Not generic
				{ 
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithInterfaceType<>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>()));
					mapper.Map<IList<DisposableTest>, int>(new List<DisposableTest>());
				}

				// Generic
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Queue<Category>>, Category>(new List<Queue<Category>>()));
					mapper.Map< IList<EquatableTest>, Product>(new List<EquatableTest>());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Product>(new List<Category>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, LimitedProduct>(new List<Product>()));
					mapper.Map<IList<LimitedProduct>, Product>(new List<LimitedProduct>());
				}

				// Complex
				{
					var mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					}), new ServiceCollection().BuildServiceProvider());

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Product>(new List<Category>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, LimitedProduct>(new List<Product>()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>()));
					mapper.Map<IList<LimitedProduct>, Product>(new List<LimitedProduct>());
				}
			}
		}

		[TestMethod]
		public void ShouldMapSingleGenericType() {
			// Generic source
			Assert.AreEqual("Elements: 2", _mapper.Map<IEnumerable<int>, string>(new[] { 1, 2 }));

			// Generic destination
			var list = _mapper.Map<int, IList<string>>(3);
			Assert.IsNotNull(list);
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.IsReadOnly);
			Assert.IsTrue(list.All(e => e == default));
		}

		[TestMethod]
		public void ShouldMapDeepGenerics() {
			var objectToMap = new Dictionary<string, IDictionary<int, IList<bool>>>();

			// Does not throw
			_mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(objectToMap);
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingDeepGenerics() {
			// Types should be the same
			TestUtils.AssertMapNotFound(() => _mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(new Dictionary<string, IDictionary<int, IList<bool>>>()));
		}

		[TestMethod]
		public void ShouldRespectConstraints() {
			Assert.AreEqual(42, _mapper.Map<IList<Product>, int>(new List<Product>()));

			Assert.AreEqual(36, _mapper.Map<IList<Guid>, int>(new List<Guid>()));
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
	}
}
