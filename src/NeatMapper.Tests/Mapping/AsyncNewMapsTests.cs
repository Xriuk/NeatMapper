using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;
using System.Collections.ObjectModel;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class AsyncNewMapsTests {
		public class Maps :
			IAsyncNewMap<int, string>,
			IAsyncMergeMap<int, string>,
			IAsyncNewMap<int, MyClassInt>,
			IAsyncNewMap<int, MyClassString>,
			IAsyncNewMap<bool, MyClassString>,
			IAsyncNewMap<MyClassString, float>,
			IAsyncNewMap<MyClassString, int>,
			IAsyncNewMap<string, KeyValuePair<string, int>>,
			IAsyncMergeMap<float, string> {

			static Task<string> IAsyncNewMap<int, string>.MapAsync(int source, AsyncMappingContext context) {
				return Task.FromResult((source * 2).ToString());
			}

			static Task<string> IAsyncMergeMap<int, string>.MapAsync(int source, string destination, AsyncMappingContext context) {
				return Task.FromResult((source * 3).ToString());
			}

			static Task<MyClassInt> IAsyncNewMap<int, MyClassInt>.MapAsync(int source, AsyncMappingContext context) {
				return Task.FromResult(new MyClassInt {
					MyInt = source
				});
			}

			// Awaited Nested NewMap
			static async Task<MyClassString> IAsyncNewMap<int, MyClassString>.MapAsync(int source, AsyncMappingContext context) {
				return new MyClassString {
					MyString = await context.Mapper.MapAsync<int, string>(source, context.CancellationToken)
				};
			}

			// Not awaited Nested NewMap
			static Task<MyClassString> IAsyncNewMap<bool, MyClassString>.MapAsync(bool source, AsyncMappingContext context) {
				return Task.FromResult(new MyClassString {
					MyString = context.Mapper.MapAsync<int, string>(source ? 1 : 0, context.CancellationToken).Result
				});
			}

			// Scope test
			public static IServiceProvider _sp1 = null!;
			public static IServiceProvider _sp2 = null!;
			static async Task<float> IAsyncNewMap<MyClassString, float>.MapAsync(MyClassString source, AsyncMappingContext context) {
				_sp1 = context.ServiceProvider;
				return await context.Mapper.MapAsync<int>(source);
			}
			static Task<int> IAsyncNewMap<MyClassString, int>.MapAsync(MyClassString source, AsyncMappingContext context) {
				_sp2 = context.ServiceProvider;
				return Task.FromResult(source.MyString.Length);
			}

			static Task<KeyValuePair<string, int>> IAsyncNewMap<string, KeyValuePair<string, int>>.MapAsync(string source, AsyncMappingContext context) {
				return Task.FromResult(new KeyValuePair<string, int>(source, source.Length));
			}

			static Task<string> IAsyncMergeMap<float, string>.MapAsync(float source, string destination, AsyncMappingContext context) {
				return Task.FromResult((source * 3).ToString());
			}
		}

		IAsyncMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions{
				ScanTypes = new List<Type> { typeof(Maps) }
			}), new ServiceCollection().BuildServiceProvider());
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public async Task ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, await _mapper.MapAsync<int, string>(input));
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			var obj = await _mapper.MapAsync<int, MyClassInt>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual(2, obj.MyInt);
		}

		[TestMethod]
		public async Task ShouldNotFindMissingMap() {
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<bool, int>(false));
		}

		[TestMethod]
		public async Task ShouldCreateNewScopeForEachMap() {
			Maps._sp2 = null!;

			await _mapper.MapAsync<int>(new MyClassString {
				MyString = "Testo"
			});

			Assert.IsNotNull(Maps._sp2);
			var service = Maps._sp2;

			await _mapper.MapAsync<int>(new MyClassString {
				MyString = "Testo2"
			});

			Assert.IsNotNull(Maps._sp2);
			Assert.AreNotSame(service, Maps._sp2);
		}

		[TestMethod]
		public async Task ShouldMapNested() {
			// Awaited
			var obj = await _mapper.MapAsync<int, MyClassString>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual("4", obj.MyString);

			// Not awaited
			obj = await _mapper.MapAsync<bool, MyClassString>(true);
			Assert.IsNotNull(obj);
			Assert.AreEqual("2", obj.MyString);
		}

		[TestMethod]
		public async Task ShouldUseSameScopeInNestedMaps() {
			Maps._sp1 = null!;
			Maps._sp2 = null!;

			var result = await _mapper.MapAsync<float>(new MyClassString {
				MyString = "Testo"
			});

			Assert.AreEqual(5, result);
			Assert.IsNotNull(Maps._sp1);
			Assert.IsNotNull(Maps._sp2);
			Assert.AreSame(Maps._sp1, Maps._sp2);
		}

		[TestMethod]
		public async Task ShouldFallbackToMergeMapIfNewMapIsNotDefined() {
			Assert.AreEqual("6", await _mapper.MapAsync<string>(2f));
		}

		[TestMethod]
		public async Task ShouldPreferNewMapIfBothAreDefined() {
			Assert.AreEqual("4", await _mapper.MapAsync<int, string>(2));
		}

		[TestMethod]
		public async Task ShouldMapCollections() {
			{
				var strings = await _mapper.MapAsync<string[]>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Length);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{
				var strings = await _mapper.MapAsync<IList<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings[0]);
				Assert.AreEqual("-6", strings[1]);
				Assert.AreEqual("0", strings[2]);
			}

			{
				var strings = await _mapper.MapAsync<LinkedList<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<Queue<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual("4", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("0", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<SortedList<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}

			{
				var strings = await _mapper.MapAsync<Stack<string>>(new[] { 2, -3, 0 });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				// Order is inverted
				Assert.AreEqual("0", strings.ElementAt(0));
				Assert.AreEqual("-6", strings.ElementAt(1));
				Assert.AreEqual("4", strings.ElementAt(2));
			}

			{
				var strings = await _mapper.MapAsync<ReadOnlyDictionary<string, int>>(new[] { "A", "BB", "CCC" });

				Assert.IsNotNull(strings);
				Assert.AreEqual(3, strings.Count);
				Assert.AreEqual(1, strings["A"]);
				Assert.AreEqual(2, strings["BB"]);
				Assert.AreEqual(3, strings["CCC"]);
			}
		}

		[TestMethod]
		public async Task ShouldMapNullCollectionsOnlyForDefinedMaps() {
			Assert.IsNull(await _mapper.MapAsync<int[]?, string[]?>(null));

			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<int[]?, float[]?>(null));
		}

		[TestMethod]
		public async Task ShouldFallbackToMergeMapInCollections() {
			var result = await _mapper.MapAsync<IList<string>>(new[] { 2f });

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual("6", result[0]);
		}

		[TestMethod]
		public async Task ShouldMapCollectionsOfCollections() {
			var strings = await _mapper.MapAsync<IList<IEnumerable<string>>>(new[] {
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
		public async Task ShouldNotMapMultidimensionalArrays() {
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<string[,]>(new[] {
				new[]{ 2, -3, 0 },
				new[]{ 1, 2, 5 }
			}));
		}
	}
}
