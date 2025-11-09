using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Transitive.Tests.Mapping.Async {
	[TestClass]
	public class AsyncTransitiveMergeMapsTests {
		// Using all IMergeMaps because they can be used in new maps too
		public class Maps :
			IAsyncMergeMap<float, double>,
			IAsyncMergeMap<double, decimal>,
			IAsyncMergeMap<decimal, Price>,
			IAsyncMergeMap<Price, PriceFloat>,

			// string > decimal > Price
			IAsyncMergeMap<string, decimal>,

			// string > decimal > int > Price
			IAsyncMergeMap<decimal, int>,
			IAsyncMergeMap<int, Price> {

			public static AsyncNestedMappingContext NestedMappingContext;

			Task<double> IAsyncMergeMap<float, double>.MapAsync(float source, double destination, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				return Task.FromResult((double)source);
			}

			Task<decimal> IAsyncMergeMap<double, decimal>.MapAsync(double source, decimal destination, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				return Task.FromResult((decimal)source);
			}

			Task<Price> IAsyncMergeMap<decimal, Price>.MapAsync(decimal source, Price destination, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				if (destination == null)
					destination = new Price();
				destination.Amount = source;
				destination.Currency = "EUR";

				return Task.FromResult(destination);
			}

			Task<PriceFloat> IAsyncMergeMap<Price, PriceFloat>.MapAsync(Price source, PriceFloat destination, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				if (source == null)
					return Task.FromResult<PriceFloat>(null);

				if(destination == null)
					destination = new PriceFloat();
				destination.Amount = (float)source.Amount;
				destination.Currency = source.Currency;

				return Task.FromResult(destination);
			}


			Task<decimal> IAsyncMergeMap<string, decimal>.MapAsync(string source, decimal destination, AsyncMappingContext context) {
				return Task.FromResult(decimal.Parse(source));
			}

			Task<int> IAsyncMergeMap<decimal, int>.MapAsync(decimal source, int destination, AsyncMappingContext context) {
				return Task.FromResult((int)source);
			}

			Task<Price> IAsyncMergeMap<int, Price>.MapAsync(int source, Price destination, AsyncMappingContext context) {
				if(destination == null)
					destination = new Price();
				destination.Amount = source;
				destination.Currency = "EUR";

				return Task.FromResult(destination);
			}
		}


		private IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncCompositeMapper(
				new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new AsyncTransitiveMapper(AsyncEmptyMapper.Instance));
		}

		private void VerifyMappingContext() {
			Assert.IsNotNull(Maps.NestedMappingContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentMapper, typeof(AsyncCompositeMapper));
			Assert.IsNotNull(Maps.NestedMappingContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentContext.ParentMapper, typeof(AsyncTransitiveMapper));
			Assert.IsNotNull(Maps.NestedMappingContext.ParentContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentContext.ParentContext.ParentMapper, typeof(AsyncCompositeMapper));
			Assert.IsNull(Maps.NestedMappingContext.ParentContext.ParentContext.ParentContext);
		}


		[TestMethod]
		public async Task ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<float, decimal>());
			Assert.IsTrue(_mapper.CanMapAsyncMerge(typeof(float), typeof(decimal)));

			Maps.NestedMappingContext = null;
			Assert.AreEqual(4m, await _mapper.MapAsync(4f, 0m));
			Assert.AreEqual(4m, await _mapper.MapAsync(4f, typeof(float), 0m, typeof(decimal)));
			VerifyMappingContext();

			using (var factory = _mapper.MapAsyncMergeFactory<float, decimal>()) { 
				Assert.AreEqual(4m, await factory.Invoke(4f, 0m));
			}
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			Assert.IsTrue(_mapper.CanMapAsyncMerge<float, PriceFloat>());
			Assert.IsTrue(_mapper.CanMapAsyncMerge(typeof(float), typeof(PriceFloat)));

			Maps.NestedMappingContext = null;
			var result = await _mapper.MapAsync(4f, new PriceFloat());
			Assert.AreEqual(4f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
			Assert.AreEqual(4f, (await _mapper.MapAsync(4f, typeof(float), new PriceFloat(), typeof(PriceFloat)) as PriceFloat)?.Amount);
			VerifyMappingContext();

			using (var factory = _mapper.MapAsyncMergeFactory<float, PriceFloat>()) {
				Assert.AreEqual(4f, (await factory.Invoke(4f, new PriceFloat()))?.Amount);
			}
		}

		[TestMethod]
		public async Task ShouldRespectLengthIfSpecified() {
			var mapper = new AsyncTransitiveMapper(_mapper);

			// Map length is 5, so shorter
			Assert.IsFalse(mapper.CanMapAsyncMerge<float, PriceFloat>(new TransitiveMappingOptions(4)));
			await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => mapper.MapAsync<float, PriceFloat>(2f, new PriceFloat(), new MappingOptions(new TransitiveMappingOptions(4))));

			// Length of 2 should invoke the map directly, if available
			Assert.IsTrue(mapper.CanMapAsyncMerge<float, double>(new TransitiveMappingOptions(2)));

			// Length of 0, 1 should not map anything
			Assert.IsFalse(mapper.CanMapAsyncMerge<float, double>(new TransitiveMappingOptions(0)));
			Assert.IsFalse(mapper.CanMapAsyncMerge<float, double>(new TransitiveMappingOptions(1)));
		}
	}
}
