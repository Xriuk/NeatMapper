using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class KeyToEntityMergeTests : KeyToEntityBase {
		protected static readonly IEnumerable mappingOptions = new[] { new EntityFrameworkCoreMappingOptions(null, null, true) };


		[TestMethod]
		public void ShouldMapKeyToEntity() {
			// Not null key
			{
				Assert.IsTrue(_mapper.CanMapMerge<int, IntKey>());
				Assert.IsTrue(_mapper.CanMapMerge<int, IntFieldKey>());

				// Null destination does not throw
				Assert.IsNotNull(_mapper.Map<int, IntKey>(2, (IntKey)null));
				Assert.IsNotNull(_mapper.Map<int, IntKey>(2, (IntKey)null, mappingOptions));

				Assert.IsNull(_mapper.Map<int, IntKey>(3, (IntKey)null));
				Assert.IsNull(_mapper.Map<int, IntKey>(3, (IntKey)null, mappingOptions));

				Assert.IsNotNull(_mapper.Map<int, IntFieldKey>(2, (IntFieldKey)null));
				Assert.IsNotNull(_mapper.Map<int, IntFieldKey>(2, (IntFieldKey)null, mappingOptions));
				Assert.IsNull(_mapper.Map<int, IntFieldKey>(3, (IntFieldKey)null));
				Assert.IsNull(_mapper.Map<int, IntFieldKey>(3, (IntFieldKey)null, mappingOptions));

				// Different destination passed
				var entity = new IntKey();
				var result = _mapper.Map<int, IntKey>(2, entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int, IntKey>(2, entity, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

				Assert.IsNull(_mapper.Map<int, IntKey>(3, new IntKey()));
				exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int, IntKey>(3, new IntKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

				var entity2 = new IntFieldKey();
				var result2 = _mapper.Map<int, IntFieldKey>(2, entity2);
				Assert.IsNotNull(result2);
				Assert.AreNotSame(entity2, result2);
				var exc2 = Assert.ThrowsException<MappingException>(() => _mapper.Map<int, IntFieldKey>(2, entity2, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			// Null key
			{
				Assert.IsTrue(_mapper.CanMapMerge<string, StringKey>());
				Assert.IsTrue(_mapper.CanMapMerge<string, StringFieldKey>());

				// Null destination does not throw
				Assert.IsNull(_mapper.Map<string, StringKey>(null, (StringKey)null));

				Assert.IsNull(_mapper.Map<string, StringFieldKey>(null, (StringFieldKey)null));

				// Different destination passed
				Assert.IsNull(_mapper.Map<string, StringKey>(null, new StringKey()));
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<string, StringKey>(null, new StringKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

				Assert.IsNull(_mapper.Map<string, StringFieldKey>(null, new StringFieldKey()));
				var exc2 = Assert.ThrowsException<MappingException>(() => _mapper.Map<string, StringFieldKey>(null, new StringFieldKey(), mappingOptions));
				Assert.IsInstanceOfType(exc2.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public void ShouldMapNullableKeyToEntity() {
			Assert.IsTrue(_mapper.CanMapMerge<int?, IntKey>());

			// Not null key
			{
				// Null destination does not throw
				Assert.IsNotNull(_mapper.Map<int?, IntKey>(2, (IntKey)null));
				Assert.IsNotNull(_mapper.Map<int?, IntKey>(2, (IntKey)null, mappingOptions));

				// Different destination passed
				var entity = new IntKey {
					Id = 2
				};
				var result = _mapper.Map<int?, IntKey>(2, entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int?, IntKey>(2, entity, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			// Null key
			{
				// Null destination does not throw
				Assert.IsNull(_mapper.Map<int?, IntKey>(null, (IntKey)null));

				Assert.IsNull(_mapper.Map<int?, IntKey>(null, new IntKey()));
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int?, IntKey>(null, new IntKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeyToEntity() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapMerge<Tuple<int, Guid>, CompositePrimitiveKey>());
				Assert.IsTrue(_mapper.CanMapMerge<Tuple<int, string>, CompositeClassKey>());

				// Not null key
				{
					// Null destination does not throw
					Assert.IsNotNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNotNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null, mappingOptions));

					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty), (CompositePrimitiveKey)null, mappingOptions));

					// Different destination passed
					var entity = new CompositePrimitiveKey();
					var result = _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity);
					Assert.IsNotNull(result);
					Assert.AreNotSame(entity, result);
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
					exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
				}

				// Null key
				{
					// Null destination does not throw
					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null, (CompositePrimitiveKey)null));

					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey()));
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey(), mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapMerge<(int, Guid), CompositePrimitiveKey>());
				Assert.IsTrue(_mapper.CanMapMerge<(int, string), CompositeClassKey>());

				// Not null key
				{
					// Null destination does not throw
					Assert.IsNotNull(_mapper.Map<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNotNull(_mapper.Map<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null, mappingOptions));

					Assert.IsNull(_mapper.Map<(int, Guid), CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNull(_mapper.Map< (int, Guid), CompositePrimitiveKey>((2, Guid.Empty), (CompositePrimitiveKey)null, mappingOptions));

					// Different destination passed
					var entity = new CompositePrimitiveKey();
					var result = _mapper.Map<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity);
					Assert.IsNotNull(result);
					Assert.AreNotSame(entity, result);
					var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

					Assert.IsNull(_mapper.Map<(int, Guid), CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
					exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<(int, Guid), CompositePrimitiveKey>((2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
				}
			}
		}

		[TestMethod]
		public void ShouldMapNullableCompositeKeyToEntity() {
			Assert.IsTrue(_mapper.CanMapMerge<(int, Guid)?, CompositePrimitiveKey>());
			Assert.IsTrue(_mapper.CanMapMerge<(int, string)?, CompositeClassKey>());

			// Not null key
			{
				// Null destination does not throw
				Assert.IsNotNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
				Assert.IsNotNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null, mappingOptions));

				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty), (CompositePrimitiveKey)null, mappingOptions));

				// Different destination passed
				var entity = new CompositePrimitiveKey();
				var result = _mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
				exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			// Null key
			{
				// Null destination does not throw
				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null, (CompositePrimitiveKey)null));

				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey()));
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public void ShouldNotMapCompositeKeyToEntityIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_mapper.CanMapMerge<Tuple<Guid, int>, CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapMerge<Tuple<string, int>, CompositeClassKey>());

				TestUtils.AssertMapNotFound(() => _mapper.Map(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), (CompositePrimitiveKey)null));
				TestUtils.AssertMapNotFound(() => _mapper.Map(Tuple.Create("Test", 2), (CompositeClassKey)null));
			}

			// ValueTuple
			{
				Assert.IsFalse(_mapper.CanMapMerge<(Guid, int), CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapMerge<(string, int), CompositeClassKey>());

				TestUtils.AssertMapNotFound(() => _mapper.Map((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), (CompositePrimitiveKey)null));
				TestUtils.AssertMapNotFound(() => _mapper.Map(("Test", 2), (CompositeClassKey)null));
				
				Assert.IsFalse(_mapper.CanMapMerge<(Guid, int)?, CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapMerge<(string, int)?, CompositeClassKey>());

				TestUtils.AssertMapNotFound(() => _mapper.Map<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), (CompositePrimitiveKey)null));
				TestUtils.AssertMapNotFound(() => _mapper.Map<(string, int)?, CompositeClassKey>(("Test", 2), (CompositeClassKey)null));
			}
		}

		[TestMethod]
		public void ShouldMapShadowKeyToEntities() {
			Assert.IsTrue(_mapper.CanMapMerge<int, ShadowIntKey>());

			Assert.IsNotNull(_mapper.Map(1, (ShadowIntKey)null));


			Assert.IsTrue(_mapper.CanMapMerge<Tuple<int, string>, ShadowCompositeKey>());

			Assert.IsNotNull(_mapper.Map(Tuple.Create(2, "Test"), (ShadowCompositeKey)null));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapMerge<int, OwnedEntity1>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(2, (OwnedEntity1)null));

			Assert.IsFalse(_mapper.CanMapMerge<Tuple<string, int>, OwnedEntity1>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(Tuple.Create("Test", 2), (OwnedEntity1)null));
			TestUtils.AssertMapNotFound(() => _mapper.Map(Tuple.Create(2, 2), (OwnedEntity1)null));

			Assert.IsFalse(_mapper.CanMapMerge<(string, int), OwnedEntity1>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(("Test", 2), (OwnedEntity1)null));
			TestUtils.AssertMapNotFound(() => _mapper.Map((2, 2), (OwnedEntity1)null));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapMerge<int, Keyless>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(2, (Keyless)null));
		}
#endif


		[TestMethod]
		public void ShouldMapKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapMerge<int[], List<IntKey>>());
			Assert.IsTrue(_mapper.CanMapMerge<int[], ICollection<IntKey>>());

			// Normal
			{
				var list = new List<IntKey>();
				var result = _mapper.Map(new[] { 2, 0 }, list);
				Assert.AreSame(list, result);
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			// Duplicate replaced
			{
				var entity = new IntKey {
					Id = 2
				};
				var result = _mapper.Map(new[] { 2, 0 }, new List<IntKey> { entity });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result.First());
				Assert.AreNotSame(entity, result.First());
				Assert.IsNull(result.Last());
			}

			// Duplicate same
			{
				var entity =  _db.Find<IntKey>(2);
				var result = _mapper.Map(new[] { 2, 0 }, new List<IntKey> { entity });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result.First());
				Assert.AreSame(entity, result.First());
				Assert.IsNull(result.Last());
			}

			// Duplicate throws
			{
				var entity = new IntKey {
					Id = 2
				};
				var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map(new[] { 2, 0 }, new List<IntKey> { entity }, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public void ShouldMapNullableKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapMerge<IEnumerable<int?>, List<IntKey>>());

			{
				var result = _mapper.Map(new int?[] { 2, null }, new List<IntKey>());
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = _mapper.Map(new List<Guid?> { null, new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new List<GuidKey>());
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
		}

		[TestMethod]
		public void ShouldMapCompositeKeysCollectionToEntitiesCollection() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapMerge<IEnumerable<Tuple<int, Guid>>, List<CompositePrimitiveKey>>());

				var result = _mapper.Map(new Tuple<int, Guid>[] { null, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")) }, new List<CompositePrimitiveKey>());
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapMerge<(int, Guid)[], List<CompositePrimitiveKey>>());

				var result = _mapper.Map(new (int, Guid)[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), default((int, Guid)) }, new List<CompositePrimitiveKey>());
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}
		}

		[TestMethod]
		public void ShouldMapNullableCompositeKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapMerge<(int, Guid)?[], List<CompositePrimitiveKey>>());

			var result = _mapper.Map(new (int, Guid)?[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null }, new List<CompositePrimitiveKey>());
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);
		}

		[TestMethod]
		public void ShouldAttachDestinationIfNotNullInLocalOrAttachMode() {
			var destination = new IntKey {
				Id = 3
			};
			var result = _mapper.Map<int, IntKey>(3, destination, new[] { new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOrAttach	) });
			Assert.AreSame(destination, result);
		}

		[TestMethod]
		public void ShouldAttachDestinationIfNotNullInLocalOrAttachModeInCollection() {
			var destination = new IntKey {
				Id = 3
			};
			var result = _mapper.Map(new[] { 2, 3, 4 }, new List<IntKey> { destination }, new[] { new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOrAttach) });
			Assert.AreEqual(3, result.Count);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result[0]);
			Assert.IsNotNull(result[1]);
			Assert.IsNotNull(result[2]);
		}
	}
}
