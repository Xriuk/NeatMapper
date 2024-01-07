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

			var nestedMatchingContext = new NestedMatchingContext(parentMatcher ?? throw new ArgumentNullException(nameof(parentMatcher)));
			var nestedMatcherInstance = new NestedMatcher(nestedMatcher, o => (o ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedMatchingContext, FactoryContext>(
					n => n != null ? new NestedMatchingContext(nestedMatchingContext.ParentMatcher, n) : nestedMatchingContext,
					_ => FactoryContext.Instance));
			_matcher = new Lazy<IMatcher>(() => MappingOptions.GetOptions<FactoryContext>() != null ?
				(IMatcher)new CachedFactoryMatcher(nestedMatcherInstance) :
				nestedMatcherInstance);

			MappingOptions = mappingOptions ?? throw new ArgumentNullException(nameof(mappingOptions));
		}


		/// <summary>
		/// Service provider which can be used to retrieve additional services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Matcher which can be used for nested matches. <see cref="MappingOptions"/> are not automatically forwarded.<br/>
		/// The only options forwarded automatically are <see cref="NestedMatchingContext"/> and <see cref="FactoryContext"/>.
		/// </summary>
		public IMatcher Matcher => _matcher.Value;

		/// <summary>
		/// Additional mapping options, contains multiple options of different types,
		/// each mapper/map should try to retrieve its options and use them.
		/// </summary>
		public MappingOptions MappingOptions { get; }
	}
}
