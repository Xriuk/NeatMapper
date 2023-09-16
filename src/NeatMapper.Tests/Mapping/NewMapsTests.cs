using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;
using System.Collections.ObjectModel;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMapsTests {
		public class Maps :
			INewMap<int, string>,
			INewMap<int, MyClassInt>,
			INewMap<int, MyClassString>,
			IMergeMap<int, MyClassString>,
			INewMap<float, MyClassString>,
			INewMap<string, KeyValuePair<string, int>>,
			IMergeMap<float, string>,
			IMergeMap<int, string>,
			INewMap<MyClassString, float>,
			INewMap<MyClassString, int> {

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

			static KeyValuePair<string, int> INewMap<string, KeyValuePair<string, int>>.Map(string source, MappingContext context) {
				return new KeyValuePair<string, int>(source, source.Length);
			}

			static string IMergeMap<float, string>.Map(float source, string destination, MappingContext context) {
				return (source * 3).ToString();
			}

			static string IMergeMap<int, string>.Map(int source, string destination, MappingContext context) {
				return (source * 10).ToString();
			}

			// Scope test
			public static IServiceProvider _sp1 = null!;
			public static IServiceProvider _sp2 = null!;
			static float INewMap<MyClassString, float>.Map(MyClassString source, MappingContext context) {
				_sp1 = context.ServiceProvider;
				return context.Mapper.Map<int>(source);
			}
			static int INewMap<MyClassString, int>.Map(MyClassString source, MappingContext context) {
				_sp2 = context.ServiceProvider;
				return source.MyString.Length;
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
		public void ShouldCreateNewScopeForEachMap() {
			Maps._sp2 = null!;

			_mapper.Map<int>(new MyClassString {
				MyString = "Testo"
			});

			Assert.IsNotNull(Maps._sp2);
			var service = Maps._sp2;

			_mapper.Map<int>(new MyClassString {
				MyString = "Testo2"
			});

			Assert.IsNotNull(Maps._sp2);
			Assert.AreNotSame(service, Maps._sp2);
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

		[TestMethod]
		public void ShouldUseSameScopeInNestedMaps() {
			Maps._sp1 = null!;
			Maps._sp2 = null!;

			var result = _mapper.Map<float>(new MyClassString {
				MyString = "Testo"
			});

			Assert.AreEqual(5, result);
			Assert.IsNotNull(Maps._sp1);
			Assert.IsNotNull(Maps._sp2);
			Assert.AreSame(Maps._sp1, Maps._sp2);
		}

		[TestMethod]
		public void ShouldFallbackToMergeMapIfNewMapIsNotDefined() {
			Assert.AreEqual("6", _mapper.Map<string>(2f));
		}

		[TestMethod]
		public void ShouldPreferNewMapIfBothAreDefined() {
			Assert.AreEqual("4", _mapper.Map<int, string>(2));
		}

		[TestMethod]
		public void ShouldMapCollections() {
			{
				var strings = _mapper.Map<string[]>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{ 
				var strings = _mapper.Map<IList<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{ 
				var strings = _mapper.Map<LinkedList<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = _mapper.Map<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = _mapper.Map<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = _mapper.Map<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				var strings = _mapper.Map<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}
		}

		[TestMethod]
		public void ShouldMapNullCollectionsOnlyForDefinedMaps() {
			Assert.IsNull(_mapper.Map<int[]?, string[]?>(null));

			TestUtils.AssertMapNotFound(() => _mapper.Map<int[]?, float[]?>(null));
		}

		[TestMethod]
		public void ShouldFallbackToMergeMapInCollections() {
			var result = _mapper.Map<IList<string>>(new[] { 2f });

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("6", result[0]);
		}

		[TestMethod]
		public void ShouldMapCollectionsOfCollections() {
			var strings = _mapper.Map<IList<IEnumerable<string>>>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2 }
			});

			Assert.IsNotNull(strings);
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
		public void ShouldNotMapMultidimensionalArrays() {
			TestUtils.AssertMapNotFound(() => _mapper.Map<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2, 5 }
			}));
		}
	}
}
