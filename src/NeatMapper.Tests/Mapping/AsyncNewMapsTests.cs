using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class AsyncNewMapsTests {
		public class Maps :
			IAsyncNewMap<int, string>,
			IAsyncNewMap<int, MyClassInt>,
			IAsyncNewMap<int, MyClassString>,
			IAsyncNewMap<bool, MyClassString> {

			static Task<string> IAsyncNewMap<int, string>.Map(int source, AsyncMappingContext context) {
				return Task.FromResult((source * 2).ToString());
			}

			static Task<MyClassInt> IAsyncNewMap<int, MyClassInt>.Map(int source, AsyncMappingContext context) {
				return Task.FromResult(new MyClassInt {
					MyInt = source
				});
			}

			// Awaited Nested NewMap
			static async Task<MyClassString> IAsyncNewMap<int, MyClassString>.Map(int source, AsyncMappingContext context) {
				return new MyClassString {
					MyString = await context.Mapper.MapAsync<int, string>(source, context.CancellationToken)
				};
			}

			// Not awaited Nested NewMap
			static Task<MyClassString> IAsyncNewMap<bool, MyClassString>.Map(bool source, AsyncMappingContext context) {
				return Task.FromResult(new MyClassString {
					MyString = context.Mapper.MapAsync<int, string>(source ? 1 : 0, context.CancellationToken).Result
				});
			}
		}

		IAsyncMapper _mapper = null!;

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
		public async Task ShouldMapCollections() {
			var strings = await _mapper.MapAsync<IList<string>>(new[] { 2, -3, 0 });

			Assert.IsNotNull(strings);
			Assert.AreEqual(3, strings.Count);
			Assert.AreEqual("4", strings[0]);
			Assert.AreEqual("-6", strings[1]);
			Assert.AreEqual("0", strings[2]);
		}

		[TestMethod]
		public async Task ShouldNotMapReadonlyCollectionDestination() {
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync<MyClassString[]>(new[] { 2, -3, 0 }));
		}
	}
}
