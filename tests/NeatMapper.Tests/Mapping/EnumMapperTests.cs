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
			C = B
		}

		public enum EnumShort : ushort {
			A,
			[EnumMember(Value = "This is B")]
			[Display(Name = "This won't be B")]
			B,
			[Display(Name = "This is C")]
			C
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
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<EnumShort>(2));
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
				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<EnumShort>(2));
			}
		}

		[TestMethod]
		public void ShouldConvertToAndFromString() {
			Assert.IsTrue(_mapper.CanMapNew<EnumShort, string>());

			Assert.AreEqual("A", _mapper.Map<string>(EnumShort.A));
			Assert.AreEqual("This is B", _mapper.Map<string>(EnumShort.B));
			Assert.AreEqual("This is C", _mapper.Map<string>(EnumShort.C));

			Assert.IsTrue(_mapper.CanMapNew<string, EnumShort>());

			Assert.AreEqual(EnumShort.A, _mapper.Map<EnumShort>("A"));
			Assert.AreEqual(EnumShort.B, _mapper.Map<EnumShort>("This is B"));
			Assert.ThrowsException<MappingException>(() => _mapper.Map<EnumShort>("This won't be B"));
			Assert.AreEqual(EnumShort.C, _mapper.Map<EnumShort>("This is C"));
		}

		[TestMethod]
		public void ShouldConvertToOtherEnum() {
			// Value
			{ 
				Assert.IsTrue(_mapper.CanMapNew<Enum1, Enum2>());
			}


		}
	}
}
