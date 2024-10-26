using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	[TestClass]
	public class AsyncMergeMapperGenericTests {
		public class Maps<T1, T2, T3> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<Tuple<T1, T2>, ValueTuple<T1, T2, T3>> 
#else
			IAsyncMergeMap<Tuple<T1, T2>, ValueTuple<T1, T2, T3>>
#endif
			{ 

#if NET7_0_OR_GREATER
			static
#endif
			Task<(T1, T2, T3)>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Tuple<T1, T2>, (T1, T2, T3)>
#else
				IAsyncMergeMap<Tuple<T1, T2>, (T1, T2, T3)>
#endif
				.MapAsync(Tuple<T1, T2> source, (T1, T2, T3) destination, AsyncMappingContext context) {
				if (source == null)
					return Task.FromResult((default(T1), default(T2), default(T3)));
				return Task.FromResult((source.Item1, source.Item2, default(T3)));
			}
		}

		public class Maps<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			IAsyncMergeMapStatic<GenericClass<T1>, GenericClassDto<T2>>,
			IMatchMapStatic<GenericClass<T1>, GenericClassDto<T2>>,
			IAsyncMergeMapStatic<T1[], T2[]>,
			IAsyncMergeMapStatic<List<T1>, List<T2>>
#else
			IAsyncMergeMap<Tuple<T1, T2>, ValueTuple<T2, T1>>,
			IAsyncMergeMap<GenericClass<T1>, GenericClassDto<T2>>,
			IMatchMap<GenericClass<T1>, GenericClassDto<T2>>,
			IAsyncMergeMap<T1[], T2[]>,
			IAsyncMergeMap<List<T1>, List<T2>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Task<(T2, T1)>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Tuple<T1, T2>, (T2, T1)>
#else
				IAsyncMergeMap<Tuple<T1, T2>, (T2, T1)>
#endif
				.MapAsync(Tuple<T1, T2> source, (T2, T1) destination, AsyncMappingContext context) {
				if (source == null)
					return Task.FromResult((default(T2), default(T1)));
				return Task.FromResult((source.Item2, source.Item1));
			}

#if NET7_0_OR_GREATER
			static
#endif
			async Task<GenericClassDto<T2>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<GenericClass<T1>, GenericClassDto<T2>>
#else
				IAsyncMergeMap<GenericClass<T1>, GenericClassDto<T2>>
#endif
				.MapAsync(GenericClass<T1> source, GenericClassDto<T2> destination, AsyncMappingContext context) {
				if (source != null) {
					if(destination == null)
						destination = new GenericClassDto<T2>();
					destination.Id = source.Id;
					destination.Value = await context.Mapper.MapAsync(source.Value, destination.Value);
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

			// Rejects itself (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<T2[]>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<T1[], T2[]>
#else
				IAsyncMergeMap<T1[], T2[]>
#endif
				.MapAsync(T1[] source, T2[] destination, AsyncMappingContext context) {

				throw new MapNotFoundException((typeof(T1[]), typeof(T2[])));
			}

			// Rejects itself (awaited)
#if NET7_0_OR_GREATER
			static
#endif
			async Task<List<T2>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<List<T1>, List<T2>>
#else
				IAsyncMergeMap<List<T1>, List<T2>>
#endif
				.MapAsync(List<T1> source, List<T2> destination, AsyncMappingContext context) {

				await Task.Delay(0);
				throw new MapNotFoundException((typeof(List<T1>), typeof(List<T2>)));
			}
		}

		public class Maps<T1> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IEnumerable<T1>, IList<T1>>,
			IAsyncMergeMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IAsyncMergeMapStatic<IEnumerable<T1>, string>,
			IAsyncMergeMapStatic<int, IList<T1>>,
			IAsyncMergeMapStatic<Queue<T1>, string>,
			IAsyncMergeMapStatic<T1[], IList<T1>>
#else
			IAsyncMergeMap<IEnumerable<T1>, IList<T1>>,
			IAsyncMergeMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			IAsyncMergeMap<IEnumerable<T1>, string>,
			IAsyncMergeMap<int, IList<T1>>,
			IAsyncMergeMap<Queue<T1>, string>,
			IAsyncMergeMap<T1[], IList<T1>>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<T1>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IEnumerable<T1>, IList<T1>>
#else
				IAsyncMergeMap<IEnumerable<T1>, IList<T1>>
#endif
				.MapAsync(IEnumerable<T1> source, IList<T1> destination, AsyncMappingContext context) {
				if (destination == null)
					destination = new List<T1>();
				destination.Clear();
				if (source != null) { 
					foreach(var el in source) {
						destination.Add(el);
					}
				}
				return Task.FromResult(destination);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<IEnumerable<T1>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#else
				IAsyncMergeMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>
#endif
				.MapAsync(IDictionary<string, IDictionary<int, IList<T1>>> source, IEnumerable<T1> destination, AsyncMappingContext context) {
				return Task.FromResult(Enumerable.Empty<T1>());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IEnumerable<T1>, string>
#else
				IAsyncMergeMap<IEnumerable<T1>, string>
#endif
				.MapAsync(IEnumerable<T1> source, string destination, AsyncMappingContext context) {
				return Task.FromResult("Elements: " + source?.Count());
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<T1>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<int, IList<T1>>
#else
				IAsyncMergeMap<int, IList<T1>>
#endif
				.MapAsync(int source, IList<T1> destination, AsyncMappingContext context) {
				return Task.FromResult((IList<T1>)new T1[source]);
			}

			// Throws exception (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<string>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Queue<T1>, string>
#else
				IAsyncMergeMap<Queue<T1>, string>
#endif
				.MapAsync(Queue<T1> source, string destination, AsyncMappingContext context) {
				throw new NotImplementedException();
			}

			// Nested NewMap (not awaited)
#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<T1>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<T1[], IList<T1>>
#else
				IAsyncMergeMap<T1[], IList<T1>>
#endif
				.MapAsync(T1[] source, IList<T1> destination, AsyncMappingContext context) {
				return context.Mapper.MapAsync<IEnumerable<T1>, IList<T1>>(source);
			}
		}

		// Aasync Task error CS0695
		public class Maps :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IEnumerable<bool>, IList<bool>>,
			IAsyncMergeMapStatic<Category, CategoryDto>,
			IAsyncMergeMapStatic<Product, ProductDto>,
			IMatchMapStatic<GenericClass<Product>, GenericClassDto<ProductDto>>
#else
			IAsyncMergeMap<IEnumerable<bool>, IList<bool>>,
			IAsyncMergeMap<Category, CategoryDto>,
			IAsyncMergeMap<Product, ProductDto>,
			IMatchMap<GenericClass<Product>, GenericClassDto<ProductDto>>
#endif
			{

			// Specific map
#if NET7_0_OR_GREATER
			static
#endif
			Task<IList<bool>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IEnumerable<bool>, IList<bool>>
#else
				IAsyncMergeMap<IEnumerable<bool>, IList<bool>>
#endif
				.MapAsync(IEnumerable<bool> source, IList<bool> destination, AsyncMappingContext context) {
				return Task.FromResult((IList<bool>)new List<bool>(32));
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<CategoryDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Category, CategoryDto>
#else
				IAsyncMergeMap<Category, CategoryDto>
#endif
				.MapAsync(Category source, CategoryDto destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new CategoryDto();
					destination.Id = source.Id;
					destination.Parent = source.Parent?.Id;
				}
				return Task.FromResult(destination);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<ProductDto>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<Product, ProductDto>
#else
				IAsyncMergeMap<Product, ProductDto>
#endif
				.MapAsync(Product source, ProductDto destination, AsyncMappingContext context) {
				if (source != null) {
					if (destination == null)
						destination = new ProductDto();
					destination.Code = source.Code;
				}
				return Task.FromResult(destination);
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
			IAsyncMergeMapStatic<IEnumerable<T1>, int>,
			IAsyncMergeMapStatic<IList<T1>, int>
#else
			IAsyncMergeMap<IEnumerable<T1>, int>,
			IAsyncMergeMap<IList<T1>, int>
#endif
			 where T1 : class {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IEnumerable<T1>, int>
#else
				IAsyncMergeMap<IEnumerable<T1>, int>
#endif
				.MapAsync(IEnumerable<T1> source, int destination, AsyncMappingContext context) {
				return Task.FromResult(source?.Count() ?? 0);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, int>
#else
				IAsyncMergeMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, int destination, AsyncMappingContext context) {
				return Task.FromResult(42);
			}
		}

		public class MapsWithStructType<T1> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IList<T1>, int>
#else
			IAsyncMergeMap<IList<T1>, int>
#endif
			 where T1 : struct {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, int>
#else
				IAsyncMergeMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, int destination, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}

		public class MapsWithUnmanagedType<T1> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IList<T1>, int>
#else
			IAsyncMergeMap<IList<T1>, int>
#endif
			where T1 : unmanaged {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, int>
#else
				IAsyncMergeMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, int destination, AsyncMappingContext context) {
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
			IAsyncMergeMapStatic<IList<T1>, int>
#else
			IAsyncMergeMap<IList<T1>, int>
#endif
			where T1 : new() {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, int>
#else
				IAsyncMergeMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, int destination, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}

		public class MapsWithBaseClassType<T1> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IList<T1>, int>
#else
			IAsyncMergeMap<IList<T1>, int>
#endif
			where T1 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, int>
#else
				IAsyncMergeMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, int destination, AsyncMappingContext context) {
				return Task.FromResult(36);
			}
		}

		public class MapsWithBaseClassType<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<T1, T2>
#else
			IAsyncMergeMap<T1, T2>
#endif
			where T1 : List<T2> {
			public
#if NET7_0_OR_GREATER
				static
#endif
				Task<T2> MapAsync(T1 source, T2 destination, AsyncMappingContext context) {
				return Task.FromResult(default(T2));
			}
		}

		public class BaseClassTest : CustomCollection<Category> { }

		public class MapsWithInterfaceType<T1> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IList<T1>, int>
#else
			IAsyncMergeMap<IList<T1>, int>
#endif
			where T1 : IDisposable {

#if NET7_0_OR_GREATER
			static
#endif
			Task<int>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, int>
#else
				IAsyncMergeMap<IList<T1>, int>
#endif
				.MapAsync(IList<T1> source, int destination, AsyncMappingContext context) {
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
			IAsyncMergeMapStatic<IList<T1>, T2>
#else
			IAsyncMergeMap<IList<T1>, T2>
#endif
			where T1 : IEquatable<T2> {
			public
#if NET7_0_OR_GREATER
				static
#endif
				Task<T2> MapAsync(IList<T1> source, T2 destination, AsyncMappingContext context) {
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
			IAsyncMergeMapStatic<IList<T1>, T2>
#else
			IAsyncMergeMap<IList<T1>, T2>
#endif
			where T1 : T2 {
			public
#if NET7_0_OR_GREATER
				static
#endif
				Task<T2> MapAsync(IList<T1> source, T2 destination, AsyncMappingContext context) {
				return Task.FromResult(default(T2));
			}
		}

		public class MapsWithGenericTypeParameterComplexType<T1, T2> :
#if NET7_0_OR_GREATER
			IAsyncMergeMapStatic<IList<T1>, Queue<T2>>
#else
			IAsyncMergeMap<IList<T1>, Queue<T2>>
#endif
			where T1 : T2 where T2 : Product {

#if NET7_0_OR_GREATER
			static
#endif
			Task<Queue<T2>>
#if NET7_0_OR_GREATER
				IAsyncMergeMapStatic<IList<T1>, Queue<T2>>
#else
				IAsyncMergeMap<IList<T1>, Queue<T2>>
#endif
				.MapAsync(IList<T1> source, Queue<T2> destination, AsyncMappingContext context) {
				return Task.FromResult(new Queue<T2>());
			}
		}


		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			});
		}


		[TestMethod]
		public async Task ShouldMapGenericTypes() {
			// 1 parameter
			{
				// No constraints
				{
					Assert.IsTrue(_mapper.CanMapAsyncMerge<IEnumerable<string>, IList<string>>());

					var source = new[] { "Test" };
					var destination = new List<string>();
					var result = await _mapper.MapAsync<IEnumerable<string>, IList<string>>(source, destination);

					Assert.IsNotNull(result);
					Assert.AreSame(destination, result);
					Assert.AreEqual(1, result.Count);
					Assert.AreEqual("Test", result[0]);
				}

				// Class constraint
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
					});

					Assert.IsTrue(mapper.CanMapAsyncMerge<IEnumerable<Product>, int>());

					Assert.AreEqual(1, await mapper.MapAsync<IEnumerable<Product>, int>(new[] { new Product() }, 0));
				}
			}

			// 2 parameters
			{
				// Different
				{
					Assert.IsTrue(_mapper.CanMapAsyncMerge<Tuple<string, int>, ValueTuple<int, string>>());

					var result = await _mapper.MapAsync<Tuple<string, int>, ValueTuple<int, string>>(new Tuple<string, int>("Test", 4), (0, "AAA"));
					Assert.AreEqual(4, result.Item1);
					Assert.AreEqual("Test", result.Item2);
				}

				// Equal
				{
					Assert.IsTrue(_mapper.CanMapAsyncMerge<Tuple<string, string>, ValueTuple<string, string>>());

					var result = await _mapper.MapAsync<Tuple<string, string>, ValueTuple<string, string>>(new Tuple<string, string>("Test1", "Test2"), ("AAA", "BBB"));
					Assert.AreEqual("Test2", result.Item1);
					Assert.AreEqual("Test1", result.Item2);
				}
			}

			// 3 parameters
			{
				// Shared
				{
					Assert.IsTrue(_mapper.CanMapAsyncMerge<Tuple<string, int>, ValueTuple<string, int, bool>>());

					var result = await _mapper.MapAsync<Tuple<string, int>, ValueTuple<string, int, bool>>(new Tuple<string, int>("Test", 2), ("AAA", 0, true));
					Assert.AreEqual("Test", result.Item1);
					Assert.AreEqual(2, result.Item2);
					Assert.IsFalse(result.Item3);
				}
			}
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingGenericTypes() {
			// Types should be the same
			Assert.IsFalse(_mapper.CanMapAsyncMerge<IEnumerable<string>, IList<int>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IEnumerable<string>, IList<int>>(new[] { "Test" }, new List<int>()));
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingGenericConstraints() {
			// struct
			{
				var mapper = new AsyncMergeMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithStructType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Product>, int>());
				Assert.IsTrue(mapper.CanMapAsyncMerge<IList<Guid>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, int>(new List<Product>(), 0));
				await mapper.MapAsync<IList<Guid>, int>(new List<Guid>(), 0);
				await mapper.MapAsync<IList<ManagedTest>, int>(new List<ManagedTest>(), 0);
				await mapper.MapAsync<IList<UnmanagedTest>, int>(new List<UnmanagedTest>(), 0);
			}

			// class
			{
				var mapper = new AsyncMergeMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithClassType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Guid>, int>());
				Assert.IsTrue(mapper.CanMapAsyncMerge<IList<Product>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Guid>, int>(new List<Guid>(), 0));
				await mapper.MapAsync<IList<Product>, int>(new List<Product>(), 0);
			}

			// notnull (no runtime constraint)

			// default (no runtime constraint)

			// unmanaged
			{
				var mapper = new AsyncMergeMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithUnmanagedType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Product>, int>());
				Assert.IsFalse(mapper.CanMapAsyncMerge<IList<ManagedTest>, int>());
				Assert.IsTrue(mapper.CanMapAsyncMerge<IList<UnmanagedTest>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, int>(new List<Product>(), 2));
				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<ManagedTest>, int>(new List<ManagedTest>(), 2));
				await mapper.MapAsync<IList<UnmanagedTest>, int>(new List<UnmanagedTest>(), 2);
				await mapper.MapAsync<IList<Guid>, int>(new List<Guid>(), 2);
				await mapper.MapAsync<IList<int>, int>(new List<int>(), 2);
			}

			// new()
			{
				var mapper = new AsyncMergeMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(MapsWithNewType<>) }
				});

				Assert.IsFalse(mapper.CanMapAsyncMerge<IList<ClassWithoutParameterlessConstructor>, int>());
				Assert.IsTrue(mapper.CanMapAsyncMerge<IList<Product>, int>());

				await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<ClassWithoutParameterlessConstructor>, int>(new List<ClassWithoutParameterlessConstructor>(), 42));
				await mapper.MapAsync<IList<Product>, int>(new List<Product>(), 42);
			}

			// base class
			{
				// Not generic
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Category>, int>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<IList<Product>, int>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, int>(new List<Category>(), 42));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<List<Product>, int>(new List<Product>(), 42));
					await mapper.MapAsync<IList<Product>, int>(new List<Product>(), 42);
					await mapper.MapAsync<IList<LimitedProduct>, int>(new List<LimitedProduct>(), 42);
				}

				// Generic
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithBaseClassType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncMerge<Queue<Category>, Category>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<CustomCollection<Category>, Category>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<BaseClassTest, Category>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<Queue<Category>, Category>(new Queue<Category>(), new Category()));
					await mapper.MapAsync<CustomCollection<Category>, Category>(new CustomCollection<Category>(), new Category());
					await mapper.MapAsync<BaseClassTest, Category>(new BaseClassTest(), new Category());
				}
			}

			// interface
			{
				// Not generic
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Category>, int>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<IList<DisposableTest>, int>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, int>(new List<Category>(), 36));
					await mapper.MapAsync<IList<DisposableTest>, int>(new List<DisposableTest>(), 36);
				}

				// Generic
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithInterfaceType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncMerge<IList<CustomCollection<Category>>, Category>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<IList<EquatableTest>, Product>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<CustomCollection<Category>>, Category>(new List<CustomCollection<Category>>(), new Category()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Queue<Category>>, Category>(new List<Queue<Category>>(), new Category()));
					await mapper.MapAsync<IList<EquatableTest>, Product>(new List<EquatableTest>(), new Product());
				}
			}

			// generic type parameter
			{
				// Simple
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Category>, Product>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<IList<CustomCollection<int>>, List<int>>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, Product>(new List<Category>(), new Product()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, LimitedProduct>(new List<Product>(), new LimitedProduct()));
					await mapper.MapAsync<IList<CustomCollection<int>>, List<int>>(new List<CustomCollection<int>>(), new List<int>());
					await mapper.MapAsync<IList<BaseClassTest>, List<Category>>(new List<BaseClassTest>(), new List<Category>());
					await mapper.MapAsync<IList<LimitedProduct>, Product>(new List<LimitedProduct>(), new Product());
				}

				// Complex
				{
					var mapper = new AsyncMergeMapper(new CustomMapsOptions {
						TypesToScan = new List<Type> { typeof(MapsWithGenericTypeParameterComplexType<,>) }
					});

					Assert.IsFalse(mapper.CanMapAsyncMerge<IList<Category>, Queue<Product>>());
					Assert.IsTrue(mapper.CanMapAsyncMerge<IList<LimitedProduct>, Queue<Product>>());

					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Category>, Queue<Product>>(new List<Category>(), new Queue<Product>()));
					await TestUtils.AssertMapNotFound(() => mapper.MapAsync<IList<Product>, Queue<LimitedProduct>>(new List<Product>(), new Queue<LimitedProduct>()));
					await mapper.MapAsync<IList<LimitedProduct>, Queue<Product>>(new List<LimitedProduct>(), new Queue<Product>());
					await mapper.MapAsync<IList<LimitedProduct>, Queue<LimitedProduct>>(new List<LimitedProduct>(), new Queue<LimitedProduct>());
					await mapper.MapAsync<IList<Product>, Queue<Product>>(new List<Product>(), new Queue<Product>());
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapSingleGenericType() {
			// Generic source
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<IEnumerable<int>, string>());

				Assert.AreEqual("Elements: 2", await _mapper.MapAsync<IEnumerable<int>, string>(new[] { 1, 2 }, "a"));
			}

			// Generic destination
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<int, IList<string>>());

				var list = await _mapper.MapAsync<int, IList<string>>(3, new List<string>());
				Assert.IsNotNull(list);
				Assert.AreEqual(3, list.Count);
				Assert.IsTrue(list.IsReadOnly);
				Assert.IsTrue(list.All(e => e == default));
			}
		}

		[TestMethod]
		public async Task ShouldMapDeepGenerics() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>());

			// Does not throw
			await _mapper.MapAsync<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(new Dictionary<string, IDictionary<int, IList<bool>>>(), new List<bool>());
		}

		[TestMethod]
		public async Task ShouldNotMapNotMatchingDeepGenerics() {
			// Types should be the same
			Assert.IsFalse(_mapper.CanMapAsyncMerge<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(new Dictionary<string, IDictionary<int, IList<bool>>>(), new List<float>()));
		}

		[TestMethod]
		public async Task ShouldRespectConstraints() {
			var mapper = new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MapsWithClassType<>), typeof(MapsWithStructType<>) }
			});

			Assert.AreEqual(42, await mapper.MapAsync<IList<Product>, int>(new List<Product>(), 2));

			Assert.AreEqual(36, await mapper.MapAsync<IList<Guid>, int>(new List<Guid>(), 3));
		}

		[TestMethod]
		public async Task ShouldPreferSpecificMaps() {
			var boolArray = new bool[] { true };
			var boolList = await _mapper.MapAsync<IEnumerable<bool>, IList<bool>>(boolArray, new List<bool>());

			Assert.IsNotNull(boolList);
			Assert.AreNotSame(boolArray, boolList);
			Assert.AreEqual(0, boolList.Count);
			Assert.AreEqual(32, (boolList as List<bool>)?.Capacity);
		}


		[TestMethod]
		public async Task ShouldMapCollectionsWithoutElementsComparer() {
			var mapper = new AsyncMergeCollectionMapper(_mapper);

			{
				var tuples = await mapper.MapAsync(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) }, new List<ValueTuple<int, string>>());

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples[0].Item1);
				Assert.AreEqual("Test1", tuples[0].Item2);
				Assert.AreEqual(5, tuples[1].Item1);
				Assert.AreEqual("Test2", tuples[1].Item2);
			}

			{
				var tuples = await mapper.MapAsync(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) }, new LinkedList<ValueTuple<int, string>>());

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}

			{
				var tuples = await mapper.MapAsync(new[] { new Tuple<string, int>("Test1", 4), new Tuple<string, int>("Test2", 5) }, new CustomCollection<ValueTuple<int, string>>());

				Assert.IsNotNull(tuples);
				Assert.AreEqual(2, tuples.Count);
				Assert.AreEqual(4, tuples.ElementAt(0).Item1);
				Assert.AreEqual("Test1", tuples.ElementAt(0).Item2);
				Assert.AreEqual(5, tuples.ElementAt(1).Item1);
				Assert.AreEqual("Test2", tuples.ElementAt(1).Item2);
			}
		}

		[TestMethod]
		public async Task ShouldMapCollectionsWithGenericElementsComparer() {
			var mapper = new AsyncMergeCollectionMapper(_mapper, new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			}));
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
			var result = await mapper.MapAsync(new[] {
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
		public async Task ShouldMapCollectionsWithSpecificElementsComparer() {
			var mapper = new AsyncMergeCollectionMapper(_mapper, new CustomMatcher(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Maps<,,>), typeof(Maps<,>), typeof(Maps<>), typeof(Maps) }
			}));
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
			var result = await mapper.MapAsync(new[] {
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
		public async Task ShouldMapCollectionsWithCustomElementsComparer() {
			var mapper = new AsyncMergeCollectionMapper(_mapper);
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
			var result = await mapper.MapAsync(new[] {
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
