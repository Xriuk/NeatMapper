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
				new MergeMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new TransitiveNewMapper(EmptyMapper.Instance),
				new TransitiveMergeMapper(EmptyMapper.Instance));
		}


		[TestMethod]
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapMerge<float, decimal>());
			Assert.IsTrue(_mapper.CanMapMerge(typeof(float), typeof(decimal)));

			Maps.NestedMappingContext = null;
			Assert.AreEqual(4m, _mapper.Map(4f, 0m));
			Assert.AreEqual(4m, _mapper.Map(4f, typeof(float), 0m, typeof(decimal)));
			Assert.IsNotNull(Maps.NestedMappingContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentMapper, typeof(TransitiveMergeMapper));

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
			Assert.IsNotNull(Maps.NestedMappingContext);
			Assert.IsInstanceOfType(Maps.NestedMappingContext.ParentMapper, typeof(TransitiveNewMapper));

			using (var factory = _mapper.MapMergeFactory<float, PriceFloat>()) {
				Assert.AreEqual(4f, factory.Invoke(4f, new PriceFloat())?.Amount);
			}
		}

		[TestMethod]
		public void ShouldReturnPreviewIfCanMap() {
			var result = _mapper.MapMergePreview<float, decimal>()?.Single();
			Assert.IsNotNull(result);
			Assert.AreEqual(3, result.Count);
			Assert.AreSame(typeof(float), result[0]);
			Assert.AreSame(typeof(double), result[1]);
			Assert.AreSame(typeof(decimal), result[2]);

			result = _mapper.MapMergePreview<float, PriceFloat>()?.Single();
			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.Count);
			Assert.AreSame(typeof(float), result[0]);
			Assert.AreSame(typeof(double), result[1]);
			Assert.AreSame(typeof(decimal), result[2]);
			Assert.AreSame(typeof(Price), result[3]);
			Assert.AreSame(typeof(PriceFloat), result[4]);

			// Length of 5 should be good
			Assert.IsNotNull(_mapper.MapMergePreview<float, PriceFloat>(new TransitiveMappingOptions(5)));

			result = _mapper.MapMergePreview<float, double>()?.Single();
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.AreSame(typeof(float), result[0]);
			Assert.AreSame(typeof(double), result[1]);
		}

		[TestMethod]
		public void ShouldReturnMultiplePreviewIfCanMap() {
			var result = _mapper.MapMergePreview<string, Price>(4);
			Assert.IsNotNull(result);
			// string > decimal > Price
			// string > decimal > int > Price
			Assert.AreEqual(2, result.Count());
			Assert.IsTrue(result.Any(r => r.Count == 3 && r[0] == typeof(string) && r[1] == typeof(decimal) && r[2] == typeof(Price)));
			Assert.IsTrue(result.Any(r => r.Count == 4 && r[0] == typeof(string) && r[1] == typeof(decimal) && r[2] == typeof(int) && r[3] == typeof(Price)));
		}

		[TestMethod]
		public void ShouldReturnNullPreviewIfCannotMap() {
			Assert.IsNull(_mapper.MapMergePreview<decimal, float>());
		}

		[TestMethod]
		public void ShouldRespectLengthIfSpecified() {
			// Map length is 5, so shorter
			Assert.IsFalse(_mapper.CanMapMerge<float, PriceFloat>(new TransitiveMappingOptions(4)));
			Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<float, PriceFloat>(2f, new PriceFloat(), new MappingOptions(new TransitiveMappingOptions(4))));
			Assert.IsNull(_mapper.MapMergePreview<float, PriceFloat>(new TransitiveMappingOptions(4)));

			// Should return only a single result since the other is long 4
			var result = _mapper.MapMergePreview<string, Price>(4, new TransitiveMappingOptions(3));
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count());
			Assert.AreSame(typeof(string), result.Single()[0]);
			Assert.AreSame(typeof(decimal), result.Single()[1]);
			Assert.AreSame(typeof(Price), result.Single()[2]);

			// Length of 2 should invoke the map directly, if available
			Assert.IsTrue(_mapper.CanMapMerge<float, double>(new TransitiveMappingOptions(2)));

			// Length of 0, 1 should not map anything
			Assert.IsFalse(_mapper.CanMapMerge<float, double>(new TransitiveMappingOptions(0)));
			Assert.IsFalse(_mapper.CanMapMerge<float, double>(new TransitiveMappingOptions(1)));
		}
	}
}
