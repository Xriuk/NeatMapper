using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;

namespace NeatMapper.EntityFrameworkCore.Tests {
	[TestClass]
	public class MatchingTests {
		private SqliteConnection _connection = null;
		private ServiceProvider _serviceProvider = null;
		protected IMatcher _matcher = null;

		protected virtual void Configure(EntityFrameworkCoreOptions options) {

		}

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			_connection = new SqliteConnection("Filename=:memory:");
			_connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(_connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();
			serviceCollection.ConfigureAll<EntityFrameworkCoreOptions>(Configure);
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_matcher = _serviceProvider.GetRequiredService<IMatcher>();
		}

		[TestCleanup]
		public void Cleanup() {
			_serviceProvider.Dispose();
			_connection.Dispose();
		}


		[TestMethod]
		public void ShouldMatchKeyWithEntity() {
			// Key == Entity
			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match(2, new IntKey { Id = 2 }));
					Assert.IsFalse(_matcher.Match(3, new IntKey { Id = 2 }));

					Assert.IsTrue(_matcher.Match(new Guid("56033406-E593-4076-B48A-70988C9F9190"), new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsFalse(_matcher.Match(Guid.Empty, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));

					Assert.IsTrue(_matcher.Match("Test", new StringKey { Id = "Test" }));
					Assert.IsFalse(_matcher.Match("Test2", new StringKey { Id = "Test" }));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<int, IntKey>(2, null));
					Assert.IsFalse(_matcher.Match<int, IntKey>(0, null));

					Assert.IsFalse(_matcher.Match<Guid, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190"), null));
					Assert.IsFalse(_matcher.Match<Guid, GuidKey>(Guid.Empty, null));

					Assert.IsFalse(_matcher.Match<string, StringKey>("Test", null));
					Assert.IsFalse(_matcher.Match<string, StringKey>(null, null));
				}
			}

			// Entity == Key
			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match(new IntKey { Id = 2 }, 2));
					Assert.IsFalse(_matcher.Match(new IntKey { Id = 2 }, 3));

					Assert.IsTrue(_matcher.Match(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new Guid("56033406-E593-4076-B48A-70988C9F9190")));
					Assert.IsFalse(_matcher.Match(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Guid.Empty));

					Assert.IsTrue(_matcher.Match(new StringKey { Id = "Test" }, "Test"));
					Assert.IsFalse(_matcher.Match(new StringKey { Id = "Test" }, "Test2"));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<int, IntKey>(2, null));
					Assert.IsFalse(_matcher.Match<int, IntKey>(0, null));

					Assert.IsFalse(_matcher.Match<GuidKey, Guid>(null, new Guid("56033406-E593-4076-B48A-70988C9F9190")));
					Assert.IsFalse(_matcher.Match<GuidKey, Guid>(null, Guid.Empty));

					Assert.IsFalse(_matcher.Match<StringKey, string>(null, "Test"));
					Assert.IsFalse(_matcher.Match<StringKey, string>(null, null));
				}
			}
		}

		[TestMethod]
		public void ShouldMatchNullableKeyWithEntity() {
			// Key == Entity
			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match<int?, IntKey>(2, new IntKey { Id = 2 }));
					Assert.IsFalse(_matcher.Match<int?, IntKey>(3, new IntKey { Id = 2 }));

					Assert.IsTrue(_matcher.Match<Guid?, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190"), new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(Guid.Empty, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<int?, IntKey>(2, null));
					Assert.IsFalse(_matcher.Match<int?, IntKey>(null, new IntKey { Id = 2 }));
					Assert.IsFalse(_matcher.Match<int?, IntKey>(null, null));

					Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190"), null));
					Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(null, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsFalse(_matcher.Match<Guid?, GuidKey>(null, null));
				}
			}

			// Entity == Key
			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match<IntKey, int?>(new IntKey { Id = 2 }, 2));
					Assert.IsFalse(_matcher.Match<IntKey, int?>(new IntKey { Id = 2 }, 3));

					Assert.IsTrue(_matcher.Match<GuidKey, Guid?>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new Guid("56033406-E593-4076-B48A-70988C9F9190")));
					Assert.IsFalse(_matcher.Match<GuidKey, Guid?>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Guid.Empty));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<IntKey, int?>(null, 2));
					Assert.IsFalse(_matcher.Match<IntKey, int?>(new IntKey { Id = 2 }, null));
					Assert.IsFalse(_matcher.Match<IntKey, int?>(null, null));

					Assert.IsFalse(_matcher.Match<GuidKey, Guid?>(null, new Guid("56033406-E593-4076-B48A-70988C9F9190")));
					Assert.IsFalse(_matcher.Match<GuidKey, Guid?>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
					Assert.IsFalse(_matcher.Match<GuidKey, Guid?>(null, null));
				}
			}
		}

		[TestMethod]
		public void ShouldMatchCompositeKeyWithEntity() {
			// Key == Entity
			{
				// Tuple
				{
					// Not null
					{
						Assert.IsTrue(_matcher.Match<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
						Assert.IsFalse(_matcher.Match<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(0, Guid.Empty), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));

						Assert.IsTrue(_matcher.Match<Tuple<int, string>, CompositeClassKey>(Tuple.Create(2, "Test"), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
						Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(Tuple.Create(0, "Test2"), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
						Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(Tuple.Create(0, (string)null), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
					}

					// Null
					{
						Assert.IsFalse(_matcher.Match<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
						Assert.IsFalse(_matcher.Match<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null));
						Assert.IsFalse(_matcher.Match<Tuple<int, Guid>, CompositePrimitiveKey>(null, null));

						Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(null, new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
						Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(Tuple.Create(2, "Test2"), null));
						Assert.IsFalse(_matcher.Match<Tuple<int, string>, CompositeClassKey>(null, null));
					}
				}

				// ValueTuple
				{
					// Not null
					{
						Assert.IsTrue(_matcher.Match<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
						Assert.IsFalse(_matcher.Match<(int, Guid), CompositePrimitiveKey>((0, Guid.Empty), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));

						Assert.IsTrue(_matcher.Match<(int, string), CompositeClassKey>((2, "Test"), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
						Assert.IsFalse(_matcher.Match<(int, string), CompositeClassKey>((0, "Test2"), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
						Assert.IsFalse(_matcher.Match<(int, string), CompositeClassKey>((0, (string)null), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
					}

					// Null
					{
						Assert.IsFalse(_matcher.Match<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null));

						Assert.IsFalse(_matcher.Match<(int, string), CompositeClassKey>((2, "Test2"), null));
					}
				}
			}

			// Entity == Key
			{
				// Tuple
				{
					// Not null
					{
						Assert.IsTrue(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
						Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Tuple.Create(0, Guid.Empty)));

						Assert.IsTrue(_matcher.Match<CompositeClassKey, Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create(2, "Test")));
						Assert.IsFalse(_matcher.Match<CompositeClassKey, Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create(0, "Test2")));
						Assert.IsFalse(_matcher.Match<CompositeClassKey, Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create(0, (string)null)));
					}

					// Null
					{
						Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
						Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(null, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
						Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, Tuple<int, Guid>>(null, null));

						Assert.IsFalse(_matcher.Match<CompositeClassKey, Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, null));
						Assert.IsFalse(_matcher.Match<CompositeClassKey, Tuple<int, string>>(null, Tuple.Create(2, "Test2")));
						Assert.IsFalse(_matcher.Match<CompositeClassKey, Tuple<int, string>>(null, null));
					}
				}

				// ValueTuple
				{
					// Not null
					{
						Assert.IsTrue(_matcher.Match<CompositePrimitiveKey, (int, Guid)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
						Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (0, Guid.Empty)));

						Assert.IsTrue(_matcher.Match<CompositeClassKey, (int, string)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (2, "Test")));
						Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (0, "Test2")));
						Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (0, (string)null)));
					}

					// Null
					{
						Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)>(null, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));

						Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)>(null, (2, "Test2")));
					}
				}
			}
		}

		[TestMethod]
		public void ShouldMatchNullableCompositeKeyWithEntity() {
			// Key == Entity
			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsFalse(_matcher.Match<(int, Guid)?, CompositePrimitiveKey>((0, Guid.Empty), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));

					Assert.IsTrue(_matcher.Match<(int, string)?, CompositeClassKey>((2, "Test"), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
					Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>((0, "Test2"), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
					Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>((0, (string)null), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsFalse(_matcher.Match<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null));
					Assert.IsFalse(_matcher.Match<(int, Guid)?, CompositePrimitiveKey>(null, null));

					Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>(null, new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
					Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>((2, "Test2"), null));
					Assert.IsFalse(_matcher.Match<(int, string)?, CompositeClassKey>(null, null));
				}
			}

			// Entity == Key
			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (0, Guid.Empty)));

					Assert.IsTrue(_matcher.Match<CompositeClassKey, (int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (2, "Test")));
					Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (0, "Test2")));
					Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, (0, (string)null)));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(null, (2, new Guid("56033406-E593-4076-B48A-70988C9F9190"))));
					Assert.IsFalse(_matcher.Match<CompositePrimitiveKey, (int, Guid)?>(null, null));

					Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, null));
					Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)?>(null, (2, "Test2")));
					Assert.IsFalse(_matcher.Match<CompositeClassKey, (int, string)?>(null, null));
				}
			}
		}

		[TestMethod]
		public void ShouldNotMatchCompositeKeyWithEntityIfOrderIsWrong() {
			// Key == Entity
			{
				// Tuple
				{
					TestUtils.AssertMapNotFound(() => _matcher.Match<Tuple<Guid, int>, CompositePrimitiveKey>(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					TestUtils.AssertMapNotFound(() => _matcher.Match<Tuple<string, int>, CompositeClassKey>(Tuple.Create("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				}

				// ValueTuple
				{
					TestUtils.AssertMapNotFound(() => _matcher.Match<(Guid, int), CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					TestUtils.AssertMapNotFound(() => _matcher.Match<(string, int), CompositeClassKey>(("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));

					TestUtils.AssertMapNotFound(() => _matcher.Match<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					TestUtils.AssertMapNotFound(() => _matcher.Match<(string, int)?, CompositeClassKey>(("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				}
			}

			// Entity == Key
			{
				// Tuple
				{
					TestUtils.AssertMapNotFound(() => _matcher.Match<CompositePrimitiveKey, Tuple<Guid, int>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
					TestUtils.AssertMapNotFound(() => _matcher.Match<CompositeClassKey, Tuple<string, int>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create("Test", 2)));
				}

				// ValueTuple
				{
					TestUtils.AssertMapNotFound(() => _matcher.Match<CompositePrimitiveKey, (Guid, int)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
					TestUtils.AssertMapNotFound(() => _matcher.Match<CompositeClassKey, (string, int)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, ("Test", 2)));

					TestUtils.AssertMapNotFound(() => _matcher.Match<CompositePrimitiveKey, (Guid, int)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
					TestUtils.AssertMapNotFound(() => _matcher.Match<CompositeClassKey, (string, int)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, ("Test", 2)));
				}
			}
		}

		[TestMethod]
		public void ShouldMatchEntityWithEntity() {
			Assert.IsTrue(_matcher.CanMatch<IntKey, IntKey>());
			Assert.IsTrue(_matcher.CanMatch<GuidKey, GuidKey>());
			Assert.IsTrue(_matcher.CanMatch<StringKey, StringKey>());

			{
				// Not null
				{
					Assert.IsTrue(_matcher.Match(new IntKey { Id = 2 }, new IntKey { Id = 2 }));
					Assert.IsFalse(_matcher.Match(new IntKey { Id = 3 }, new IntKey { Id = 2 }));

					Assert.IsTrue(_matcher.Match(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					Assert.IsFalse(_matcher.Match(new GuidKey(), new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));

					Assert.IsTrue(_matcher.Match(new StringKey { Id = "Test" }, new StringKey { Id = "Test" }));
					Assert.IsFalse(_matcher.Match(new StringKey { Id = "Test2" }, new StringKey { Id = "Test" }));
				}

				// Null
				{
					Assert.IsFalse(_matcher.Match<IntKey, IntKey>(new IntKey { Id = 2 }, null));
					Assert.IsFalse(_matcher.Match<IntKey, IntKey>(null, null));
					Assert.IsFalse(_matcher.Match<IntKey, IntKey>(new IntKey { Id = 3 }, null));

					Assert.IsFalse(_matcher.Match<GuidKey, GuidKey>(new GuidKey { Id = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, null));
					Assert.IsFalse(_matcher.Match<GuidKey, GuidKey>(new GuidKey(), null));

					Assert.IsFalse(_matcher.Match<StringKey, StringKey>(new StringKey { Id = "Test" }, null));
					Assert.IsFalse(_matcher.Match<StringKey, StringKey>(null, null));
				}
			}
		}
	}
}
