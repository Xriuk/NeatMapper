using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	[TestClass]
	public class AsyncNewMapperGenericTests {
		public class Maps<T1, T2, T3> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#else
			IAsyncNewMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#endif
			{
#if NET7_0_OR_GREATER
			static
#endif
			Task<(T1, T2, T3)>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Tuple<T1, T2>, (T1, T2, T3)>
#else
				IAsyncNewMap<Tuple<T1, T2>, (T1, T2, T3)>
#endif
				.MapAsync(Tuple<T1, T2> source, AsyncMappingContext context) {
				if (source == null)
					return Task.FromResult((default(T1), default(T2), default(T3)));
				return Task.FromResult((source.Item1, source.Item2, default(T3)));
			}
		}

		public class Maps<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<Tuple<T1, T2>, ValueTuple<T2, T1>>
#else
			IAsyncNewMap<Tuple<T1, T2>, ValueTuple<T2, T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Task<(T2, T1)>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Tuple<T1, T2>, (T2, T1)>
#else
				IAsyncNewMap<Tuple<T1, T2>, (T2, T1)>
#endif
				.MapAsync(Tuple<T1, T2> source, AsyncMappingContext context) {
				if(source == null)
					return Task.FromResult((default(T2), default(T1)));
				return Task.FromResult((source.Item2, source.Item1));
			}
		}

		public class Maps<T1> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IEnumerable<T1>, IList<T1>>,
			IAsyncNewMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IAsyncNewMapStatic<IEnumerable<T1>, string>,
			IAsyncNewMapStatic<int, IList<T1>>,
			IAsyncNewMapStatic<Queue<T1>, string>,
			IAsyncNewMapStatic<T1[], IList<T1>>
#else
			IAsyncNewMap<IEnumerable<T1>, IList<T1>>,
			IAsyncNewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IAsyncNewMap<IEnumerable<T1>, string>,
			IAsyncNewMap<int, IList<T1>>,
			IAsyncNewMap<Queue<T1>, string>,
			IAsyncNewMap<T1[], IList<T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<T1>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IEnumerable<T1>, IList<T1>>
#else
				IAsyncNewMap<IEnumerable<T1>, IList<T1>>
#endif
				.MapAsync(IEnumerable<T1> source, AsyncMappingContext context) {
				return Task.FromResult((IList<T1>)source?.ToList());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<IEnumerable<T1>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#else
				IAsyncNewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#endif
				.MapAsync(IDictionary<string, IDictionary<int, IList<T1>>> source, AsyncMappingContext context) {
				return Task.FromResult(Enumerable.Empty<T1>());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IEnumerable<T1>, string>
#else
				IAsyncNewMap<IEnumerable<T1>, string>
#endif
				.MapAsync(IEnumerable<T1> source, AsyncMappingContext context) {
				return Task.FromResult("Elements: " + source?.Count());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<T1>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<int, IList<T1>>
#else
				IAsyncNewMap<int, IList<T1>>
#endif
				.MapAsync(int source, AsyncMappingContext context) {
				return Task.FromResult((IList<T1>)new T1[source]);
			}

			// Throws exception (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<Queue<T1>, string>
#else
				IAsyncNewMap<Queue<T1>, string>
#endif
				.MapAsync(Queue<T1> source, AsyncMappingContext context) {
				throw new NotImplementedException();
			}

			// Nested NewMap (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<T1>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<T1[], IList<T1>>
#else
				IAsyncNewMap<T1[], IList<T1>>
#endif
				.MapAsync(T1[] source, AsyncMappingContext context) {
				return context.Mapper.MapAsync<IEnumerable<T1>, IList<T1>>(source);
			}
		}

		// Aasync Task error CS0695
		public class Maps :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IEnumerable<bool>, IList<bool>>
#else
			IAsyncNewMap<IEnumerable<bool>, IList<bool>>
#endif
			{

			// Specific map
#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<bool>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IEnumerable<bool>, IList<bool>>
#else
				IAsyncNewMap<IEnumerable<bool>, IList<bool>>
#endif
				.MapAsync(IEnumerable<bool> source, AsyncMappingContext context) {
				return Task.FromResult((IList<bool>)new List<bool>(32));
			}
		}


		public class MapsWithClassType<T1> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IEnumerable<T1>, int>,
			IAsyncNewMapStatic<IList<T1>, int>
#else
			IAsyncNewMap<IEnumerable<T1>, int>,
			IAsyncNewMap<IList<T1>, int>
#endif
			where T1 : class {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IEnumerable<T1>, int>
#else
				IAsyncNewMap<IEnumerable<T1>, int>
#endif
				.MapAsync(IEnumerable<T1> source, AsyncMappingContext context) {
				return Task.FromResult(source?.Count() ?? 0);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, int>
#else
				IAsyncNewMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(42);
			}
		}

		public class MapsWithStructType<T1> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, int>
#else
			IAsyncNewMap<IList<T1>, int>
#endif
			where T1 : struct {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, int>
#else
				IAsyncNewMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}
		
		public class MapsWithUnmanagedType<T1> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, int>
#else
			IAsyncNewMap<IList<T1>, int>
#endif
			where T1 : unmanaged {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, int>
#else
				IAsyncNewMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(36);
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
			IAsyncNewMapStatic<IList<T1>, int>
#else
			IAsyncNewMap<IList<T1>, int>
#endif
			where T1 : new() {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, int>
#else
				IAsyncNewMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}

		public class MapsWithBaseClassType<T1> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, int>
#else
			IAsyncNewMap<IList<T1>, int>
#endif
			where T1 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, int>
#else
				IAsyncNewMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<T1, T2>
#else
			IAsyncNewMap<T1, T2>
#endif
			where T1 : List<T2> {
			public 
#if NET7_0_OR_GREATER
				static 
#endif
				Task<T2> MapAsync(T1 source, AsyncMappingContext context) {
				return Task.FromResult(default(T2));
			}
		}

		public class BaseClassTest : CustomCollection<Category>{}

		public class MapsWithInterfaceType<T1> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, int>
#else
			IAsyncNewMap<IList<T1>, int>
#endif
			where T1 : IDisposable {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, int>
#else
				IAsyncNewMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}

		public class DisposableTest : IDisposable {
			public void Dispose() {
				throw new NotImplementedException();
			}
		}

		public class MapsWithInterfaceType<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, T2>
#else
			IAsyncNewMap<IList<T1>, T2>
#endif
			where T1 : IEquatable<T2> {
			public 
#if NET7_0_OR_GREATER
				static 
#endif
				Task<T2> MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(default(T2));
			}
		}

		public class EquatableTest : IEquatable<Product> {
			public bool Equals(Product other) {
				return false;
			}
		}

		public class MapsWithGenericTypeParameterType<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, T2>
#else
			IAsyncNewMap<IList<T1>, T2>
#endif
			where T1 : T2 {
			public 
#if NET7_0_OR_GREATER
				static 
#endif
				Task<T2> MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(default(T2));
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncNewMapStatic<IList<T1>, Queue<T2>>
#else
			IAsyncNewMap<IList<T1>, Queue<T2>>
#endif
			where T1 : T2 where T2 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Task<Queue<T2>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IList<T1>, Queue<T2>>
#else
				IAsyncNewMap<IList<T1>, Queue<T2>>
#endif
				.MapAsync(IList<T1> source, AsyncMappingContext context) {
				return Task.FromResult(new Queue<T2>());
			}
		}


		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			});
		}


		[TestMethod]
		public async Task ShouldMapGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{ 
					var source = new[] { "Test" };
					var result = await _mapper.MapAsync<IEnumerable<string>, IList<string>>(source);

					Assert.IsNotNull(result);
					Assert.AreNotSame(source, result);
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("Test", result[0]);
				}

				// Class constraint
				{
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
					});
					Assert.AreEqual(1, await mapper.MapAsync<IEnumerable<Product>, int>(new[] { new Product() }));
				}
			}

			// 2 parameters
			{
				// Different
				{
					var result = await _mapper.MapAsync<Tuple<string, int>, ValueTuple<int, string>>(new Tuple<string, int>("Test", 4));
					Assert.AreEqual(4, result.Item1);
					Assert.AreEqual("Test", result.Item2);
				}

				// Equal
				{
					var result = await _mapper.MapAsync<Tuple<string, string>, ValueTuple<string, string>>(new Tuple<string, string>("Test1", "Test2"));
					Assert.AreEqual("Test2", result.Item1);
					Assert.AreEqual("Test1", result.Item2);
				}
			}

			// 3 parameters
			{
				// Shared
				{
					var result = await _mapper.MapAsync<Tuple<string, int>, ValueTuple<string, int, bool>>(new Tuple<string, int>("Test", 2));
					Assert.AreEqual("Test", result.Item1);
					Assert.AreEqual(2, result.Item2);
					Assert.IsFalse(result.Item3);
				}
			}
		}

		[TestMethod]
		public Task ShouldNotMapNotMatchingGenericTypes() {
			// Types should be the same
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<string>, IList<int>>(new[] { "Test" }));
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingGenericConstraints() {
			// struct
			{
				var mapper = new AsyncNewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithStructType<>) }
				});

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, int>(new List<Product>()));
				await mapper.MapAsync<IList<Guid>, int>(new List<Guid>());
				await mapper.MapAsync<IList<ManagedTest>, int>(new List<ManagedTest>());
				await mapper.MapAsync<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
			}

			// class
			{
				var mapper = new AsyncNewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
				});
				
				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Guid>, int>(new List<Guid>()));
				await mapper.MapAsync<IList<Product>, int>(new List<Product>());
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new AsyncNewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				});

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, int>(new List<Product>()));
				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<ManagedTest>, int>(new List<ManagedTest>()));
				await mapper.MapAsync<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
				await mapper.MapAsync<IList<Guid>, int>(new List<Guid>());
				await mapper.MapAsync<IList<int>, int>(new List<int>());
			}

			// new()
			{
				var mapper = new AsyncNewMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithNewType<>) }
				});

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>()));
				await mapper.MapAsync<IList<Product>, int>(new List<Product>());
			}

			// base class
			{
				// Not generic
				{ 
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<>) }
					});

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, int>(new List<Category>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<List<Product>, int>(new List<Product>()));
					await mapper.MapAsync<IList<Product>, int>(new List<Product>());
					await mapper.MapAsync<IList<LimitedProduct>, int>(new List<LimitedProduct>());
				}

				// Generic
				{
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					});

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<Queue<Category>, Category>(new Queue<Category>()));
					await mapper.MapAsync<CustomCollection<Category>, Category>(new CustomCollection<Category>());
					await mapper.MapAsync<BaseClassTest, Category>(new BaseClassTest());
				}
			}

			// interface
			{
				// Not generic
				{ 
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<>) }
					});

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, int>(new List<Category>()));
					await mapper.MapAsync<IList<DisposableTest>, int>(new List<DisposableTest>());
				}

				// Generic
				{
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					});

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Queue<Category>>, Category>(new List<Queue<Category>>()));
					await mapper.MapAsync< IList<EquatableTest>, Product>(new List<EquatableTest>());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					});

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, Product>(new List<Category>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, LimitedProduct>(new List<Product>()));
					await mapper.MapAsync<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>());
					await mapper.MapAsync<IList<BaseClassTest>, List<Category>>(new List<BaseClassTest>());
					await mapper.MapAsync<IList<LimitedProduct>, Product>(new List<LimitedProduct>());
				}

				// Complex
				{
					var mapper = new AsyncNewMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterComplexType<,>) }
					});

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, Queue<Product>>(new List<Category>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, Queue<LimitedProduct>>(new List<Product>()));
					await mapper.MapAsync<IList<LimitedProduct>, Queue<Product>>(new List<LimitedProduct>());
					await mapper.MapAsync<IList<LimitedProduct>, Queue<LimitedProduct>>(new List<LimitedProduct>());
					await mapper.MapAsync<IList<Product>, Queue<Product>>(new List<Product>());
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapSingleGenericType() {
			// Generic source
			Assert.AreEqual("Elements: 2", await _mapper.MapAsync<IEnumerable<int>, string>(new[] { 1, 2 }));

			// Generic destination
			var list = await _mapper.MapAsync<int, IList<string>>(3);
			Assert.IsNotNull(list);
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.IsReadOnly);
			Assert.IsTrue(list.All(e => e == default));
		}

		[TestMethod]
		public Task ShouldMapDeepGenerics() {
			// Does not throw
			return _mapper.MapAsync<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(new Dictionary<string, IDictionary<int, IList<bool>>>());
		}

		[TestMethod]
		public Task ShouldNotMapNotMatchingDeepGenerics() {
			// Types should be the same
			return TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(new Dictionary<string, IDictionary<int, IList<bool>>>()));
		}

		[TestMethod]
		public async Task ShouldRespectConstraints() {
			var mapper = new AsyncNewMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MapsWithClassType<>), typeof(MapsWithStructType<>) }
			});

			Assert.AreEqual(42, await mapper.MapAsync<IList<Product>, int>(new List<Product>()));

			Assert.AreEqual(36, await mapper.MapAsync<IList<Guid>, int>(new List<Guid>()));
		}

		[TestMethod]
		public async Task ShouldPreferSpecificMaps() {
			var boolArray = new bool[] { true };
			var boolList = await _mapper.MapAsync<IEnumerable<bool>, IList<bool>>(boolArray);

			Assert.IsNotNull(boolList);
			Assert.AreNotSame(boolArray, boolList);
			Assert.AreEqual(0, boolList.Count);
			Assert.AreEqual(32, (boolList as List<bool>)?.Capacity);
		}


		[TestMethod]
		public async Task ShouldMapCollections() {
			var mapper = new AsyncNewCollectionMapper(_mapper);

			{
				var tuples = await mapper.MapAsync<ValueTuple<int, string>[]>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Length);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				var tuples = await mapper.MapAsync<IList<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				var tuples = await mapper.MapAsync<LinkedList<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				var tuples = await mapper.MapAsync<Queue<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				var tuples = await mapper.MapAsync<Stack<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				// Order is inverted
				Assert.AreEqual(5, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(0).Item2);
				Assert.AreEqual(4, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(1).Item2);
			}

			{
				var tuples = await mapper.MapAsync<CustomCollection<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

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
