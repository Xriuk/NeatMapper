#if NET7_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeatMapper.Tests.Configuration {
	[TestClass]
	public class StaticConfigurationsTests {
		public class Map1 : IMatchMap<string, int> {
			bool IMatchMap<string, int>.Match(string source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class Map2 : IMatchMapStatic<string, int> {
			static bool IMatchMapStatic<string, int>.Match(string source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		internal static CustomMapsConfiguration Configure(CustomMapsOptions options, CustomMatchAdditionalMapsOptions additionalMaps = null) {
			var matcher = new CustomMatcher(options, additionalMaps);
			return (CustomMapsConfiguration)typeof(CustomMatcher).GetField("_configuration", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(matcher);
		}


		[TestMethod]
		public void ShouldNotAllowDuplicateMaps() {
			TestUtils.AssertDuplicateMap(() => Configure(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Map1), typeof(Map2) }
			}));
		}
	}
}
#endif