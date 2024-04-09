using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class QueryableExtensionsTests {
		public class Maps :
			IProjectionMap<string, int>{

			public static int maps = 0;

			Expression<Func<string, int>> IProjectionMap<string, int>.Project(ProjectionContext context) {
				return source => source != null ? source.Length : 0;
			}
		}


		IProjector _projector;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => o.TypesToScan.Add(typeof(Maps)));
			using(var services = serviceCollection.BuildServiceProvider()) { 
				_projector = services.GetRequiredService<IProjector>();
			}
		}

		[TestMethod]
		public void ShouldProject() {
			var mappeQueryable = new[] { "AAA", "BB", "C" }.AsQueryable().Project<int>(_projector);
			var mappedResult = mappeQueryable.Take(2).ToArray();

			Assert.AreEqual(2, mappedResult.Length);
			Assert.AreEqual(3, mappedResult[0]);
			Assert.AreEqual(2, mappedResult[1]);
		}
	}
}
