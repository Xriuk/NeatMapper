using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Transitive;

namespace NeatMapper.Transitive.Tests.DependencyInjection {
	[TestClass]
	public class IProjectorServiceCollectionExtensionsTests {
		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperTransitive();

			using(var services = serviceCollection.BuildServiceProvider()) { 
				var projector = services.GetRequiredService<IProjector>();
				var transitive = services.GetRequiredService<TransitiveProjector>();

				using (var scope = services.CreateScope()) {
					var projector2 = services.GetRequiredService<IProjector>();
					var transitive2 = services.GetRequiredService<TransitiveProjector>();

					Assert.AreSame(transitive, transitive2);
					Assert.AreSame(projector, projector2);

					var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();
					var transitive3 = services.GetRequiredService<TransitiveProjector>();

					Assert.AreSame(transitive2, transitive3);
					Assert.AreSame(projector2, projector3);
				}

				{
					var projector2 = services.GetRequiredService<IProjector>();
					var transitive2 = services.GetRequiredService<TransitiveProjector>();

					Assert.AreSame(transitive, transitive2);
					Assert.AreSame(projector, projector2);
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Scoped);
			serviceCollection.AddNeatMapperTransitive();

			using(var services = serviceCollection.BuildServiceProvider()) { 
				// Not throwing?
				//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IProjector>());
				//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IProjector>());

				using (var scope = services.CreateScope()) {
					var transitive2 = services.GetRequiredService<TransitiveProjector>();
					var projector2 = scope.ServiceProvider.GetRequiredService<IProjector>();

					var transitive3 = services.GetRequiredService<TransitiveProjector>();
					var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();

					Assert.AreSame(transitive2, transitive3);
					Assert.AreSame(projector2, projector3);
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Transient);
			serviceCollection.AddNeatMapperTransitive();

			using(var services = serviceCollection.BuildServiceProvider()) { 
				var projector = services.GetRequiredService<IProjector>();
				var transitive = services.GetRequiredService<TransitiveProjector>();

				using (var scope = services.CreateScope()) {
					var projector2 = services.GetRequiredService<IProjector>();
					var transitive2 = services.GetRequiredService<TransitiveProjector>();

					Assert.AreNotSame(transitive, transitive2);
					Assert.AreNotSame(projector, projector2);

					var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();
					var transitive3 = services.GetRequiredService<TransitiveProjector>();

					Assert.AreNotSame(transitive2, transitive3);
					Assert.AreNotSame(projector2, projector3);
				}

				{
					var projector2 = services.GetRequiredService<IProjector>();
					var transitive2 = services.GetRequiredService<TransitiveProjector>();

					Assert.AreNotSame(transitive, transitive2);
					Assert.AreNotSame(projector, projector2);
				}
			}
		}
	}
}
