using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.EntityFrameworkCore.Tests.DependencyInjection {
	[TestClass]
	public class IProjectorServiceCollectionExtensionsTests {
		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			using(var connection = new SqliteConnection("Filename=:memory:")) { 
				connection.Open();
				serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

				serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Singleton);
				serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

				using(var services = serviceCollection.BuildServiceProvider()) { 
					var projector = services.GetRequiredService<IProjector>();
					var efCore = services.GetRequiredService<EntityFrameworkCoreProjector>();

					using (var scope = services.CreateScope()) {
						var projector2 = services.GetRequiredService<IProjector>();
						var efCore2 = services.GetRequiredService<EntityFrameworkCoreProjector>();

						Assert.AreSame(efCore, efCore2);
						Assert.AreSame(projector, projector2);

						var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();
						var efCore3 = services.GetRequiredService<EntityFrameworkCoreProjector>();

						Assert.AreSame(efCore2, efCore3);
						Assert.AreSame(projector2, projector3);
					}

					{
						var projector2 = services.GetRequiredService<IProjector>();
						var efCore2 = services.GetRequiredService<EntityFrameworkCoreProjector>();

						Assert.AreSame(efCore, efCore2);
						Assert.AreSame(projector, projector2);
					}
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			using(var connection = new SqliteConnection("Filename=:memory:")) { 
				connection.Open();
				serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

				serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Scoped);
				serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

				using(var services = serviceCollection.BuildServiceProvider()) { 
					// Not throwing?
					//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IProjector>());
					//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IProjector>());

					using (var scope = services.CreateScope()) {
						var efCore2 = services.GetRequiredService<EntityFrameworkCoreProjector>();
						var projector2 = scope.ServiceProvider.GetRequiredService<IProjector>();

						var efCore3 = services.GetRequiredService<EntityFrameworkCoreProjector>();
						var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();

						Assert.AreSame(efCore2, efCore3);
						Assert.AreSame(projector2, projector3);
					}
				}
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			using(var connection = new SqliteConnection("Filename=:memory:")) { 
				connection.Open();
				serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

				serviceCollection.AddNeatMapper(projectorsLifetime: ServiceLifetime.Transient);
				serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

				using(var services = serviceCollection.BuildServiceProvider()) { 
					var projector = services.GetRequiredService<IProjector>();
					var efCore = services.GetRequiredService<EntityFrameworkCoreProjector>();

					using (var scope = services.CreateScope()) {
						var projector2 = services.GetRequiredService<IProjector>();
						var efCore2 = services.GetRequiredService<EntityFrameworkCoreProjector>();

						Assert.AreNotSame(efCore, efCore2);
						Assert.AreNotSame(projector, projector2);

						var projector3 = scope.ServiceProvider.GetRequiredService<IProjector>();
						var efCore3 = services.GetRequiredService<EntityFrameworkCoreProjector>();

						Assert.AreNotSame(efCore2, efCore3);
						Assert.AreNotSame(projector2, projector3);
					}

					{
						var projector2 = services.GetRequiredService<IProjector>();
						var efCore2 = services.GetRequiredService<EntityFrameworkCoreProjector>();

						Assert.AreNotSame(efCore, efCore2);
						Assert.AreNotSame(projector, projector2);
					}
				}
			}
		}
	}
}
