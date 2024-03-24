using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeatMapper.Tests.DependencyInjection {
	[TestClass]
	public class IMapperServiceCollectionExtensionsTests {
		public class Maps :
			INewMap<string, int>,
			IMergeMap<string, float> {

			public static IMapper Mapper;

			int INewMap<string, int>.Map(string source, MappingContext context) {
				Mapper = (IMapper)context.Mapper.GetType().GetField("_mapper", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context.Mapper);
				return source?.Length ?? 0;
			}

			float IMergeMap<string, float>.Map(string source, float destination, MappingContext context) {
				Mapper = (IMapper)context.Mapper.GetType().GetField("_mapper", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(context.Mapper);
				return source?.Length ?? 0;
			}
		}


		[TestMethod]
		public void ShouldRespectLifetime_Singleton() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Singleton, matchersLifetime: ServiceLifetime.Singleton);
			ServiceProvider services = serviceCollection.BuildServiceProvider();

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

			services.Dispose();
		}

		[TestMethod]
		public void ShouldRespectLifetime_Scoped() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Scoped, matchersLifetime: ServiceLifetime.Scoped);
			ServiceProvider services = serviceCollection.BuildServiceProvider();

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

			services.Dispose();
		}

		[TestMethod]
		public void ShouldRespectLifetime_Transient() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Transient, matchersLifetime: ServiceLifetime.Transient);
			ServiceProvider services = serviceCollection.BuildServiceProvider();

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

			services.Dispose();
		}

		[TestMethod]
		public void ShouldUseCustomMapsOptions() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper();
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			mapper.Map<string, int>("AAA");
			mapper.Map<IEnumerable<string>, List<int>>(new[] { "AAA" });

			services.Dispose();
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
			ServiceProvider services = serviceCollection.BuildServiceProvider();

			var mapper = services.GetRequiredService<IMapper>();
			mapper.Map<string, int>("AAA");
			mapper.Map<string, float>("BBB", 42f);

			services.Dispose();
		}

		[TestMethod]
		public void NestedMapperShouldBeEqualToIMapper() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Singleton, matchersLifetime: ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => {
				o.TypesToScan.Add(typeof(Maps));
			});
			ServiceProvider services = serviceCollection.BuildServiceProvider();

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

			services.Dispose();
		}
	}
}
