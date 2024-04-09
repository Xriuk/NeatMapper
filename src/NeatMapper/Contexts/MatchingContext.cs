#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;

namespace NeatMapper {
	/// <summary>
	/// Contains informations and services for the current matching operation.
	/// </summary>
	public sealed class MatchingContext {
		private readonly Lazy<IMatcher> _matcher;

		public MatchingContext(IServiceProvider serviceProvider, IMatcher matcher, MappingOptions mappingOptions) :
			this(serviceProvider, matcher, matcher, mappingOptions) {}
		public MatchingContext(IServiceProvider serviceProvider, IMatcher nestedMatcher, IMatcher parentMatcher, MappingOptions mappingOptions) {
			ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

			if(parentMatcher == null)
				throw new ArgumentNullException(nameof(parentMatcher));
			_matcher = new Lazy<IMatcher>(() => {
				var nestedMatchingContext = new NestedMatchingContext(parentMatcher);
				return new NestedMatcher(nestedMatcher, o => (o ?? MappingOptions.Empty)
					.ReplaceOrAdd<NestedMatchingContext>(
						n => n != null ? new NestedMatchingContext(nestedMatchingContext.ParentMatcher, n) : nestedMatchingContext));
			}, true);

			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Matcher which can be used for nested matches. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only option forwarded automatically is <see cref="NestedMatchingContext"/>.
		/// </summary>
		public IMatcher Matcher => _matcher.Value;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
