using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;

namespace NeatMapper.EntityFrameworkCore.Tests.Matching {
	[TestClass]
	public class EntityEntityTests {
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
			Assert.IsTrue(_matcher.CanMatch<IntKey, IntKey>());
			Assert.IsTrue(_matcher.CanMatch<StringKey, StringKey>());

			// Not null
			{
				Assert.IsTrue(_matcher.Match(new IntKey { Id = 2 }, new IntKey { Id = 2 }));
				Assert.IsTrue(_matcher.Match(new IntKey { Id = 0 }, new IntKey { Id = 0 }));
				Assert.IsTrue(_matcher.Match(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.IsFalse(_matcher.Match(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new GuidKey { Id = Guid.Empty }));
				Assert.IsTrue(_matcher.Match(new StringKey { Id = "Test" }, new StringKey { Id = "Test" }));
			}

			// Null
			{
				Assert.IsFalse(_matcher.Match<IntKey, IntKey>(null, new IntKey { Id = 0 }));
				Assert.IsFalse(_matcher.Match<GuidKey, GuidKey>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
				Assert.IsFalse(_matcher.Match<StringKey, StringKey>(null, null));
				Assert.IsFalse(_matcher.Match<StringKey, StringKey>(null, new StringKey { Id = null }));
			}
		}

		[TestMethod]
		public void ShouldMatchEntitiesWithCompositeKey() {
			Assert.IsTrue(_matcher.CanMatch<CompositePrimitiveKey, CompositePrimitiveKey>());

			// Not null
			{
				Assert.IsTrue(_matcher.Match(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.IsTrue(_matcher.Match(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}

			// Null
			{
				Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, CompositePrimitiveKey>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
				Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, CompositePrimitiveKey>(null, null));
				Assert.IsFalse(_matcher.Match<CompositeClassKey, CompositeClassKey>(null, new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				Assert.IsFalse(_matcher.Match<CompositeClassKey, CompositeClassKey>(null, null));
			}
		}

		[TestMethod]
		public void ShouldNotMatchEntitiesWithShadowKeysWithoutContext() {
			var options = new[] { new MatcherOverrideMappingOptions(serviceProvider: new ServiceCollection().BuildServiceProvider()) };

			// Should be ObjectEqualsMatcher
			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, ShadowIntKey>(options));

			Assert.IsFalse(_matcher.Match(new ShadowIntKey(), new ShadowIntKey(), options));
			var ent = new ShadowIntKey();
			Assert.IsTrue(_matcher.Match(ent, ent, options));
		}

		[TestMethod]
		public void ShouldNotMatchEntitiesWithShadowKeysWithContextIfNotTracked() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity1 = new ShadowIntKey();
			var entity2 = new ShadowIntKey();

			var options = new object[] { new EntityFrameworkCoreMappingOptions(dbContextInstance: db) };

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

			var exc = Assert.ThrowsException<MatcherException>(() => _matcher.Match(entity1, entity2, options));
			Assert.IsInstanceOfType(exc.InnerException, typeof(InvalidOperationException));
			Assert.IsTrue(exc.InnerException?.Message.StartsWith($"The entity(ies) of type {typeof(ShadowIntKey).FullName} is/are not being tracked by the provided {nameof(DbContext)}"));
		}

		[TestMethod]
		public void ShouldMatchEntitiesWithShadowKeysWithContextIfTracked() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity1 = new ShadowIntKey();
			db.Add(entity1);
			var entity2 = new ShadowIntKey();
			db.Add(entity2);
			db.SaveChanges();

			var options = new object[] { new EntityFrameworkCoreMappingOptions(dbContextInstance: db) };

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, ShadowIntKey>(options));

			Assert.IsTrue(_matcher.Match(entity1, entity1, options));
			Assert.IsFalse(_matcher.Match(entity1, entity2, options));
		}

		[TestMethod]
		public void ShouldNotMatchOwnedEntities() {
			// Should be ObjectEqualsMatcher
			Assert.IsTrue(_matcher.CanMatch<OwnedEntity, OwnedEntity>());

			Assert.IsFalse(_matcher.Match(new OwnedEntity(), new OwnedEntity()));
			var ent = new OwnedEntity();
			Assert.IsTrue(_matcher.Match(ent, ent));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMatchKeylessEntities() {
			// Should be ObjectEqualsMatcher
			Assert.IsTrue(_matcher.CanMatch<Keyless, Keyless>());

			Assert.IsFalse(_matcher.Match(new Keyless(), new Keyless()));
			var ent = new Keyless();
			Assert.IsTrue(_matcher.Match(ent, ent));
		}
#endif

		[TestMethod]
		public void ShouldNotLockSemaphoreInsideNestedSemaphoreContext() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			db.Database.EnsureDeleted();
			db.Database.EnsureCreated();
			var entity1 = new ShadowIntKey();
			db.Add(entity1);
			var entity2 = new ShadowIntKey();
			db.Add(entity2);
			db.SaveChanges();

			var semaphore = EfCoreUtils.GetOrCreateSemaphoreForDbContext(db);

			var options = new object[] {
				new EntityFrameworkCoreMappingOptions(dbContextInstance: db),
				NestedSemaphoreContext.Instance
			};

			semaphore.Wait();
			try {
				Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, int>(options));

				Assert.IsTrue(_matcher.Match(entity1, entity1, options));
				Assert.IsFalse(_matcher.Match(entity1, entity2, options));
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
			var entity1 = new ShadowIntKey();
			db.Add(entity1);
			var entity2 = new ShadowIntKey();
			db.Add(entity2);
			db.SaveChanges();

			var options = new object[] { new EntityFrameworkCoreMappingOptions(dbContextInstance: db) };

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, ShadowIntKey>(options));

			Assert.IsTrue(_matcher.Match(entity1, entity1, options));
			Assert.IsFalse(_matcher.Match(entity1, entity2, options));

			db.Dispose();

			Assert.IsTrue(_matcher.CanMatch<ShadowIntKey, ShadowIntKey>(options));

			var exc = Assert.ThrowsException<MatcherException>(() => _matcher.Match(entity1, entity1, options));
			Assert.IsInstanceOfType(exc.InnerException, typeof(ObjectDisposedException));
		}
	}
}
