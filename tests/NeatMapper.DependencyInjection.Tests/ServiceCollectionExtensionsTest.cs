using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;
using System;

namespace NeatMapper.Tests {
	[TestClass]
	public class ServiceCollectionExtensionsTest {
		public class Maps : INewMap<string, int> {
			int INewMap<string, int>.Map(string source, MappingContext context) {
				return source?.Length ?? 0;
			}
		}


		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var matcher = services.GetRequiredService<IMatcher>();
			var mapper = services.GetRequiredService<IMapper>();

			using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IMapper>();

				Assert.AreSame(matcher, matcher2);
				Assert.AreSame(mapper, mapper2);

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();

				Assert.AreSame(matcher2, matcher3);
				Assert.AreSame(mapper2, mapper3);
			}

			{
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IMapper>();

				Assert.AreSame(matcher, matcher2);
				Assert.AreSame(mapper, mapper2);
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(ServiceLifetime.Scoped, ServiceLifetime.Scoped);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			// Not throwing?
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMatcher>());
			//Assert.ThrowsException<InvalidOperationException>(() => services.GetRequiredService<IMapper>());

			using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				var matcher2 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper2 = scope.ServiceProvider.GetRequiredService<IMapper>();

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();

				Assert.AreSame(matcher2, matcher3);
				Assert.AreSame(mapper2, mapper3);
			}
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(ServiceLifetime.Transient, ServiceLifetime.Transient);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var matcher = services.GetRequiredService<IMatcher>();
			var mapper = services.GetRequiredService<IMapper>();

			using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IMapper>();

				Assert.AreNotSame(matcher, matcher2);
				Assert.AreNotSame(mapper, mapper2);

				var matcher3 = scope.ServiceProvider.GetRequiredService<IMatcher>();
				var mapper3 = scope.ServiceProvider.GetRequiredService<IMapper>();

				Assert.AreNotSame(matcher2, matcher3);
				Assert.AreNotSame(mapper2, mapper3);
			}

			{
				var matcher2 = services.GetRequiredService<IMatcher>();
				var mapper2 = services.GetRequiredService<IMapper>();

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
			serviceCollection.AddNeatMapper(mapperLifetime, matcherLifetime);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			using(var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
				var matcher = scope.ServiceProvider.GetRequiredService<IMatcher>();
				if(shouldBeSameInstance)
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
				s => new Mapper(s.GetRequiredService<IOptions<MapperConfigurationOptions>>().Value, s),
				matcherLifetime
			));
			serviceCollection.AddNeatMapper(mapperLifetime);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
				scope.ServiceProvider.GetRequiredService<IMapper>();
				scope.ServiceProvider.GetRequiredService<IMatcher>();
			}
		}

		[TestMethod]
		public void ShouldUseMapperConfigurationOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<MapperConfigurationOptions>(o => {
				o.ScanTypes.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			mapper.Map<string, int>("AAA");
		}

		[TestMethod]
		public void ShouldUseMapperOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<MapperOptions>(o => {
				o.AddNewMap<string, int>((s, _) => s?.Length ?? 0);
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			mapper.Map<string, int>("AAA");
		}
	}
}
