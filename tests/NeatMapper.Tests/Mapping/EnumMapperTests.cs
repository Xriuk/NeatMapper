using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class EnumMapperTests {
		public enum Enum1 {
			A,
			B,
			C
		}

		public enum Enum2 {
			A1,
			B2,
			C3
		}

		public enum Enum3 {
			A,
			C
		}

		public enum Enum4 {
			A,
			B,
			C = B,
			D
		}

		public enum EnumShort : ushort {
			A,
			[EnumMember(Value = "This is B")]
			[Display(Name = "This won't be B")]
			B,
			[Display(Name = "This is C")]
			C
		}

		public enum EnumCaseSensitive {
			a,
			B,
			c
		}

		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new EnumMapper();
		}


		[TestMethod]
		public void ShouldConvertToAndFromUnderlyingType() {
			// Default (int)
			{ 
				Assert.IsTrue(_mapper.CanMapNew<Enum1, int>());
				Assert.IsTrue(_mapper.CanMapNew<int, Enum1>());

				Assert.IsFalse(_mapper.CanMapNew<Enum1, ushort>());
				Assert.IsFalse(_mapper.CanMapNew<ushort, Enum1>());

				Assert.AreEqual(1, _mapper.Map<int>(Enum1.B));
				Assert.ThrowsException<MappingException>(() => _mapper.Map<Enum1>(3));
				Assert.AreEqual(Enum1.C, _mapper.Map<Enum1>(2));
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<ushort>(Enum1.B));
				ushort n = 2;
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<Enum1>(n));

				using(var factory = _mapper.MapNewFactory<Enum1, int>()) {
					Assert.AreEqual(1, factory.Invoke(Enum1.B));
				}
				using (var factory = _mapper.MapNewFactory<int, Enum1>()) {
					Assert.ThrowsException<MappingException>(() => factory.Invoke(3));
					Assert.AreEqual(Enum1.C, factory.Invoke(2));
				}
			}

			// ushort
			{
				Assert.IsTrue(_mapper.CanMapNew<EnumShort, ushort>());
				Assert.IsTrue(_mapper.CanMapNew<ushort, EnumShort>());

				Assert.IsFalse(_mapper.CanMapNew<EnumShort, int>());
				Assert.IsFalse(_mapper.CanMapNew<int, EnumShort>());

				ushort n1 = 1;
				Assert.AreEqual(n1, _mapper.Map<ushort>(EnumShort.B));
				ushort n2 = 2;
				Assert.AreEqual(EnumShort.C, _mapper.Map<EnumShort>(n2));
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<int>(EnumShort.B));
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<EnumShort>(2)); // int

				using (var factory = _mapper.MapNewFactory<EnumShort, ushort>()) {
					Assert.AreEqual(n1, factory.Invoke(EnumShort.B));
				}
				using (var factory = _mapper.MapNewFactory<ushort, EnumShort>()) {
					Assert.AreEqual(EnumShort.C, factory.Invoke(2)); // compiler-cast
					Assert.AreEqual(EnumShort.C, factory.Invoke(n2));
				}
			}
		}
		
		// DEV: cannot test 8 bits are 256 values, too many
		/*[TestMethod]
		public void ShouldNotConvertToAndFromUnderlyingTypeIfItHasHashCollisions() {
			Assert.IsFalse(_mapper.CanMapNew<EnumCollisions, byte>());
			Assert.IsFalse(_mapper.CanMapNew<byte, EnumCollisions>());
		}*/

		[TestMethod]
		public void ShouldConvertToAndFromUnderlyingTypeHashedName() {
			var options = new MappingOptions(new EnumMapperMappingOptions(null, EnumToNumberMapping.HashedName, null));

			// Default (int)
			{
				Assert.IsTrue(_mapper.CanMapNew<Enum1, int>(options));
				Assert.IsTrue(_mapper.CanMapNew<int, Enum1>(options));

				Assert.IsFalse(_mapper.CanMapNew<Enum1, ushort>(options));
				Assert.IsFalse(_mapper.CanMapNew<ushort, Enum1>(options));

				Assert.AreEqual(-445612321, _mapper.Map<int>(Enum1.B, options));
				Assert.ThrowsException<MappingException>(() => _mapper.Map<Enum1>(12345678, options));
				Assert.AreEqual(Enum1.C, _mapper.Map<Enum1>(-708828309, options));
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<ushort>(Enum1.B, options));
				ushort n = 48960;
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<Enum1>(n, options));

				using (var factory = _mapper.MapNewFactory<Enum1, int>(options)) {
					Assert.AreEqual(-445612321, factory.Invoke(Enum1.B));
				}
				using (var factory = _mapper.MapNewFactory<int, Enum1>(options)) {
					Assert.ThrowsException<MappingException>(() => factory.Invoke(12345678));
					Assert.AreEqual(Enum1.C, factory.Invoke(-708828309));
				}
			}

			// ushort
			{
				Assert.IsTrue(_mapper.CanMapNew<EnumShort, ushort>(options));
				Assert.IsTrue(_mapper.CanMapNew<ushort, EnumShort>(options));

				Assert.IsFalse(_mapper.CanMapNew<EnumShort, int>(options));
				Assert.IsFalse(_mapper.CanMapNew<int, EnumShort>(options));

				ushort n1 = 48960;
				Assert.AreEqual(n1, _mapper.Map<ushort>(EnumShort.B, options));
				ushort n2 = 58885;
				Assert.AreEqual(EnumShort.C, _mapper.Map<EnumShort>(n2, options));
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<int>(EnumShort.B, options));
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<EnumShort>(58885, options)); // int

				using (var factory = _mapper.MapNewFactory<EnumShort, ushort>(options)) {
					Assert.AreEqual(n1, factory.Invoke(EnumShort.B));
				}
				using (var factory = _mapper.MapNewFactory<ushort, EnumShort>(options)) {
					Assert.AreEqual(EnumShort.C, factory.Invoke(58885)); // compiler-cast
					Assert.AreEqual(EnumShort.C, factory.Invoke(n2));
				}
			}
		}

		[TestMethod]
		public void ShouldConvertToAndFromString() {
			Assert.IsTrue(_mapper.CanMapNew<EnumShort, string>());

			Assert.AreEqual("A", _mapper.Map<string>(EnumShort.A));
			Assert.AreEqual("This is B", _mapper.Map<string>(EnumShort.B));
			Assert.AreEqual("This is C", _mapper.Map<string>(EnumShort.C));

			using (var factory = _mapper.MapNewFactory<EnumShort, string>()) {
				Assert.AreEqual("This is B", factory.Invoke(EnumShort.B));
			}


			Assert.IsTrue(_mapper.CanMapNew<string, EnumShort>());

			Assert.AreEqual(EnumShort.A, _mapper.Map<EnumShort>("A"));
			Assert.AreEqual(EnumShort.B, _mapper.Map<EnumShort>("This is B"));
			Assert.ThrowsException<MappingException>(() => _mapper.Map<EnumShort>("This won't be B"));
			Assert.AreEqual(EnumShort.C, _mapper.Map<EnumShort>("This is C"));

			using (var factory = _mapper.MapNewFactory<string, EnumShort>()) {
				Assert.AreEqual(EnumShort.B, factory.Invoke("This is B"));
				Assert.ThrowsException<MappingException>(() => factory.Invoke("This won't be B"));
			}
		}

		[TestMethod]
		public void ShouldConvertToOtherEnum() {
			// Value
			{ 
				Assert.IsTrue(_mapper.CanMapNew<Enum1, Enum2>());
				Assert.IsTrue(_mapper.CanMapNew<Enum2, Enum1>());
				Assert.IsTrue(_mapper.CanMapNew<Enum1, Enum4>());

				Assert.AreEqual(Enum2.B2, _mapper.Map<Enum2>(Enum1.B));
				Assert.AreEqual(Enum1.C, _mapper.Map<Enum1>(Enum2.C3));

				var result = _mapper.Map<Enum4>(Enum1.B);
				Assert.IsTrue(result == Enum4.B || result == Enum4.C); // Same value, unexpected result

				using (var factory = _mapper.MapNewFactory<Enum1, Enum2>()) {
					Assert.AreEqual(Enum2.B2, factory.Invoke(Enum1.B));
				}
			}

			// Name (case-insensitive)
			{
				var options = new MappingOptions(new EnumMapperMappingOptions(null, null, EnumToEnumMapping.NameCaseInsensitive));

				Assert.IsFalse(_mapper.CanMapNew<Enum1, Enum2>(options));
				Assert.IsFalse(_mapper.CanMapNew<Enum2, Enum1>(options));
				Assert.IsFalse(_mapper.CanMapNew<Enum1, Enum3>(options)); // Not all values can be mapped
				Assert.IsTrue(_mapper.CanMapNew<Enum3, Enum1>(options));
				Assert.IsTrue(_mapper.CanMapNew<Enum1, Enum4>(options));
				Assert.IsTrue(_mapper.CanMapNew<Enum1, EnumCaseSensitive>(options));

				Assert.AreEqual(Enum1.A, _mapper.Map<Enum1>(Enum3.A, options));
				Assert.AreEqual(Enum1.C, _mapper.Map<Enum1>(Enum3.C, options));

				Assert.AreEqual(Enum4.A, _mapper.Map<Enum4>(Enum1.A, options));
				Assert.AreEqual(Enum4.B, _mapper.Map<Enum4>(Enum1.B, options));
				Assert.AreEqual(Enum4.C, _mapper.Map<Enum4>(Enum1.C, options));

				Assert.AreEqual(EnumCaseSensitive.a, _mapper.Map<EnumCaseSensitive>(Enum1.A, options));
				Assert.AreEqual(EnumCaseSensitive.B, _mapper.Map<EnumCaseSensitive>(Enum1.B, options));
				Assert.AreEqual(EnumCaseSensitive.c, _mapper.Map<EnumCaseSensitive>(Enum1.C, options));

				using (var factory = _mapper.MapNewFactory<Enum1, EnumCaseSensitive> (options)) {
					Assert.AreEqual(EnumCaseSensitive.a, factory.Invoke(Enum1.A));
				}
			}

			// Name (case-insensitive)
			{
				var options = new MappingOptions(new EnumMapperMappingOptions(null, null, EnumToEnumMapping.NameCaseSensitive));

				Assert.IsFalse(_mapper.CanMapNew<Enum1, Enum2>(options));
				Assert.IsFalse(_mapper.CanMapNew<Enum2, Enum1>(options));
				Assert.IsFalse(_mapper.CanMapNew<Enum1, Enum3>(options)); // Not all values can be mapped
				Assert.IsTrue(_mapper.CanMapNew<Enum3, Enum1>(options));
				Assert.IsTrue(_mapper.CanMapNew<Enum1, Enum4>(options));
				Assert.IsFalse(_mapper.CanMapNew<Enum1, EnumCaseSensitive>(options)); // Not all values can be mapped

				Assert.AreEqual(Enum4.A, _mapper.Map<Enum4>(Enum1.A, options));
				Assert.AreEqual(Enum4.B, _mapper.Map<Enum4>(Enum1.B, options));
				Assert.AreEqual(Enum4.C, _mapper.Map<Enum4>(Enum1.C, options));

				using (var factory = _mapper.MapNewFactory<Enum1, Enum4>(options)) {
					Assert.AreEqual(Enum4.A, factory.Invoke(Enum1.A));
				}
			}
		}
	}
}
