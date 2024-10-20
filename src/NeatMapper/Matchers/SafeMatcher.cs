using System;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which wraps another <see cref="IMatcher"/>
	/// and returns false in case it throws <see cref="MapNotFoundException"/>.
	/// </summary>
	public sealed class SafeMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Matcher to which delegate all the operations.
		/// </summary>
		private readonly IMatcher _matcher;

		/// <summary>
		/// Cached nested matching context.
		/// </summary>
		private readonly NestedMatchingContext _nestedMatchingContext;


		/// <summary>
		/// Creates a new instance of <see cref="SafeMatcher"/>.
		/// </summary>
		/// <param name="matcher">Matcher to which delegate all the operations.</param>
		public SafeMatcher(IMatcher matcher) {
			_matcher = matcher ?? throw new ArgumentNullException(nameof(matcher));
			_nestedMatchingContext = new NestedMatchingContext(this);
		}


#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

		private MappingOptions GetOptions(MappingOptions mappingOptions) {
			return (mappingOptions ?? MappingOptions.Empty)
				.ReplaceOrAdd<NestedMatchingContext>(n => n != null ? new NestedMatchingContext(this, n) : _nestedMatchingContext);
		}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		// Forwarding all the methods because we want to check on the wrapper matcher
		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			return _matcher.CanMatch(sourceType, destinationType, GetOptions(mappingOptions));
		}

		public bool Match(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			try {
				return _matcher.Match(source, sourceType, destination, destinationType, GetOptions(mappingOptions));
			}
			catch(MapNotFoundException){
				return false;
			}
		}

		// Forwarding all the methods because we want to check on the wrapper matcher
		public IMatchMapFactory MatchFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			IMatchMapFactory factory;
			try { 
				factory = _matcher.MatchFactory(sourceType, destinationType, GetOptions(mappingOptions));
			}
			catch (MapNotFoundException) {
				return new DefaultMatchMapFactory(sourceType, destinationType, (source, destination) => false);
			}

			return new DisposableMatchMapFactory(
				sourceType, destinationType,
				(source, destination) => {
					try {
						return factory.Invoke(source, destination);
					}
					catch (MapNotFoundException) {
						return false;
					}
				}, factory);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
