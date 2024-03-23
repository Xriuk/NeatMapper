using System;

namespace NeatMapper{
	/// <summary>
	/// Optional interface to be implemented by matchers, which allows to create a factory to match objects
	/// of given types, instead of matching them directly. The created factories should share the same
	/// <see cref="MappingContext"/>.<br/>
	/// The implementation should be more efficient than calling
	/// <see cref="IMatcher.Match(object, Type, object, Type, MappingOptions)"/> multiple times,
	/// and thus can be used for collections.
	/// </summary>
	/// <remarks>Implementations of this interface must be thread-safe, this includes the returned factories too.</remarks>
	public interface IMatcherFactory : IMatcher {
		/// <summary>
		/// Creates a factory to check if two objects are equivalent.
		/// </summary>
		/// <param name="sourceType">Type of the source object, used to retrieve the available maps.</param>
		/// <param name="destinationType">
		/// Type of the destination object, used to retrieve the available maps.
		/// </param>
		/// <param name="mappingOptions">
		/// Additional options passed to the context, support depends on the matcher and/or the maps, null to ignore.
		/// </param>
		/// <returns>
		/// A factory which can be used to check if two objects are equivalent.<br/>
		/// The factory when invoked may throw <see cref="MapNotFoundException"/> or <see cref="MatcherException"/> exceptions.
		/// </returns>
		/// <exception cref="MapNotFoundException">The provided types could not be matched.</exception>
		Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		object?, object?, bool
#else
		object, object, bool
#endif
			> MatchFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null);
	}
}
