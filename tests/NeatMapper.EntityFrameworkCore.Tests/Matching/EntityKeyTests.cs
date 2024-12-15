using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;

namespace NeatMapper.EntityFrameworkCore.Tests.Matching {
	[TestClass]
	public class EntityKeyTests {
		private SqliteConnection _connection = null;
		private ServiceProvider _serviceProvider = null;
		private IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			_connection = new SqliteConnection("Filename=:memory:");
			_connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(_connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_matcher = _serviceProvider.GetRequiredService<IMatcher>();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider?.Dispose();
			_connection?.Dispose();
		}


		[TestMethod]
		public void ShouldMatchEntitiesWithKey() {
			Assert.IsTrue(_matcher.CanMatch<IntKey, int>());
			Assert.IsTrue(_matcher.CanMatch<string, StringKey>());
			Assert.IsTrue(_matcher.CanMatch<IntFieldKey, int>());
			Assert.IsTrue(_matcher.CanMatch<string, StringFieldKey>());

			// Not null
			{
				Assert.IsTrue(_matcher.Match(new IntKey { Id = 2 }, 2));
				Assert.IsTrue(_matcher.Match(0, new IntKey { Id = 0 }));
				Assert.IsTrue(_matcher.Match(new Guid("56033406-E593-4076-B48A-70988C9F9190"), new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.IsFalse(_matcher.Match(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Guid.Empty));
				Assert.IsTrue(_matcher.Match(new StringKey { Id = "Test" }, "Test"));

				Assert.IsTrue(_matcher.Match(new IntFieldKey { Id = 2 }, 2));
				Assert.IsTrue(_matcher.Match(0, new IntFieldKey { Id = 0 }));
				Assert.IsTrue(_matcher.Match(new StringFieldKey { Id = "Test" }, "Test"));
			}

			// Null
			{
				Assert.IsFalse(_matcher.Match<IntKey, int>(null, 0));
				Assert.IsFalse(_matcher.Match<Guid, GuidKey>(Guid.Empty, null));
				Assert.IsFalse(_matcher.Match<StringKey, string>(null, null));
				Assert.IsFalse(_matcher.Match<string, StringKey>(null, new StringKey { Id = null }));

				Assert.IsFalse(_matcher.Match<IntFieldKey, int>(null, 0));
				Assert.IsFalse(_matcher.Match<StringFieldKey, string>(null, null));
				Assert.IsFalse(_matcher.Match<string, StringFieldKey>(null, new StringFieldKey { Id = null }));
			}

			Assert.IsFalse(_matcher.CanMatch<string, IntKey>());
			Assert.IsFalse(_matcher.CanMatch<StringKey, int>());
		}

		[TestMethod]
		public void ShouldMatchEntitiesWithNullableKey() {
			Assert.IsTrue(_matcher.CanMatch<IntKey, int?>());
			Assert.IsTrue(_matcher.CanMatch<IntFieldKey, int?>());

			// Not null
			{
				Assert.IsTrue(_matcher.Match<IntKey, int?>(new IntKey { Id = 2 }, 2));
				Assert.IsTrue(_matcher.Match<Guid?, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190"), new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));

				Assert.IsTrue(_matcher.Match<IntFieldKey, int?>(new IntFieldKey { Id = 2 }, 2));
			}

			// Null
			{
				Assert.IsFalse(_matcher.Match<IntKey, int?>(null, 0));
				Assert.IsFalse(_matcher.Match<IntKey, int?>(new IntKey { Id = 2 }, null));
				Assert.IsFalse(_matcher.Match<IntKey, int?>(null, null));
				Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(Guid.Empty, null));
				Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(null, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(null, null));

				Assert.IsFalse(_matcher.Match<IntFieldKey, int?>(null, 0));
				Assert.IsFalse(_matcher.Match<IntFieldKey, int?>(new IntFieldKey { Id = 2 }, null));
				Assert.IsFalse(_matcher.Match<IntFieldKey, int?>(null, null));
			}
		}

		[TestMethod]
		public void ShouldMatchEntitiesWithCompositeKey() {
			// Tuple
			{
				Assert.IsTrue(_matcher.CanMatch<CompositePrimitiveKey, Tuple<int, Guid>>());

				// Not null
				{
					Assert.IsTrue(_matcher.Match(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsTrue(_matcher.Match(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create(2, "Test")));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(null, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(null, null));
					Assert.IsFalse(_matcher.Match< Tuple<int, string>, CompositeClassKey>(null, new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
					Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(Tuple.Create(2, "Test"), null));
					Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(null, null));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_matcher.CanMatch<CompositePrimitiveKey, (int, Guid)>());

				// Not null
				{
					Assert.IsTrue(_matcher.Match((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsTrue(_matcher.Match(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (2, "Test")));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)>(null, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsFalse(_matcher.Match<(int, string), CompositeClassKey>((2, "Test"), null));
				}
			}
		}

		[TestMethod]
		public void ShouldMatchEntitiesWithNullableCompositeKey() {
			Assert.IsTrue(_matcher.CanMatch<CompositePrimitiveKey, (int, Guid)?>());

			// Not null
			{
				Assert.IsTrue(_matcher.Match<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.IsTrue(_matcher.Match<CompositeClassKey, (int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (2, "Test")));
			}

			// Null
			{
				Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
				Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(null, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
				Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(null, null));
				Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>(null, new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>((2, "Test"), null));
				Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>(null, null));
			}

			Assert.IsFalse(_matcher.CanMatch<(Guid, int)?, CompositePrimitiveKey>());
		}

		[TestMethod]
		public void ShouldNotMatchEntitiesWithCompositeKeyIfTypesAreNullable() {
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, Tuple<int?, Guid>>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, Tuple<int, Guid?>>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, Tuple<int?, Guid?>>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (int?, Guid)>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (int, Guid?)>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (int?, Guid?)>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (int?, Guid)?>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (int, Guid?)?>());
			Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (int?, Guid?)?>());
		}

		[TestMethod]
		public void ShouldNotMatchEntitiesWithCompositeKeyIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, Tuple<Guid, int>>());

				TestUtils.AssertMapNotFound(() => _matcher.Match(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _matcher.Match(Tuple.Create("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}

			// ValueTuple
			{
				Assert.IsFalse(_matcher.CanMatch<CompositePrimitiveKey, (Guid, int)>());

				TestUtils.AssertMapNotFound(() => _matcher.Match((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _matcher.Match(("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));

				TestUtils.AssertMapNotFound(() => _matcher.Match<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _matcher.Match<(string, int)?, CompositeClassKey>(("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}
		}

		[TestMethod]
		public void ShouldNotMatchEntitiesWithShadowKeysWithoutContext() {
			var options = new []{ new MatcherOverrideMappingOptions(serviceProvider: EmptyServiceProvider.Instance) };

			Assert.IsFalse(_matcher.CanMatch<ShadowIntKey, int>(options));

			TestUtils.AssertMapNotFound(() => _matcher.Match(2, new ShadowIntKey(), options));
		}

		[TestMethod]
		public void ShouldNotMatchEntitiesWithShadowKeysWithContextIfNotTracked() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity = new ShadowIntKey();

			var options = new object[] { new EntityFrameworkCoreMappingOptions(dbContextInstance: db) };

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

			var exc = Assert.ThrowsException<MatcherException>(() => _matcher.Match(entity, 1, options));
			Assert.IsInstanceOfType(exc.InnerException, typeof(InvalidOperationException));
			Assert.IsTrue(exc.InnerException?.Message.StartsWith($"The entity of type {typeof(ShadowIntKey).FullName} is not being tracked by the provided {nameof(DbContext)}"));
		}

		[TestMethod]
		public void ShouldMatchEntitiesWithShadowKeysWithContextIfTracked() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity = new ShadowIntKey();
			db.Add(entity);
			db.SaveChanges();

			var options = new object[] { new EntityFrameworkCoreMappingOptions(dbContextInstance: db) };

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

			Assert.IsTrue(_matcher.Match(entity, 1, options));
			Assert.IsFalse(_matcher.Match(entity, 2, options));

			var entity2 = new ShadowCompositeKey {
				Id1 = 2
			};
			var entity2Entry = db.Add(entity2);
			entity2Entry.Property("Id2").CurrentValue = "Test";
			db.SaveChanges();

			Assert.IsTrue(_matcher.CanMatch<(int, string), ShadowCompositeKey>(options));

			Assert.IsTrue(_matcher.Match(entity2, (2, "Test"), options));
			Assert.IsFalse(_matcher.Match(entity2, (1, "Nope"), options));
		}

		// Owned entities which can be used both as single and collection (and thus have different key configurations)
		[TestMethod]
		public void ShouldNotMatchPromiscuousOwnedEntities() {
			Assert.IsFalse(_matcher.CanMatch<OwnedEntity1, int>());

			TestUtils.AssertMapNotFound(() => _matcher.Match(2, new OwnedEntity1 { Id = 2 }));

			Assert.IsFalse(_matcher.CanMatch<OwnedEntity1, Tuple<string, int>>());

			TestUtils.AssertMapNotFound(() => _matcher.Match(Tuple.Create("Test", 2), new OwnedEntity1 { Id = 2 }));
			TestUtils.AssertMapNotFound(() => _matcher.Match(Tuple.Create(1, 2), new OwnedEntity1 { Id = 2 }));

			Assert.IsFalse(_matcher.CanMatch<OwnedEntity1, (string, int)>());

			TestUtils.AssertMapNotFound(() => _matcher.Match(new OwnedEntity1(), ("Test", 2)));
			TestUtils.AssertMapNotFound(() => _matcher.Match(new OwnedEntity1(), (1, 2)));
		}

		// Owned entities which are used only in collections (so they have a composite key which we can compare)
		[TestMethod]
		public void ShouldMatchNotPromiscuousOwnedEntities() {
			Assert.IsTrue(_matcher.CanMatch<OwnedEntity2, int>());

			Assert.IsTrue(_matcher.Match(2, new OwnedEntity2 { Id = 2 }));

			// We cannot match composite keys because we cannot check the parent type if the entity is not tracked
			Assert.IsFalse(_matcher.CanMatch<OwnedEntity2, Tuple<string, int>>());

			TestUtils.AssertMapNotFound(() => _matcher.Match(Tuple.Create("Test", 2), new OwnedEntity1()));
			TestUtils.AssertMapNotFound(() => _matcher.Match(Tuple.Create(1, 2), new OwnedEntity1()));

			Assert.IsFalse(_matcher.CanMatch<OwnedEntity2, (string, int)>());

			TestUtils.AssertMapNotFound(() => _matcher.Match(new OwnedEntity1(), ("Test", 2)));
			TestUtils.AssertMapNotFound(() => _matcher.Match(new OwnedEntity1(), (1, 2)));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMatchKeylessEntities() {
			Assert.IsFalse(_matcher.CanMatch<Keyless, int>());

			TestUtils.AssertMapNotFound(() => _matcher.Match(1, new Keyless()));
		}
#endif

		[TestMethod]
		public void ShouldNotLockSemaphoreInsideNestedSemaphoreContext() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity = new ShadowIntKey();
			db.Add(entity);
			db.SaveChanges();

			var semaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(db);

			var options = new object[] {
				new EntityFrameworkCoreMappingOptions(dbContextInstance: db),
				NestedSemaphoreContext.Instance
			};

			semaphore.Wait();
			try { 
				Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

				Assert.IsTrue(_matcher.Match(entity, 1, options));
			}
			finally {
				semaphore.Release();
			}
		}

		[TestMethod]
		public void ShouldThrowMatcherExceptionIfDbContextIsDisposed() {
			var db = new TestContext(_serviceProvider.GetRequiredService<DbContextOptions<TestContext>>());
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity = new ShadowIntKey();
			db.Add(entity);
			db.SaveChanges();

			var options = new object[] { new EntityFrameworkCoreMappingOptions(dbContextInstance: db) };

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

			Assert.IsTrue(_matcher.Match(entity, 1, options));
			Assert.IsFalse(_matcher.Match(entity, 2, options));

			db.Dispose();

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

			var exc = Assert.ThrowsException<MatcherException>(() => _matcher.Match(entity, 1, options));
			Assert.IsInstanceOfType(exc.InnerException, typeof(ObjectDisposedException));
		}
	}
}
