using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.AsyncMapping {
	[TestClass]
	public class AsyncNewMapsGenericTests {
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
			IAsyncNewMapStatic<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			IAsyncNewMapStatic<T1[], T2[]>,
			IAsyncNewMapStatic<List<T1>, List<T2>>,
			ICanMapAsyncNewStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>,
			IAsyncNewMapStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>
#else
			IAsyncNewMap<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			IAsyncNewMap<T1[], T2[]>,
			IAsyncNewMap<List<T1>, List<T2>>,
			ICanMapAsyncNew<IEnumerable<T1>, CustomCollectionComplex<T2>>,
			IAsyncNewMap<IEnumerable<T1>, CustomCollectionComplex<T2>>
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

			// Rejects itself (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<T2[]>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<T1[], T2[]>
#else
				IAsyncNewMap<T1[], T2[]>
#endif
				.MapAsync(T1[] source, AsyncMappingContext context) {

				throw new MapNotFoundException((typeof(T1[]), typeof(T2[])));
			}

			// Rejects itself (awaited)
#if NET7_0_OR_GREATER
			static
#endif
			async Task<List<T2>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<List<T1>, List<T2>>
#else
				IAsyncNewMap<List<T1>, List<T2>>
#endif
				.MapAsync(List<T1> source, AsyncMappingContext context) {

				await Task.Delay(0);
				throw new MapNotFoundException((typeof(List<T1>), typeof(List<T2>)));
			}


#if NET7_0_OR_GREATER
			static
#endif
			bool
#if NET7_0_OR_GREATER
				ICanMapAsyncNewStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>
#else
				ICanMapAsyncNew<IEnumerable<T1>, CustomCollectionComplex<T2>>
#endif
				.CanMapAsyncNew(AsyncMappingContextOptions context) {

				return context.Mapper.CanMapAsyncNew<T1, T2>(context.MappingOptions);
			}

#if NET7_0_OR_GREATER
			static
#endif
			async Task<CustomCollectionComplex<T2>>
#if NET7_0_OR_GREATER
				IAsyncNewMapStatic<IEnumerable<T1>, CustomCollectionComplex<T2>>
#else
				IAsyncNewMap<IEnumerable<T1>, CustomCollectionComplex<T2>>
#endif
				.MapAsync(IEnumerable<T1> source, AsyncMappingContext context) {

				var coll = new CustomCollectionComplex<T2>();
				using (var factory = context.Mapper.MapAsyncNewFactory<T1, T2>(context.MappingOptions)) {
					foreach (var el in source) {
						coll.Add(await factory.Invoke(el, context.CancellationToken));
					}
				}

				return coll;
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
			_mapper = new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			});
		}


		[TestMethod]
		public async Task ShouldMapGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{
					Assert.IsTrue(_mapper.CanMapAsyncNew<IEnumerable<string>, IList<string>>());

					var source = new[] { "Test" };
					var result = await _mapper.MapAsync<IEnumerable<string>, IList<string>>(source);

					Assert.IsNotNull(result);
					Assert.AreNotSame(source, result);
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("Test", result[0]);
				}

				// Class constraint
				{
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
					});

					Assert.IsTrue(mapper.CanMapAsyncNew<IEnumerable<Product>, int>());

					Assert.AreEqual(1, await mapper.MapAsync<IEnumerable<Product>, int>(new[] { new Product() }));
				}
			}

			// 2 parameters
			{
				// Different
				{
					Assert.IsTrue(_mapper.CanMapAsyncNew<Tuple<string, int>, ValueTuple<int, string>>());

					var result = await _mapper.MapAsync<Tuple<string, int>, ValueTuple<int, string>>(new Tuple<string, int>("Test", 4));
					Assert.AreEqual(4, result.Item1);
					Assert.AreEqual("Test", result.Item2);
				}

				// Equal
				{
					Assert.IsTrue(_mapper.CanMapAsyncNew<Tuple<string, string>, ValueTuple<string, string>>());

					var result = await _mapper.MapAsync<Tuple<string, string>, ValueTuple<string, string>>(new Tuple<string, string>("Test1", "Test2"));
					Assert.AreEqual("Test2", result.Item1);
					Assert.AreEqual("Test1", result.Item2);
				}
			}

			// 3 parameters
			{
				// Shared
				{
					Assert.IsTrue(_mapper.CanMapAsyncNew<Tuple<string, int>, ValueTuple<string, int, bool>>());

					var result = await _mapper.MapAsync<Tuple<string, int>, ValueTuple<string, int, bool>>(new Tuple<string, int>("Test", 2));
					Assert.AreEqual("Test", result.Item1);
					Assert.AreEqual(2, result.Item2);
					Assert.IsFalse(result.Item3);
				}
			}
		}

		[TestMethod]
		public async Task ShouldCheckButNotMapOpenGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{
					Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(IEnumerable<>), typeof(IList<>)));

					await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => _mapper.MapAsync(new[] { "Test" }, typeof(IEnumerable<>), typeof(IList<>)));
				}

				// Class constraint
				{
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
					});

					Assert.IsTrue(mapper.CanMapAsyncNew(typeof(IEnumerable<>), typeof(int)));

					await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => mapper.MapAsync(new[] { new Product() }, typeof(IEnumerable<>), typeof(int)));
				}
			}

			// 2 parameters
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(Tuple<,>), typeof(ValueTuple<,>)));

				await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => _mapper.MapAsync(new Tuple<string, int>("Test", 4), typeof(Tuple<,>), typeof(ValueTuple<,>)));
			}

			// 3 parameters
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(Tuple<,>), typeof(ValueTuple<,,>)));

				await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => _mapper.MapAsync(new Tuple<string, int>("Test", 2), typeof(Tuple<,>), typeof(ValueTuple<,,>)));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingGenericTypes() {
			// Types should be the same
			Assert.IsFalse(_mapper.CanMapAsyncNew<IEnumerable<string>, IList<int>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<string>, IList<int>>(new[] { "Test" }));
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingGenericConstraints() {
			// struct
			{
				var mapper = new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithStructType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncNew<IList<Product>, int>());
				Assert.IsTrue(mapper.CanMapAsyncNew<IList<Guid>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, int>(new List<Product>()));
				await mapper.MapAsync<IList<Guid>, int>(new List<Guid>());
				await mapper.MapAsync<IList<ManagedTest>, int>(new List<ManagedTest>());
				await mapper.MapAsync<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
			}

			// class
			{
				var mapper = new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncNew<IList<Guid>, int>());
				Assert.IsTrue(mapper.CanMapAsyncNew<IList<Product>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Guid>, int>(new List<Guid>()));
				await mapper.MapAsync<IList<Product>, int>(new List<Product>());
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncNew<IList<Product>, int>());
				Assert.IsFalse(mapper.CanMapAsyncNew<IList<ManagedTest>, int>());
				Assert.IsTrue(mapper.CanMapAsyncNew<IList<UnmanagedTest>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, int>(new List<Product>()));
				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<ManagedTest>, int>(new List<ManagedTest>()));
				await mapper.MapAsync<IList<UnmanagedTest>, int>(new List<UnmanagedTest>());
				await mapper.MapAsync<IList<Guid>, int>(new List<Guid>());
				await mapper.MapAsync<IList<int>, int>(new List<int>());
			}

			// new()
			{
				var mapper = new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithNewType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncNew<IList<ClassWithoutParameterlessConstructor>, int>());
				Assert.IsTrue(mapper.CanMapAsyncNew<IList<Product>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>()));
				await mapper.MapAsync<IList<Product>, int>(new List<Product>());
			}

			// base class
			{
				// Not generic
				{ 
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncNew<IList<Category>, int>());
					Assert.IsTrue(mapper.CanMapAsyncNew<IList<Product>, int>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, int>(new List<Category>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<List<Product>, int>(new List<Product>()));
					await mapper.MapAsync<IList<Product>, int>(new List<Product>());
					await mapper.MapAsync<IList<LimitedProduct>, int>(new List<LimitedProduct>());
				}

				// Generic
				{
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncNew<Queue<Category>, Category>());
					Assert.IsTrue(mapper.CanMapAsyncNew<CustomCollection<Category>, Category>());
					Assert.IsTrue(mapper.CanMapAsyncNew<BaseClassTest, Category>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<Queue<Category>, Category>(new Queue<Category>()));
					await mapper.MapAsync<CustomCollection<Category>, Category>(new CustomCollection<Category>());
					await mapper.MapAsync<BaseClassTest, Category>(new BaseClassTest());
				}
			}

			// interface
			{
				// Not generic
				{ 
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncNew<IList<Category>, int>());
					Assert.IsTrue(mapper.CanMapAsyncNew<IList<DisposableTest>, int>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, int>(new List<Category>()));
					await mapper.MapAsync<IList<DisposableTest>, int>(new List<DisposableTest>());
				}

				// Generic
				{
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncNew<IList<CustomCollection<Category>>, Category>());
					Assert.IsTrue(mapper.CanMapAsyncNew<IList<EquatableTest>, Product>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Queue<Category>>, Category>(new List<Queue<Category>>()));
					await mapper.MapAsync< IList<EquatableTest>, Product>(new List<EquatableTest>());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncNew<IList<Category>, Product>());
					Assert.IsTrue(mapper.CanMapAsyncNew<IList<CustomCollection<int>>, List<int>>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, Product>(new List<Category>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, LimitedProduct>(new List<Product>()));
					await mapper.MapAsync<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>());
					await mapper.MapAsync<IList<BaseClassTest>, List<Category>>(new List<BaseClassTest>());
					await mapper.MapAsync<IList<LimitedProduct>, Product>(new List<LimitedProduct>());
				}

				// Complex
				{
					var mapper = new AsyncCustomMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterComplexType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncNew<IList<Category>, Queue<Product>>());
					Assert.IsTrue(mapper.CanMapAsyncNew<IList<LimitedProduct>, Queue<Product>>());

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
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<IEnumerable<int>, string>());

				Assert.AreEqual("Elements: 2", await _mapper.MapAsync<IEnumerable<int>, string>(new[] { 1, 2 }));
			}

			// Generic destination
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<int, IList<string>>());

				var list = await _mapper.MapAsync<int, IList<string>>(3);
				Assert.IsNotNull(list);
				Assert.AreEqual(3, list.Count);
				Assert.IsTrue(list.IsReadOnly);
				Assert.IsTrue(list.All(e => e == default));
			}
		}

		[TestMethod]
		public async Task ShouldCheckButNotMapSingleOpenGenericType() {
			// Generic source
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(IEnumerable<>), typeof(string)));

				await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => _mapper.MapAsync(new[] { 1, 2 }, typeof(IEnumerable<>), typeof(string)));
			}

			// Generic destination
			{
				Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(int), typeof(IList<>)));

				await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => _mapper.MapAsync(3, typeof(int), typeof(IList<>)));
			}
		}

		[TestMethod]
		public async Task ShouldMapDeepGenerics() {
			Assert.IsTrue(_mapper.CanMapAsyncNew<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>());

			// Does not throw
			await _mapper.MapAsync<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(new Dictionary<string, IDictionary<int, IList<bool>>>());
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingDeepGenerics() {
			// Types should be the same
			Assert.IsFalse(_mapper.CanMapAsyncNew<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(new Dictionary<string, IDictionary<int, IList<bool>>>()));
		}

		[TestMethod]
		public async Task ShouldRespectConstraints() {
			var mapper = new AsyncCustomMapper(new CustomMapsOptions {
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
			var mapper = new AsyncCollectionMapper(_mapper);

			{
				Assert.IsTrue(mapper.CanMapAsyncNew<Tuple<string, int>[], ValueTuple<int, string>[]>());

				var tuples = await mapper.MapAsync<ValueTuple<int, string>[]>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Length);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapAsyncNew<Tuple<string, int>[], IList<ValueTuple<int, string>>>());

				var tuples = await mapper.MapAsync<IList<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapAsyncNew<Tuple<string, int>[], LinkedList<ValueTuple<int, string>>>());

				var tuples = await mapper.MapAsync<LinkedList<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapAsyncNew<Tuple<string, int>[], Queue<ValueTuple<int, string>>>());

				var tuples = await mapper.MapAsync<Queue<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				Assert.IsTrue(mapper.CanMapAsyncNew<Tuple<string, int>[], Stack<ValueTuple<int, string>>>());

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
				Assert.IsTrue(mapper.CanMapAsyncNew<IEnumerable<Tuple<string, int>>, CustomCollection<ValueTuple<int, string>>>());

				var tuples = await mapper.MapAsync<CustomCollection<ValueTuple<int, string>>>(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) });

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}
		}


		[TestMethod]
		public async Task ShouldCheckCanMapAsyncIfPresent() {
			var nestedMapper = new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps) }
			});

			var options = new MappingOptions(new AsyncMapperOverrideMappingOptions(nestedMapper));

			Assert.IsTrue(_mapper.CanMapAsyncNew<IEnumerable<int>, CustomCollectionComplex<string>>(options));
			Assert.IsFalse(_mapper.CanMapAsyncNew<IEnumerable<int>, CustomCollectionComplex<decimal>>(options));

			var result = await _mapper.MapAsync<IEnumerable<int>, CustomCollectionComplex<string>>(new[] { 2, -3, 0 }, options);
			Assert.AreEqual(3, result.Elements.Count());
			Assert.AreEqual("4", result.Elements.ElementAt(0));
			Assert.AreEqual("-6", result.Elements.ElementAt(1));
			Assert.AreEqual("0", result.Elements.ElementAt(2));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<int>, CustomCollectionComplex<decimal>>(new[] { 2, -3, 0 }, options));
		}
	}
}
