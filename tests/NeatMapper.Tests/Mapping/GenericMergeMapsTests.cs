using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class GenericMergeMapsTests {
		public class Maps<T1, T2, T3> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<Tuple<T1, T2>, ValueTuple<T1, T2, T3>> 
#else
			IMergeMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#endif
			{ 

#if NET7_0_OR_GREATER
			static
#endif
			(T1, T2, T3)
#if NET7_0_OR_GREATER
				IMergeMapStatic<Tuple<T1, T2>, (T1, T2, T3)>
#else
				IMergeMap<Tuple<T1, T2>, (T1, T2, T3)>
#endif
				.Map(Tuple<T1, T2> source, (T1, T2, T3) destination, MappingContext context) {
				if (source == null)
					return (default(T1), default(T2), default(T3));
				return (source.Item1, source.Item2, default(T3));
			}
		}

		public class Maps<T1, T2> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			IMergeMapStatic<GenericClass<T1>, GenericClassDto<T2>>,
			IMatchMapStatic<GenericClass<T1>, GenericClassDto<T2>>
#else
			IMergeMap<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			IMergeMap<GenericClass<T1>, GenericClassDto<T2>>,
			IMatchMap<GenericClass<T1>, GenericClassDto<T2>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			(T2, T1)
#if NET7_0_OR_GREATER
				IMergeMapStatic<Tuple<T1, T2>, (T2, T1)>
#else
				IMergeMap<Tuple<T1, T2>, (T2, T1)>
#endif
				.Map(Tuple<T1, T2> source, (T2, T1) destination, MappingContext context) {
				if (source == null)
					return (default(T2), default(T1));
				return (source.Item2, source.Item1);
			}

#if NET7_0_OR_GREATER
			static
#endif
			GenericClassDto<T2>
#if NET7_0_OR_GREATER
				IMergeMapStatic<GenericClass<T1>, GenericClassDto<T2>>
#else
				IMergeMap<GenericClass<T1>, GenericClassDto<T2>>
#endif
				.Map(GenericClass<T1> source, GenericClassDto<T2> destination, MappingContext context) {
				if (source != null) {
					if(destination == null)
						destination = new GenericClassDto<T2>();
					destination.Id = source.Id;
					destination.Value = context.Mapper.Map(source.Value, destination.Value);
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<GenericClass<T1>, GenericClassDto<T2>>
#else
				IMatchMap<GenericClass<T1>, GenericClassDto<T2>>
#endif
				.Match(GenericClass<T1> source, GenericClassDto<T2> destination, MatchingContext context) {
				return source?.Id == destination?.Id;
			}
		}

		public class Maps<T1> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IEnumerable<T1>, IList<T1>>,
			IMergeMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IMergeMapStatic<IEnumerable<T1>, string>,
			IMergeMapStatic<int, IList<T1>>,
			IMergeMapStatic<Queue<T1>, string>,
			IMergeMapStatic<T1[], IList<T1>>
#else
			IMergeMap<IEnumerable<T1>, IList<T1>>,
			IMergeMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IMergeMap<IEnumerable<T1>, string>,
			IMergeMap<int, IList<T1>>,
			IMergeMap<Queue<T1>, string>,
			IMergeMap<T1[], IList<T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			IList<T1>
#if NET7_0_OR_GREATER
				IMergeMapStatic<IEnumerable<T1>, IList<T1>>
#else
				IMergeMap<IEnumerable<T1>, IList<T1>>
#endif
				.Map(IEnumerable<T1> source, IList<T1> destination, MappingContext context) {
				if (destination == null)
					destination = new List<T1>();
				destination.Clear();
				if (source != null) { 
					foreach(var el in source) {
						destination.Add(el);
					}
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			IEnumerable<T1>
#if NET7_0_OR_GREATER
				IMergeMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#else
				IMergeMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#endif
				.Map(IDictionary<string, IDictionary<int, IList<T1>>> source, IEnumerable<T1> destination, MappingContext context) {
				return Enumerable.Empty<T1>();
			}

#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<IEnumerable<T1>, string>
#else
				IMergeMap<IEnumerable<T1>, string>
#endif
				.Map(IEnumerable<T1> source, string destination, MappingContext context) {
				return "Elements: " + source?.Count();
			}

#if NET7_0_OR_GREATER
			static
#endif
			IList<T1>
#if NET7_0_OR_GREATER
				IMergeMapStatic<int, IList<T1>>
#else
				IMergeMap<int, IList<T1>>
#endif
				.Map(int source, IList<T1> destination, MappingContext context) {
				return new T1[source];
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			string
#if NET7_0_OR_GREATER
				IMergeMapStatic<Queue<T1>, string>
#else
				IMergeMap<Queue<T1>, string>
#endif
				.Map(Queue<T1> source, string destination, MappingContext context) {
				throw new NotImplementedException();
			}

			// Nested NewMap
#if NET7_0_OR_GREATER
			static
#endif
			IList<T1>
#if NET7_0_OR_GREATER
				IMergeMapStatic<T1[], IList<T1>>
#else
				IMergeMap<T1[], IList<T1>>
#endif
				.Map(T1[] source, IList<T1> destination, MappingContext context) {
				return context.Mapper.Map<IEnumerable<T1>, IList<T1>>(source);
			}
		}

		// Avoid error CS0695
		public class Maps :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IEnumerable<bool>, IList<bool>>,
			IMergeMapStatic<Category, CategoryDto>,
			IMergeMapStatic<Product, ProductDto>,
			IMatchMapStatic<GenericClass<Product>, GenericClassDto<ProductDto>>
#else
			IMergeMap<IEnumerable<bool>, IList<bool>>,
			IMergeMap<Category, CategoryDto>,
			IMergeMap<Product, ProductDto>,
			IMatchMap<GenericClass<Product>, GenericClassDto<ProductDto>>
#endif
			{

			// Specific map
#if NET7_0_OR_GREATER
			static
#endif
			IList<bool>
#if NET7_0_OR_GREATER
				IMergeMapStatic<IEnumerable<bool>, IList<bool>>
#else
				IMergeMap<IEnumerable<bool>, IList<bool>>
#endif
				.Map(IEnumerable<bool> source, IList<bool> destination, MappingContext context) {
				return new List<bool>(32);
			}

#if NET7_0_OR_GREATER
			static
#endif
			CategoryDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<Category, CategoryDto>
#else
				IMergeMap<Category, CategoryDto>
#endif
				.Map(Category source, CategoryDto destination, MappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = source.Parent?.Id;
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			ProductDto
#if NET7_0_OR_GREATER
				IMergeMapStatic<Product, ProductDto>
#else
				IMergeMap<Product, ProductDto>
#endif
				.Map(Product source, ProductDto destination, MappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new ProductDto();
					destination.Code = source.Code;
				}
				return destination;
			}

#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				IMatchMapStatic<GenericClass<Product>, GenericClassDto<ProductDto>>
#else
				IMatchMap<GenericClass<Product>, GenericClassDto<ProductDto>>
#endif
				.Match(GenericClass<Product> source, GenericClassDto<ProductDto> destination, MatchingContext context) {
				return source?.Value.Code == destination?.Value.Code;
			}
		}


		public class MapsWithClassType<T1> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IEnumerable<T1>, int>,
			IMergeMapStatic<IList<T1>, int>
#else
			IMergeMap<IEnumerable<T1>, int>,
			IMergeMap<IList<T1>, int>
#endif
			 where T1 : class {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IEnumerable<T1>, int>
#else
				IMergeMap<IEnumerable<T1>, int>
#endif
				.Map(IEnumerable<T1> source, int destination, MappingContext context) {
				return source?.Count() ?? 0;
			}

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, int>
#else
				IMergeMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, int destination, MappingContext context) {
				return 42;
			}
		}

		public class MapsWithStructType<T1> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IList<T1>, int>
#else
			IMergeMap<IList<T1>, int>
#endif
			 where T1 : struct {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, int>
#else
				IMergeMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithUnmanagedType<T1> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IList<T1>, int>
#else
			IMergeMap<IList<T1>, int>
#endif
			where T1 : unmanaged {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, int>
#else
				IMergeMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, int destination, MappingContext context) {
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
			IMergeMapStatic<IList<T1>, int>
#else
			IMergeMap<IList<T1>, int>
#endif
			where T1 : new() {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, int>
#else
				IMergeMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IList<T1>, int>
#else
			IMergeMap<IList<T1>, int>
#endif
			where T1 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, int>
#else
				IMergeMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, int destination, MappingContext context) {
				return 36;
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<T1, T2>
#else
			IMergeMap<T1, T2>
#endif
			where T1 : List<T2> {
			public
#if NET7_0_OR_GREATER
				static
#endif
				T2 Map(T1 source, T2 destination, MappingContext context) {
				return default(T2);
			}
		}

		public class BaseClassTest : CustomCollection<Category> { }

		public class MapsWithInterfaceType<T1> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IList<T1>, int>
#else
			IMergeMap<IList<T1>, int>
#endif
			where T1 : IDisposable {

#if NET7_0_OR_GREATER
			static
#endif
			int
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, int>
#else
				IMergeMap<IList<T1>, int>
#endif
				.Map(IList<T1> source, int destination, MappingContext context) {
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
			IMergeMapStatic<IList<T1>, T2>
#else
			IMergeMap<IList<T1>, T2>
#endif
			where T1 : IEquatable<T2> {
			public
#if NET7_0_OR_GREATER
				static
#endif
				T2 Map(IList<T1> source, T2 destination, MappingContext context) {
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
			IMergeMapStatic<IList<T1>, T2>
#else
			IMergeMap<IList<T1>, T2>
#endif
			where T1 : T2 {
			public
#if NET7_0_OR_GREATER
				static
#endif
				T2 Map(IList<T1> source, T2 destination, MappingContext context) {
				return default(T2);
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
#if NET7_0_OR_GREATER
			IMergeMapStatic<IList<T1>, Queue<T2>>
#else
			IMergeMap<IList<T1>, Queue<T2>>
#endif
			where T1 : T2 where T2 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Queue<T2>
#if NET7_0_OR_GREATER
				IMergeMapStatic<IList<T1>, Queue<T2>>
#else
				IMergeMap<IList<T1>, Queue<T2>>
#endif
				.Map(IList<T1> source, Queue<T2> destination, MappingContext context) {
				return new Queue<T2>();
			}
		}


		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			});
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
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithClassType<>) }
					});
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
				var mapper = new Mapper(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithStructType<>) }
				});

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>(), 0));
				mapper.Map<IList<Guid>, int>(new List<Guid>(), 0);
				mapper.Map<IList<ManagedTest>, int>(new List<ManagedTest>(), 0);
				mapper.Map<IList<UnmanagedTest>, int>(new List<UnmanagedTest>(), 0);
			}

			// class
			{
				var mapper = new Mapper(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithClassType<>) }
				});

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Guid>, int>(new List<Guid>(), 0));
				mapper.Map<IList<Product>, int>(new List<Product>(), 0);
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new Mapper(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				});

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, int>(new List<Product>(), 2));
				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ManagedTest>, int>(new List<ManagedTest>(), 2));
				mapper.Map<IList<UnmanagedTest>, int>(new List<UnmanagedTest>(), 2);
				mapper.Map<IList<Guid>, int>(new List<Guid>(), 2);
				mapper.Map<IList<int>, int>(new List<int>(), 2);
			}

			// new()
			{
				var mapper = new Mapper(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(MapsWithNewType<>) }
				});

				TestUtils.AssertMapNotFound(() => mapper.Map<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>(), 42));
				mapper.Map<IList<Product>, int>(new List<Product>(), 42);
			}

			// base class
			{
				// Not generic
				{
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithBaseClassType<>) }
					});

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>(), 42));
					TestUtils.AssertMapNotFound(() => mapper.Map<List<Product>, int>(new List<Product>(), 42));
					mapper.Map<IList<Product>, int>(new List<Product>(), 42);
					mapper.Map<IList<LimitedProduct>, int>(new List<LimitedProduct>(), 42);
				}

				// Generic
				{
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					});

					TestUtils.AssertMapNotFound(() => mapper.Map<Queue<Category>, Category>(new Queue<Category>(), new Category()));
					mapper.Map<CustomCollection<Category>, Category>(new CustomCollection<Category>(), new Category());
					mapper.Map<BaseClassTest, Category>(new BaseClassTest(), new Category());
				}
			}

			// interface
			{
				// Not generic
				{
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithInterfaceType<>) }
					});

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, int>(new List<Category>(), 36));
					mapper.Map<IList<DisposableTest>, int>(new List<DisposableTest>(), 36);
				}

				// Generic
				{
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					});

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>(), new Category()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Queue<Category>>, Category>(new List<Queue<Category>>(), new Category()));
					mapper.Map<IList<EquatableTest>, Product>(new List<EquatableTest>(), new Product());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					});

					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Category>, Product>(new List<Category>(), new Product()));
					TestUtils.AssertMapNotFound(() => mapper.Map<IList<Product>, LimitedProduct>(new List<Product>(), new LimitedProduct()));
					mapper.Map<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>(), new List<int>());
					mapper.Map<IList<BaseClassTest>, List<Category>>(new List<BaseClassTest>(), new List<Category>());
					mapper.Map<IList<LimitedProduct>, Product>(new List<LimitedProduct>(), new Product());
				}

				// Complex
				{
					var mapper = new Mapper(new MapperConfigurationOptions {
						ScanTypes = new List<Type> { typeof(MapsWithGenericTypeParameterComplexType<,>) }
					});

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
			var mapper = new Mapper(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(MapsWithClassType<>), typeof(MapsWithStructType<>) }
			});

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
		public void ShouldMapCollectionsWithGenericElementsComparer() {
			var a = new GenericClassDto<CategoryDto> {
				Id = 2,
				Value = new CategoryDto {
					Id = 2,
					Parent = 2
				}
			};
			var b = new GenericClassDto<CategoryDto> {
				Id = 3,
				Value = new CategoryDto {
					Id = 3
				}
			};
			var c = new GenericClassDto<CategoryDto> {
				Id = 5,
				Value = new CategoryDto {
					Id = 5
				}
			};
			var destination = new CustomCollection<GenericClassDto<CategoryDto>> { a, b, c };
			var result = _mapper.Map(new[] {
				new GenericClass<Category>{
					Id = 3,
					Value = new Category {
						Id = 3,
						Parent = new Category {
							Id = 7
						}
					}
				},
				new GenericClass<Category>{
					Id = 2,
					Value = new Category {
						Id = 2
					}
				},
				new GenericClass<Category>{
					Id = 6,
					Value = new Category {
						Id = 6
					}
				}
			}, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreSame(a, result[0]);
			Assert.IsNull(result[0].Value.Parent);
			Assert.AreSame(b, result[1]);
			Assert.AreEqual(7, result[1].Value.Parent);
			Assert.AreEqual(6, result[2].Id);
		}

		[TestMethod]
		public void ShouldMapCollectionsWithSpecificElementsComparer() {
			var pa = new ProductDto {
				Code = "Test1"
			};
			var a = new GenericClassDto<ProductDto> {
				Id = 2,
				Value = pa
			};
			var pb = new ProductDto {
				Code = "Test2"
			};
			var b = new GenericClassDto<ProductDto> {
				Id = 3,
				Value = pb
			};
			var pc = new ProductDto {
				Code = "Test3"
			};
			var c = new GenericClassDto<ProductDto> {
				Id = 5,
				Value = pc
			};
			var destination = new CustomCollection<GenericClassDto<ProductDto>> { a, b, c };
			var result = _mapper.Map(new[] {
				new GenericClass<Product>{
					Id = 3,
					Value = new Product {
						Code = "Test3"
					}
				},
				new GenericClass<Product>{
					Id = 2,
					Value = new Product {
						Code = "Test4"
					}
				},
				new GenericClass<Product>{
					Id = 6,
					Value = new Product {
						Code = "Test1"
					}
				}
			}, destination);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreSame(a, result[0]);
			Assert.AreSame(pa, result[0].Value);
			Assert.AreEqual(6, result[0].Id);
			Assert.AreSame(c, result[1]);
			Assert.AreSame(pc, result[1].Value);
			Assert.AreEqual(3, result[1].Id);
			Assert.AreEqual(2, result[2].Id);
			Assert.AreEqual("Test4", result[2].Value.Code);
		}

		[TestMethod]
		public void ShouldMapCollectionsWithCustomElementsComparer() {
			var pa = new ProductDto {
				Code = "Test1"
			};
			var a = new GenericClassDto<ProductDto> {
				Id = 2,
				Value = pa
			};
			var pb = new ProductDto {
				Code = "Test2"
			};
			var b = new GenericClassDto<ProductDto> {
				Id = 3,
				Value = pb
			};
			var pc = new ProductDto {
				Code = "Test3"
			};
			var c = new GenericClassDto<ProductDto> {
				Id = 5,
				Value = pc
			};
			var destination = new CustomCollection<GenericClassDto<ProductDto>> { a, b, c };
			var result = _mapper.Map(new[] {
				new GenericClass<Product>{
					Id = 3,
					Value = new Product {
						Code = "Test3"
					}
				},
				new GenericClass<Product>{
					Id = 2,
					Value = new Product {
						Code = "Test4"
					}
				},
				new GenericClass<Product>{
					Id = 6,
					Value = new Product {
						Code = "Test1"
					}
				}
			}, destination, (s, d, _) => s?.Id == d?.Id);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreSame(a, result.ElementAt(0));
			Assert.AreSame(pa, result.ElementAt(0).Value);
			Assert.AreEqual("Test4", result.ElementAt(0).Value.Code);
			Assert.AreSame(b, result.ElementAt(1));
			Assert.AreSame(pb, result.ElementAt(1).Value);
			Assert.AreEqual("Test3", result.ElementAt(1).Value.Code);
			Assert.AreEqual(6, result.ElementAt(2).Id);
			Assert.AreEqual("Test1", result.ElementAt(2).Value.Code);
		}
	}
}
