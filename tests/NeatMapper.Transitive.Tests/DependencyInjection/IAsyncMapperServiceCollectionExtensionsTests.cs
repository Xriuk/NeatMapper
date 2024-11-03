using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Transitive;

namespace NeatMapper.Transitive.Tests.DependencyInjection {
	[TestClass]
	public class IAsyncMapperServiceCollectionExtensionsTests {
		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(asyncMappersLifetime: ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperTransitive();

			using(var services = serviceCollection.BuildServiceProvider()) { 
				var mapper = services.GetRequiredService<IAsyncMapper>();
				var transitive = services.GetRequiredService<AsyncTransitiveMapper>();

				using (var scope = services.CreateScope()) {
					var mapper2 = services.GetRequiredService<IAsyncMapper>();
					var transitive2 = services.GetRequiredService<AsyncTransitiveMapper>();

					Assert.AreSame(transitive, transitive2);
					Assert.AreSame(mapper, mapper2);

					var mapper3 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();
					var transitive3 = services.GetRequiredService<AsyncTransitiveMapper>();

					Assert.AreSame(transitive2, transitive3);
					Assert.AreSame(mapper2, mapper3);
				}

				{
					var mapper2 = services.GetRequiredService<IAsyncMapper>();
					var transitive2 = services.GetRequiredService<AsyncTransitiveMapper>();

					Assert.AreSame(transitive, transitive2);
					Assert.AreSame(mapper, mapper2);
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(asyncMappersLifetime: ServiceLifetime.Scoped);
			serviceCollection.AddNeatMapperTransitive();

			using(var services = serviceCollection.BuildServiceProvider()) { 
				// Not throwing?
				//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());
				//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IAsyncMapper>());

				using (var scope = services.CreateScope()) {
					var transitive2 = services.GetRequiredService<AsyncTransitiveMapper>();
					var mapper2 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();

					var transitive3 = services.GetRequiredService<AsyncTransitiveMapper>();
					var mapper3 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();

					Assert.AreSame(transitive2, transitive3);
					Assert.AreSame(mapper2, mapper3);
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(asyncMappersLifetime: ServiceLifetime.Transient);
			serviceCollection.AddNeatMapperTransitive();

			using (var services = serviceCollection.BuildServiceProvider()) { 
				var mapper = services.GetRequiredService<IAsyncMapper>();
				var transitive = services.GetRequiredService<AsyncTransitiveMapper>();

				using (var scope = services.CreateScope()) {
					var mapper2 = services.GetRequiredService<IAsyncMapper>();
					var transitive2 = services.GetRequiredService<AsyncTransitiveMapper>();

					Assert.AreNotSame(transitive, transitive2);
					Assert.AreNotSame(mapper, mapper2);

					var mapper3 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();
					var transitive3 = services.GetRequiredService<AsyncTransitiveMapper>();

					Assert.AreNotSame(transitive2, transitive3);
					Assert.AreNotSame(mapper2, mapper3);
				}

				{
					var mapper2 = services.GetRequiredService<IAsyncMapper>();
					var transitive2 = services.GetRequiredService<AsyncTransitiveMapper>();

					Assert.AreNotSame(transitive, transitive2);
					Assert.AreNotSame(mapper, mapper2);
				}
			}
		}
	}
}
