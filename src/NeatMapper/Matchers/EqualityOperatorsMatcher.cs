#if NET7_0_OR_GREATER
using System;
using System.Numerics;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IMatcher"/> which matches class implementing
	/// <see cref="IEqualityOperators{TSelf, TOther, TResult}"/> (with TResult being <see cref="bool"/>).
	/// The types are matched in the provided order: source type is checked for matching implementations of
	/// <see cref="IEqualityOperators{TSelf, TOther, TResult}"/> (so multiple types can be matched too).
	/// </summary>
	public sealed class EqualityOperatorsMatcher : InterfaceNullableMatcher {
		/// <summary>
		/// Cached map delegates.
		/// </summary>
		private static readonly ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>> _equalityOperatorsCache =
			new ConcurrentDictionary<(Type From, Type To), Func<object?, object?, bool>>();

		/// <summary>
		/// Singleton instance of the matcher.
		/// </summary>
		[Obsolete("The singleton instance will be removed in future versions, use DI or explicit instance creation instead.")]
		public static readonly IMatcher Instance = new EqualityOperatorsMatcher();


		/// <summary>
		/// Creates a new instance of <see cref="EqualityOperatorsMatcher"/>.
		/// </summary>
		/// <param name="nullableTypesOptions">
		/// Additional options to handle <see cref="Nullable{T}"/> types automatic matching, null to use default.<br/>
		/// Can be overridden during matching with <see cref="NullableTypesMatchingMappingOptions"/>.
		/// </param>
		public EqualityOperatorsMatcher(NullableTypesMatchingOptions? nullableTypesOptions = null) : base(nullableTypesOptions) {}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected override bool CanMatchTypes((Type From, Type To) types) {
			return types.From.GetInterfaces().Any(i => {
				if (!i.IsGenericType || i.GetGenericTypeDefinition() != typeof(IEqualityOperators<,,>))
					return false;
				var args = i.GetGenericArguments();
				return args.Length == 3 &&
					args[0] == types.From &&
					args[1] == types.To &&
					args[2] == typeof(bool);
			});
		}

		protected override Func<object?, object?, bool> GetOrCreateDelegate(Type sourceType, Type destinationType) {
			return _equalityOperatorsCache.GetOrAdd((sourceType, destinationType), types => {
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
	}
}
#endif
