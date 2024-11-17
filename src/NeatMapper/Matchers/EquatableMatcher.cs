using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing <see cref="IEquatable{T}"/>.
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEquatable{T}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EquatableMatcher : IMatcher, IMatcherFactory {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool CanMatchInternal(Type sourceType, Type destinationType) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType.GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(destinationType));
		}

		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>> _equalityComparersCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>>();

		private static Func<object?, object?, bool> GetOrCreateEqualityComparer(Type sourceType, Type destinationType) {
			return _equalityComparersCache.GetOrAdd((sourceType, destinationType), types => {
				var sourceParam = Expression.Parameter(typeof(object), "source");
				var destinationParam = Expression.Parameter(typeof(object), "destination");
				var method = types.From.GetInterfaceMap(typeof(IEquatable<>).MakeGenericType(types.To)).TargetMethods.Single();

				// (source != null) ? ((TSource)source).Equals((TDestination)destination) : (destination == null)
				var body = Expression.Condition(Expression.NotEqual(sourceParam, Expression.Constant(null)),
					Expression.Call(Expression.Convert(sourceParam, types.From), method, Expression.Convert(destinationParam, types.To)),
					Expression.Equal(destinationParam, Expression.Constant(null)));

				return Expression.Lambda<Func<object?, object?, bool>>(body, sourceParam, destinationParam).Compile();
			});
		}

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new EquatableMatcher();


		private EquatableMatcher() { }


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanMatchInternal(sourceType, destinationType);
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			TypeUtils.CheckObjectType(source, sourceType, nameof(source));
			TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

			return GetOrCreateEqualityComparer(sourceType, destinationType).Invoke(source, destination);
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatchInternal(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			var comparer = GetOrCreateEqualityComparer(sourceType, destinationType);

			return new DefaultMatchMapFactory(sourceType, destinationType,
				(source, destination) => {
					TypeUtils.CheckObjectType(source, sourceType, nameof(source));
					TypeUtils.CheckObjectType(destination, destinationType, nameof(destination));

					return comparer.Invoke(source, destination);
				});
		}
	}
}
