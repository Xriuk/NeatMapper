using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NeatMapper.Tests.AsyncMapping {
	[TestClass]
	public class AsyncCompositeMapperTests {
		public class TestMapper1 : IAsyncMapper, IAsyncMapperFactory {
			public bool CanMapAsyncMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return sourceType == typeof(float) && destinationType == typeof(string);
			}

			public bool CanMapAsyncNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				return false;
			}

			public Task<object> MapAsync(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public Task<object> MapAsync(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null, CancellationToken cancellationToken = default) {
				throw new MapNotFoundException((sourceType, destinationType));
			}

			public IAsyncMergeMapFactory MapAsyncMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				if (sourceType != typeof(float) || destinationType != typeof(string))
					throw new MapNotFoundException((sourceType, destinationType));

				return new DefaultAsyncMergeMapFactory<float, string>((f, s, c) => Task.FromResult((f * 2f).ToString()));
			}

			public IAsyncNewMapFactory MapAsyncNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new MapNotFoundException((sourceType, destinationType));
			}
		}


		[TestMethod]
		public async Task ShouldFallbackFromNewMapToMergeMapAndForwardOptions() {
			var mapper = new AsyncCompositeMapper(new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncMergeMapsTests.Maps) }
			}));

			Assert.IsTrue(mapper.CanMapAsyncNew<float, string>());

			// No Options
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				Assert.AreEqual("6", await mapper.MapAsync<string>(2f));

				Assert.IsNull(MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (without matcher)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				await mapper.MapAsync<string>(2f, new[] { opts });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.IsNull(MappingOptionsUtils.mergeOptions);
			}

			// Options (with matcher, forwards everything)
			{
				MappingOptionsUtils.options = null;
				MappingOptionsUtils.mergeOptions = null;

				var opts = new TestOptions();
				var merge = new MergeCollectionsMappingOptions(false, EmptyMatcher.Instance);
				await mapper.MapAsync<string>(2f, new object[] { opts, merge });

				Assert.AreSame(opts, MappingOptionsUtils.options);
				Assert.AreSame(merge, MappingOptionsUtils.mergeOptions);
				Assert.IsNotNull(MappingOptionsUtils.mergeOptions.Matcher);
				Assert.IsFalse(MappingOptionsUtils.mergeOptions.RemoveNotMatchedDestinationElements);
			}
		}

		[TestMethod]
		public async Task ShouldNotFallbackFromNewMapToMergeMapIfCannotCreateDestination() {
			var mapper = new AsyncCompositeMapper(new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncMergeMapsTests.Maps) }
			}));

			Assert.IsFalse(mapper.CanMapAsyncNew<string, ClassWithoutParameterlessConstructor>());

			await TestUtils.AssertMapNotFound(() => mapper.MapAsync<ClassWithoutParameterlessConstructor>(""));
		}

		[TestMethod]
		public async Task MapAsyncNewFactoryShouldReturnMergeToo() {
			var compositeMapper = new AsyncCompositeMapper(new TestMapper1());

			using (var factory = compositeMapper.MapAsyncNewFactory<float, string>()) {
				Assert.AreEqual("4", await factory.Invoke(2f));
			}
		}
	}
}
