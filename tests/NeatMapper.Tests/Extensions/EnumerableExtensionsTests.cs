using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class EnumerableExtensionsTests {
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
			ServiceProvider services = serviceCollection.BuildServiceProvider();

			_mapper = services.GetRequiredService<IMapper>();
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
	}
}
