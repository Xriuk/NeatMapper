using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing <see cref="IStructuralEquatable"/>.
	/// </summary>
	public sealed class StructuralEquatableMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new StructuralEquatableMatcher();


		private StructuralEquatableMatcher(){ }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType.GetInterfaces().Contains(typeof(IStructuralEquatable)); // DEV: check if type is the same?
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			return MatchInternal(source, sourceType, destination, destinationType);
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType,
				(source, destination) => MatchInternal(source, sourceType, destination, destinationType));
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool MatchInternal(object? source, Type sourceType, object? destination, Type destinationType) {
			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			try {
				if (source == null)
					return destination == null;
				else
					return ((IStructuralEquatable)source).Equals(destination, StructuralComparisons.StructuralEqualityComparer);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception e) {
				throw new MatcherException(e, (sourceType, destinationType));
			}
		}
	}
}
