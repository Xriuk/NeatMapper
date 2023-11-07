using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;
using System.Collections;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class KeyToEntityMergeTests : KeyToEntityBase {
		protected static readonly IEnumerable mappingOptions = new[] { new EntityFrameworkCoreMappingOptions(null, null, true) };


		[TestMethod]
		public void ShouldMapKeyToEntity() {
			// Not null key
			{
				Assert.IsTrue(_mapper.CanMapMerge<int, IntKey>());

				// Null destination does not throw
				Assert.IsNotNull(_mapper.Map<int, IntKey>(2, (IntKey)null));
				Assert.IsNotNull(_mapper.Map<int, IntKey>(2, (IntKey)null, mappingOptions));

				Assert.IsNull(_mapper.Map<int, IntKey>(3, (IntKey)null));
				Assert.IsNull(_mapper.Map<int, IntKey>(3, (IntKey)null, mappingOptions));

				// Different destination passed
				var entity = new IntKey();
				var result = _mapper.Map<int, IntKey>(2, entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<int, IntKey>(2, entity, mappingOptions));

				Assert.IsNull(_mapper.Map<int, IntKey>(3, new IntKey()));
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<int, IntKey>(3, new IntKey(), mappingOptions));
			}

			// Null key
			{
				Assert.IsTrue(_mapper.CanMapMerge<string, StringKey>());

				// Null destination does not throw
				Assert.IsNull(_mapper.Map<string, StringKey>(null, (StringKey)null));

				Assert.IsNull(_mapper.Map<string, StringKey>(null, new StringKey()));
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<string, StringKey>(null, new StringKey(), mappingOptions));
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
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<int?, IntKey>(2, entity, mappingOptions));
			}

			// Null key
			{
				// Null destination does not throw
				Assert.IsNull(_mapper.Map<int?, IntKey>(null, (IntKey)null));

				Assert.IsNull(_mapper.Map<int?, IntKey>(null, new IntKey()));
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<int?, IntKey>(null, new IntKey(), mappingOptions));
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
					Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));

					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
					Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
				}

				// Null key
				{
					// Null destination does not throw
					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null, (CompositePrimitiveKey)null));

					Assert.IsNull(_mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey()));
					Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey(), mappingOptions));
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
					Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));

					Assert.IsNull(_mapper.Map<(int, Guid), CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
					Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<(int, Guid), CompositePrimitiveKey>((2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
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
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));

				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
			}

			// Null key
			{
				// Null destination does not throw
				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null, (CompositePrimitiveKey)null));

				Assert.IsNull(_mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey()));
				Assert.ThrowsException<DuplicateEntityException>(() => _mapper.Map<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey(), mappingOptions));
			}
		}

		[TestMethod]
		public void ShouldNotMapCompositeKeyToEntitysIfOrderIsWrong() {
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
		public void ShouldNotMapEntitiesWithShadowKeys() {
			Assert.IsFalse(_mapper.CanMapMerge<int, ShadowIntKey>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(2, (ShadowIntKey)null));
		}

		[TestMethod]
		public void ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapMerge<int, OwnedEntity>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(2, (OwnedEntity)null));

			Assert.IsFalse(_mapper.CanMapMerge<Tuple<string, int>, OwnedEntity>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(Tuple.Create("Test", 2), (OwnedEntity)null));
			TestUtils.AssertMapNotFound(() => _mapper.Map(Tuple.Create(2, 2), (OwnedEntity)null));

			Assert.IsFalse(_mapper.CanMapMerge<(string, int), OwnedEntity>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(("Test", 2), (OwnedEntity)null));
			TestUtils.AssertMapNotFound(() => _mapper.Map((2, 2), (OwnedEntity)null));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public void ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapMerge<int, Keyless>());

			TestUtils.AssertMapNotFound(() => _mapper.Map(2, (Keyless)null));
		}
#endif
	}
}
