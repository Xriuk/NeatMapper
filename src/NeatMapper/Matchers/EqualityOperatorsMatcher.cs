#if NET7_0_OR_GREATER
using System;
using System.Numerics;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing
	/// <see cref="IEqualityOperators{TSelf, TOther, TResult}"/> (with TResult being <see cref="bool"/>).
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEqualityOperators{TSelf, TOther, TResult}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EqualityOperatorsMatcher : IMatcher, IMatcherFactory {
		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>> _equalityComparersCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>>();

		private static Func<object?, object?, bool> GetOrCreateEqualityComparer(Type sourceType, Type destinationType) {
			return _equalityComparersCache.GetOrAdd((sourceType, destinationType), types => {
				var sourceParam = Expression.Parameter(typeof(object), "source");
				var destinationParam = Expression.Parameter(typeof(object), "destination");
				var method = types.From.GetInterfaceMap(typeof(IEqualityOperators<,,>).MakeGenericType(sourceType, destinationType, typeof(bool)))
					.TargetMethods.Single(m => m.Name.EndsWith("op_Equality"));

				// source == destination
				// If we don't have an explicit implementation of the operator we can just create an Equal expression,
				// otherwise we have to invoke it explicitly
				Expression body;
				if (method.Name == "op_Equality")
					body = Expression.Equal(Expression.Convert(sourceParam, types.From), Expression.Convert(destinationParam, types.To));
				else 
					body = Expression.Call(method, Expression.Convert(sourceParam, types.From), Expression.Convert(destinationParam, types.To));
				
				return Expression.Lambda<Func<object?, object?, bool>>(body, sourceParam, destinationParam).Compile();
			});
		}

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new EqualityOperatorsMatcher();


		private EqualityOperatorsMatcher() { }


		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			return sourceType.GetInterfaces().Any(i => {
				if (!i.IsGenericType || i.GetGenericTypeDefinition() != typeof(IEqualityOperators<,,>))
					return false;
				var args = i.GetGenericArguments();
				return args.Length == 3 &&
					args[0] == sourceType &&
					args[1] == destinationType &&
					args[2] == typeof(bool);
			});
		}

		public bool Match(object? source, Type sourceType, object? destination, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return GetOrCreateEqualityComparer(sourceType, destinationType).Invoke(source, destination);
		}

		public IMatchMapFactory MatchFactory(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (!CanMatch(sourceType, destinationType))
				throw new MapNotFoundException((sourceType, destinationType));

			return new DefaultMatchMapFactory(sourceType, destinationType, GetOrCreateEqualityComparer(sourceType, destinationType));
		}
	}
}
#endif
