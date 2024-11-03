using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.Transitive.Tests.DependencyInjection {
	[TestClass]
	public class IMapperServiceCollectionExtensionsTests {
		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperTransitive();

			using (var services = serviceCollection.BuildServiceProvider()) { 
				var mapper = services.GetRequiredService<IMapper>();
				var transitive = services.GetRequiredService<TransitiveMapper>();

				using (var scope = services.CreateScope()) {
					var mapper2 = services.GetRequiredService<IMapper>();
					var transitive2 = services.GetRequiredService<TransitiveMapper>();

					Assert.AreSame(transitive, transitive2);
					Assert.AreSame(mapper, mapper2);

					var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();
					var transitive3 = services.GetRequiredService<TransitiveMapper>();

					Assert.AreSame(transitive2, transitive3);
					Assert.AreSame(mapper2, mapper3);
				}

				{
					var mapper2 = services.GetRequiredService<IMapper>();
					var transitive2 = services.GetRequiredService<TransitiveMapper>();

					Assert.AreSame(transitive, transitive2);
					Assert.AreSame(mapper, mapper2);
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Scoped);
			serviceCollection.AddNeatMapperTransitive();

			using(var services = serviceCollection.BuildServiceProvider()) { 
				// Not throwing?
				//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());
				//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMapper>());

				using (var scope = services.CreateScope()) {
					var transitive2 = services.GetRequiredService<TransitiveMapper>();
					var mapper2 = scope.ServiceProvider.GetRequiredService<IMapper>();

					var transitive3 = services.GetRequiredService<TransitiveMapper>();
					var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();

					Assert.AreSame(transitive2, transitive3);
					Assert.AreSame(mapper2, mapper3);
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Transient);
			serviceCollection.AddNeatMapperTransitive();

			using (var services = serviceCollection.BuildServiceProvider()) { 
				var mapper = services.GetRequiredService<IMapper>();
				var transitive = services.GetRequiredService<TransitiveMapper>();

				using (var scope = services.CreateScope()) {
					var mapper2 = services.GetRequiredService<IMapper>();
					var transitive2 = services.GetRequiredService<TransitiveMapper>();

					Assert.AreNotSame(transitive, transitive2);
					Assert.AreNotSame(mapper, mapper2);

					var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();
					var transitive3 = services.GetRequiredService<TransitiveMapper>();

					Assert.AreNotSame(transitive2, transitive3);
					Assert.AreNotSame(mapper2, mapper3);
				}

				{
					var mapper2 = services.GetRequiredService<IMapper>();
					var transitive2 = services.GetRequiredService<TransitiveMapper>();

					Assert.AreNotSame(transitive, transitive2);
					Assert.AreNotSame(mapper, mapper2);
				}
			}
		}
	}
}
