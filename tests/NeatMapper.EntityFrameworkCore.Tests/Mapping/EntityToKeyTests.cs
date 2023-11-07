using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class EntityToKeyTests {
		private SqliteConnection _connection = null;
		private ServiceProvider _serviceProvider = null;
		private IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			_connection = new SqliteConnection("Filename=:memory:");
			_connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(_connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_mapper = _serviceProvider.GetRequiredService<IMapper>();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider.Dispose();
			_connection.Dispose();
		}


		[TestMethod]
		public void ShouldMapEntityToKey() {
			Assert.IsTrue(_mapper.CanMapNew<IntKey, int>());

			// Not null
			{
				Assert.AreEqual(2, _mapper.Map<int>(new IntKey { Id = 2 }));
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), _mapper.Map<Guid>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				Assert.AreEqual("Test", _mapper.Map<string>(new StringKey { Id = "Test" }));
			}

			// Null
			{
				Assert.AreEqual(0, _mapper.Map<IntKey, int>(null));
				Assert.AreEqual(Guid.Empty, _mapper.Map<GuidKey, Guid>(null));
				Assert.IsNull(_mapper.Map<StringKey, string>(null));
			}
		}

		[TestMethod]
		public void ShouldMapEntityToNullableKey() {
			Assert.IsTrue(_mapper.CanMapNew<IntKey, int?>());

			// Not null
			{
				Assert.AreEqual(2, _mapper.Map<int?>(new IntKey { Id = 2 }));
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), _mapper.Map<Guid?>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<IntKey, int?>(null));
				Assert.IsNull(_mapper.Map<GuidKey, Guid?>(null));
			}
		}

		[TestMethod]
		public void ShouldMapEntityToCompositeKey() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapNew<CompositePrimitiveKey, Tuple<int, Guid>>());

				// Not null
				{
					{ 
						var result = _mapper.Map<Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
						Assert.IsNotNull(result);
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result.Item2);
					}

					{ 
						var result = _mapper.Map<Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
						Assert.IsNotNull(result);
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual("Test", result.Item2);
					}
				}

				// Null
				{
					Assert.IsNull(_mapper.Map<CompositePrimitiveKey, Tuple<int, Guid>>(null));
					Assert.IsNull(_mapper.Map<CompositeClassKey, Tuple<int, string>>(null));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapNew<CompositePrimitiveKey, (int, Guid)>());

				// Not null
				{
					{ 
						var result = _mapper.Map<(int, Guid)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result.Item2);
					}

					{ 
						var result = _mapper.Map<(int, string)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
						Assert.AreEqual(2, result.Item1);
						Assert.AreEqual("Test", result.Item2);
					}
				}

				// Null
				{
					{ 
						var result = _mapper.Map<CompositePrimitiveKey, (int, Guid)>(null);
						Assert.AreEqual(0, result.Item1);
						Assert.AreEqual(Guid.Empty, result.Item2);
					}

					{ 
						var result = _mapper.Map<CompositeClassKey, (int, string)>(null);
						Assert.AreEqual(0, result.Item1);
						Assert.IsNull(result.Item2);
					}
				}
			}
		}

		[TestMethod]
		public void ShouldMapEntityToNullableCompositeKey() {
			Assert.IsTrue(_mapper.CanMapNew<CompositePrimitiveKey, (int, Guid)?>());

			// Not null
			{
				{ 
					var result = _mapper.Map<(int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Value.Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result.Value.Item2);
				}

				{ 
					var result = _mapper.Map<(int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
					Assert.IsNotNull(result);
					Assert.AreEqual(2, result.Value.Item1);
					Assert.AreEqual("Test", result.Value.Item2);
				}
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<CompositePrimitiveKey, (int, Guid)?>(null));
				Assert.IsNull(_mapper.Map<CompositeClassKey, (int, string)?>(null));
			}
		}

		[TestMethod]
		public void ShouldNotMapEntityToCompositeKeyIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_mapper.CanMapNew<CompositePrimitiveKey, Tuple<Guid, int>>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<Guid, int>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<string, int>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}

			// ValueTuple
			{
				Assert.IsFalse(_mapper.CanMapNew<CompositePrimitiveKey, (Guid, int)>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}
		}

		[TestMethod]
		public void ShouldNotMapMerge() {
			Assert.IsFalse(_mapper.CanMapMerge<StringKey, string>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(new StringKey { Id = "Test" }, ""));
			TestUtils.AssertMapNotFound(() => _mapper.Map<IntKey, int?>(new IntKey { Id = 2 }, 3));
		}

		[TestMethod]
		public void ShouldNotMapEntitiesWithShadowKeys() {
			Assert.IsFalse(_mapper.CanMapNew<ShadowIntKey, int>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(new ShadowIntKey()));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapNew<OwnedEntity, int>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(new OwnedEntity()));

			Assert.IsFalse(_mapper.CanMapNew<OwnedEntity, Tuple<string, int>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<string, int>>(new OwnedEntity()));
			TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<int, int>>(new OwnedEntity()));

			Assert.IsFalse(_mapper.CanMapNew<OwnedEntity, (string, int)>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)>(new OwnedEntity()));
			TestUtils.AssertMapNotFound(() => _mapper.Map<(int, int)>(new OwnedEntity()));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapNew<Keyless, int>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(new Keyless()));
		}
#endif


		[TestMethod]
		public void ShouldMapEntitiesCollectionToKeysCollection() {
			Assert.IsTrue(_mapper.CanMapNew<IEnumerable<IntKey>, int[]>());

			{ 
				var result = _mapper.Map<IEnumerable<int>>(new[] { new IntKey { Id = 2 }, null });
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(2, result.First());
				Assert.AreEqual(0, result.Last());
			}

			{
				var result = _mapper.Map<Guid[]>(new List<GuidKey>{ null, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") } });
				Assert.AreEqual(2, result.Length);
				Assert.AreEqual(Guid.Empty, result[0]);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[1]);
			}

			{
				var result = _mapper.Map<string[]>(new[] { null, new StringKey { Id = "Test" } });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.AreEqual("Test", result[1]);
			}
		}

		[TestMethod]
		public void ShouldMapEntitiesCollectionToNullableKeysCollection() {
			Assert.IsTrue(_mapper.CanMapNew<IEnumerable<IntKey>, int?[]>());

			{
				var result = _mapper.Map<IEnumerable<int?>>(new[] { new IntKey { Id = 2 }, null });
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(2, result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = _mapper.Map<Guid?[]>(new List<GuidKey> { null, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") } });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[1]);
			}
		}

		[TestMethod]
		public void ShouldMapEntitiesCollectionToCompositeKeysCollection() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapNew<IEnumerable<CompositePrimitiveKey>, Tuple<int, Guid>[]>());

				{ 
					var result = _mapper.Map<Tuple<int, Guid>[]>(new[]{ null,  new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } });
					Assert.AreEqual(2, result.Length);
					Assert.IsNull(result[0]);
					Assert.AreEqual(2, result[1].Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[1].Item2);
				}

				{ 
					var result = _mapper.Map<List<Tuple<int, string>>>(new[]{ new CompositeClassKey { Id1 = 2, Id2 = "Test" }, null });
					Assert.AreEqual(2, result.Count);
					Assert.AreEqual(2, result[0].Item1);
					Assert.AreEqual("Test", result[0].Item2);
					Assert.IsNull(result[1]);
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapNew<CompositePrimitiveKey[], List<(int, Guid)>>());

				{ 
					var result = _mapper.Map<(int, Guid)[]>(new[]{ new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null });
					Assert.AreEqual(2, result.Length);
					Assert.AreEqual(2, result[0].Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[0].Item2);
					Assert.AreEqual(0, result[1].Item1);
					Assert.AreEqual(Guid.Empty, result[1].Item2);
				}

				{ 
					var result = _mapper.Map<(int, string)[]>(new[]{ null, new CompositeClassKey { Id1 = 2, Id2 = "Test" } });
					Assert.AreEqual(2, result.Length);
					Assert.AreEqual(0, result[0].Item1);
					Assert.IsNull(result[0].Item2);
					Assert.AreEqual(2, result[1].Item1);
					Assert.AreEqual("Test", result[1].Item2);
				}
			}
		}

		[TestMethod]
		public void ShouldMapEntitiesCollectionToNullableCompositeKeysCollection() {
			Assert.IsTrue(_mapper.CanMapNew<CompositePrimitiveKey[], (int, Guid)?[]>());

			{
				var result = _mapper.Map<(int, Guid)?[]>(new[] { new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null });
				Assert.AreEqual(2, result.Length);
				Assert.AreEqual(2, result[0].Value.Item1);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result[0].Value.Item2);
				Assert.IsNull(result[1]);
			}

			{
				var result = _mapper.Map<(int, string)?[]>(new[] { null, new CompositeClassKey { Id1 = 2, Id2 = "Test" } });
				Assert.AreEqual(2, result.Length);
				Assert.IsNull(result[0]);
				Assert.AreEqual(2, result[1].Value.Item1);
				Assert.AreEqual("Test", result[1].Value.Item2);
			}
		}

		[TestMethod]
		public void ShouldNotMapEntitiesCollectionToCompositeKeysCollectionIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_mapper.CanMapNew<IEnumerable<CompositePrimitiveKey>, Tuple<Guid, int>[]>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<Guid, int>[]>(new[]{ new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<string, int>[]>(new[]{ new CompositeClassKey { Id1 = 2, Id2 = "Test" } }));
			}

			// ValueTuple
			{
				Assert.IsFalse(_mapper.CanMapNew<CompositePrimitiveKey[], (Guid, int)[]>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)[]>(new[] { new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)[]>(new[] { new CompositeClassKey { Id1 = 2, Id2 = "Test" } }));

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)?[]>(new[] { new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") } }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)?[]>(new[] { new CompositeClassKey { Id1 = 2, Id2 = "Test" } }));
			}
		}

		[TestMethod]
		public void ShouldNotMapMergeCollections() {
			Assert.IsFalse(_mapper.CanMapMerge<IEnumerable<StringKey>, List<string>>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { new StringKey { Id = "Test" } }, new List<string>()));
		}
	}
}
