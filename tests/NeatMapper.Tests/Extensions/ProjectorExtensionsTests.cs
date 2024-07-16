using System.Collections;
using System.Collections.Generic;

namespace NeatMapper.Tests.Extensions {
	public class ProjectorExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IProjector projector =
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
#else
			MappingOptions options = null;
			IEnumerable enumerable1 = null;
			IEnumerable<string> enumerable2 = null;
			object option1 = null;
			object option2 = null;
#endif

			// Project
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					projector.Project<string, int>();

					// MappingOptions
					projector.Project<string, int>(options);

					// IEnumerable
					projector.Project<string, int>(enumerable1);
					projector.Project<string, int>(enumerable2);

					// Params
					projector.Project<string, int>(option1);
					projector.Project<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					projector.Project(typeof(string), typeof(int));

					// MappingOptions
					projector.Project(typeof(string), typeof(int), options);

					// IEnumerable
					projector.Project(typeof(string), typeof(int), enumerable1);
					projector.Project(typeof(string), typeof(int), enumerable2);

					// Params
					projector.Project(typeof(string), typeof(int), option1);
					projector.Project(typeof(string), typeof(int), option1, option2);
				}
			}


			// CanMatch
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					projector.CanProject<string, int>();

					// MappingOptions
					projector.CanProject<string, int>(options);

					// IEnumerable
					projector.CanProject<string, int>(enumerable1);
					projector.CanProject<string, int>(enumerable2);

					// Params
					projector.CanProject<string, int>(option1);
					projector.CanProject<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					projector.CanProject(typeof(string), typeof(int));

					// MappingOptions
					projector.CanProject(typeof(string), typeof(int), options);

					// IEnumerable
					projector.CanProject(typeof(string), typeof(int), enumerable1);
					projector.CanProject(typeof(string), typeof(int), enumerable2);

					// Params
					projector.CanProject(typeof(string), typeof(int), option1);
					projector.CanProject(typeof(string), typeof(int), option1, option2);
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}
	}
}
