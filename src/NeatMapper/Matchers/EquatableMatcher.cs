using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing <see cref="IEquatable{T}"/>.
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEquatable{T}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EquatableMatcher : IMatcher, IMatcherFactory {
		private static bool EquatableEquals<TSource, TDestination>(object? source, object? destination) where TSource : IEquatable<TDestination> {
			if(source == null)
				return destination == null;
			else
				return ((TSource)source).Equals((TDestination?)destination!);
		}
		private static readonly MethodInfo this_EquatableEquals = TypeUtils.GetMethod(() => EquatableEquals<int, int>(default, default));

		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>> _equalityComparersCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>>();

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new EquatableMatcher();


		private EquatableMatcher(){ }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType.GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(destinationType));
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			var comparer = GetOrCreateDelegate(sourceType, destinationType);

			try {
				return comparer.Invoke(source, destination);
			}
			catch (OperationCanceledException) {
				throw;
			}
			catch (Exception e) {
				throw new MatcherException(e, (sourceType, destinationType));
			}
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType, mappingOptions))
				throw new MapNotFoundException((sourceType, destinationType));

			var comparer = GetOrCreateDelegate(sourceType, destinationType);

			return new DefaultMatchMapFactory(sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					try {
						return comparer.Invoke(source, destination);
					}
					catch (OperationCanceledException) {
						throw;
					}
					catch (Exception e) {
						throw new MatcherException(e, (sourceType, destinationType));
					}
				});
		}


		private static Func<object?, object?, bool> GetOrCreateDelegate(Type sourceType, Type destinationType) {
			return _equalityComparersCache.GetOrAdd((sourceType, destinationType), types =>
				(Func<object?, object?, bool>)Delegate.CreateDelegate(typeof(Func<object?, object?, bool>), this_EquatableEquals.MakeGenericMethod(types.From, types.To)));
		}
	}
}
