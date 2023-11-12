using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class EntityToKeyAsyncTests {
		private SqliteConnection _connection = null;
		private ServiceProvider _serviceProvider = null;
		private IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			_connection = new SqliteConnection("Filename=:memory:");
			_connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(_connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>(ServiceLifetime.Singleton);
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_mapper = _serviceProvider.GetRequiredService<IAsyncMapper>();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider.Dispose();
			_connection.Dispose();
		}


		[TestMethod]
		public async Task ShouldMapEntityToKey() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<IntKey, int>());

			// Not null
			{
				Assert.AreEqual(2, await _mapper.MapAsync<int>(new IntKey { Id = 2 }));
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), await _mapper.MapAsync<Guid>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.AreEqual("Test", await _mapper.MapAsync<string>(new StringKey { Id = "Test" }));
			}

			// Null
			{
				Assert.AreEqual(0, await _mapper.MapAsync<IntKey, int>(null));
				Assert.AreEqual(Guid.Empty, await _mapper.MapAsync<GuidKey, Guid>(null));
				Assert.IsNull(await _mapper.MapAsync<StringKey, string>(null));
			}
		}

		[TestMethod]
		public async Task ShouldMapEntityToNullableKey() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<IntKey, int?>());

			// Not null
			{
				Assert.AreEqual(2, await _mapper.MapAsync<int?>(new IntKey { Id = 2 }));
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), await _mapper.MapAsync<Guid?>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
			}

			// Null
			{
				Assert.IsNull(await _mapper.MapAsync<IntKey, int?>(null));
				Assert.IsNull(await _mapper.MapAsync<GuidKey, Guid?>(null));
			}
		}

		[TestMethod]
		public async Task ShouldMapEntityToCompositeKey() {
			// Tuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<CompositePrimitiveKey, Tuple<int, Guid>>());

				// Not null
				{
					{ 
						var result = await _mapper.MapAsync<Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
						Assert.IsNotNull(result);
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result.Item2);
					}

					{ 
						var result = await _mapper.MapAsync<Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
						Assert.IsNotNull(result);
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual("Test", result.Item2);
					}
				}

				// Null
				{
					Assert.IsNull(await _mapper.MapAsync<CompositePrimitiveKey, Tuple<int, Guid>>(null));
					Assert.IsNull(await _mapper.MapAsync<CompositeClassKey, Tuple<int, string>>(null));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<CompositePrimitiveKey, (int, Guid)>());

				// Not null
				{
					{ 
						var result = await _mapper.MapAsync<(int, Guid)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result.Item2);
					}

					{ 
						var result = await _mapper.MapAsync<(int, string)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual("Test", result.Item2);
					}
				}

				// Null
				{
					{ 
						var result = await _mapper.MapAsync<CompositePrimitiveKey, (int, Guid)>(null);
						Assert.AreEqual(0, result.Item1);
						Assert.AreEqual(Guid.Empty, result.Item2);
					}

					{ 
						var result = await _mapper.MapAsync<CompositeClassKey, (int, string)>(null);
						Assert.AreEqual(0, result.Item1);
						Assert.IsNull(result.Item2);
					}
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapEntityToNullableCompositeKey() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<CompositePrimitiveKey, (int, Guid)?>());

			// Not null
			{
				{ 
					var result = await _mapper.MapAsync<(int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Value.Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result.Value.Item2);
				}

				{ 
					var result = await _mapper.MapAsync<(int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Value.Item1);
					Assert.AreEqual("Test", result.Value.Item2);
				}
			}

			// Null
			{
				Assert.IsNull(await _mapper.MapAsync<CompositePrimitiveKey, (int, Guid)?>(null));
				Assert.IsNull(await _mapper.MapAsync<CompositeClassKey, (int, string)?>(null));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapEntityToCompositeKeyIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<CompositePrimitiveKey, Tuple<Guid, int>>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Tuple<Guid, int>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Tuple<string, int>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}

			// ValueTuple
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<CompositePrimitiveKey, (Guid, int)>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(Guid, int)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(Guid, int)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapMerge() {
			Assert.IsFalse(await _mapper.CanMapAsyncMerge<StringKey, string>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new StringKey { Id = "Test" }, ""));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<IntKey, int?>(new IntKey { Id = 2 }, 3));
		}

		[TestMethod]
		public async Task ShouldNotMapEntitiesWithShadowKeys() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<ShadowIntKey, int>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int>(new ShadowIntKey()));
		}

		[TestMethod]
		public async Task ShouldNotMapOwnedEntities() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<OwnedEntity, int>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int>(new OwnedEntity()));

			Assert.IsFalse(await _mapper.CanMapAsyncNew<OwnedEntity, Tuple<string, int>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Tuple<string, int>>(new OwnedEntity()));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Tuple<int, int>>(new OwnedEntity()));

			Assert.IsFalse(await _mapper.CanMapAsyncNew<OwnedEntity, (string, int)>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)>(new OwnedEntity()));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(int, int)>(new OwnedEntity()));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public async Task ShouldNotMapKeylessEntities() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<Keyless, int>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int>(new Keyless()));
		}
#endif


		[TestMethod]
		public async Task ShouldMapEntitiesCollectionToKeysCollection() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<IEnumerable<IntKey>, int[]>());

			{ 
				var result = await _mapper.MapAsync<IEnumerable<int>>(new[] { new IntKey { Id = 2 }, null });
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(2, result.First());
				Assert.AreEqual(0, result.Last());
			}

			{
				var result = await _mapper.MapAsync<Guid[]>(new List<GuidKey>{ null, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") } });
				Assert.AreEqual(2, result.Length);
				Assert.AreEqual(Guid.Empty, result[0]);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[1]);
			}

			{
				var result = await _mapper.MapAsync<string[]>(new[] { null, new StringKey { Id = "Test" } });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.AreEqual("Test", result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapEntitiesCollectionToNullableKeysCollection() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<IEnumerable<IntKey>, int?[]>());

			{
				var result = await _mapper.MapAsync<IEnumerable<int?>>(new[] { new IntKey { Id = 2 }, null });
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(2, result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = await _mapper.MapAsync<Guid?[]>(new List<GuidKey> { null, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") } });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapEntitiesCollectionToCompositeKeysCollection() {
			// Tuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<IEnumerable<CompositePrimitiveKey>, Tuple<int, Guid>[]>());

				{ 
					var result = await _mapper.MapAsync<Tuple<int, Guid>[]>(new[]{ null,  new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } });
					Assert.AreEqual(2, result.Length);
					Assert.IsNull(result[0]);
					Assert.AreEqual(2, result[1].Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[1].Item2);
				}

				{ 
					var result = await _mapper.MapAsync<List<Tuple<int, string>>>(new[]{ new CompositeClassKey { Id1 = 2, Id2 = "Test" }, null });
					Assert.AreEqual(2, result.Count);
					Assert.AreEqual(2, result[0].Item1);
					Assert.AreEqual("Test", result[0].Item2);
					Assert.IsNull(result[1]);
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<CompositePrimitiveKey[], List<(int, Guid)>>());

				{ 
					var result = await _mapper.MapAsync<(int, Guid)[]>(new[]{ new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null });
					Assert.AreEqual(2, result.Length);
					Assert.AreEqual(2, result[0].Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[0].Item2);
					Assert.AreEqual(0, result[1].Item1);
					Assert.AreEqual(Guid.Empty, result[1].Item2);
				}

				{ 
					var result = await _mapper.MapAsync<(int, string)[]>(new[]{ null, new CompositeClassKey { Id1 = 2, Id2 = "Test" } });
					Assert.AreEqual(2, result.Length);
					Assert.AreEqual(0, result[0].Item1);
					Assert.IsNull(result[0].Item2);
					Assert.AreEqual(2, result[1].Item1);
					Assert.AreEqual("Test", result[1].Item2);
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapEntitiesCollectionToNullableCompositeKeysCollection() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<CompositePrimitiveKey[], (int, Guid)?[]>());

			{
				var result = await _mapper.MapAsync<(int, Guid)?[]>(new[] { new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null });
				Assert.AreEqual(2, result.Length);
				Assert.AreEqual(2, result[0].Value.Item1);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[0].Value.Item2);
				Assert.IsNull(result[1]);
			}

			{
				var result = await _mapper.MapAsync<(int, string)?[]>(new[] { null, new CompositeClassKey { Id1 = 2, Id2 = "Test" } });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.AreEqual(2, result[1].Value.Item1);
				Assert.AreEqual("Test", result[1].Value.Item2);
			}
		}

		[TestMethod]
		public async Task ShouldNotMapEntitiesCollectionToCompositeKeysCollectionIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<IEnumerable<CompositePrimitiveKey>, Tuple<Guid, int>[]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Tuple<Guid, int>[]>(new[]{ new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } }));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Tuple<string, int>[]>(new[]{ new CompositeClassKey { Id1 = 2, Id2 = "Test" } }));
			}

			// ValueTuple
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<CompositePrimitiveKey[], (Guid, int)[]>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(Guid, int)[]>(new[] { new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } }));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)[]>(new[] { new CompositeClassKey { Id1 = 2, Id2 = "Test" } }));

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(Guid, int)?[]>(new[] { new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } }));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)?[]>(new[] { new CompositeClassKey { Id1 = 2, Id2 = "Test" } }));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapMergeCollections() {
			Assert.IsFalse(await _mapper.CanMapAsyncMerge<IEnumerable<StringKey>, List<string>>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(new[] { new StringKey { Id = "Test" } }, new List<string>()));
		}
	}
}
