using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests {
	[TestClass]
	public class EntityFrameworkCoreServiceCollectionExtensionsTests {
		public class IntKey {
			public int Id { get; set; }

			public OwnedEntity Entity { get; set; }
		}

		public class GuidKey {
			public Guid Id { get; set; }
		}

		public class StringKey {
			public string Id { get; set; }

			public ICollection<OwnedEntity> Entities { get; set; }
		}

		public class CompositePrimitiveKey {
			public int Id1 { get; set; }

			public Guid Id2 { get; set; }
		}

		public class CompositeClassKey {
			public int Id1 { get; set; }

			public string Id2 { get; set; }
		}

		public class ShadowIntKey {

		}

		public class ShadowStringKey {
			public StringKey String { get; set; }
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		public class Keyless {
			public int Id { get; set; }
		}
#endif

		public class OwnedEntity {
			public int Id { get; set; }
		}

		public class TestContext : DbContext {
			protected override void OnModelCreating(ModelBuilder modelBuilder) {
				base.OnModelCreating(modelBuilder);

				modelBuilder.Entity<IntKey>()
					.OwnsOne(o => o.Entity);

				modelBuilder.Entity<GuidKey>();

				var ownedBuilder = modelBuilder.Entity<StringKey>()
					.OwnsMany(o => o.Entities);
				ownedBuilder.Property(typeof(string), "StringId");
				ownedBuilder.WithOwner()
					.HasForeignKey("StringId");
				ownedBuilder.HasKey("StringId", "Id");

				modelBuilder.Entity<CompositePrimitiveKey>()
					.HasKey(o => new { o.Id1, o.Id2 });

				modelBuilder.Entity<CompositeClassKey>()
					.HasKey(o => new { o.Id1, o.Id2 });

				modelBuilder.Entity<ShadowIntKey>()
					.Property(typeof(int), "Id");
				modelBuilder.Entity<ShadowIntKey>()
					.HasKey("Id");

				modelBuilder.Entity<ShadowStringKey>()
					.Property(typeof(string), "StringId");
				modelBuilder.Entity<ShadowStringKey>()
					.HasOne(s => s.String).WithMany()
					.HasForeignKey("StringId");
				modelBuilder.Entity<ShadowStringKey>()
					.HasKey("StringId");

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
				modelBuilder.Entity<Keyless>()
					.HasNoKey();
#endif
			}

			protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
				base.OnConfiguring(optionsBuilder);

				optionsBuilder.UseInMemoryDatabase("Test");
			}
		}

		IServiceProvider _serviceProvider = null;
		IMapper _mapper = null;
		IMatcher _matcher = null;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddDbContext<TestContext>();
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddEntitiesMaps<TestContext>();
			_serviceProvider = serviceCollection.BuildServiceProvider();
			_mapper = _serviceProvider.GetRequiredService<IMapper>();
			_matcher = _serviceProvider.GetRequiredService<IMatcher>();
		}


		[TestMethod]
		public void ShouldMapEntityToKey() {
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
				// Not null
				{
					var result1 = _mapper.Map<Tuple<int, Guid>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
					Assert.IsNotNull(result1);
					Assert.AreEqual(2, result1.Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result1.Item2);
					var result2 = _mapper.Map<Tuple<int, string>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
					Assert.IsNotNull(result2);
					Assert.AreEqual(2, result2.Item1);
					Assert.AreEqual("Test", result2.Item2);
				}

				// Null
				{
					Assert.IsNull(_mapper.Map<CompositePrimitiveKey, Tuple<int, Guid>>(null));
					Assert.IsNull(_mapper.Map<CompositeClassKey, Tuple<int, string>>(null));
				}
			}

			// ValueTuple
			{
				// Not null
				{
					var result1 = _mapper.Map<(int, Guid)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
					Assert.AreEqual(2, result1.Item1);
					Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result1.Item2);
					var result2 = _mapper.Map<(int, string)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
					Assert.AreEqual(2, result2.Item1);
					Assert.AreEqual("Test", result2.Item2);
				}

				// Null
				{
					var result1 = _mapper.Map<CompositePrimitiveKey, (int, Guid)>(null);
					Assert.AreEqual(0, result1.Item1);
					Assert.AreEqual(Guid.Empty, result1.Item2);
					var result2 = _mapper.Map<CompositeClassKey, (int, string)>(null);
					Assert.AreEqual(0, result2.Item1);
					Assert.IsNull(result2.Item2);
				}
			}
		}

		[TestMethod]
		public void ShouldMapEntityToNullableCompositeKey() {
			// Not null
			{
				var result1 = _mapper.Map<(int, Guid)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") });
				Assert.IsNotNull(result1);
				Assert.AreEqual(2, result1.Value.Item1);
				Assert.AreEqual(new Guid("56033406-E593-4076-B48A-70988C9F9190"), result1.Value.Item2);
				var result2 = _mapper.Map<(int, string)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" });
				Assert.IsNotNull(result2);
				Assert.AreEqual(2, result2.Value.Item1);
				Assert.AreEqual("Test", result2.Value.Item2);
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
				TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<Guid, int>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<string, int>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}

			// ValueTuple
			{
				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
			}
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

						Assert.IsTrue(_matcher.Match<CompositeClassKey, Tuple<int, string> > (new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create(2, "Test")));
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
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<Tuple<Guid, int>, CompositePrimitiveKey>(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<Tuple<string, int>, CompositeClassKey>(Tuple.Create("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				}

				// ValueTuple
				{
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<(Guid, int), CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<(string, int), CompositeClassKey>(("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));

					TestUtils.AssertMatcherNotFound(() => _matcher.Match<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }));
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<(string, int)?, CompositeClassKey>(("Test", 2), new CompositeClassKey { Id1 = 2, Id2 = "Test" }));
				}
			}

			// Entity == Key
			{
				// Tuple
				{
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<CompositePrimitiveKey, Tuple<Guid, int>>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<CompositeClassKey, Tuple<string, int>>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, Tuple.Create("Test", 2)));
				}

				// ValueTuple
				{
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<CompositePrimitiveKey, (Guid, int)>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<CompositeClassKey, (string, int)>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, ("Test", 2)));

					TestUtils.AssertMatcherNotFound(() => _matcher.Match<CompositePrimitiveKey, (Guid, int)?>(new CompositePrimitiveKey { Id1 = 2, Id2 = new Guid("56033406-E593-4076-B48A-70988C9F9190") }, (new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
					TestUtils.AssertMatcherNotFound(() => _matcher.Match<CompositeClassKey, (string, int)?>(new CompositeClassKey { Id1 = 2, Id2 = "Test" }, ("Test", 2)));
				}
			}
		}


		TestContext _db = null;

		protected void InitializeDb() {
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
			foreach(var entry in _db.ChangeTracker.Entries().ToArray()) {
				entry.State = EntityState.Detached;
			}
#endif
		}

		[TestMethod]
		public void ShouldMapKeyToEntity() {
			InitializeDb();

			// Not null
			{
				Assert.IsNotNull(_mapper.Map<IntKey>(2));
				Assert.IsNull(_mapper.Map<IntKey>(3));
				Assert.IsNotNull(_mapper.Map<GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				Assert.IsNull(_mapper.Map<GuidKey>(Guid.Empty));
				Assert.IsNotNull(_mapper.Map<StringKey>("Test"));
				Assert.IsNull(_mapper.Map<StringKey>("Test2"));
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<string, StringKey>(null));
			}
		}

		[TestMethod]
		public void ShouldMapNullableKeyToEntity() {
			InitializeDb();

			// Not null
			{
				Assert.IsNotNull(_mapper.Map<int?, IntKey>(2));
				Assert.IsNull(_mapper.Map<int?, IntKey>(3));
				Assert.IsNotNull(_mapper.Map<Guid?, GuidKey>(new Guid("56033406-E593-4076-B48A-70988C9F9190")));
				Assert.IsNull(_mapper.Map<Guid?, GuidKey>(Guid.Empty));
			}

			// Null
			{
				Assert.IsNull(_mapper.Map<int?, IntKey>(null));
				Assert.IsNull(_mapper.Map<Guid?, GuidKey>(null));
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeyToEntity() {
			InitializeDb();

			// Tuple
			{
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
			InitializeDb();

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
				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositePrimitiveKey>(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositeClassKey>(Tuple.Create("Test", 2)));
			}

			// ValueTuple
			{
				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<CompositeClassKey>(("Test", 2)));

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2)));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)?, CompositeClassKey>(("Test", 2)));
			}
		}


		[TestMethod]
		public void ShouldNotMapEntitiesWithShadowKeys() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(new ShadowIntKey()));

			TestUtils.AssertMapNotFound(() => _mapper.Map<ShadowIntKey>(2));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(new OwnedEntity()));

			TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<string, int>>(new OwnedEntity()));
			TestUtils.AssertMapNotFound(() => _mapper.Map<Tuple<int, int>>(new OwnedEntity()));

			TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)>(new OwnedEntity()));
			TestUtils.AssertMapNotFound(() => _mapper.Map<(int, int)>(new OwnedEntity()));


			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(2));

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(Tuple.Create("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(Tuple.Create(2, 2)));

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>((2, 2)));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(new Keyless()));

			TestUtils.AssertMapNotFound(() => _mapper.Map<Keyless>(2));
		}
#endif

		// DEV: test options
	}
}
