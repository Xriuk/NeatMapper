using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
		public void ShouldFallbackFromNewMapToMergeMapAndForwardOptions() {
			var mapper = new CompositeMapper(new CustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MergeMapsTests.Maps) }
			}));

			Assert.IsTrue(mapper.CanMapNew<float, string>());

			// No Options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				Assert.AreEqual("6", mapper.Map<string>(2f));

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (without matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				mapper.Map<string>(2f, new object[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				mapper.Map<string>(2f, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public void ShouldNotFallbackFromNewMapToMergeMapIfCannotCreateDestination() {
			var mapper = new CompositeMapper(new CustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(MergeMapsTests.Maps) }
			}));

			Assert.IsTrue(mapper.CanMapMerge<string, ClassWithoutParameterlessConstructor>());
			Assert.IsFalse(mapper.CanMapNew<string, ClassWithoutParameterlessConstructor>());

			TestUtils.AssertMapNotFound(() => mapper.Map<ClassWithoutParameterlessConstructor>(""));
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
