using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class TypeConverterMapperTests {
		[Flags]
		private enum TestEnum {
			A = 1 << 0,
			B = 1 << 1,
			C = 1 << 2
		}


		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = TypeConverterMapper.Instance;
		}


		[TestMethod]
		public void ShouldMap() {
			// NullableConverter
			Assert.IsTrue(_mapper.CanMapNew<int?, int>());
			Assert.AreEqual(2, _mapper.Map<int?, int>((int?)2));

			// EnumConverter
			Assert.IsTrue(_mapper.CanMapNew<TestEnum, Enum[]>());
			var result = _mapper.Map<Enum[]>(TestEnum.A | TestEnum.C);
			Assert.AreEqual(2, result.Length);
			Assert.AreEqual((int)TestEnum.A, (int)(object)result[0]);
			Assert.AreEqual((int)TestEnum.C, (int)(object)result[1]);
		}
	}
}
