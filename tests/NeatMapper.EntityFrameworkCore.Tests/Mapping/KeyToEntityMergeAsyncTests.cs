using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.EntityFrameworkCore.Tests.Mapping {
	[TestClass]
	public class KeyToEntityMergeAsyncTests : KeyToEntityAsyncBase {
		protected static readonly IEnumerable mappingOptions = new[] { new EntityFrameworkCoreMappingOptions(null, null, true) };


		[TestMethod]
		public async Task ShouldMapKeyToEntity() {
			// Not null key
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<int, IntKey>());

				// Null destination does not throw
				Assert.IsNotNull(await _mapper.MapAsync<int, IntKey>(2, (IntKey)null));
				Assert.IsNotNull(await _mapper.MapAsync<int, IntKey>(2, (IntKey)null, mappingOptions));

				Assert.IsNull(await _mapper.MapAsync<int, IntKey>(3, (IntKey)null));
				Assert.IsNull(await _mapper.MapAsync<int, IntKey>(3, (IntKey)null, mappingOptions));

				// Different destination passed
				var entity = new IntKey();
				var result = await _mapper.MapAsync<int, IntKey>(2, entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int, IntKey>(2, entity, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

				Assert.IsNull(await _mapper.MapAsync<int, IntKey>(3, new IntKey()));
				exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int, IntKey>(3, new IntKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			// Null key
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<string, StringKey>());

				// Null destination does not throw
				Assert.IsNull(await _mapper.MapAsync<string, StringKey>(null, (StringKey)null));

				Assert.IsNull(await _mapper.MapAsync<string, StringKey>(null, new StringKey()));
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<string, StringKey>(null, new StringKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableKeyToEntity() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<int?, IntKey>());

			// Not null key
			{
				// Null destination does not throw
				Assert.IsNotNull(await _mapper.MapAsync<int?, IntKey>(2, (IntKey)null));
				Assert.IsNotNull(await _mapper.MapAsync<int?, IntKey>(2, (IntKey)null, mappingOptions));

				// Different destination passed
				var entity = new IntKey {
					Id = 2
				};
				var result = await _mapper.MapAsync<int?, IntKey>(2, entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int?, IntKey>(2, entity, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			// Null key
			{
				// Null destination does not throw
				Assert.IsNull(await _mapper.MapAsync<int?, IntKey>(null, (IntKey)null));

				Assert.IsNull(await _mapper.MapAsync<int?, IntKey>(null, new IntKey()));
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int?, IntKey>(null, new IntKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public async Task ShouldMapCompositeKeyToEntity() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<Tuple<int, Guid>, CompositePrimitiveKey>());
				Assert.IsTrue(_mapper.CanMapAsyncMerge<Tuple<int, string>, CompositeClassKey>());

				// Not null key
				{
					// Null destination does not throw
					Assert.IsNotNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNotNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null, mappingOptions));

					Assert.IsNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty), (CompositePrimitiveKey)null, mappingOptions));

					// Different destination passed
					var entity = new CompositePrimitiveKey();
					var result = await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity);
					Assert.IsNotNull(result);
					Assert.AreNotSame(entity, result);
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

					Assert.IsNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
					exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(Tuple.Create(2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
				}

				// Null key
				{
					// Null destination does not throw
					Assert.IsNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(null, (CompositePrimitiveKey)null));

					Assert.IsNull(await _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey()));
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<Tuple<int, Guid>, CompositePrimitiveKey>(null, new CompositePrimitiveKey(), mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
				}
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<(int, Guid), CompositePrimitiveKey>());
				Assert.IsTrue(_mapper.CanMapAsyncMerge<(int, string), CompositeClassKey>());

				// Not null key
				{
					// Null destination does not throw
					Assert.IsNotNull(await _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNotNull(await _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null, mappingOptions));

					Assert.IsNull(await _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
					Assert.IsNull(await _mapper.MapAsync< (int, Guid), CompositePrimitiveKey>((2, Guid.Empty), (CompositePrimitiveKey)null, mappingOptions));

					// Different destination passed
					var entity = new CompositePrimitiveKey();
					var result = await _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity);
					Assert.IsNotNull(result);
					Assert.AreNotSame(entity, result);
					var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

					Assert.IsNull(await _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
					exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<(int, Guid), CompositePrimitiveKey>((2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
					Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
				}
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableCompositeKeyToEntity() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<(int, Guid)?, CompositePrimitiveKey>());
			Assert.IsTrue(_mapper.CanMapAsyncMerge<(int, string)?, CompositeClassKey>());

			// Not null key
			{
				// Null destination does not throw
				Assert.IsNotNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
				Assert.IsNotNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null, mappingOptions));

				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), (CompositePrimitiveKey)null));
				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty), (CompositePrimitiveKey)null, mappingOptions));

				// Different destination passed
				var entity = new CompositePrimitiveKey();
				var result = await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity);
				Assert.IsNotNull(result);
				Assert.AreNotSame(entity, result);
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), entity, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));

				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((3, new Guid("56033406-E593-4076-B48A-70988C9F9190")), new CompositePrimitiveKey()));
				exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>((2, Guid.Empty), new CompositePrimitiveKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			// Null key
			{
				// Null destination does not throw
				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>(null, (CompositePrimitiveKey)null));

				Assert.IsNull(await _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey()));
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<(int, Guid)?, CompositePrimitiveKey>(null, new CompositePrimitiveKey(), mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}
		}

		[TestMethod]
		public async Task ShouldNotMapCompositeKeyToEntityIfOrderIsWrong() {
			// Tuple
			{
				Assert.IsFalse(_mapper.CanMapAsyncMerge<Tuple<Guid, int>, CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapAsyncMerge<Tuple<string, int>, CompositeClassKey>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(Tuple.Create(new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), (CompositePrimitiveKey)null));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(Tuple.Create("Test", 2), (CompositeClassKey)null));
			}

			// ValueTuple
			{
				Assert.IsFalse(_mapper.CanMapAsyncMerge<(Guid, int), CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapAsyncMerge<(string, int), CompositeClassKey>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), (CompositePrimitiveKey)null));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(("Test", 2), (CompositeClassKey)null));
				
				Assert.IsFalse(_mapper.CanMapAsyncMerge<(Guid, int)?, CompositePrimitiveKey>());
				Assert.IsFalse(_mapper.CanMapAsyncMerge<(string, int)?, CompositeClassKey>());

				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(Guid, int)?, CompositePrimitiveKey>((new Guid("56033406-E593-4076-B48A-70988C9F9190"), 2), (CompositePrimitiveKey)null));
				await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<(string, int)?, CompositeClassKey>(("Test", 2), (CompositeClassKey)null));
			}
		}

		[TestMethod]
		public async Task ShouldMapShadowKeyToEntities() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<int, ShadowIntKey>());

			Assert.IsNotNull(await _mapper.MapAsync(1, (ShadowIntKey)null));


			Assert.IsTrue(_mapper.CanMapAsyncMerge<Tuple<int, string>, ShadowCompositeKey>());

			Assert.IsNotNull(await _mapper.MapAsync(Tuple.Create(2, "Test"), (ShadowCompositeKey)null));
		}

		[TestMethod]
		public async Task ShouldNotMapOwnedEntities() {
			Assert.IsFalse(_mapper.CanMapAsyncMerge<int, OwnedEntity1>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(2, (OwnedEntity1)null));

			Assert.IsFalse(_mapper.CanMapAsyncMerge<Tuple<string, int>, OwnedEntity1>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(Tuple.Create("Test", 2), (OwnedEntity1)null));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(Tuple.Create(2, 2), (OwnedEntity1)null));

			Assert.IsFalse(_mapper.CanMapAsyncMerge<(string, int), OwnedEntity1>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(("Test", 2), (OwnedEntity1)null));
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync((2, 2), (OwnedEntity1)null));
		}

#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
		[TestMethod]
		public async Task ShouldNotMapKeylessEntities() {
			Assert.IsFalse(_mapper.CanMapAsyncMerge<int, Keyless>());

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(2, (Keyless)null));
		}
#endif


		[TestMethod]
		public async Task ShouldMapKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<int[], List<IntKey>>());
			Assert.IsTrue(_mapper.CanMapAsyncMerge<int[], ICollection<IntKey>>());

			// Normal
			{
				var list = new List<IntKey>();
				var result = await _mapper.MapAsync(new[] { 2, 0 }, list);
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
				var result = await _mapper.MapAsync(new[] { 2, 0 }, new List<IntKey> { entity });
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result.First());
				Assert.AreNotSame(entity, result.First());
				Assert.IsNull(result.Last());
			}

			// Duplicate same
			{
				var entity =  _db.Find<IntKey>(2);
				var result = await _mapper.MapAsync(new[] { 2, 0 }, new List<IntKey> { entity });
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
				var exc = await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync(new[] { 2, 0 }, new List<IntKey> { entity }, mappingOptions));
				Assert.IsInstanceOfType(exc.InnerException, typeof(DuplicateEntityException));
			}

			{
				var list = new List<StringKey>();
				var result = await _mapper.MapAsync(new DefaultAsyncEnumerable<string>(new[] { null, "Test" }), list);
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
			using (var factory = _mapper.MapAsyncMergeFactory<IAsyncEnumerable<string>, List<StringKey>>()) {
				var list = new List<StringKey>();
				var result = await factory.Invoke(new DefaultAsyncEnumerable<string>(new[] { null, "Test" }), list);
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableKeysCollectioToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<IEnumerable<int?>, List<IntKey>>());

			{
				var result = await _mapper.MapAsync(new int?[] { 2, null }, new List<IntKey>());
				Assert.AreEqual(2, result.Count());
				Assert.IsNotNull(result.First());
				Assert.IsNull(result.Last());
			}

			{
				var result = await _mapper.MapAsync(new List<Guid?> { null, new Guid("56033406-E593-4076-B48A-70988C9F9190") }, new List<GuidKey>());
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapCompositeKeysCollectionToEntitiesCollection() {
			// Tuple
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<IEnumerable<Tuple<int, Guid>>, List<CompositePrimitiveKey>>());

				var result = await _mapper.MapAsync(new Tuple<int, Guid>[] { null, Tuple.Create(2, new Guid("56033406-E593-4076-B48A-70988C9F9190")) }, new List<CompositePrimitiveKey>());
				Assert.AreEqual(2, result.Count);
				Assert.IsNull(result[0]);
				Assert.IsNotNull(result[1]);
			}

			// ValueTuple
			{
				Assert.IsTrue(_mapper.CanMapAsyncMerge<(int, Guid)[], List<CompositePrimitiveKey>>());

				var result = await _mapper.MapAsync(new (int, Guid)[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), default((int, Guid)) }, new List<CompositePrimitiveKey>());
				Assert.AreEqual(2, result.Count);
				Assert.IsNotNull(result[0]);
				Assert.IsNull(result[1]);
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableCompositeKeysCollectionToEntitiesCollection() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<(int, Guid)?[], List<CompositePrimitiveKey>>());

			var result = await _mapper.MapAsync(new (int, Guid)?[] { (2, new Guid("56033406-E593-4076-B48A-70988C9F9190")), null }, new List<CompositePrimitiveKey>());
			Assert.AreEqual(2, result.Count);
			Assert.IsNotNull(result[0]);
			Assert.IsNull(result[1]);
		}

		[TestMethod]
		public async Task ShouldAttachDestinationIfNotNullInLocalOrAttachMode() {
			var destination = new IntKey {
				Id = 3
			};
			var result = await _mapper.MapAsync<int, IntKey>(3, destination, new[] { new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOrAttach) });
			Assert.AreSame(destination, result);
		}

		[TestMethod]
		public async Task ShouldAttachDestinationIfNotNullInLocalOrAttachModeInCollection() {
			var destination = new IntKey {
				Id = 3
			};
			var result = await _mapper.MapAsync(new[] { 2, 3, 4 }, new List<IntKey> { destination }, new[] { new EntityFrameworkCoreMappingOptions(EntitiesRetrievalMode.LocalOrAttach) });
			Assert.AreEqual(3, result.Count);
			Assert.IsNotNull(result);
			Assert.AreSame(destination, result[0]);
			Assert.IsNotNull(result[1]);
			Assert.IsNotNull(result[2]);
		}
	}
}
