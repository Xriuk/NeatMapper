using System;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IMatcher"/> which matches by invoking <see cref="Object.Equals(object, object)">
	/// (and overloads). Objects need to be of the same type (or derived).
	/// </summary>
	public sealed class ObjectEqualsMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Singleton instance of the matcher
		/// </summary>
		public static readonly IMatcher Instance = new ObjectEqualsMatcher();


		private ObjectEqualsMatcher() { }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType.IsAssignableFrom(destinationType) || destinationType.IsAssignableFrom(sourceType);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if(!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			return Object.Equals(source, destination);
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					return Object.Equals(source, destination);
				});
		}
	}
}
