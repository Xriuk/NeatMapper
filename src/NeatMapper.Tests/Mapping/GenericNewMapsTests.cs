using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class GenericNewMapsTests {
		public class Maps<T1> :
			INewMap<IEnumerable<T1>, IList<T1>> {

			public static IList<T1> Map(IEnumerable<T1> source, MappingContext context) {
				return source.ToList();
			}
		}

		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
				MapTypes = new List<Type> { typeof(Maps<>) }
			}), new ServiceCollection().BuildServiceProvider());
		}


		[TestMethod]
		public void ShouldMapGenerics() {
			var stringArray = new[] { "Test" };
			var stringsList = _mapper.Map<IEnumerable<string>, IList<string>>(stringArray);

			Assert.IsNotNull(stringsList);
			Assert.AreNotSame(stringArray, stringsList);
			Assert.AreEqual(1, stringsList.Count);
			Assert.AreEqual("Test", stringsList[0]);
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingGenerics() {
			var stringArray = new[] { "Test" };
			Assert.ThrowsException<ArgumentException>(() => _mapper.Map<IEnumerable<string>, IList<int>>(stringArray));
		}
	}
}
