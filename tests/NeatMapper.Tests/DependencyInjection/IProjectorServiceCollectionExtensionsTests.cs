using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NeatMapper.Tests.DependencyInjection {
	[TestClass]
	public class IProjectorServiceCollectionExtensionsTests {
		public class Maps :
			IProjectionMap<string, int> {

			public static IProjector Projector;

			Expression<Func<string, int>> IProjectionMap<string, int>.Project(ProjectionContext context) {
				Projector = context.Projector.Projector;

				return source => source != null ? source.Length : 0;
			}
		}


		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Singleton);
			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var projector = services.GetRequiredService<IProjector>();

			using (var scope = services.CreateScope()) {
				var projector2 = services.GetRequiredService<IProjector>();

				Assert.AreSame(projector, projector2);

				var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();

				Assert.AreSame(projector2, projector3);
			}

			{
				var projector2 = services.GetRequiredService<IProjector>();

				Assert.AreSame(projector, projector2);
			}

			services.Dispose();
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Scoped);
			ServiceProvider services = serviceCollection.BuildServiceProvider();

			// Not throwing?
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IProjector>());

			using (var scope = services.CreateScope()) {
				var projector2 = scope.ServiceProvider.GetRequiredService<IProjector>();

				var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();

				Assert.AreSame(projector2, projector3);
			}

			services.Dispose();
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Transient);
			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var projector = services.GetRequiredService<IProjector>();

			using (var scope = services.CreateScope()) {
				var projector2 = services.GetRequiredService<IProjector>();

				Assert.AreNotSame(projector, projector2);

				var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();

				Assert.AreNotSame(projector2, projector3);
			}

			{
				var projector2 = services.GetRequiredService<IProjector>();

				Assert.AreNotSame(projector, projector2);
			}

			services.Dispose();
		}

		[TestMethod]
		public void ShouldUseCustomMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var projector = services.GetRequiredService<IProjector>();
			projector.Project<string, int>();
			projector.Project<IEnumerable<string>, List<int>>();
		}

		[TestMethod]
		public void ShouldUseAdditionalMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomProjectionAdditionalMapsOptions>(o => {
				o.AddMap<string, int>(c => s => s != null ? s.Length : 0);
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var projector = services.GetRequiredService<IProjector>();
			projector.Project<string, int>();
		}

		[TestMethod]
		public void NestedProjectorShouldBeEqualToIProjector() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var projector = services.GetRequiredService<IProjector>();

			// Normal
			Maps.Projector = null;
			projector.Project<string, int>();
			Assert.IsNotNull(Maps.Projector);
			Assert.AreSame(projector, Maps.Projector);

			// Collection
			Maps.Projector = null;
			projector.Project<IEnumerable<string>, List<int>>();
			Assert.IsNotNull(Maps.Projector);
			Assert.AreSame(projector, Maps.Projector);
		}
	}
}
