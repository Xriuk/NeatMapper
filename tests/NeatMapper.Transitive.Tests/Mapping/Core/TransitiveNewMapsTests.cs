using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;

namespace NeatMapper.Transitive.Tests.Mapping {
	[TestClass]
	public class TransitiveNewMapsTests {
		public class Maps :
			INewMap<float, double>,
			INewMap<double, decimal>,
			INewMap<decimal, Price>,
			INewMap<Price, PriceFloat> {

			public static NestedMappingContext NestedMappingContext;

			double INewMap<float, double>.Map(float source, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				return (double)source;
			}

			decimal INewMap<double, decimal>.Map(double source, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				return (decimal)source;
			}

			Price INewMap<decimal, Price>.Map(decimal source, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				return new Price {
					Amount = source,
					Currency = "EUR"
				};
			}

			PriceFloat INewMap<Price, PriceFloat>.Map(Price source, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				if (source == null)
					return null;
				else {
					return new PriceFloat {
						Amount = (float)source.Amount,
						Currency = source.Currency
					};
				}
			}
		}


		private IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new CompositeMapper(
				new CustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new TransitiveMapper(EmptyMapper.Instance));
		}

		private void VerifyMappingContext() {
			Assert.IsNotNull(Maps.NestedMappingContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentMapper, typeof(CompositeMapper));
			Assert.IsNotNull(Maps.NestedMappingContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentContext.ParentMapper, typeof(TransitiveMapper));
			Assert.IsNotNull(Maps.NestedMappingContext.ParentContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentContext.ParentContext.ParentMapper, typeof(CompositeMapper));
			Assert.IsNull(Maps.NestedMappingContext.ParentContext.ParentContext.ParentContext);
		}


		[TestMethod]
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapNew<float, decimal>());
			Assert.IsTrue(_mapper.CanMapNew(typeof(float), typeof(decimal)));

			Maps.NestedMappingContext = null;
			Assert.AreEqual(4m, _mapper.Map<decimal>(4f));
			Assert.AreEqual(4m, _mapper.Map(4f, typeof(float), typeof(decimal)));
			Assert.AreEqual(4m, _mapper.Map<float, decimal>(4f));
			VerifyMappingContext();

			using (var factory = _mapper.MapNewFactory<float, decimal>()) { 
				Assert.AreEqual(4m, factory.Invoke(4f));
			}
		}

		[TestMethod]
		public void ShouldMapClasses() {
			Assert.IsTrue(_mapper.CanMapNew<float, PriceFloat>());
			Assert.IsTrue(_mapper.CanMapNew(typeof(float), typeof(PriceFloat)));

			Maps.NestedMappingContext = null;
			var result = _mapper.Map<PriceFloat>(4f);
			Assert.AreEqual(4f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
			Assert.AreEqual(4f, (_mapper.Map(4f, typeof(float), typeof(PriceFloat)) as PriceFloat)?.Amount);
			Assert.AreEqual(4f, _mapper.Map<float, PriceFloat>(4f)?.Amount);
			VerifyMappingContext();

			using (var factory = _mapper.MapNewFactory<float, PriceFloat>()) {
				Assert.AreEqual(4f, factory.Invoke(4f)?.Amount);
			}
		}

		[TestMethod]
		public void ShouldRespectLengthIfSpecified() {
			var mapper = new TransitiveMapper(_mapper);

			// Map length is 5, so shorter
			Assert.IsFalse(mapper.CanMapNew<float, PriceFloat>(new TransitiveMappingOptions(4)));
			Assert.ThrowsException<MapNotFoundException>(() => mapper.Map<float, PriceFloat>(2f, new MappingOptions(new TransitiveMappingOptions(4))));

			// Length of 2 should invoke the map directly, if available
			Assert.IsTrue(mapper.CanMapNew<float, double>(new TransitiveMappingOptions(2)));

			// Length of 0, 1 should not map anything
			Assert.IsFalse(mapper.CanMapNew<float, double>(new TransitiveMappingOptions(0)));
			Assert.IsFalse(mapper.CanMapNew<float, double>(new TransitiveMappingOptions(1)));
		}
	}
}
