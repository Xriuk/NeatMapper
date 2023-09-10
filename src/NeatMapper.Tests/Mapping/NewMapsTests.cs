using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMapsTests {
		public class Maps :
			INewMap<int, string>,
			INewMap<int, MyClassInt>,
			INewMap<int, MyClassString>,
			IMergeMap<int, MyClassString>,
			INewMap<float, MyClassString> {

			static string INewMap<int, string>.Map(int source, MappingContext context) {
				return (source * 2).ToString();
			}

			static MyClassInt INewMap<int, MyClassInt>.Map(int source, MappingContext context) {
				return new MyClassInt {
					MyInt = source
				};
			}

			// Nested NewMap
			static MyClassString INewMap<int, MyClassString>.Map(int source, MappingContext context) {
				return new MyClassString {
					MyString = context.Mapper.Map<int, string>(source)
				};
			}

			static MyClassString IMergeMap<int, MyClassString>.Map(int source, MyClassString destination, MappingContext context) {
				destination.MyString = (source * 2).ToString();
				return destination;
			}

			// Nested MergeMap
			static MyClassString INewMap<float, MyClassString>.Map(float source, MappingContext context) {
				return context.Mapper.Map((int)source, new MyClassString());
			}
		}

		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions{
				MapTypes = new List<Type> { typeof(Maps) }
			}), new ServiceCollection().BuildServiceProvider());
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public void ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, _mapper.Map<int, string>(input));
			Assert.AreEqual(output, _mapper.Map<string>(input));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			var obj = _mapper.Map<int, MyClassInt>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual(2, obj.MyInt);

			obj = _mapper.Map<MyClassInt>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual(2, obj.MyInt);
		}

		[TestMethod]
		public void ShouldNotFindMissingMap() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<bool, int>(false));
			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(false));
		}

		[TestMethod]
		public void ShouldMapNested() {
			var obj = _mapper.Map<int, MyClassString>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual("4", obj.MyString);

			obj = _mapper.Map<float, MyClassString>(2f);
			Assert.IsNotNull(obj);
			Assert.AreEqual("4", obj.MyString);
		}

		// DEV: check scope (in nested?)

		[TestMethod]
		public void ShouldMapCollections() {
			var strings = _mapper.Map<IList<string>>(new[] { 2, -3, 0 });

			Assert.IsNotNull(strings);
			Assert.AreEqual(3, strings.Count);
			Assert.AreEqual("4", strings[0]);
			Assert.AreEqual("-6", strings[1]);
			Assert.AreEqual("0", strings[2]);
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestination() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<MyClassString[]>(new[] { 2, -3, 0 }));
		}

		// DEV: test null in source and/or destination for collections

		// DEV: test scope in collection

		// DEV: test element mapping in collection, elements to update will use MergeMap or NewMap, elements to add will use NewMap or MergeMap, in this order
	}
}
