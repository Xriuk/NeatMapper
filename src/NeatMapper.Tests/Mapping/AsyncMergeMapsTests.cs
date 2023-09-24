using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class AsyncMergeMapsTests {
		public class Maps :
			IAsyncMergeMap<int, string>,
			IAsyncNewMap<int, string>,
			IAsyncMergeMap<float, string>,
			IAsyncMergeMap<int, MyClassInt>,
			IAsyncMergeMap<int, MyClassString> {

			static Task<string> IAsyncMergeMap<int, string>.MapAsync(int source, string destination, AsyncMappingContext context) {
				return Task.FromResult((source * 2).ToString());
			}

			static Task<string> IAsyncNewMap<int, string>.MapAsync(int source, AsyncMappingContext context) {
				return Task.FromResult((source * 3).ToString());
			}

			static Task<string> IAsyncMergeMap<float, string>.MapAsync(float source, string destination, AsyncMappingContext context) {
				return Task.FromResult((source * 2).ToString());
			}

			static Task<MyClassInt> IAsyncMergeMap<int, MyClassInt>.MapAsync(int source, MyClassInt destination, AsyncMappingContext context) {
				return Task.FromResult(new MyClassInt {
					MyInt = source
				});
			}

			static Task<MyClassString> IAsyncMergeMap<int, MyClassString>.MapAsync(int source, MyClassString destination, AsyncMappingContext context) {
				destination.MyString = (source * 2).ToString();
				return Task.FromResult(destination);
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
			Assert.AreEqual(output, await _mapper.MapAsync<int, string>(input, ""));
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			var obj = await _mapper.MapAsync<int, MyClassInt>(2, new MyClassInt());
			Assert.IsNotNull(obj);
			Assert.AreEqual(2, obj.MyInt);
		}

		[TestMethod]
		public async Task ShouldNotFindMissingMap() {
			await TestUtils.AssertMapNotFound(() => _mapper.MapAsync(false, 0));
		}

		[TestMethod]
		public async Task ShouldRespectReturnedValue() {
			var myIntDestination = new MyClassInt();
			var myIntReturn = await _mapper.MapAsync(1, myIntDestination);
			Assert.IsNotNull(myIntReturn);
			Assert.AreNotSame(myIntDestination, myIntReturn);

			var myStringDestination = new MyClassString();
			var myStringReturn = await _mapper.MapAsync(1, myStringDestination);
			Assert.IsNotNull(myStringReturn);
			Assert.AreSame(myStringDestination, myStringReturn);
		}

		[TestMethod]
		public async Task ShouldFallbackToMergeMap() {
			Assert.AreEqual("4", await _mapper.MapAsync<float, string>(2f));
		}

		[TestMethod]
		public async Task ShouldPreferNewMap() {
			Assert.AreEqual("6", await _mapper.MapAsync<int, string>(2));
		}
	}
}
