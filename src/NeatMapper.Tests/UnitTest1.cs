using NeatMapper.Core;

namespace NeatMapper.Tests {
	public sealed class TestClass1 : IMap<int, string> {
		public static string Map(int source, MappingContext context) {
			return "1";
		}
	}

	public sealed class TestClass2 : IMap<int, string> {
		static string IMap<int, string>.Map(int source, MappingContext context) {
			return "2";
		}
	}

	[TestClass]
	public class UnitTest1 {
		[TestMethod]
		public void TestMethod1() {
			Assert.AreEqual("1", typeof(TestClass1).GetInterfaceMap(typeof(IMap<int, string>)).TargetMethods.First().Invoke(null, new object[] { 1, new MappingContext() }));
			Assert.AreEqual("2", typeof(TestClass2).GetInterfaceMap(typeof(IMap<int, string>)).TargetMethods.First().Invoke(null, new object[] { 1, new MappingContext() }));
		}
	}
}