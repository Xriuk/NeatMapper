using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class EnumerableExtensionsTests {
		private class NewFactory : INewMapFactory {
			Type INewMapFactory.SourceType => throw new NotImplementedException();

			Type INewMapFactory.DestinationType => throw new NotImplementedException();

			public static bool Disposed = false;

			void IDisposable.Dispose() {
				Disposed = true;
			}

			object INewMapFactory.Invoke(object source) {
				return 1;
			}
		}

		private class CustomMapper : IMapper, IMapperFactory {
			public bool CanMapMerge(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			public bool CanMapNew(Type sourceType, Type destinationType, MappingOptions mappingOptions = null) {
				throw new NotImplementedException();
			}

			object IMapper.Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions) {
				throw new NotImplementedException();
			}

			object IMapper.Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions) {
				throw new NotImplementedException();
			}

			IMergeMapFactory IMapperFactory.MapMergeFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions) {
				throw new NotImplementedException();
			}

			INewMapFactory IMapperFactory.MapNewFactory(Type sourceType, Type destinationType, MappingOptions mappingOptions) {
				return new NewFactory();
			}
		}

		public class Maps :
			INewMap<string, int>{

			public static int maps = 0;

			int INewMap<string, int>.Map(string source, MappingContext context) {
				maps++;
				return source?.Length ?? 0;
			}
		}


		IMapper _mapper;

		[TestInitialize]
		public void Initialize() {
			var serviceCollection = new ServiceCollection();
			serviceCollection.AddNeatMapper(mappersLifetime: ServiceLifetime.Singleton);
			serviceCollection.Configure<CustomMapsOptions>(o => o.TypesToScan.Add(typeof(Maps)));
			using(var services = serviceCollection.BuildServiceProvider()) { 
				_mapper = services.GetRequiredService<IMapper>();
			}
		}

		[TestMethod]
		public void ShouldProjectLazily() {
			var mappedEnumerable = new[] { "AAA", "BB", "C" }.Project<int>(_mapper);
			var mappedResult = mappedEnumerable.Take(2).ToArray();

			Assert.AreEqual(2, mappedResult.Length);
			Assert.AreEqual(3, mappedResult[0]);
			Assert.AreEqual(2, mappedResult[1]);
			Assert.AreEqual(2, Maps.maps); // Only 2 elements actually mapped, not 3
		}

		[TestMethod]
		public void ProjectShouldImplementIDisposable() {
			Assert.IsInstanceOfType(new[] { "AAA", "BB", "C" }.Project<int>(_mapper), typeof(IDisposable));
		}

		[TestMethod]
		public void ProjectShouldDisposeTheFactoryInAForeachLoop() {
			NewFactory.Disposed = false;
			var mappedEnumerable = new[] { "AAA", "BB", "C" }.Project<int>(new CustomMapper());
			Assert.IsFalse(NewFactory.Disposed);
			foreach(var _ in mappedEnumerable) { }
			Assert.IsTrue(NewFactory.Disposed);
		}

		[TestMethod]
		public void ProjectShouldDisposeTheFactoryViaLinqOperatorsEvenIfNotFullyEnumerated() {
			NewFactory.Disposed = false;
			var mappedEnumerable = new[] { "AAA", "BB", "C" }.Project<int>(new CustomMapper());
			Assert.IsFalse(NewFactory.Disposed);
			mappedEnumerable.Take(2).ToArray();
			Assert.IsTrue(NewFactory.Disposed);
		}
	}
}
