namespace NeatMapper.Tests.Extensions {
	public class MatchMapFactoryExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			MatchMapFactory<string, int> genericFactory1 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif
			MatchMapFactory<string, string> genericFactory2 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif

			// Predicate
			{
				// Explicit source and destination
				{
					genericFactory1.Predicate("Test");
					genericFactory1.Predicate(2);
					//genericFactory2.Predicate("Test"); // Ambiguous
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}
	}
}
