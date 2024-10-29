using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace NeatMapper.Transitive.Tests.Mapping {
	[TestClass]
	public class TransitiveMergeMapperTests {
		// Using all IMergeMaps because they can be used in new maps too
		public class Maps :
			IMergeMap<float, double>,
			IMergeMap<double, decimal>,
			IMergeMap<decimal, Price>,
			IMergeMap<Price, PriceFloat>,

			// string > decimal > Price
			IMergeMap<string, decimal>,

			// string > decimal > int > Price
			IMergeMap<decimal, int>,
			IMergeMap<int, Price> {

			public static NestedMappingContext NestedMappingContext;

			double IMergeMap<float, double>.Map(float source, double destination, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				return (double)source;
			}

			decimal IMergeMap<double, decimal>.Map(double source, decimal destination, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				return (decimal)source;
			}

			Price IMergeMap<decimal, Price>.Map(decimal source, Price destination, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				if (destination == null)
					destination = new Price();
				destination.Amount = source;
				destination.Currency = "EUR";

				return destination;
			}

			PriceFloat IMergeMap<Price, PriceFloat>.Map(Price source, PriceFloat destination, MappingContext context) {
				NestedMappingContext = context.MappingOptions.GetOptions<NestedMappingContext>();
				if (source == null)
					return null;

				if(destination == null)
					destination = new PriceFloat();
				destination.Amount = (float)source.Amount;
				destination.Currency = source.Currency;

				return destination;
			}


			decimal IMergeMap<string, decimal>.Map(string source, decimal destination, MappingContext context) {
				return decimal.Parse(source);
			}

			int IMergeMap<decimal, int>.Map(decimal source, int destination, MappingContext context) {
				return (int)source;
			}

			Price IMergeMap<int, Price>.Map(int source, Price destination, MappingContext context) {
				if(destination == null)
					destination = new Price();
				destination.Amount = source;
				destination.Currency = "EUR";

				return destination;
			}
		}


		private IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new CompositeMapper(
				new CustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new TransitiveNewMapper(EmptyMapper.Instance),
				new TransitiveMergeMapper(EmptyMapper.Instance));
		}

		private void VerifyMappingContext() {
			Assert.IsNotNull(Maps.NestedMappingContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentMapper, typeof(CompositeMapper));
			Assert.IsNotNull(Maps.NestedMappingContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentContext.ParentMapper, typeof(TransitiveMergeMapper));
			Assert.IsNotNull(Maps.NestedMappingContext.ParentContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentContext.ParentContext.ParentMapper, typeof(CompositeMapper));
			Assert.IsNull(Maps.NestedMappingContext.ParentContext.ParentContext.ParentContext);
		}


		[TestMethod]
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapMerge<float, decimal>());
			Assert.IsTrue(_mapper.CanMapMerge(typeof(float), typeof(decimal)));

			Maps.NestedMappingContext = null;
			Assert.AreEqual(4m, _mapper.Map(4f, 0m));
			Assert.AreEqual(4m, _mapper.Map(4f, typeof(float), 0m, typeof(decimal)));
			VerifyMappingContext();

			using (var factory = _mapper.MapMergeFactory<float, decimal>()) { 
				Assert.AreEqual(4m, factory.Invoke(4f, 0m));
			}
		}

		[TestMethod]
		public void ShouldMapClasses() {
			Assert.IsTrue(_mapper.CanMapMerge<float, PriceFloat>());
			Assert.IsTrue(_mapper.CanMapMerge(typeof(float), typeof(PriceFloat)));

			Maps.NestedMappingContext = null;
			var result = _mapper.Map(4f, new PriceFloat());
			Assert.AreEqual(4f, result.Amount);
			Assert.AreEqual("EUR", result.Currency);
			Assert.AreEqual(4f, (_mapper.Map(4f, typeof(float), new PriceFloat(), typeof(PriceFloat)) as PriceFloat)?.Amount);
			VerifyMappingContext();

			using (var factory = _mapper.MapMergeFactory<float, PriceFloat>()) {
				Assert.AreEqual(4f, factory.Invoke(4f, new PriceFloat())?.Amount);
			}
		}

		[TestMethod]
		public void ShouldRespectLengthIfSpecified() {
			var mapper = new TransitiveMergeMapper(_mapper);

			// Map length is 5, so shorter
			Assert.IsFalse(mapper.CanMapMerge<float, PriceFloat>(new TransitiveMappingOptions(4)));
			Assert.ThrowsException<MapNotFoundException>(() => mapper.Map<float, PriceFloat>(2f, new PriceFloat(), new MappingOptions(new TransitiveMappingOptions(4))));

			// Length of 2 should invoke the map directly, if available
			Assert.IsTrue(mapper.CanMapMerge<float, double>(new TransitiveMappingOptions(2)));

			// Length of 0, 1 should not map anything
			Assert.IsFalse(mapper.CanMapMerge<float, double>(new TransitiveMappingOptions(0)));
			Assert.IsFalse(mapper.CanMapMerge<float, double>(new TransitiveMappingOptions(1)));
		}
	}
}
