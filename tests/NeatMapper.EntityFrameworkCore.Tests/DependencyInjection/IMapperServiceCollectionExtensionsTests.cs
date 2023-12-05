using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.EntityFrameworkCore.Tests.DependencyInjection {
	[TestClass]
	public class IMapperServiceCollectionExtensionsTests {
		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			var efCore = services.GetRequiredService<EntityFrameworkCoreMapper>();

			using (var scope = services.CreateScope()) {
				var mapper2 = services.GetRequiredService<IMapper>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMapper>();

				Assert.AreSame(efCore, efCore2);
				Assert.AreSame(mapper, mapper2);

				var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();
				var efCore3 = services.GetRequiredService<EntityFrameworkCoreMapper>();

				Assert.AreSame(efCore2, efCore3);
				Assert.AreSame(mapper2, mapper3);
			}

			{
				var mapper2 = services.GetRequiredService<IMapper>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMapper>();

				Assert.AreSame(efCore, efCore2);
				Assert.AreSame(mapper, mapper2);
			}

			connection.Dispose();
			services.Dispose();
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Scoped);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

			ServiceProvider services = serviceCollection.BuildServiceProvider();

			// Not throwing?
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMapper>());

			using (var scope = services.CreateScope()) {
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMapper>();
				var mapper2 = scope.ServiceProvider.GetRequiredService<IMapper>();

				var efCore3 = services.GetRequiredService<EntityFrameworkCoreMapper>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();

				Assert.AreSame(efCore2, efCore3);
				Assert.AreSame(mapper2, mapper3);
			}

			connection.Dispose();
			services.Dispose();
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Transient);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			var efCore = services.GetRequiredService<EntityFrameworkCoreMapper>();

			using (var scope = services.CreateScope()) {
				var mapper2 = services.GetRequiredService<IMapper>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMapper>();

				Assert.AreNotSame(efCore, efCore2);
				Assert.AreNotSame(mapper, mapper2);

				var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();
				var efCore3 = services.GetRequiredService<EntityFrameworkCoreMapper>();

				Assert.AreNotSame(efCore2, efCore3);
				Assert.AreNotSame(mapper2, mapper3);
			}

			{
				var mapper2 = services.GetRequiredService<IMapper>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMapper>();

				Assert.AreNotSame(efCore, efCore2);
				Assert.AreNotSame(mapper, mapper2);
			}

			connection.Dispose();
			services.Dispose();
		}
	}
}
