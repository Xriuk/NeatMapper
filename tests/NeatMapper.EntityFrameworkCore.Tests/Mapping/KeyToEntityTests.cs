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

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	public abstract class KeyToEntityBase {
		private SqliteConnection _connection = null;
		protected ServiceProvider _serviceProvider = null;
		protected Mock<DbCommandInterceptor> _interceptorMock = null;
		protected IMapper _mapper = null;
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
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();
			serviceCollection.ConfigureAll<EntityFrameworkCoreOptions>(Configure);
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_mapper = _serviceProvider.GetRequiredService<IMapper>();

			_db = _serviceProvider.GetRequiredService<TestContext>();
			_db.Database.EnsureDeleted();
			_db.Database.EnsureCreated();

			_db.Add(new IntKey {
				Id = 2,
				Entity = new OwnedEntity1 {
					Id = 4
				}
			});
			_db.Add(new IntFieldKey {
				Id = 2
			});
			_db.Add(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
			_db.Add(new StringKey {
				Id = "Test",
				Entities = new List<OwnedEntity1> {
					new OwnedEntity1 {
						Id = 7
					}
				}
			});
			_db.Add(new StringFieldKey {
				Id = "Test"
			});

			_db.Add(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
			_db.Add(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
			_db.Add(new ShadowIntKey());
			var comp = _db.Add(new ShadowCompositeKey {
				Id1 = 2
			});
			comp.Property("Id2").CurrentValue = "Test";

			_db.SaveChanges();
#if NET5_0_OR_GREATER
			_db.ChangeTracker.Clear();
#else
			foreach (var entry in _db.ChangeTracker.Entries().ToArray()) {
				entry.State = EntityState.Detached;
			}
#endif

			_interceptorMock.Invocations.Clear();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider?.Dispose();
			_connection?.Dispose();
		}
	}

	[TestClass]
	public class KeyToEntityTests : KeyToEntityBase {
		[TestMethod]
		public void ShouldMapKeyToEntity() {
			// Not null
			{
				Assert.IsTrue(_mapper.CanMapNew<int, IntKey>());
				Assert.IsTrue(_mapper.CanMapNew<Guid, GuidKey>());
				Assert.IsTrue(_mapper.CanMapNew<int, IntFieldKey>());

				Assert.IsNotNull(_mapper.Map<IntKey>(2));
				Assert.IsNull(_mapper.Map<IntKey>(3));
				Assert.IsNotNull(_mapper.Map<GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				Assert.IsNull(_mapper.Map<GuidKey>(Guid.Empty));
				Assert.IsNotNull(_mapper.Map<StringKey>("Test"));
				Assert.IsNull(_mapper.Map<StringKey>("Test2"));

				Assert.IsNotNull(_mapper.Map<IntFieldKey>(2));
				Assert.IsNull(_mapper.Map<IntFieldKey>(3));
				Assert.IsNotNull(_mapper.Map<StringFieldKey>("Test"));
				Assert.IsNull(_mapper.Map<StringFieldKey>("Test2"));
			}

			// Null
			{
				Assert.IsTrue(_mapper.CanMapNew<string, StringKey>());

				Assert.IsNull(_mapper.Map<string, StringKey>(null));
			}
		}

		[TestMethod]
		public void ShouldMapNullableKeyToEntity() {
			Assert.IsTrue(_mapper.CanMapNew<int?, IntKey>());
			Assert.IsTrue(_mapper.CanMapNew<Guid?, GuidKey>());
			Assert.IsTrue(_mapper.CanMapNew<int?, IntFieldKey>());

			// Not null
			{
				Assert.IsNotNull(_mapper.Map<int?, IntKey>(2));
				Assert.IsNull(_mapper.Map<int?, IntKey>(3));
				Assert.IsNotNull(_mapper.Map<Guid?, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				Assert.IsNull(_mapper.Map<Guid?, GuidKey>(Guid.Empty));

				Assert.IsNotNull(_mapper.Map<int?, IntFieldKey>(2));
				Assert.IsNull(_mapper.Map<int?, IntFieldKey>(3));
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<int?, IntKey>(null));
				Assert.IsNull(_mapper.Map<Guid?, GuidKey>(null));

				Assert.IsNull(_mapper.Map<int?, IntFieldKey>(null));
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeyToEntity() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapNew<Tuple<int, Guid>, CompositePrimitiveKey>());
				Assert.IsTrue(_mapper.CanMapNew<Tuple<int, string>, CompositeClassKey>());

				// Not null
				{
					Assert.IsNotNull(_mapper.Map<CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(_mapper.Map<CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(_mapper.Map<CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty)));

					Assert.IsNotNull(_mapper.Map<CompositeClassKey>(Tuple.Create(2, "Test")));
					Assert.IsNull(_mapper.Map<CompositeClassKey>(Tuple.Create(3, "Test")));
					Assert.IsNull(_mapper.Map<CompositeClassKey>(Tuple.Create(2, "Test3")));
				}

				// Null
				{
					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null));
					Assert.IsNull(_mapper.Map<Tuple<int, string>, CompositeClassKey>(null));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapNew<(int, Guid), CompositePrimitiveKey>());
				Assert.IsTrue(_mapper.CanMapNew<(int, string), CompositeClassKey>());

				// Not null
				{
					Assert.IsNotNull(_mapper.Map<CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(_mapper.Map<CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsNull(_mapper.Map<CompositePrimitiveKey>((2, Guid.Empty)));

					Assert.IsNotNull(_mapper.Map<CompositeClassKey>((2, "Test")));
					Assert.IsNull(_mapper.Map<CompositeClassKey>((3, "Test")));
					Assert.IsNull(_mapper.Map<CompositeClassKey>((2, "Test3")));
				}
			}
		}

		[TestMethod]
		public void ShouldMapNullableCompositeKeyToEntity() {
			Assert.IsTrue(_mapper.CanMapNew<(int, Guid)?, CompositePrimitiveKey>());
			Assert.IsTrue(_mapper.CanMapNew<(int, string)?, CompositeClassKey>());

			// Not null
			{
				Assert.IsNotNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty)));

				Assert.IsNotNull(_mapper.Map<(int, string)?, CompositeClassKey>((2, "Test")));
				Assert.IsNull(_mapper.Map<(int, string)?, CompositeClassKey>((3, "Test")));
				Assert.IsNull(_mapper.Map<(int, string)?, CompositeClassKey>((2, "Test3")));
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null));

				Assert.IsNull(_mapper.Map<(int, string)?, CompositeClassKey>(null));
			}
		}

		[TestMethod]
		public void ShouldNotMapCompositeKeyToEntitysIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_mapper.CanMapNew<Tuple<Guid, int>, CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapNew<Tuple<string, int>, CompositeClassKey>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositePrimitiveKey>(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositeClassKey>(Tuple.Create("Test", 2)));
			}

			// ValueTuple
			{
				Assert.IsFalse(_mapper.CanMapNew<(Guid, int), CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapNew<(string, int), CompositeClassKey>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositeClassKey>(("Test", 2)));

				Assert.IsFalse(_mapper.CanMapNew<(Guid, int)?, CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapNew<(string, int)?, CompositeClassKey>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)?, CompositeClassKey>(("Test", 2)));
			}
		}

		[TestMethod]
		public void ShouldMapShadowKeyToEntities() {
			Assert.IsTrue(_mapper.CanMapNew<int, ShadowIntKey>());

			Assert.IsNotNull(_mapper.Map<ShadowIntKey>(1));


			Assert.IsTrue(_mapper.CanMapNew<Tuple<int, string>, ShadowCompositeKey>());

			Assert.IsNotNull(_mapper.Map<ShadowCompositeKey>(Tuple.Create(2, "Test")));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapNew<int, OwnedEntity1>());
			Assert.IsFalse(_mapper.CanMapNew<int, OwnedEntity2>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity1>(2));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity2>(2));

			Assert.IsFalse(_mapper.CanMapNew<Tuple<string, int>, OwnedEntity1>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity1>(Tuple.Create("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity1>(Tuple.Create(2, 2)));

			Assert.IsFalse(_mapper.CanMapNew<(string, int), OwnedEntity1>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity1>(("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity1>((2, 2)));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapNew<int, Keyless>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Keyless>(2));
		}
#endif


		[TestMethod]
		public void ShouldMapKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapNew<int[], IEnumerable<IntKey>>());
			Assert.IsTrue(_mapper.CanMapNew<int[], IEnumerable<IntFieldKey>>());

			{
				var result = _mapper.Map<IEnumerable<IntKey>>(new[] { 2 , 0 });
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = _mapper.Map<GuidKey[]>(new List<Guid> { Guid.Empty, new Guid("56033406-E593-4076-B48A-70988C9F9190") });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			{
				var result = _mapper.Map<StringKey[]>(new[] { null, "Test" });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			{
				var result = _mapper.Map<IEnumerable<IntFieldKey>>(new[] { 2, 0 });
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}
		}

		[TestMethod]
		public void ShouldMapNullableKeysCollectioToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapNew<IEnumerable<int?>, IntKey[]>());

			{
				var result = _mapper.Map<IEnumerable<IntKey>>(new int?[] { 2, null });
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = _mapper.Map<GuidKey[]>(new List<Guid?> { null, new Guid("56033406-E593-4076-B48A-70988C9F9190") });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeysCollectionToEntitiesCollection() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapNew<IEnumerable<Tuple<int, Guid>>, CompositePrimitiveKey[]>());

				var result = _mapper.Map<CompositePrimitiveKey[]>(new Tuple<int, Guid>[] { null, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")) });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapNew<(int, Guid)[], List<CompositePrimitiveKey>>());

				var result = _mapper.Map<CompositePrimitiveKey[]>(new (int, Guid)[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), default((int, Guid)) });
				Assert.AreEqual(2, result.Length);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}
		}

		[TestMethod]
		public void ShouldMapNullableCompositeKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapNew<(int, Guid)?[], CompositePrimitiveKey[]>());

			var result = _mapper.Map<CompositePrimitiveKey[]>(new (int, Guid)?[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null });
			Assert.AreEqual(2, result.Length);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);
		}

		[TestMethod]
		public void ShouldNotReturnNullEntities() {
			// Should not be ambiguous
			{
				new EntityFrameworkCoreMappingOptions();
				new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOnly);
				new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOnly, null);
				new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOnly, null, true);
				new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOnly, null, true, true);

				new EntityFrameworkCoreMappingOptions(entitiesRetrievalMode: EntitiesRetrievalMode.LocalOnly);
				new EntityFrameworkCoreMappingOptions(dbContextInstance: null);
				new EntityFrameworkCoreMappingOptions(throwOnDuplicateEntity: true);
				new EntityFrameworkCoreMappingOptions(ignoreNullEntities: true);
			}

			var options = new MappingOptions(new EntityFrameworkCoreMappingOptions(ignoreNullEntities: true));

			{
				var result = _mapper.Map<IEnumerable<IntKey>>(new[] { 2, 0 }, options);
				Assert.AreEqual(1, result.Count());
				Assert.IsNotNull(result.First());
			}

			{
				var result = _mapper.Map<GuidKey[]>(new List<Guid> { Guid.Empty, new Guid("56033406-E593-4076-B48A-70988C9F9190") }, options);
				Assert.AreEqual(1, result.Length);
				Assert.IsNotNull(result[0]);
			}

			{
				var result = _mapper.Map<StringKey[]>(new[] { null, "Test" }, options);
				Assert.AreEqual(1, result.Length);
				Assert.IsNotNull(result[0]);
			}

			using(var factory = _mapper.MapNewFactory<int[], IEnumerable<IntFieldKey>>(options)){
				var result = factory.Invoke(new[] { 2, 0 });
				Assert.AreEqual(1, result.Count());
				Assert.IsNotNull(result.First());
			}
		}
	}


	[TestClass]
	public class KeyToEntityLocalOnlyTests : KeyToEntityBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOnly;
		}


		[TestMethod]
		public void ShouldNotFindNotLoadedSingleEntities() {
			Assert.IsNull(_mapper.Map<IntKey>(2));
			Assert.IsNull(_mapper.Map<IntKey>(3));

			using(var factory = _mapper.MapNewFactory<int, IntKey>()) {
				Assert.IsNull(factory.Invoke(2));
				Assert.IsNull(factory.Invoke(3));
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(0, _db.ChangeTracker.Entries<IntKey>().Count());
		}

		[TestMethod]
		public void ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			Assert.IsNotNull(_mapper.Map<IntKey>(2));
			Assert.IsNull(_mapper.Map<IntKey>(3));

			using (var factory = _mapper.MapNewFactory<int, IntKey>()) {
				Assert.IsNotNull(factory.Invoke(2));
				Assert.IsNull(factory.Invoke(3));
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldNotFindNotLoadedMultipleEntities() {
			var result = _mapper.Map<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNull(result[0]);
			Assert.IsNull(result[1]);

			using (var factory = _mapper.MapNewFactory<int[], IList<IntKey>>()) {
				result = factory.Invoke(new int[] { 2, 3 });
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNull(result[1]);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(0, _db.ChangeTracker.Entries<IntKey>().Count());
		}

		[TestMethod]
		public void ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = _mapper.Map<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			using (var factory = _mapper.MapNewFactory<int[], IList<IntKey>>()) {
				result = factory.Invoke(new int[] { 2, 3 });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}

	[TestClass]
	public class KeyToEntityLocalOrAttachTests : KeyToEntityBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrAttach;
		}


		[TestMethod]
		public void ShouldAttachNotLoadedSingleEntities() {
			Assert.IsNotNull(_mapper.Map<IntKey>(2));
			Assert.IsNotNull(_mapper.Map<IntKey>(3));

			using(var factory = _mapper.MapNewFactory<int, IntKey>()) {
				Assert.IsNotNull(factory.Invoke(2));
				Assert.IsNotNull(factory.Invoke(3));
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			Assert.IsNotNull(_mapper.Map<IntKey>(2));
			Assert.IsNotNull(_mapper.Map<IntKey>(3));

			using(var factory = _mapper.MapNewFactory<int, IntKey>()) {
				Assert.IsNotNull(factory.Invoke(2));
				Assert.IsNotNull(factory.Invoke(3));
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldAttachNotLoadedMultipleEntities() {
			var result = _mapper.Map<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNotNull(result[1]);

			using(var factory = _mapper.MapNewFactory<int[], IList<IntKey>>()) {
				result = factory.Invoke(new int[] { 2, 3 });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Never());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = _mapper.Map<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNotNull(result[1]);

			using (var factory = _mapper.MapNewFactory<int[], IList<IntKey>>()) {
				result = factory.Invoke(new int[] { 2, 3 });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Once());
			Assert.AreEqual(2, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}

	[TestClass]
	public class KeyToEntityLocalOrRemoteTests : KeyToEntityBase {
		protected override void Configure(EntityFrameworkCoreOptions options) {
			options.EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrRemote;
		}


		[TestMethod]
		public void ShouldFindNotLoadedSingleEntities() {
			Assert.IsNotNull(_mapper.Map<IntKey>(2));
			Assert.IsNull(_mapper.Map<IntKey>(3));

			using(var factory = _mapper.MapNewFactory<int, IntKey>()) {
				Assert.IsNotNull(factory.Invoke(2));
				Assert.IsNull(factory.Invoke(3));
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(3));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldFindLoadedSingleEntities() {
			_db.Find<IntKey>(2);

			Assert.IsNotNull(_mapper.Map<IntKey>(2));
			Assert.IsNull(_mapper.Map<IntKey>(3));

			using(var factory = _mapper.MapNewFactory<int, IntKey>()) {
				Assert.IsNotNull(factory.Invoke(2));
				Assert.IsNull(factory.Invoke(3));
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(3));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldFindNotLoadedMultipleEntities() {
			var result = _mapper.Map<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			using(var factory = _mapper.MapNewFactory<int[], IList<IntKey>>()){
				result = factory.Invoke(new int[] { 2, 3 });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(2));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}

		[TestMethod]
		public void ShouldFindLoadedMultipleEntities() {
			_db.Find<IntKey>(2);

			var result = _mapper.Map<IList<IntKey>>(new int[] { 2, 3 });
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);

			using (var factory = _mapper.MapNewFactory<int[], IList<IntKey>>()) {
				result = factory.Invoke(new int[] { 2, 3 });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}

			_interceptorMock.Verify(i => i.CommandCreated(It.IsAny<CommandEndEventData>(), It.IsAny<DbCommand>()), Times.Exactly(3));
			Assert.AreEqual(1, _db.ChangeTracker.Entries<IntKey>().Count());
			Assert.IsTrue(_db.ChangeTracker.Entries<IntKey>().All(e => e.State == EntityState.Unchanged));
		}
	}
}
