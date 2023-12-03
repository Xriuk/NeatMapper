using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeatMapper.Tests.DependencyInjection {
	[TestClass]
	public class IAsyncMapperServiceCollectionExtensionsTests {
		public class Maps :
			IAsyncNewMap<string, int>,
			IAsyncMergeMap<string, float> {

			public static IAsyncMapper Mapper;

			Task<int> IAsyncNewMap<string, int>.MapAsync(string source, AsyncMappingContext context) {
				Mapper = context.Mapper;
				return Task.FromResult(source?.Length ?? 0);
			}

			Task<float> IAsyncMergeMap<string, float>.MapAsync(string source, float destination, AsyncMappingContext context) {
				Mapper = context.Mapper;
				return Task.FromResult((float)(source?.Length ?? 0));
			}
		}


		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(asyncMappersLifetime: ServiceLifetime.Singleton, matchersLifetime: ServiceLifetime.Singleton);
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
			serviceCollection.AddNeatMapper(asyncMappersLifetime: ServiceLifetime.Scoped, matchersLifetime: ServiceLifetime.Scoped);
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
			serviceCollection.AddNeatMapper(asyncMappersLifetime: ServiceLifetime.Transient, matchersLifetime: ServiceLifetime.Transient);
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
		public async Task ShouldUseCustomMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IAsyncMapper>();
			await mapper.MapAsync<string, int>("AAA");
			await mapper.MapAsync<IEnumerable<string>, List<int>>(new[] { "AAA" });
		}

		[TestMethod]
		public async Task ShouldUseAdditionalMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomAsyncNewAdditionalMapsOptions>(o => {
				o.AddMap<string, int>((s, _) => Task.FromResult(s?.Length ?? 0));
			});
			serviceCollection.Configure<CustomAsyncMergeAdditionalMapsOptions>(o => {
				o.AddMap<string, float>((s, d, _) => Task.FromResult((float)(s?.Length ?? 0)));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IAsyncMapper>();
			await mapper.MapAsync<string, int>("AAA");
			await mapper.MapAsync<string, float>("BBB", 42f);
		}

		[TestMethod]
		public async Task NestedMapperShouldBeEqualToIAsyncMapper() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IAsyncMapper>();

			// NewMap
			{
				// Normal
				Maps.Mapper = null;
				await mapper.MapAsync<string, int>("AAA");
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);

				// Collection
				Maps.Mapper = null;
				await mapper.MapAsync<IEnumerable<string>, List<int>>(new[] { "AAA" });
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);
			}

			// MergeMap
			{
				// Normal
				Maps.Mapper = null;
				await mapper.MapAsync<string, float>("AAA", 2f);
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);

				// Collection
				Maps.Mapper = null;
				await mapper.MapAsync<IEnumerable<string>, List<float>>(new[] { "AAA" }, new List<float>());
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);
			}
		}
	}
}
