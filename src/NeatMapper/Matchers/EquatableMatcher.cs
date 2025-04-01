using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing <see cref="IEquatable{T}"/>.
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEquatable{T}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EquatableMatcher : InterfaceNullableMatcher, IMatcher, IMatcherFactory {
		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>> _equalityComparersCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>>();

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		[Obsolete("The singleton instance will be removed in future versions, use DI or explicit instance creation instead.")]
		public static readonly IMatcher Instance = new EquatableMatcher();


		/// <summary>
		/// Creates a new instance of <see cref="EquatableMatcher"/>.
		/// </summary>
		/// <param name="nullableTypesOptions">
		/// Additional options to handle <see cref="Nullable{T}"/> types automatic matching, null to use default.<br/>
		/// Can be overridden during matching with <see cref="NullableTypesMatchingMappingOptions"/>.
		/// </param>
		public EquatableMatcher(NullableTypesMatchingOptions? nullableTypesOptions = null) : base(nullableTypesOptions) { }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override bool CanMatchTypes((Type From, Type To) types) {
			return types.From.GetInterfaces().Contains(typeof(IEquatable<>).MakeGenericType(types.To));
		}

		protected override Func<object?, object?, bool> GetOrCreateDelegate(Type sourceType, Type destinationType) {
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
	}
}
