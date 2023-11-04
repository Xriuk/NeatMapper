using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class KeyToEntityTests {
		IMapper _mapper = null;
		TestContext _db = null;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddDbContext<TestContext>();
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.AddEntitiesMaps<TestContext>();
			var serviceProvider = serviceCollection.BuildServiceProvider();
			_mapper = serviceProvider.GetRequiredService<IMapper>();

			_db = serviceProvider.GetRequiredService<TestContext>();
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
			foreach (var entry in _db.ChangeTracker.Entries().ToArray()) {
				entry.State = EntityState.Detached;
			}
#endif
		}


		[TestMethod]
		public void ShouldMapKeyToEntity() {
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
			Assert.IsFalse(_mapper.CanMapNew<int, ShadowIntKey>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<ShadowIntKey>(2));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapNew<int, OwnedEntity>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(2));

			Assert.IsFalse(_mapper.CanMapNew<Tuple<string, int>, OwnedEntity>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(Tuple.Create("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(Tuple.Create(2, 2)));

			Assert.IsFalse(_mapper.CanMapNew<(string, int), OwnedEntity>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>(("Test", 2)));
			TestUtils.AssertMapNotFound(() => _mapper.Map<OwnedEntity>((2, 2)));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapNew<int, Keyless>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<Keyless>(2));
		}
#endif

		// DEV: test entities retrieval modes
	}
}
