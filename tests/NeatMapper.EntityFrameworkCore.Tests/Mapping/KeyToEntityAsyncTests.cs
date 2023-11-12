using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System.Collections.Generic;
using System;
using Moq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	public abstract class KeyToEntityAsyncBase {
		private SqliteConnection _connection = null;
		private ServiceProvider _serviceProvider = null;
		protected Mock<DbCommandInterceptor> _interceptorMock = null;
		protected IAsyncMapper _mapper = null;
		protected TestContext _db = null;

		protected virtual void Configure(EntityFrameworkCoreOptions options) {

		}

		[TestInitialize]
		public void Initialize() {
			_interceptorMock = new Mock<DbCommandInterceptor>(MockBehavior.Loose) {
				CallBase = true
			};

			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			_connection = new SqliteConnection("Filename=:memory:");
			_connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(_connection).AddInterceptors(_interceptorMock.Object), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>(ServiceLifetime.Singleton, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.ConfigureAll<EntityFrameworkCoreOptions>(Configure);
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_mapper = _serviceProvider.GetRequiredService<IAsyncMapper>();

			_db = _serviceProvider.GetRequiredService<TestContext>();
			_db.Database.EnsureDeleted();
			_db.Database.EnsureCreated();

			_db.Add(new IntKey {
				Id = 2,
				Entity = new OwnedEntity {
					Id = 4
				}
			});
			_db.Add(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
			_db.Add(new StringKey {
				Id = "Test",
				Entities = new List<OwnedEntity> {
					new OwnedEntity {
						Id = 7
					}
				}
			});

			_db.Add(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
			_db.Add(new CompositeClassKey { Id1 = 2, Id2 = "Test" });

			_db.SaveChanges();
#if NET5_0_OR_GREATER
			_db.ChangeTracker.Clear();
#else
			foreach (var entry in _db.ChangeTracker.Entries<IntKey>().ToArray()) {
				entry.State = EntityState.Detached;
			}
#endif

			_interceptorMock.Invocations.Clear();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider.Dispose();
			_connection.Dispose();
		}
	}

	[TestClass]
	public class KeyToEntityAsyncTests : KeyToEntityAsyncBase {
		[TestMethod]
		public async Task ShouldMapKeyToEntity() {
			// Not null
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<int, IntKey>());
				Assert.IsTrue(await _mapper.CanMapAsyncNew<Guid, GuidKey>());

				Assert.IsNotNull(await _mapper.MapAsync<IntKey>(2));
				Assert.IsNull(await _mapper.MapAsync<IntKey>(3));
				Assert.IsNotNull(await _mapper.MapAsync<GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				Assert.IsNull(await _mapper.MapAsync<GuidKey>(Guid.Empty));
				Assert.IsNotNull(await _mapper.MapAsync<StringKey>("Test"));
				Assert.IsNull(await _mapper.MapAsync<StringKey>("Test2"));
			}

			// Null
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<string, StringKey>());

				Assert.IsNull(await _mapper.MapAsync<string, StringKey>(null));
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableKeyToEntity() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<int?, IntKey>());
			Assert.IsTrue(await _mapper.CanMapAsyncNew<Guid?, GuidKey>());

			// Not null
			{
				Assert.IsNotNull(await _mapper.MapAsync<int?, IntKey>(2));
				Assert.IsNull(await _mapper.MapAsync<int?, IntKey>(3));
				Assert.IsNotNull(await _mapper.MapAsync<Guid?, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				Assert.IsNull(await _mapper.MapAsync<Guid?, GuidKey>(Guid.Empty));
			}

			// Null
			{
				Assert.IsNull(await _mapper.MapAsync<int?, IntKey>(null));
				Assert.IsNull(await _mapper.MapAsync<Guid?, GuidKey>(null));
			}
		}

		[TestMethod]
		public async Task ShouldMapCompositeKeyToEntity() {
			// Tuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<Tuple<int, Guid>, CompositePrimitiveKey>());
				Assert.IsTrue(await _mapper.CanMapAsyncNew<Tuple<int, string>, CompositeClassKey>());

				// Not null
				{
					Assert.IsNotNull(await _mapper.MapAsync<CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(await _mapper.MapAsync<CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(await _mapper.MapAsync<CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty)));

					Assert.IsNotNull(await _mapper.MapAsync<CompositeClassKey>(Tuple.Create(2, "Test")));
					Assert.IsNull(await _mapper.MapAsync<CompositeClassKey>(Tuple.Create(3, "Test")));
					Assert.IsNull(await _mapper.MapAsync<CompositeClassKey>(Tuple.Create(2, "Test3")));
				}

				// Null
				{
					Assert.IsNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(null));
					Assert.IsNull(await _mapper.MapAsync<Tuple<int, string>, CompositeClassKey>(null));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<(int, Guid), CompositePrimitiveKey>());
				Assert.IsTrue(await _mapper.CanMapAsyncNew<(int, string), CompositeClassKey>());

				// Not null
				{
					Assert.IsNotNull(await _mapper.MapAsync<CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(await _mapper.MapAsync<CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(await _mapper.MapAsync<CompositePrimitiveKey>((2, Guid.Empty)));

					Assert.IsNotNull(await _mapper.MapAsync<CompositeClassKey>((2, "Test")));
					Assert.IsNull(await _mapper.MapAsync<CompositeClassKey>((3, "Test")));
					Assert.IsNull(await _mapper.MapAsync<CompositeClassKey>((2, "Test3")));
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableCompositeKeyToEntity() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<(int, Guid)?, CompositePrimitiveKey>());
			Assert.IsTrue(await _mapper.CanMapAsyncNew<(int, string)?, CompositeClassKey>());

			// Not null
			{
				Assert.IsNotNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty)));

				Assert.IsNotNull(await _mapper.MapAsync<(int, string)?, CompositeClassKey>((2, "Test")));
				Assert.IsNull(await _mapper.MapAsync<(int, string)?, CompositeClassKey>((3, "Test")));
				Assert.IsNull(await _mapper.MapAsync<(int, string)?, CompositeClassKey>((2, "Test3")));
			}

			// Null
			{
				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>(null));

				Assert.IsNull(await _mapper.MapAsync<(int, string)?, CompositeClassKey>(null));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapCompositeKeyToEntitysIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<Tuple<Guid, int>, CompositePrimitiveKey>());
				Assert.IsFalse(await _mapper.CanMapAsyncNew<Tuple<string, int>, CompositeClassKey>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CompositePrimitiveKey>(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CompositeClassKey>(Tuple.Create("Test", 2)));
			}

			// ValueTuple
			{
				Assert.IsFalse(await _mapper.CanMapAsyncNew<(Guid, int), CompositePrimitiveKey>());
				Assert.IsFalse(await _mapper.CanMapAsyncNew<(string, int), CompositeClassKey>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<CompositeClassKey>(("Test", 2)));

				Assert.IsFalse(await _mapper.CanMapAsyncNew<(Guid, int)?, CompositePrimitiveKey>());
				Assert.IsFalse(await _mapper.CanMapAsyncNew<(string, int)?, CompositeClassKey>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)?, CompositeClassKey>(("Test", 2)));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapEntitiesWithShadowKeys() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<int, ShadowIntKey>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<ShadowIntKey>(2));
		}

		[TestMethod]
		public async Task ShouldNotMapOwnedEntities() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<int, OwnedEntity>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<OwnedEntity>(2));

			Assert.IsFalse(await _mapper.CanMapAsyncNew<Tuple<string, int>, OwnedEntity>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<OwnedEntity>(Tuple.Create("Test", 2)));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<OwnedEntity>(Tuple.Create(2, 2)));

			Assert.IsFalse(await _mapper.CanMapAsyncNew<(string, int), OwnedEntity>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<OwnedEntity>(("Test", 2)));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<OwnedEntity>((2, 2)));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public async Task ShouldNotMapKeylessEntities() {
			Assert.IsFalse(await _mapper.CanMapAsyncNew<int, Keyless>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<Keyless>(2));
		}
#endif


		[TestMethod]
		public async Task ShouldMapKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<int[], IEnumerable<IntKey>>());

			{
				var result = await _mapper.MapAsync<IEnumerable<IntKey>>(new[] { 2 , 0 });
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = await _mapper.MapAsync<GuidKey[]>(new List<Guid> { Guid.Empty, new Guid("56033406-E593-4076-B48A-70988C9F9190") });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			{
				var result = await _mapper.MapAsync<StringKey[]>(new[] { null, "Test" });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableKeysCollectioToEntitiesCollection() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<IEnumerable<int?>, IntKey[]>());

			{
				var result = await _mapper.MapAsync<IEnumerable<IntKey>>(new int?[] { 2, null });
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = await _mapper.MapAsync<GuidKey[]>(new List<Guid?> { null, new Guid("56033406-E593-4076-B48A-70988C9F9190") });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapCompositeKeysCollectionToEntitiesCollection() {
			// Tuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<IEnumerable<Tuple<int, Guid>>, CompositePrimitiveKey[]>());

				var result = await _mapper.MapAsync<CompositePrimitiveKey[]>(new Tuple<int, Guid>[] { null, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")) });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			// ValueTuple
			{
				Assert.IsTrue(await _mapper.CanMapAsyncNew<(int, Guid)[], List<CompositePrimitiveKey>>());

				var result = await _mapper.MapAsync<CompositePrimitiveKey[]>(new (int, Guid)[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), default((int, Guid)) });
				Assert.AreEqual(2, result.Length);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableCompositeKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(await _mapper.CanMapAsyncNew<(int, Guid)?[], CompositePrimitiveKey[]>());

			var result = await _mapper.MapAsync<CompositePrimitiveKey[]>(new (int, Guid)?[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null });
			Assert.AreEqual(2, result.Length);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);
		}
	}


	[TestClass]
	public class KeyToEntityAsyncLocalTests : KeyToEntityAsyncBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.Local;
		}


		[TestMethod]
		public async Task ShouldNotFindNotLoadedSingleEntities() {
			Assert.IsNull(await _mapper.MapAsync<IntKey>(2));
			Assert.IsNull(await _mapper.MapAsync<IntKey>(3));

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(0, _db.ChangeTracker.Entries<IntKey>().Count());
		}

		[TestMethod]
		public async Task ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			Assert.IsNotNull(await _mapper.MapAsync<IntKey>(2));
			Assert.IsNull(await _mapper.MapAsync<IntKey>(3));

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldNotFindNotLoadedMultipleEntities() {
			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNull(result[0]);
			Assert.IsNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(0, _db.ChangeTracker.Entries<IntKey>().Count());
		}

		[TestMethod]
		public async Task ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}

	[TestClass]
	public class KeyToEntityAsyncLocalOrAttachTests : KeyToEntityAsyncBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrAttach;
		}


		[TestMethod]
		public async Task ShouldAttachNotLoadedSingleEntities() {
			{ 
				var result = await _mapper.MapAsync<IntKey>(2);
				Assert.IsNotNull(result);
			}

			{
				var result = await _mapper.MapAsync<IntKey>(3);
				Assert.IsNotNull(result);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			{
				var result = await _mapper.MapAsync<IntKey>(2);
				Assert.IsNotNull(result);
			}

			{
				var result = await _mapper.MapAsync<IntKey>(3);
				Assert.IsNotNull(result);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldAttachNotLoadedMultipleEntities() {
			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNotNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNotNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}

	[TestClass]
	public class KeyToEntityAsyncLocalOrRemoteTests : KeyToEntityAsyncBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrRemote;
		}


		[TestMethod]
		public async Task ShouldFindNotLoadedSingleEntities() {
			{
				var result = await _mapper.MapAsync<IntKey>(2);
				Assert.IsNotNull(result);
			}

			Assert.IsNull(await _mapper.MapAsync<IntKey>(3));

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(2));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			{
				var result = await _mapper.MapAsync<IntKey>(2);
				Assert.IsNotNull(result);
			}

			Assert.IsNull(await _mapper.MapAsync<IntKey>(3));

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(2));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindNotLoadedMultipleEntities() {
			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(2));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}

	[TestClass]
	public class KeyToEntityAsyncRemoteTests : KeyToEntityAsyncBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.Remote;
		}


		[TestMethod]
		public async Task ShouldFindNotLoadedSingleEntities() {
			{
				var result = await _mapper.MapAsync<IntKey>(2);
				Assert.IsNotNull(result);
			}

			Assert.IsNull(await _mapper.MapAsync<IntKey>(3));

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(2));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			{
				var result = await _mapper.MapAsync<IntKey>(2);
				Assert.IsNotNull(result);
			}

			Assert.IsNull(await _mapper.MapAsync<IntKey>(3));

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(3));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindNotLoadedMultipleEntities() {
			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public async Task ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = await _mapper.MapAsync<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(2));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}
}
