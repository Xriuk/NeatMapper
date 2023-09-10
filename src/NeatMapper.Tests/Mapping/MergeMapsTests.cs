using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class MergeMapsTests {
		public class Maps :
			IMergeMap<int, string>,
			INewMap<int, string>,
			IMergeMap<float, string>,
			IMergeMap<int, MyClassInt>,
			IMergeMap<int, MyClassString>,
			IMergeMap<bool, MyClassString>,
			IMergeMap<float, MyClassString>,
			ICollectionElementComparer<float, MyClassString>{

			static string IMergeMap<int, string>.Map(int source, string destination, MappingContext context) {
				return (source * 2).ToString();
			}

			static string INewMap<int, string>.Map(int source, MappingContext context) {
				return (source * 3).ToString();
			}

			static string IMergeMap<float, string>.Map(float source, string destination, MappingContext context) {
				return (source * 2).ToString();
			}

			// Returns new destination
			static MyClassInt IMergeMap<int, MyClassInt>.Map(int source, MyClassInt destination, MappingContext context) {
				return new MyClassInt {
					MyInt = source
				};
			}

			// Returns passed destination
			static MyClassString IMergeMap<int, MyClassString>.Map(int source, MyClassString destination, MappingContext context) {
				destination.MyString = (source * 2).ToString();
				return destination;
			}

			// Nested NewMap
			static MyClassString IMergeMap<bool, MyClassString>.Map(bool source, MyClassString destination, MappingContext context) {
				destination.MyString = context.Mapper.Map<int, string>(source ? 1 : 0);
				return destination;
			}

			// Nested MergeMap
			static MyClassString IMergeMap<float, MyClassString>.Map(float source, MyClassString destination, MappingContext context) {
				return context.Mapper.Map((int)source, destination);
			}

			static bool ICollectionElementComparer<float, MyClassString>.Match(float source, MyClassString destination, MappingContext context) {
				return source.ToString() == destination.MyString;
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
			Assert.AreEqual(output, _mapper.Map<int, string>(input, ""));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			var obj = _mapper.Map<int, MyClassInt>(2, new MyClassInt());
			Assert.IsNotNull(obj);
			Assert.AreEqual(2, obj.MyInt);
		}

		[TestMethod]
		public void ShouldNotFindMissingMap() {
			TestUtils.AssertMapNotFound(() => _mapper.Map(false, 0));
		}

		[TestMethod]
		public void ShouldMapNested() {
			var obj = _mapper.Map<bool, MyClassString>(true);
			Assert.IsNotNull(obj);
			Assert.AreEqual("3", obj.MyString);

			obj = _mapper.Map<float, MyClassString>(2f);
			Assert.IsNotNull(obj);
			Assert.AreEqual("4", obj.MyString);
		}

		[TestMethod]
		public void ShouldRespectReturnedValue() {
			var myIntDestination = new MyClassInt();
			var myIntReturn = _mapper.Map(1, myIntDestination);
			Assert.IsNotNull(myIntReturn);
			Assert.AreNotSame(myIntDestination, myIntReturn);

			var myStringDestination = new MyClassString();
			var myStringReturn = _mapper.Map(1, myStringDestination);
			Assert.IsNotNull(myStringReturn);
			Assert.AreSame(myStringDestination, myStringReturn);
		}

		[TestMethod]
		public void ShouldFallbackToMergeMap() {
			Assert.AreEqual("4", _mapper.Map<float, string>(2));
		}

		[TestMethod]
		public void ShouldPreferNewMap() {
			Assert.AreEqual("6", _mapper.Map<int, string>(2));
		}

		[TestMethod]
		public void ShouldMapCollectionsWithoutElementsComparer() {
			var da = new MyClassString();
			var db = new MyClassString();
			var dc = new MyClassString();
			var destination = new List<MyClassString> { da, db, dc };

			// Should recreate all the elements since it cannot match them to update them
			var result = _mapper.Map(new[] { 2, -3, 0 }, destination);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreNotSame(da, result[0]);
			Assert.AreEqual("4", result[0].MyString);
			Assert.AreNotSame(db, result[1]);
			Assert.AreEqual("-6", result[1].MyString);
			Assert.AreNotSame(dc, result[2]);
			Assert.AreEqual("0", result[2].MyString);
		}

		[TestMethod]
		public void ShouldMapCollectionsWithElementsComparer() {
			var da = new MyClassString{
				MyString = "2"
			};
			var db = new MyClassString {
				MyString = "-3"
			};
			var dc = new MyClassString();
			var destination = new List<MyClassString> { da, db, dc };

			var result = _mapper.Map(new[] { 2f, -3f, 0f }, destination);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreSame(da, result[0]);
			Assert.AreEqual("4", result[0].MyString);
			Assert.AreSame(db, result[1]);
			Assert.AreEqual("-6", result[1].MyString);
			Assert.AreNotSame(dc, result[2]);
			Assert.AreEqual("0", result[2].MyString);
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestination() {
			var destination = new MyClassString[3];

			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { 2, -3, 0 }, destination));

			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<int>, ICollection<MyClassString>>(new[] { 2, -3, 0 }, destination));
		}

		[TestMethod]
		public void ShouldRespectReturnedValueInCollections() {
			var da = new MyClassInt {
				MyInt = 2
			};
			var db = new MyClassInt {
				MyInt = -3
			};
			var dc = new MyClassInt();
			var destination = new List<MyClassInt> { da, db, dc };

			var result = _mapper.Map(new[] { 2, -3, 4 }, destination);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreNotSame(da, result[0]);
			Assert.AreEqual(2, result[0].MyInt);
			Assert.AreNotSame(db, result[1]);
			Assert.AreEqual(-3, result[1].MyInt);
			Assert.AreNotSame(dc, result[2]);
			Assert.AreEqual(4, result[2].MyInt);
		}

		[TestMethod]
		public void ShouldFallbackToMergeMapInCollections() {
			Assert.AreEqual("4", _mapper.Map<float, string>(2));
		}
	}
}
