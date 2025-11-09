#if NET7_0_OR_GREATER
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches classes implementing
	/// <see cref="IEqualityOperators{TSelf, TOther, TResult}"/> (with TResult being <see cref="bool"/>).
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEqualityOperators{TSelf, TOther, TResult}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EqualityOperatorsMatcher : IMatcher, IMatcherFactory {
		private static bool EqualityOperatorsEqual<TSource, TDestination>(object? source, object? destination) where TSource : IEqualityOperators<TSource, TDestination, bool> {
			return (TSource?)source == (TDestination?)destination;
		}
		private static readonly MethodInfo this_EqualityOperatorsEqual = TypeUtils.GetMethod(() => EqualityOperatorsEqual<int, int>(default, default));

		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>> _equalityOperatorsCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>>();

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		public static readonly IMatcher Instance = new EqualityOperatorsMatcher();


		private EqualityOperatorsMatcher(){}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool CanMatch(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			// Cannot construct the interface type because it has recursive constraints and might throw
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
			return _equalityOperatorsCache.GetOrAdd((sourceType, destinationType), types => 
				(Func<object?, object?, bool>)Delegate.CreateDelegate(typeof(Func<object?, object?, bool>), this_EqualityOperatorsEqual.MakeGenericMethod(types.From, types.To)));
		}
	}
}
#endif
