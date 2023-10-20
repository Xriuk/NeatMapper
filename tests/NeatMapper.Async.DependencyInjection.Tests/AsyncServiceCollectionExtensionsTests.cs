using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Async;
using NeatMapper.Configuration;
using System;
using System.Threading.Tasks;

namespace NeatMapper.Tests {
	[TestClass]
	public class ServiceCollectionExtensionsTests {
		public class Maps : IAsyncNewMap<string, int> {
			Task<int> IAsyncNewMap<string, int>.MapAsync(string source, AsyncMappingContext context) {
				return Task.FromResult(source?.Length ?? 0);
			}
		}


		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapperAsync(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var matcher = services.GetRequiredService<IMatcher>();
			var mapper = services.GetRequiredService<IAsyncMapper>();

			using (var scope = services.CreateScope()) {
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IAsyncMapper>();

				Assert.AreSame(matcher, matcher2);
				Assert.AreSame(mapper, mapper2);

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();

				Assert.AreSame(matcher2, matcher3);
				Assert.AreSame(mapper2, mapper3);
			}

			{
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IAsyncMapper>();

				Assert.AreSame(matcher, matcher2);
				Assert.AreSame(mapper, mapper2);
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapperAsync(ServiceLifetime.Scoped, ServiceLifetime.Scoped);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			// Not throwing?
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IAsyncMapper>());

			using (var scope = services.CreateScope()) {
				var matcher2 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper2 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();

				Assert.AreSame(matcher2, matcher3);
				Assert.AreSame(mapper2, mapper3);
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapperAsync(ServiceLifetime.Transient, ServiceLifetime.Transient);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var matcher = services.GetRequiredService<IMatcher>();
			var mapper = services.GetRequiredService<IAsyncMapper>();

			using (var scope = services.CreateScope()) {
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IAsyncMapper>();

				Assert.AreNotSame(matcher, matcher2);
				Assert.AreNotSame(mapper, mapper2);

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();

				Assert.AreNotSame(matcher2, matcher3);
				Assert.AreNotSame(mapper2, mapper3);
			}

			{
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IAsyncMapper>();

				Assert.AreNotSame(matcher, matcher2);
				Assert.AreNotSame(mapper, mapper2);
			}
		}

		[TestMethod]
		[DataRow(ServiceLifetime.Singleton, ServiceLifetime.Singleton, true)]
		[DataRow(ServiceLifetime.Singleton, ServiceLifetime.Scoped, false)]
		[DataRow(ServiceLifetime.Singleton, ServiceLifetime.Transient, false)]
		[DataRow(ServiceLifetime.Scoped, ServiceLifetime.Singleton, false)]
		[DataRow(ServiceLifetime.Scoped, ServiceLifetime.Scoped, true)]
		[DataRow(ServiceLifetime.Scoped, ServiceLifetime.Transient, false)]
		[DataRow(ServiceLifetime.Transient, ServiceLifetime.Singleton, false)]
		[DataRow(ServiceLifetime.Transient, ServiceLifetime.Scoped, false)]
		[DataRow(ServiceLifetime.Transient, ServiceLifetime.Transient, false)]
		public void ShouldReturnSameInstanceForMapperAndMatcherWhenPossible(ServiceLifetime mapperLifetime, ServiceLifetime matcherLifetime, bool shouldBeSameInstance) {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapperAsync(mapperLifetime, matcherLifetime);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			using (var scope = services.CreateScope()) {
				var mapper = scope.ServiceProvider.GetRequiredService<IAsyncMapper>();
				var matcher = scope.ServiceProvider.GetRequiredService<IMatcher>();
				if (shouldBeSameInstance)
					Assert.AreSame(mapper, matcher);
				else
					Assert.AreNotSame(mapper, matcher);
			}
		}

		[TestMethod]
		[DataRow(ServiceLifetime.Singleton, ServiceLifetime.Singleton)]
		[DataRow(ServiceLifetime.Singleton, ServiceLifetime.Scoped)]
		[DataRow(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
		[DataRow(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
		[DataRow(ServiceLifetime.Scoped, ServiceLifetime.Scoped)]
		[DataRow(ServiceLifetime.Scoped, ServiceLifetime.Transient)]
		[DataRow(ServiceLifetime.Transient, ServiceLifetime.Singleton)]
		[DataRow(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
		[DataRow(ServiceLifetime.Transient, ServiceLifetime.Transient)]
		public void ShouldRegisterMapperCorrectlyEvenIfMatcherAlreadyRegistered(ServiceLifetime matcherLifetime, ServiceLifetime mapperLifetime) {
			var serviceCollection = new ServiceCollection();
			serviceCollection.Add(new ServiceDescriptor(
				typeof(IMatcher),
				s => new AsyncMapper(s.GetRequiredService<IOptions<MapperConfigurationOptions>>().Value, s),
				matcherLifetime
			));
			serviceCollection.AddNeatMapperAsync(mapperLifetime);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			using (var scope = services.CreateScope()) {
				scope.ServiceProvider.GetRequiredService<IAsyncMapper>();
				scope.ServiceProvider.GetRequiredService<IMatcher>();
			}
		}

		[TestMethod]
		public Task ShouldUseMapperConfigurationOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapperAsync();
			serviceCollection.Configure<MapperConfigurationOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IAsyncMapper>();
			return mapper.MapAsync<string, int>("AAA");
		}

		[TestMethod]
		public Task ShouldUseMapperOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapperAsync();
			serviceCollection.Configure<AsyncMapperOptions>(o => {
				o.AddNewMap<string, int>((s, _) => Task.FromResult(s?.Length ?? 0));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IAsyncMapper>();
			return mapper.MapAsync<string, int>("AAA");
		}
	}
}
