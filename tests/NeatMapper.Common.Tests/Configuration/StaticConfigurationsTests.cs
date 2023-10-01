#if NET7_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;

namespace NeatMapper.Tests.Configuration {
	[TestClass]
	public class StaticConfigurationsTests {
		public class Map1 : IMatchMap<string, int> {
			bool IMatchMap<string, int>.Match(string? source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class Map2 : IMatchMapStatic<string, int> {
			static bool IMatchMapStatic<string, int>.Match(string? source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		protected static void Configure(MapperConfigurationOptions options) {
			// MatchMap is always configured
			new MapperConfiguration(i => false, i => false, options);
		}


		[TestMethod]
		public void ShouldNotAllowDuplicateMaps() {
			TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Map1), typeof(Map2) }
			}));
		}
	}
}
#endif