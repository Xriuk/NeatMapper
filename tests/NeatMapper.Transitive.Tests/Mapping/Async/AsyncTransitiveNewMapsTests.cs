using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace NeatMapper.Transitive.Tests.Mapping.Async {
	[TestClass]
	public class AsyncTransitiveNewMapsTests {
		public class Maps :
			IAsyncNewMap<float, double>,
			IAsyncNewMap<double, decimal>,
			IAsyncNewMap<decimal, Price>,
			IAsyncNewMap<Price, PriceFloat> {

			public static AsyncNestedMappingContext NestedMappingContext;

			Task<double> IAsyncNewMap<float, double>.MapAsync(float source, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				return Task.FromResult((double)source);
			}

			Task<decimal> IAsyncNewMap<double, decimal>.MapAsync(double source, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				return Task.FromResult((decimal)source);
			}

			Task<Price> IAsyncNewMap<decimal, Price>.MapAsync(decimal source, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				return Task.FromResult(new Price {
					Amount = source,
					Currency = "EUR"
				});
			}

			Task<PriceFloat> IAsyncNewMap<Price, PriceFloat>.MapAsync(Price source, AsyncMappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<AsyncNestedMappingContext>();
				if (source == null)
					return Task.FromResult<PriceFloat>(null);
				else {
					return Task.FromResult(new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					});
				}
			}
		}


		private IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
#pragma warning disable CS0618
			_mapper = new AsyncCompositeMapper(
				new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new AsyncTransitiveMapper(AsyncEmptyMapper.Instance));
#pragma warning restore CS0618
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
			Assert.IsTrue(_mapper.CanMapAsyncNew<float, decimal>());
			Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(float), typeof(decimal)));

			Maps.NestedMappingContext = null;
			Assert.AreEqual(4m, await _mapper.MapAsync<decimal>(4f));
			Assert.AreEqual(4m, await _mapper.MapAsync(4f, typeof(float), typeof(decimal)));
			Assert.AreEqual(4m, await _mapper.MapAsync<float, decimal>(4f));
			VerifyMappingContext();

			using (var factory = _mapper.MapAsyncNewFactory<float, decimal>()) { 
				Assert.AreEqual(4m, await factory.Invoke(4f));
			}
		}

		[TestMethod]
		public async Task ShouldMapClasses() {
			Assert.IsTrue(_mapper.CanMapAsyncNew<float, PriceFloat>());
			Assert.IsTrue(_mapper.CanMapAsyncNew(typeof(float), typeof(PriceFloat)));

			Maps.NestedMappingContext = null;
			var result = await _mapper.MapAsync<PriceFloat>(4f);
			Assert.AreEqual(4f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
			Assert.AreEqual(4f, (await _mapper.MapAsync(4f, typeof(float), typeof(PriceFloat)) as PriceFloat)?.Amount);
			Assert.AreEqual(4f, (await _mapper.MapAsync<float, PriceFloat>(4f))?.Amount);
			VerifyMappingContext();

			using (var factory = _mapper.MapAsyncNewFactory<float, PriceFloat>()) {
				Assert.AreEqual(4f, (await factory.Invoke(4f))?.Amount);
			}
		}

		[TestMethod]
		public async Task ShouldRespectLengthIfSpecified() {
			var mapper = new AsyncTransitiveMapper(_mapper);

			// Map length is 5, so shorter
			Assert.IsFalse(mapper.CanMapAsyncNew<float, PriceFloat>(new TransitiveMappingOptions(4)));
			await Assert.ThrowsExceptionAsync<MapNotFoundException>(() => mapper.MapAsync<float, PriceFloat>(2f, new MappingOptions(new TransitiveMappingOptions(4))));

			// Length of 2 should invoke the map directly, if available
			Assert.IsTrue(mapper.CanMapAsyncNew<float, double>(new TransitiveMappingOptions(2)));

			// Length of 0, 1 should not map anything
			Assert.IsFalse(mapper.CanMapAsyncNew<float, double>(new TransitiveMappingOptions(0)));
			Assert.IsFalse(mapper.CanMapAsyncNew<float, double>(new TransitiveMappingOptions(1)));
		}
	}
}
