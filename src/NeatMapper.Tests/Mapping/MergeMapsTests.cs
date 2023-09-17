using Microsoft.Extensions.DependencyInjection;
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
			ICollectionElementComparer<float, MyClassString>,
			IMergeMap<MyClassString, float>,
			IMergeMap<MyClassString, int>,
			IMergeMap<IEnumerable<int>, MyClassInt[]> {

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

			// Scope test
			public static IServiceProvider _sp1 = null!;
			public static IServiceProvider _sp2 = null!;
			static float IMergeMap<MyClassString, float>.Map(MyClassString source, float destination, MappingContext context) {
				_sp1 = context.ServiceProvider;
				return context.Mapper.Map<MyClassString, int>(source, (int)destination);
			}
			static int IMergeMap<MyClassString, int>.Map(MyClassString source, int destination, MappingContext context) {
				_sp2 = context.ServiceProvider;
				return source.MyString.Length + destination;
			}

			static MyClassInt[] IMergeMap<IEnumerable<int>, MyClassInt[]>.Map(IEnumerable<int> source, MyClassInt[] destination, MappingContext context) {
				for(int i = 0; i < destination.Length; i++) {
					destination[i] = new MyClassInt {
						MyInt = source.ElementAt(i) * 4
					};
				}
				return destination;
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
		public void ShouldCreateNewScopeForEachMap() {
			Maps._sp2 = null!;

			_mapper.Map(new MyClassString {
				MyString = "Testo"
			}, 2);

			Assert.IsNotNull(Maps._sp2);
			var service = Maps._sp2;

			_mapper.Map(new MyClassString {
				MyString = "Testo2"
			}, 2);

			Assert.IsNotNull(Maps._sp2);
			Assert.AreNotSame(service, Maps._sp2);
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
		public void ShouldUseSameScopeInNestedMaps() {
			Maps._sp1 = null!;
			Maps._sp2 = null!;

			var result = _mapper.Map(new MyClassString {
				MyString = "Testo"
			}, 2f);

			Assert.AreEqual(5 + 2, result);
			Assert.IsNotNull(Maps._sp1);
			Assert.IsNotNull(Maps._sp2);
			Assert.AreSame(Maps._sp1, Maps._sp2);
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
		public void ShouldMapCollectionsByUsingCustomElementsComparer() {
			var da = new MyClassString {
				MyString = "4"
			};
			var db = new MyClassString {
				MyString = "-6"
			};
			var dc = new MyClassString();
			var destination = new List<MyClassString> { da, db, dc };

			var result = _mapper.MapCollection(new[] { 2f, -3f, 0f }, destination, (s, d, _) => d.MyString == (s * 2f).ToString());

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(3, result.Count());
			Assert.AreSame(da, result.ElementAt(0));
			Assert.AreEqual("4", result.ElementAt(0).MyString);
			Assert.AreSame(db, result.ElementAt(1));
			Assert.AreEqual("-6", result.ElementAt(1).MyString);
			Assert.AreNotSame(dc, result.ElementAt(2));
			Assert.AreEqual("0", result.ElementAt(2).MyString);
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestinationWithoutExplicitMap() {
			var a = new MyClassString {
				MyString = "A"
			};
			var b = new MyClassString {
				MyString = "B"
			};
			var c = new MyClassString {
				MyString = "C"
			};
			var destination = new MyClassString[3]{ a, b, c };

			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { 2, -3, 0 }, destination));

			// Should not alter destination
			Assert.AreSame(a, destination[0]);
			Assert.AreSame(b, destination[1]);
			Assert.AreSame(c, destination[2]);

			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<int>, ICollection<MyClassString>>(new[] { 2, -3, 0 }, destination));

			// Should not alter destination
			Assert.AreSame(a, destination[0]);
			Assert.AreSame(b, destination[1]);
			Assert.AreSame(c, destination[2]);
		}

		[TestMethod]
		public void ShouldNotMapReadonlyCollectionDestinationNestedWithoutExplicitMap() {
			throw new NotImplementedException("AAAA");

			var a = new MyClassString {
				MyString = "A"
			};
			var b = new MyClassString {
				MyString = "B"
			};
			var c = new MyClassString {
				MyString = "C"
			};
			var destination = new MyClassString[3] { a, b, c };

			TestUtils.AssertMapNotFound(() => _mapper.Map(new[] { 2, -3, 0 }, destination));

			// Should not alter destination
			Assert.AreSame(a, destination[0]);
			Assert.AreSame(b, destination[1]);
			Assert.AreSame(c, destination[2]);

			TestUtils.AssertMapNotFound(() => _mapper.Map<IEnumerable<int>, ICollection<MyClassString>>(new[] { 2, -3, 0 }, destination));

			// Should not alter destination
			Assert.AreSame(a, destination[0]);
			Assert.AreSame(b, destination[1]);
			Assert.AreSame(c, destination[2]);
		}

		[TestMethod]
		public void ShouldMapReadonlyCollectionDestinationWithExplicitMap() {
			var destination = new MyClassInt[3];

			var result = _mapper.Map<IEnumerable<int>, MyClassInt[]>(new[] { 2, -3, 0 }, destination);
			Assert.AreSame(destination, result);
			Assert.AreEqual(8, destination[0].MyInt);
			Assert.AreEqual(-12, destination[1].MyInt);
			Assert.AreEqual(0, destination[2].MyInt);
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollectionsWithoutElementsComparer() {
			var destination = new List<IEnumerable<string>>();
			var strings = _mapper.Map(new[] {
				new[]{ 2f, -3f, 0f },
				new[]{ 1f, 2f }
			}, destination);

			Assert.AreSame(destination, strings);
			Assert.AreEqual(2, strings.Count);
			Assert.AreEqual(3, strings[0].Count());
			Assert.AreEqual(2, strings[1].Count());
			Assert.AreEqual("4", strings[0].ElementAt(0));
			Assert.AreEqual("-6", strings[0].ElementAt(1));
			Assert.AreEqual("0", strings[0].ElementAt(2));
			Assert.AreEqual("2", strings[1].ElementAt(0));
			Assert.AreEqual("4", strings[1].ElementAt(1));
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollectionsWithElementsComparer() {
			var da = new MyClassString {
				MyString = "2"
			};
			var db = new MyClassString {
				MyString = "-3"
			};
			var dc = new MyClassString();
			var destination1 = new List<MyClassString> { da, db, dc };
			var dd = new MyClassString();
			var de = new MyClassString {
				MyString = "5"
			};
			ICollection<MyClassString> destination2 = new LinkedList<MyClassString>();
			destination2.Add(dd);
			destination2.Add(de);
			var destination = new List<IEnumerable<MyClassString>> {
				destination1, destination2
			};

			var result = _mapper.Map<IEnumerable<IEnumerable<float>>, IList<IEnumerable<MyClassString>>>(new[] { 
				new []{ 2f, -3f, 0f },
				new []{ 0f, 5f, 1f }
			}, destination);

			// A collection of collections, even though the innermost has an elements comparer the outer ones could not be matched
			// so they will be recreated, and also the children will be recreated regardless
			Assert.AreSame(destination, result);
			Assert.AreEqual(2, result.Count());
			Assert.AreEqual(3, result[0].Count());
			Assert.IsTrue(result[0].All(e => e != da && e != db && e != dc & e != dd && e != de));
			Assert.IsTrue(result[0].Any(e => e.MyString == "4"));
			Assert.IsTrue(result[0].Any(e => e.MyString == "-6"));
			Assert.IsTrue(result[0].Any(e => e.MyString == "0"));
			Assert.AreEqual(3, result[1].Count());
			Assert.IsTrue(result[1].All(e => e != da && e != db && e != dc & e != dd && e != de));
			Assert.IsTrue(result[1].Any(e => e.MyString == "10"));
			Assert.IsTrue(result[1].Any(e => e.MyString == "0"));
			Assert.IsTrue(result[1].Any(e => e.MyString == "2"));
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
		public void ShouldPreferMergeMapForElementsToUpdateAndNewMapForElementsToAddInCollections() {
			var destination = new List<string> { "2", "-4", "0" };
			var result = _mapper.MapCollection(new[] { 2, -4, 5, 6 }, destination, (s, d, _) => s.ToString() == d);

			Assert.IsNotNull(result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(4, result.Count);
			Assert.AreEqual("4", result.ElementAt(0));
			Assert.AreEqual("-8", result.ElementAt(1));
			Assert.AreEqual("15", result.ElementAt(2));
			Assert.AreEqual("18", result.ElementAt(3));
		}
	}
}
