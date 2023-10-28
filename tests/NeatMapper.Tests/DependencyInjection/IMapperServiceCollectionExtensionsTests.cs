using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.DependencyInjection {
	[TestClass]
	public class IMapperServiceCollectionExtensionsTests {
		public class Maps :
			INewMap<string, int>,
			IMergeMap<string, float> {

			public static IMapper Mapper;

			int INewMap<string, int>.Map(string source, MappingContext context) {
				Mapper = context.Mapper;
				return source?.Length ?? 0;
			}

			float IMergeMap<string, float>.Map(string source, float destination, MappingContext context) {
				Mapper = context.Mapper;
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

			using (var scope = services.CreateScope()) {
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

			using (var scope = services.CreateScope()) {
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

			using (var scope = services.CreateScope()) {
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
				s => new Matcher(),
				matcherLifetime
			));
			serviceCollection.AddNeatMapper(mapperLifetime);
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			using (var scope = services.CreateScope()) {
				scope.ServiceProvider.GetRequiredService<IMapper>();
				scope.ServiceProvider.GetRequiredService<IMatcher>();
			}
		}

		[TestMethod]
		public void ShouldUseCustomMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			mapper.Map<string, int>("AAA");
			mapper.Map<IEnumerable<string>, List<int>>(new[] { "AAA" });
		}

		[TestMethod]
		public void ShouldUseAdditionalMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomNewAdditionalMapsOptions>(o => {
				o.AddMap<string, int>((s, _) => s?.Length ?? 0);
			});
			serviceCollection.Configure<CustomMergeAdditionalMapsOptions>(o => {
				o.AddMap<string, float>((s, d, _) => s?.Length ?? 0);
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			mapper.Map<string, int>("AAA");
			mapper.Map<string, float>("BBB", 42f);
		}

		[TestMethod]
		public void NestedMapperShouldBeEqualToIMapper() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(ServiceLifetime.Singleton, ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			IServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();

			// NewMap
			{
				// Normal
				Maps.Mapper = null;
				mapper.Map<string, int>("AAA");
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);

				// Collection
				Maps.Mapper = null;
				mapper.Map<IEnumerable<string>, List<int>>(new[] { "AAA" });
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);
			}

			// MergeMap
			{
				// Normal
				Maps.Mapper = null;
				mapper.Map<string, float>("AAA", 2f);
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);

				// Collection
				Maps.Mapper = null;
				mapper.Map<IEnumerable<string>, List<float>>(new[] { "AAA" }, new List<float>());
				Assert.IsNotNull(Maps.Mapper);
				Assert.AreSame(mapper, Maps.Mapper);
			}
		}
	}
}
