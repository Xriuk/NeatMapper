using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Core;
using NeatMapper.Core.Configuration;
using NeatMapper.Core.Mapper;
using NeatMapper.Tests.Classes;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NewMaps :
		INewMap<int, string>,
		INewMap<int, MyClassInt>,
		INewMap<int, MyClassString> {

		static string INewMap<int, string>.Map(int source, MappingContext context) {
			return (source * 2).ToString();
		}

		static MyClassInt INewMap<int, MyClassInt>.Map(int source, MappingContext context) {
			return new MyClassInt {
				MyInt = source
			};
		}

		static MyClassString INewMap<int, MyClassString>.Map(int source, MappingContext context) {
			return new MyClassString {
				MyString = context.Mapper.Map<int, string>(source)
			};
		}

		IMapper _mapper = null!;

		[TestInitialize]
		public void Initialize() {
			_mapper = new Mapper(new MapperConfiguration(new MapperConfigurationOptions{
				MapTypes = new List<Type> { typeof(NewMaps) }
			}), new ServiceCollection().BuildServiceProvider());
		}


		[TestMethod]
		[DataRow(2, "4")]
		[DataRow(-3, "-6")]
		[DataRow(0, "0")]
		public void ShouldMapPrimitives(int input, string output) {
			Assert.AreEqual(output, _mapper.Map<int, string>(input));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			var obj = _mapper.Map<int, MyClassInt>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual(2, obj.MyInt);
		}

		[TestMethod]
		public void ShouldNotFindMissingMap() {
			Assert.ThrowsException<ArgumentException>(() => _mapper.Map<bool, int>(false));
		}

		[TestMethod]
		public void ShouldMapNested() {
			var obj = _mapper.Map<int, MyClassString>(2);
			Assert.IsNotNull(obj);
			Assert.AreEqual("4", obj.MyString);
		}
	}
}
