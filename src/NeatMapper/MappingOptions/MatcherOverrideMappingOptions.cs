using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IMatcher"/> which allow to override matcher and service provider
	/// inside the created <see cref="MatchingContext"/>
	/// </summary>
	public sealed class MatcherOverrideMappingOptions {
		public MatcherOverrideMappingOptions(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMatcher?
#else
			IMatcher
#endif
			matcher = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			Matcher = matcher;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Matcher to be used for nested matches, null to use the one provided by the matcher
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IMatcher?
#else
			IMatcher
#endif
			Matcher {
				get;
#if NET5_0_OR_GREATER
				init;
#endif
		}

		/// <summary>
		/// Service provider to use for the matches, null to use the one provided by the matcher
		/// </summary>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			ServiceProvider {
				get;
#if NET5_0_OR_GREATER
				init;
#endif
		}
	}
}
