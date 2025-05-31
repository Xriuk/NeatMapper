using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class CompositeMapperTests {
		public class TestMapper1 : IMapper, IMapperFactory {
			public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return sourceType == typeof(float) && destinationType == typeof(string);
			}

			public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return false;
			}

			public object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public IMergeMapFactory MapMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				if(sourceType != typeof(float) || destinationType != typeof(string))
					throw new MapNotFoundException((sourceType, destinationType));

				return new DefaultMergeMapFactory<float, string>((f, s) => (f * 2f).ToString());
			}

			public INewMapFactory MapNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}
		}


		[TestMethod]
		public void MapNewFactoryShouldReturnMergeToo() {
			var compositeMapper = new CompositeMapper(new TestMapper1());

			using(var factory = compositeMapper.MapNewFactory<float, string>()) {
				Assert.AreEqual("4", factory.Invoke(2f));
			}
		}
	}
}
