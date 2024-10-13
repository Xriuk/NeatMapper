using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing <see cref="IEquatable{T}"/>.
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEquatable{T}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EquatableMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object, object, bool>> _equalityComparersCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object, object, bool>>();

		private static Func<object, object, bool> GetOrCreateEqualityComparer(Type sourceType, Type destinationType) {
			return _equalityComparersCache.GetOrAdd((sourceType, destinationType), types => {
				var sourceParam = Expression.Parameter(typeof(object), "source");
				var destinationParam = Expression.Parameter(typeof(object), "destination");
				var method = types.From.GetInterfaceMap(typeof(IEquatable<>).MakeGenericType(types.To)).TargetMethods.Single();

				// (source != null) ? ((TSource)source).Equals((TDestination)destination) : (destination == null)
				var body = Expression.Condition(Expression.NotEqual(sourceParam, Expression.Constant(null)),
					Expression.Call(Expression.Convert(sourceParam, types.From), method, Expression.Convert(destinationParam, types.To)),
					Expression.Equal(destinationParam, Expression.Constant(null)));

				return Expression.Lambda<Func<object, object, bool>>(body, sourceParam, destinationParam).Compile();
			});
		}

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new EquatableMatcher();


		private EquatableMatcher() { }


		public bool CanMatch(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType.GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(destinationType));
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

			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return GetOrCreateEqualityComparer(sourceType, destinationType).Invoke(source, destination);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public IMatchMapFactory MatchFactory(
			Type sourceType,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType, GetOrCreateEqualityComparer(sourceType, destinationType));
		}
	}
}
