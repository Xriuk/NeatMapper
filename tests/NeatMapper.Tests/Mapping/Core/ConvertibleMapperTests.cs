using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class ConvertibleMapperTests {
		private enum MyEnum {
			A,
			B,
			C
		}

		private class MyClass : IConvertible {
			public TypeCode GetTypeCode() {
				return TypeCode.Object;
			}

			public bool ToBoolean(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public byte ToByte(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public char ToChar(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public DateTime ToDateTime(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public decimal ToDecimal(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public double ToDouble(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public short ToInt16(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public int ToInt32(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public long ToInt64(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public sbyte ToSByte(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public float ToSingle(IFormatProvider provider) {
				return 42f;
			}

			public string ToString(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public object ToType(Type conversionType, IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public ushort ToUInt16(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public uint ToUInt32(IFormatProvider provider) {
				throw new InvalidCastException();
			}

			public ulong ToUInt64(IFormatProvider provider) {
				throw new InvalidCastException();
			}
		}


		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = ConvertibleMapper.Instance;
		}


		[TestMethod]
		public void ShouldMap() {
			Assert.IsTrue(_mapper.CanMapNew<MyClass, float>());
			Assert.IsFalse(_mapper.CanMapNew<MyClass, bool>());
			Assert.AreEqual(42f, _mapper.Map<float>(new MyClass()));
			TestUtils.AssertMapNotFound(() => _mapper.Map<bool>(new MyClass()));

			Assert.IsTrue(_mapper.CanMapNew<MyEnum, decimal>());
			Assert.AreEqual(1m, _mapper.Map<decimal>(MyEnum.B));
		}
	}
}
