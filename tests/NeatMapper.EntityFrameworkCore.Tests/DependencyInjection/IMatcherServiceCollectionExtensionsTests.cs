using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NeatMapper.EntityFrameworkCore.Tests.DependencyInjection {
	[TestClass]
	public class IMatcherServiceCollectionExtensionsTests {
		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddLogging();
			var connection = new SqliteConnection("Filename=:memory:");
			connection.Open();
			serviceCollection.AddDbContext<TestContext>(o => o.UseSqlite(connection), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

			serviceCollection.AddNeatMapper(matchersLifetime: ServiceLifetime.Singleton);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var matcher = services.GetRequiredService<IMatcher>();
			var efCore = services.GetRequiredService<EntityFrameworkCoreMatcher>();

			using (var scope = services.CreateScope()) {
				var matcher2 = services.GetRequiredService<IMatcher>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMatcher>();

				Assert.AreSame(efCore, efCore2);
				Assert.AreSame(matcher, matcher2);

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var efCore3 = services.GetRequiredService<EntityFrameworkCoreMatcher>();

				Assert.AreSame(efCore2, efCore3);
				Assert.AreSame(matcher2, matcher3);
			}

			{
				var matcher2 = services.GetRequiredService<IMatcher>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMatcher>();

				Assert.AreSame(efCore, efCore2);
				Assert.AreSame(matcher, matcher2);
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

			serviceCollection.AddNeatMapper(matchersLifetime: ServiceLifetime.Scoped);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

			ServiceProvider services = serviceCollection.BuildServiceProvider();

			// Not throwing?
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());

			using (var scope = services.CreateScope()) {
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMatcher>();
				var matcher2 = scope.ServiceProvider.GetRequiredService<IMatcher>();

				var efCore3 = services.GetRequiredService<EntityFrameworkCoreMatcher>();
				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();

				Assert.AreSame(efCore2, efCore3);
				Assert.AreSame(matcher2, matcher3);
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

			serviceCollection.AddNeatMapper(matchersLifetime: ServiceLifetime.Transient);
			serviceCollection.AddNeatMapperEntityFrameworkCore<TestContext>();

			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var matcher = services.GetRequiredService<IMatcher>();
			var efCore = services.GetRequiredService<EntityFrameworkCoreMatcher>();

			using (var scope = services.CreateScope()) {
				var matcher2 = services.GetRequiredService<IMatcher>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMatcher>();

				Assert.AreNotSame(efCore, efCore2);
				Assert.AreNotSame(matcher, matcher2);

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var efCore3 = services.GetRequiredService<EntityFrameworkCoreMatcher>();

				Assert.AreNotSame(efCore2, efCore3);
				Assert.AreNotSame(matcher2, matcher3);
			}

			{
				var matcher2 = services.GetRequiredService<IMatcher>();
				var efCore2 = services.GetRequiredService<EntityFrameworkCoreMatcher>();

				Assert.AreNotSame(efCore, efCore2);
				Assert.AreNotSame(matcher, matcher2);
			}

			connection.Dispose();
			services.Dispose();
		}
	}
}
