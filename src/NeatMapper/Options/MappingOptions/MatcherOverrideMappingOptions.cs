using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IMatcher"/> which allow to override matcher and service provider
	/// inside the created <see cref="MatchingContext"/>.
	/// </summary>
	public sealed class MatcherOverrideMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="MatcherOverrideMappingOptions"/>.
		/// </summary>
		/// <param name="matcher">
		/// <inheritdoc cref="Matcher" path="/summary"/><inheritdoc cref="Matcher" path="/remarks"/>
		/// </param>
		/// <param name="serviceProvider">
		/// <inheritdoc cref="ServiceProvider" path="/summary"/><inheritdoc cref="ServiceProvider" path="/remarks"/>
		/// </param>
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
		/// Matcher to be used for nested matches.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent matcher.</remarks>
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
		/// Service provider to use for the matches.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent matcher.</remarks>
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
