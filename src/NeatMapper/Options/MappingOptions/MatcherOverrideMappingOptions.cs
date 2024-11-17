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
		public MatcherOverrideMappingOptions(IMatcher? matcher = null, IServiceProvider? serviceProvider = null) {
			Matcher = matcher;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Matcher to be used for nested matches.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent matcher.</remarks>
		public IMatcher? Matcher {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}

		/// <summary>
		/// Service provider to use for the matches.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent matcher.</remarks>
		public IServiceProvider? ServiceProvider {
			get;
#if NET5_0_OR_GREATER
			init;
#endif
		}
	}
}
