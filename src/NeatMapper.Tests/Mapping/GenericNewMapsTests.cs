using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class GenericNewMapsTests {
		public class Maps<T1> :
			INewMap<IEnumerable<T1>, IList<T1>>,
			INewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>,
			INewMap<IEnumerable<T1>, string>,
			INewMap<int, IList<T1>>{

			static IList<T1> INewMap<IEnumerable<T1>, IList<T1>>.Map(IEnumerable<T1> source, MappingContext context) {
				return source.ToList();
			}

			

			static IEnumerable<T1> INewMap<IDictionary<string, IDictionary<int, IList<T1>>>, IEnumerable<T1>>.Map(IDictionary<string, IDictionary<int, IList<T1>>> source, MappingContext context) {
				return Enumerable.Empty<T1>();
			}

			static string INewMap<IEnumerable<T1>, string>.Map(IEnumerable<T1> source, MappingContext context) {
				return "Elements: " + source.Count();
			}

			static IList<T1> INewMap<int, IList<T1>>.Map(int source, MappingContext context) {
				return new T1[source];
			}
		}

		// Avoid error CS0695
		public class Maps2 :
			INewMap<IEnumerable<bool>, IList<bool>> {

			// Specific map
			static IList<bool> INewMap<IEnumerable<bool>, IList<bool>>.Map(IEnumerable<bool> source, MappingContext context) {
				return new List<bool>(32);
			}
		}

		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions {
				MapTypes = new List<Type> { typeof(Maps<>), typeof(Maps2) }
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

		[TestMethod]
		public void ShouldMapSingleGenerics() {
			Assert.AreEqual("Elements: 2", _mapper.Map<IEnumerable<int>, string>(new[] { 1, 2 }));

			var list = _mapper.Map<int, IList<string>>(3);
			Assert.IsNotNull(list);
			Assert.AreEqual(3, list.Count);
			Assert.IsTrue(list.IsReadOnly);
			Assert.IsTrue(list.All(e => e == default));
		}

		[TestMethod]
		public void ShouldMapDeepGenerics() {
			var objectToMap = new Dictionary<string, IDictionary<int, IList<bool>>>();

			// Does not throw
			_mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<bool>>(objectToMap);
		}

		[TestMethod]
		public void ShouldNotMapNotMatchingDeepGenerics() {
			var objectToMap = new Dictionary<string, IDictionary<int, IList<bool>>>();
			Assert.ThrowsException<ArgumentException>(() => _mapper.Map<IDictionary<string, IDictionary<int, IList<bool>>>, IEnumerable<float>>(objectToMap));
		}

		[TestMethod]
		public void ShouldPreferSpecificMaps() {
			var boolArray = new bool[] { true };
			var boolList = _mapper.Map<IEnumerable<bool>, IList<bool>>(boolArray);

			Assert.IsNotNull(boolList);
			Assert.AreNotSame(boolArray, boolList);
			Assert.AreEqual(0, boolList.Count);
			Assert.AreEqual(32, (boolList as List<bool>)?.Capacity);
		}
	}
}
