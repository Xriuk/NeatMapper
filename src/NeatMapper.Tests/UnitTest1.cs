using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;

namespace NeatMapper.Tests {
	public sealed class TestClass1 : INewMap<int, string> {
		public static string Map(int source, MappingContext context) {
			return "1";
		}
	}

	public sealed class TestClass2 : INewMap<int, string> {
		static string INewMap<int, string>.Map(int source, MappingContext context) {
			return "2";
		}
	}

	public class MyGenericClass<T> : INewMap<IDictionary<int, T>, string> {
		public static string Map(IDictionary<int, T> source, MappingContext context) {
			throw new NotImplementedException();
		}
	}

	[TestClass]
	public class UnitTest1 {
		[TestMethod]
		public void TestMethod1() {
			var options = new MapperConfigurationOptions();
			options.MapTypes.Add(typeof(MyGenericClass<>));
			new MapperConfiguration(options);
			//Assert.AreEqual("1", typeof(TestClass1).GetInterfaceMap(typeof(INewMap<int, string>)).TargetMethods.First().Invoke(null, new object[] { 1, new MappingContext() }));
			//Assert.AreEqual("2", typeof(TestClass2).GetInterfaceMap(typeof(INewMap<int, string>)).TargetMethods.First().Invoke(null, new object[] { 1, new MappingContext() }));
		}
	}
}