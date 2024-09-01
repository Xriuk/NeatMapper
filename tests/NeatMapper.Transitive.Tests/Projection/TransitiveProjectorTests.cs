using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests;
using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Globalization;

namespace NeatMapper.Transitive.Tests.Projection {
	[TestClass]
	public class TransitiveProjectorTests {
		public class Maps :
			IProjectionMap<float, string>,
			IProjectionMap<string, decimal>,
			IProjectionMap<decimal, Price>,
			IProjectionMap<Price, PriceFloat> {

			public static NestedProjectionContext NestedProjectionContext;

			Expression<Func<float, string>> IProjectionMap<float, string>.Project(ProjectionContext context) {
				NestedProjectionContext = context.MappingOptions.GetOptions<NestedProjectionContext>();
				return source => source.ToString(CultureInfo.InvariantCulture);
			}

			Expression<Func<string, decimal>> IProjectionMap<string, decimal>.Project(ProjectionContext context) {
				NestedProjectionContext = context.MappingOptions.GetOptions<NestedProjectionContext>();
				return source => decimal.Parse(source);
			}

			Expression<Func<decimal, Price>> IProjectionMap<decimal, Price>.Project(ProjectionContext context) {
				NestedProjectionContext = context.MappingOptions.GetOptions<NestedProjectionContext>();
				return source => new Price {
					Amount = source,
					Currency = "EUR"
				};
			}

			Expression<Func<Price, PriceFloat>> IProjectionMap<Price, PriceFloat>.Project(ProjectionContext context) {
				NestedProjectionContext = context.MappingOptions.GetOptions<NestedProjectionContext>();
				return source => source == null ? null : new PriceFloat {
					Amount = (float)source.Amount,
					Currency = source.Currency
				};
			}
		}


		private IProjector _projector = null;

		[TestInitialize]
		public void Initialize() {
			_projector = new CompositeProjector(
				new CustomProjector(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(Maps) }
				}),
				new TransitiveProjector(EmptyProjector.Instance));
		}

		private void VerifyProjectionContext() {
			Assert.IsNotNull(Maps.NestedProjectionContext);
			Assert.IsInstanceOfType(Maps.NestedProjectionContext.ParentProjector, typeof(CompositeProjector));
			Assert.IsNotNull(Maps.NestedProjectionContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedProjectionContext.ParentContext.ParentProjector, typeof(TransitiveProjector));
			Assert.IsNotNull(Maps.NestedProjectionContext.ParentContext.ParentContext);
			Assert.IsInstanceOfType(Maps.NestedProjectionContext.ParentContext.ParentContext.ParentProjector, typeof(CompositeProjector));
			Assert.IsNull(Maps.NestedProjectionContext.ParentContext.ParentContext.ParentContext);
		}


		[TestMethod]
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_projector.CanProject<float, decimal>());
			Assert.IsTrue(_projector.CanProject(typeof(float), typeof(decimal)));

			Maps.NestedProjectionContext = null;
			Expression<Func<float, decimal>> expr = source => decimal.Parse(source.ToString(CultureInfo.InvariantCulture));
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<float, decimal>());
			VerifyProjectionContext();
		}

		[TestMethod]
		public void ShouldMapClasses() {
			Assert.IsTrue(_projector.CanProject<float, PriceFloat>());
			Assert.IsTrue(_projector.CanProject(typeof(float), typeof(PriceFloat)));

			// Horrible to look at but this is what will happen
			Maps.NestedProjectionContext = null;
			Expression<Func<float, PriceFloat>> expr = source => new Price {
				Amount = decimal.Parse(source.ToString(CultureInfo.InvariantCulture)),
				Currency = "EUR"
			} == null ? null : new PriceFloat{
				Amount = (float)new Price {
					Amount = decimal.Parse(source.ToString(CultureInfo.InvariantCulture)),
					Currency = "EUR"
				}.Amount,
				Currency = new Price {
					Amount = decimal.Parse(source.ToString(CultureInfo.InvariantCulture)),
					Currency = "EUR"
				}.Currency
			};
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<float, PriceFloat>());
			VerifyProjectionContext();
		}

		[TestMethod]
		public void ShouldRespectLengthIfSpecified() {
			var projector = new TransitiveProjector(_projector);

			// Map length is 5, so shorter
			Assert.IsFalse(projector.CanProject<float, PriceFloat>(new TransitiveMappingOptions(4)));
			Assert.ThrowsException<MapNotFoundException>(() => projector.Project<float, PriceFloat>(new MappingOptions(new TransitiveMappingOptions(4))));

			// Length of 2 should invoke the map directly, if available
			Assert.IsTrue(projector.CanProject<float, string>(new TransitiveMappingOptions(2)));

			// Length of 0, 1 should not map anything
			Assert.IsFalse(projector.CanProject<float, string>(new TransitiveMappingOptions(0)));
			Assert.IsFalse(projector.CanProject<float, string>(new TransitiveMappingOptions(1)));
		}
	}
}
