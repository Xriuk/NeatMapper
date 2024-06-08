using System.Collections;
using System.Collections.Generic;

namespace NeatMapper.Tests.Extensions {
	public class MatcherExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IMatcher matcher =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable

			MappingOptions? options = null;
			IEnumerable? enumerable1 = null;
			IEnumerable<string>? enumerable2 = null;
			object? option1 = null;
			object? option2 = null;
			object? str = null;
			object strNonNull = null!;
#else
			MappingOptions options = null;
			IEnumerable enumerable1 = null;
			IEnumerable<string> enumerable2 = null;
			object option1 = null;
			object option2 = null;
			object str = "Test";
			object strNonNull = null;
#endif

			// Match
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					matcher.Match("Test", 2);
					matcher.Match<string, int>("Test", 2);

					// MappingOptions
					matcher.Match("Test", 2, options);
					matcher.Match<string, int>("Test", 2, options);

					// IEnumerable
					matcher.Match("Test", 2, enumerable1);
					matcher.Match<string, int>("Test", 2, enumerable1);
					matcher.Match("Test", 2, enumerable2);
					matcher.Match<string, int>("Test", 2, enumerable2);

					// Params (causes ambiguity with Runtime)
					//matcher.Match("Test", 2, option1);
					//matcher.Match<string, int>("Test", 2, option1);
					//matcher.Match("Test", 2, option1, option2);
					//matcher.Match<string, int>("Test", 2, option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.Match("Test", typeof(string), 2, typeof(int));
					matcher.Match(str, typeof(string), 2, typeof(int));
					matcher.Match(strNonNull, typeof(string), 2, typeof(int));

					// MappingOptions
					matcher.Match("Test", typeof(string), 2, typeof(int), options);
					matcher.Match(str, typeof(string), 2, typeof(int), options);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), options);

					// IEnumerable
					matcher.Match("Test", typeof(string), 2, typeof(int), enumerable1);
					matcher.Match(str, typeof(string), 2, typeof(int), enumerable1);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), enumerable1);
					matcher.Match("Test", typeof(string), 2, typeof(int), enumerable2);
					matcher.Match(str, typeof(string), 2, typeof(int), enumerable2);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), enumerable2);

					// Params
					matcher.Match("Test", typeof(string), 2, typeof(int), option1);
					matcher.Match(str, typeof(string), 2, typeof(int), option1);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), option1);
					matcher.Match("Test", typeof(string), 2, typeof(int), option1, option2);
					matcher.Match(str, typeof(string), 2, typeof(int), option1, option2);
					matcher.Match(strNonNull, typeof(string), 2, typeof(int), option1, option2);
				}
			}


			// CanMatch
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					matcher.CanMatch<string, int>();

					// MappingOptions
					matcher.CanMatch<string, int>(options);

					// IEnumerable
					matcher.CanMatch<string, int>(enumerable1);
					matcher.CanMatch<string, int>(enumerable2);

					// Params
					matcher.CanMatch<string, int>(option1);
					matcher.CanMatch<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.CanMatch(typeof(string), typeof(int));

					// MappingOptions
					matcher.CanMatch(typeof(string), typeof(int), options);

					// IEnumerable
					matcher.CanMatch(typeof(string), typeof(int), enumerable1);
					matcher.CanMatch(typeof(string), typeof(int), enumerable2);

					// Params
					matcher.CanMatch(typeof(string), typeof(int), option1);
					matcher.CanMatch(typeof(string), typeof(int), option1, option2);
				}
			}


			// MatchFactory
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					matcher.MatchFactory<string, int>();

					// MappingOptions
					matcher.MatchFactory<string, int>(options);

					// IEnumerable
					matcher.MatchFactory<string, int>(enumerable1);
					matcher.MatchFactory<string, int>(enumerable2);

					// Params
					matcher.MatchFactory<string, int>(option1);
					matcher.MatchFactory<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					matcher.MatchFactory(typeof(string), typeof(int));

					// MappingOptions
					matcher.MatchFactory(typeof(string), typeof(int), options);

					// IEnumerable
					matcher.MatchFactory(typeof(string), typeof(int), enumerable1);
					matcher.MatchFactory(typeof(string), typeof(int), enumerable2);

					// Params
					matcher.MatchFactory(typeof(string), typeof(int), option1);
					matcher.MatchFactory(typeof(string), typeof(int), option1, option2);
				}
			}


			// Predicate

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}
	}
}
