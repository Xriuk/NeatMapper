using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NeatMapper.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class KeyToExpressionTests {
		private SqliteConnection _connection = null;
		protected ServiceProvider _serviceProvider = null;
		protected Mock<DbCommandInterceptor> _interceptorMock = null;
		protected IMapper _mapper = null;
		protected TestContext _db = null;

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


		[TestMethod]
		public void ShouldMapKeyToExpression() {
			// Not null
			{
				Assert.IsTrue(_mapper.CanMapNew<int, Expression<Func<IntKey, bool>>>());
				Assert.IsTrue(_mapper.CanMapNew<Guid, Expression<Func<GuidKey, bool>>>());
				Assert.IsTrue(_mapper.CanMapNew<int, Expression<Func<IntFieldKey, bool>>>());

				Expression<Func<IntKey, bool>> expr1 = entity => entity.Id == 2;
				TestUtils.AssertExpressionsEqual(expr1, _mapper.Map<Expression<Func<IntKey, bool>>>(2));

				{ 
					var param = Expression.Parameter(typeof(GuidKey));
					Expression body = Expression.Property(param, nameof(GuidKey.Id));
					body = Expression.Equal(body, Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
					var expr2 = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr2, _mapper.Map<Expression<Func<GuidKey, bool>>>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				}
				{
					var param = Expression.Parameter(typeof(GuidKey));
					Expression body = Expression.Property(param, nameof(GuidKey.Id));
					body = Expression.Equal(body, Expression.Constant(Guid.Empty));
					var expr3 = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr3, _mapper.Map<Expression<Func<GuidKey, bool>>>(Guid.Empty));
				}

				Expression<Func<StringKey, bool>> expr4 = entity => entity.Id == "Test";
				TestUtils.AssertExpressionsEqual(expr4, _mapper.Map<Expression<Func<StringKey, bool>>>("Test"));

				Expression<Func<IntFieldKey, bool>> expr5 = entity => entity.Id == 3;
				TestUtils.AssertExpressionsEqual(expr5, _mapper.Map<Expression<Func<IntFieldKey, bool>>>(3));

				Expression<Func<StringFieldKey, bool>> expr6 = entity => entity.Id == "Test";
				TestUtils.AssertExpressionsEqual(expr6, _mapper.Map<Expression<Func<StringFieldKey, bool>>>("Test"));
			}

			// Null
			{
				Assert.IsTrue(_mapper.CanMapNew<string, Expression<Func<StringKey, bool>>>());

				Assert.IsNull(_mapper.Map<string, Expression<Func<StringKey, bool>>>(null));
			}
		}

		[TestMethod]
		public void ShouldMapNullableKeyToExpression() {
			Assert.IsTrue(_mapper.CanMapNew<int?, Expression<Func<IntKey, bool>>>());
			Assert.IsTrue(_mapper.CanMapNew<Guid?, Expression<Func<GuidKey, bool>>>());
			Assert.IsTrue(_mapper.CanMapNew<int?, Expression<Func<IntFieldKey, bool>>>());

			// Not null
			{
				{
					var param = Expression.Parameter(typeof(IntKey));
					Expression body = Expression.Property(param, nameof(IntKey.Id));
					body = Expression.Equal(body, Expression.Constant((int?)2));
					var expr3 = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr3, _mapper.Map<int?, Expression<Func<IntKey, bool>>>(2));
				}

				{
					var param = Expression.Parameter(typeof(GuidKey));
					Expression body = Expression.Property(param, nameof(GuidKey.Id));
					body = Expression.Equal(body, Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
					var expr2 = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr2, _mapper.Map<Guid?, Expression<Func<GuidKey, bool>>>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				}
				{
					var param = Expression.Parameter(typeof(GuidKey));
					Expression body = Expression.Property(param, nameof(GuidKey.Id));
					body = Expression.Equal(body, Expression.Constant((Guid?)Guid.Empty));
					var expr3 = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr3, _mapper.Map<Guid?, Expression<Func<GuidKey, bool>>>(Guid.Empty));
				}

				{
					var param = Expression.Parameter(typeof(IntFieldKey));
					Expression body = Expression.Field(param, nameof(IntFieldKey.Id));
					body = Expression.Equal(body, Expression.Constant((int?)3));
					var expr3 = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr3, _mapper.Map<int?, Expression<Func<IntFieldKey, bool>>>(3));
				}
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<int?, Expression<Func<IntKey, bool>>>(null));
				Assert.IsNull(_mapper.Map<Guid?, Expression<Func<GuidKey, bool>>>(null));

				Assert.IsNull(_mapper.Map<int?, Expression<Func<IntFieldKey, bool>>>(null));
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeyToExpression() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapNew<Tuple<int, Guid>, Expression<Func<CompositePrimitiveKey, bool>>>());
				Assert.IsTrue(_mapper.CanMapNew<Tuple<int, string>, Expression<Func<CompositeClassKey, bool>>>());

				// Not null
				{
					{
						var param = Expression.Parameter(typeof(CompositePrimitiveKey));
						var body = Expression.AndAlso(
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
						var expr = Expression.Lambda(body, param);
						TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					}

					{
						var param = Expression.Parameter(typeof(CompositePrimitiveKey));
						var body = Expression.AndAlso(
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(Guid.Empty)));
						var expr = Expression.Lambda(body, param);
						TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>(Tuple.Create(2, Guid.Empty)));
					}

					{
						var param = Expression.Parameter(typeof(CompositeClassKey));
						var body = Expression.AndAlso(
							Expression.Equal(Expression.Property(param, nameof(CompositeClassKey.Id1)), Expression.Constant(2)),
							Expression.Equal(Expression.Property(param, nameof(CompositeClassKey.Id2)), Expression.Constant("Test")));
						var expr = Expression.Lambda(body, param);
						TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositeClassKey, bool>>>(Tuple.Create(2, "Test")));
					}
				}

				// Null
				{
					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, Expression<Func<CompositePrimitiveKey, bool>>>(null));
					Assert.IsNull(_mapper.Map<Tuple<int, string>, Expression<Func<CompositeClassKey, bool>>>(null));
					Assert.IsNull(_mapper.Map<Tuple<int, string>, Expression<Func<CompositeClassKey, bool>>>(Tuple.Create<int, string>(2, null)));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapNew<(int, Guid), Expression<Func<CompositePrimitiveKey, bool>>>());
				Assert.IsTrue(_mapper.CanMapNew<(int, string), Expression<Func<CompositeClassKey, bool>>>());

				// Not null
				{
					{
						var param = Expression.Parameter(typeof(CompositePrimitiveKey));
						var body = Expression.AndAlso(
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
						var expr = Expression.Lambda(body, param);
						TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					}

					{
						var param = Expression.Parameter(typeof(CompositePrimitiveKey));
						var body = Expression.AndAlso(
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
							Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(Guid.Empty)));
						var expr = Expression.Lambda(body, param);
						TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>((2, Guid.Empty)));
					}

					{
						var param = Expression.Parameter(typeof(CompositeClassKey));
						var body = Expression.AndAlso(
							Expression.Equal(Expression.Property(param, nameof(CompositeClassKey.Id1)), Expression.Constant(2)),
							Expression.Equal(Expression.Property(param, nameof(CompositeClassKey.Id2)), Expression.Constant("Test")));
						var expr = Expression.Lambda(body, param);
						TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositeClassKey, bool>>>((2, "Test")));
					}
				}

				// Null
				{
					Assert.IsNull(_mapper.Map<(int, string), Expression<Func<CompositeClassKey, bool>>>((2, null)));
				}
			}
		}

		[TestMethod]
		public void ShouldMapNullableCompositeKeyToExpression() {
			Assert.IsTrue(_mapper.CanMapNew<(int, Guid)?, Expression<Func<CompositePrimitiveKey, bool>>>());
			Assert.IsTrue(_mapper.CanMapNew<(int, string)?, Expression<Func<CompositeClassKey, bool>>>());

			// Not null
			{
				{
					var param = Expression.Parameter(typeof(CompositePrimitiveKey));
					var body = Expression.AndAlso(
						Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
						Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					var expr = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr, _mapper.Map< (int, Guid)?, Expression <Func<CompositePrimitiveKey, bool>>>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
				}

				{
					var param = Expression.Parameter(typeof(CompositePrimitiveKey));
					var body = Expression.AndAlso(
						Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
						Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(Guid.Empty)));
					var expr = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr, _mapper.Map<(int, Guid)?, Expression<Func<CompositePrimitiveKey, bool>>>((2, Guid.Empty)));
				}

				{
					var param = Expression.Parameter(typeof(CompositeClassKey));
					var body = Expression.AndAlso(
						Expression.Equal(Expression.Property(param, nameof(CompositeClassKey.Id1)), Expression.Constant(2)),
						Expression.Equal(Expression.Property(param, nameof(CompositeClassKey.Id2)), Expression.Constant("Test")));
					var expr = Expression.Lambda(body, param);
					TestUtils.AssertExpressionsEqual(expr, _mapper.Map<(int, string)?, Expression<Func<CompositeClassKey, bool>>>((2, "Test")));
				}
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<(int, string)?, Expression<Func<CompositeClassKey, bool>>>(null));
				Assert.IsNull(_mapper.Map<(int, string)?, Expression<Func<CompositeClassKey, bool>>>((2, null)));
			}
		}

		[TestMethod]
		public void ShouldNotMapCompositeKeyToExpressionIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_mapper.CanMapNew<Tuple<Guid, int>, Expression<Func<CompositePrimitiveKey, bool>>>());
				Assert.IsFalse(_mapper.CanMapNew<Tuple<string, int>, Expression<Func<CompositeClassKey, bool>>>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<CompositeClassKey, bool>>>(Tuple.Create("Test", 2)));
			}

			// ValueTuple
			{
				Assert.IsFalse(_mapper.CanMapNew<(Guid, int), Expression<Func<CompositePrimitiveKey, bool>>>());
				Assert.IsFalse(_mapper.CanMapNew<(string, int), Expression<Func<CompositeClassKey, bool>>>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<CompositeClassKey, bool>>>(("Test", 2)));
			}
		}

		[TestMethod]
		public void ShouldMapShadowKeyToExpression() {
			Assert.IsTrue(_mapper.CanMapNew<int, Expression<Func<ShadowIntKey, bool>>>());

			Expression<Func<ShadowIntKey, bool>> expr1 = entity => EF.Property<int>(entity, "Id") == 2;
			TestUtils.AssertExpressionsEqual(expr1, _mapper.Map<Expression<Func<ShadowIntKey, bool>>>(2));


			Assert.IsTrue(_mapper.CanMapNew<Tuple<int, string>, Expression<Func<ShadowCompositeKey, bool>>>());

			Expression<Func<ShadowCompositeKey, bool>> expr2 = entity => entity.Id1 == 2 && EF.Property<string>(entity, "Id2") == "Test";
			TestUtils.AssertExpressionsEqual(expr2, _mapper.Map<Expression<Func<ShadowCompositeKey, bool>>>(Tuple.Create(2, "Test")));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapNew<int, Expression<Func<OwnedEntity1, bool>>>());
			Assert.IsFalse(_mapper.CanMapNew<int, Expression<Func<OwnedEntity1, bool>>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<OwnedEntity1, bool>>>(2));
			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<OwnedEntity2, bool>>>(2));

			Assert.IsFalse(_mapper.CanMapNew<Tuple<string, int>, Expression<Func<OwnedEntity1, bool>>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<OwnedEntity1, bool>>>(Tuple.Create("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<OwnedEntity1, bool>>>(Tuple.Create(2, 2)));

			Assert.IsFalse(_mapper.CanMapNew<(string, int), Expression<Func<OwnedEntity1, bool>>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<OwnedEntity1, bool>>>(("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<OwnedEntity1, bool>>>((2, 2)));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapNew<int, Expression<Func<Keyless, bool>>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Expression<Func<Keyless, bool>>>(2));
		}
#endif

		[TestMethod]
		public void ShouldMapKeysCollectionToExpression() {
			Assert.IsTrue(_mapper.CanMapNew<int[], Expression<Func<IntKey, bool>>>());
			Assert.IsTrue(_mapper.CanMapNew<int[], Expression<Func<IntFieldKey, bool>>>());


			Expression<Func<IntKey, bool>> expr1 = entity => new[] { 2, 0 }.Contains(entity.Id);
			TestUtils.AssertExpressionsEqual(expr1, _mapper.Map<Expression<Func<IntKey, bool>>>(new[] { 2, 0 }));

			Expression<Func<IntKey, bool>> expr2 = entity => entity.Id == 2;
			TestUtils.AssertExpressionsEqual(expr2, _mapper.Map<Expression<Func<IntKey, bool>>>(new[] { 2 }));

			{
				var param = Expression.Parameter(typeof(GuidKey));
				var body = Expression.Call(NeatMapper.TypeUtils.GetMethod(() => default(IEnumerable<object>).Contains(default(object))).MakeGenericMethod(typeof(Guid)),
					Expression.NewArrayInit(typeof(Guid), Expression.Constant(Guid.Empty), Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190"))),
					Expression.Property(param, nameof(GuidKey.Id)));
				var expr3 = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr3, _mapper.Map<Expression<Func<GuidKey, bool>>>(new List<Guid> { Guid.Empty, new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
			}

			Expression<Func<StringKey, bool>> expr4 = entity => entity.Id == "Test";
			TestUtils.AssertExpressionsEqual(expr4, _mapper.Map<Expression<Func<StringKey, bool>>>(new[] { null, "Test" }));

			Expression<Func<IntFieldKey, bool>> expr5 = entity => new[] { 2, 0 }.Contains(entity.Id);
			TestUtils.AssertExpressionsEqual(expr5, _mapper.Map<Expression<Func<IntFieldKey, bool>>>(new[] { 2, 0 }));

			Expression<Func<StringFieldKey, bool>> expr6 = entity => new[] { "Test", "Test2" }.Contains(entity.Id);
			TestUtils.AssertExpressionsEqual(expr6, _mapper.Map<Expression<Func<StringFieldKey, bool>>>(new[] { "Test", "Test2" }));
		}

		[TestMethod]
		public void ShouldMapNullableKeysCollectionToExpression() {
			Assert.IsTrue(_mapper.CanMapNew<IEnumerable<int?>, Expression<Func<IntKey, bool>>>());

			Expression<Func<IntKey, bool>> expr1 = entity => entity.Id == 2;
			TestUtils.AssertExpressionsEqual(expr1, _mapper.Map<Expression<Func<IntKey, bool>>>(new int?[] { 2, null }));

			{
				var param = Expression.Parameter(typeof(GuidKey));
				Expression body = Expression.Property(param, nameof(GuidKey.Id));
				body = Expression.Equal(body, Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				var expr2 = Expression.Lambda(body, param);
				TestUtils.AssertExpressionsEqual(expr2, _mapper.Map<Expression<Func<GuidKey, bool>>>(new List<Guid?> { null, new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeysCollectionToExpression() {
			var param = Expression.Parameter(typeof(CompositePrimitiveKey));
			var body = Expression.OrElse(
				Expression.AndAlso(
					Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
					Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190")))),
				Expression.AndAlso(
					Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(3)),
					Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(Guid.Empty))));
			var expr = Expression.Lambda(body, param);

			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapNew<IEnumerable<Tuple<int, Guid>>, Expression<Func<CompositePrimitiveKey, bool>>>());

				TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>(new Tuple<int, Guid>[] { Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), Tuple.Create(3, Guid.Empty) }));
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapNew<(int, Guid)[], Expression<Func<CompositePrimitiveKey, bool>>>());

				TestUtils.AssertExpressionsEqual(expr, _mapper.Map<Expression<Func<CompositePrimitiveKey, bool>>>(new (int, Guid)[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (3, Guid.Empty) }));
			}
		}

		[TestMethod]
		public void ShouldMapNullableCompositeKeysCollectionToExpression() {
			Assert.IsTrue(_mapper.CanMapNew<(int, Guid)?[], Expression<Func<CompositePrimitiveKey, bool>>>());

			var param = Expression.Parameter(typeof(CompositePrimitiveKey));
			var body = Expression.AndAlso(
				Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id1)), Expression.Constant(2)),
				Expression.Equal(Expression.Property(param, nameof(CompositePrimitiveKey.Id2)), Expression.Constant(new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
			var expr = Expression.Lambda(body, param);
			TestUtils.AssertExpressionsEqual(expr, _mapper.Map<(int, Guid)?[], Expression<Func<CompositePrimitiveKey, bool>>>(new (int, Guid)?[] { null, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")) }));
		}
	}
}
