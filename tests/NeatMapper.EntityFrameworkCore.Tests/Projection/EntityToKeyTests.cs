using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore.Tests.Projection {
	[TestClass]
	public class EntityToKeyTests {
		private SqliteConnection _connection = null;
		private ServiceProvider _serviceProvider = null;
		private IProjector _projector = null;

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
			_projector = _serviceProvider.GetRequiredService<IProjector>();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider?.Dispose();
			_connection?.Dispose();
		}


		[TestMethod]
		public void ShouldProjectEntityToKey() {
			{ 
				Assert.IsTrue(_projector.CanProject<IntKey, int>());

				var param = Expression.Parameter(typeof(IntKey));
				Expression body = Expression.Property(param, nameof(IntKey.Id));
				body = Expression.Condition(
					Expression.NotEqual(param, Expression.Constant(null, typeof(IntKey))),
					body,
					Expression.Default(typeof(int)));
				var expr = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<IntKey, int>());
			}

			{
				Assert.IsTrue(_projector.CanProject<IntFieldKey, int>());

				var param = Expression.Parameter(typeof(IntFieldKey));
				Expression body = Expression.Field(param, nameof(IntFieldKey.Id));
				body = Expression.Condition(
					Expression.NotEqual(param, Expression.Constant(null, typeof(IntFieldKey))),
					body,
					Expression.Default(typeof(int)));
				var expr = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<IntFieldKey, int>());
			}
		}

		[TestMethod]
		public void ShouldProjectEntityToNullableKey() {
			{ 
				Assert.IsTrue(_projector.CanProject<IntKey, int?>());

				var param = Expression.Parameter(typeof(IntKey));
				Expression body = Expression.Convert(Expression.Property(param, nameof(IntKey.Id)), typeof(int?));
				body = Expression.Condition(
					Expression.NotEqual(param, Expression.Constant(null, typeof(IntKey))),
					body,
					Expression.Default(typeof(int?)));
				var expr = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<IntKey, int?>());
			}

			{
				Assert.IsTrue(_projector.CanProject<IntFieldKey, int?>());

				var param = Expression.Parameter(typeof(IntFieldKey));
				Expression body = Expression.Convert(Expression.Field(param, nameof(IntFieldKey.Id)), typeof(int?));
				body = Expression.Condition(
					Expression.NotEqual(param, Expression.Constant(null, typeof(IntFieldKey))),
					body,
					Expression.Default(typeof(int?)));
				var expr = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<IntFieldKey, int?>());
			}
		}

		[TestMethod]
		public void ShouldProjectEntityToShadowKey() {
			Assert.IsTrue(_projector.CanProject<ShadowStringKey, string>());

			var param = Expression.Parameter(typeof(ShadowStringKey));
			Expression body = Expression.Call(typeof(EF).GetMethod(nameof(EF.Property)).MakeGenericMethod(typeof(string)), param, Expression.Constant("StringId"));
			body = Expression.Condition(
				Expression.NotEqual(param, Expression.Constant(null, typeof(ShadowStringKey))),
				body,
				Expression.Default(typeof(string)));
			var expr = Expression.Lambda(body, param);
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<ShadowStringKey, string>());
		}

		[TestMethod]
		public void ShouldProjectEntityToShadowKeyCompilable() {
			var db = _serviceProvider.GetRequiredService<TestContext>();
			var options = new EntityFrameworkCoreMappingOptions(dbContextInstance: db);
			Assert.IsTrue(_projector.CanProject<ShadowStringKey, string>(options, ProjectionCompilationContext.Instance));

			_projector.Project<ShadowStringKey, string>(options, ProjectionCompilationContext.Instance);
		}

		[TestMethod]
		public void ShouldThrowIfDbContextDisposedInProjectEntityToShadowKeyCompilable() {
			TestContext db = new TestContext(_serviceProvider.GetRequiredService<IOptions<DbContextOptions<TestContext>>>().Value);
			db.Dispose();
			Assert.ThrowsException<ObjectDisposedException>(() => db.Database);

			var options = new EntityFrameworkCoreMappingOptions(dbContextInstance: db);

			// Returns true regardless because the map is not checked
			Assert.IsTrue(_projector.CanProject<ShadowStringKey, string>(options, ProjectionCompilationContext.Instance));

			var map = _projector.Project<ShadowStringKey, string>(options, ProjectionCompilationContext.Instance);
			var deleg = map.Compile();

			var exc = Assert.ThrowsException<ProjectionException>(() => deleg.Invoke(new ShadowStringKey()));
			Assert.IsInstanceOfType(exc.InnerException, typeof(ObjectDisposedException));
		}

		[TestMethod]
		public void ShouldProjectEntityToCompositeKey() {
			// Tuple
			{
				Assert.IsTrue(_projector.CanProject<CompositePrimitiveKey, Tuple<int, Guid>>());

				var param = Expression.Parameter(typeof(CompositePrimitiveKey));
				Expression body = Expression.New(
					typeof(Tuple<int, Guid>).GetConstructors().Single(),
					Expression.Property(param, nameof(CompositePrimitiveKey.Id1)),
					Expression.Property(param, nameof(CompositePrimitiveKey.Id2)));
				body = Expression.Condition(
					Expression.NotEqual(param, Expression.Constant(null, typeof(CompositePrimitiveKey))),
					body,
					Expression.Default(typeof(Tuple<int, Guid>)));
				var expr = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<CompositePrimitiveKey, Tuple<int, Guid>>());
			}

			// ValueTuple
			{
				Assert.IsTrue(_projector.CanProject<CompositePrimitiveKey, (int, Guid)>());

				var param = Expression.Parameter(typeof(CompositePrimitiveKey));
				Expression body = Expression.New(
					typeof((int, Guid)).GetConstructors().Single(),
					Expression.Property(param, nameof(CompositePrimitiveKey.Id1)),
					Expression.Property(param, nameof(CompositePrimitiveKey.Id2)));
				body = Expression.Condition(
					Expression.NotEqual(param, Expression.Constant(null, typeof(CompositePrimitiveKey))),
					body,
					Expression.Default(typeof((int, Guid))));
				var expr = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<CompositePrimitiveKey, (int, Guid)>());
			}
		}

		[TestMethod]
		public void ShouldProjectEntityToNullableCompositeKey() {
			Assert.IsTrue(_projector.CanProject<CompositePrimitiveKey, (int, Guid)?>());

			var param = Expression.Parameter(typeof(CompositePrimitiveKey));
			Expression body = Expression.Convert(
				Expression.New(
					typeof((int, Guid)).GetConstructors().Single(),
					Expression.Property(param, nameof(CompositePrimitiveKey.Id1)),
					Expression.Property(param, nameof(CompositePrimitiveKey.Id2))),
				typeof((int, Guid)?));
			body = Expression.Condition(
				Expression.NotEqual(param, Expression.Constant(null, typeof(CompositePrimitiveKey))),
				body,
				Expression.Default(typeof((int, Guid)?)));
			var expr = Expression.Lambda(body, param);
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<CompositePrimitiveKey, (int, Guid)?>());
		}

		[TestMethod]
		public void ShouldProjectEntityToCompositeShadowKey() {
			Assert.IsTrue(_projector.CanProject<ShadowCompositeKey, Tuple<int, string>>());

			var param = Expression.Parameter(typeof(ShadowCompositeKey));
			Expression body = Expression.New(
				typeof(Tuple<int, string>).GetConstructors().Single(),
				Expression.Property(param, nameof(ShadowCompositeKey.Id1)),
				Expression.Call(typeof(EF).GetMethod(nameof(EF.Property)).MakeGenericMethod(typeof(string)), param, Expression.Constant("Id2")));
			body = Expression.Condition(
				Expression.NotEqual(param, Expression.Constant(null, typeof(ShadowCompositeKey))),
				body,
				Expression.Default(typeof(Tuple<int, string>)));
			var expr = Expression.Lambda(body, param);
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<ShadowCompositeKey, Tuple<int, string>>());
		}

		[TestMethod]
		public void ShouldNotProjectEntityToCompositeKeyIfOrderIsWrong() {
			// Tuple
			{
				{
					Assert.IsFalse(_projector.CanProject<CompositePrimitiveKey, Tuple<Guid, int>>());

					TestUtils.AssertMapNotFound(() => _projector.Project<CompositePrimitiveKey, Tuple<Guid, int>>());
				}

				{
					Assert.IsFalse(_projector.CanProject<CompositeClassKey, Tuple<string, int>>());

					TestUtils.AssertMapNotFound(() => _projector.Project<CompositeClassKey, Tuple<string, int>>());
				}
			}

			// ValueTuple
			{
				{
					Assert.IsFalse(_projector.CanProject<CompositePrimitiveKey, (Guid, int)>());

					TestUtils.AssertMapNotFound(() => _projector.Project<CompositePrimitiveKey, (Guid, int)>());
				}

				{
					Assert.IsFalse(_projector.CanProject<CompositeClassKey, (string, int)>());

					TestUtils.AssertMapNotFound(() => _projector.Project<CompositeClassKey, (string, int)>());
				}
			}
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_projector.CanProject<OwnedEntity1, int>());

			TestUtils.AssertMapNotFound(() => _projector.Project<OwnedEntity1, int>());

			Assert.IsFalse(_projector.CanProject<OwnedEntity1, Tuple<string, int>>());

			TestUtils.AssertMapNotFound(() => _projector.Project<OwnedEntity1, Tuple<string, int>>());
			TestUtils.AssertMapNotFound(() => _projector.Project<OwnedEntity1, Tuple<int, int>>());

			Assert.IsFalse(_projector.CanProject<OwnedEntity1, (string, int)>());

			TestUtils.AssertMapNotFound(() => _projector.Project<OwnedEntity1, (string, int)>());
			TestUtils.AssertMapNotFound(() => _projector.Project<OwnedEntity1, (int, int)>());
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_projector.CanProject<Keyless, int>());

			TestUtils.AssertMapNotFound(() => _projector.Project<Keyless, int>());
		}
#endif
	}
}
