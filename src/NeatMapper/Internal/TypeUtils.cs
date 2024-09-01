﻿#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NeatMapper {
	internal static class TypeUtils {
		private static readonly ConcurrentDictionary<Type, Func<object, bool>> ICollection_IsReadOnlyCache =
			new ConcurrentDictionary<Type, Func<object, bool>>();


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasInterface(this Type type, Type openInterfaceType) {
			return (type.IsGenericType && type.GetGenericTypeDefinition() == openInterfaceType) ||
				type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetInterface(this Type type, Type openInterfaceType) {
			if (type.IsGenericType && type.GetGenericTypeDefinition() == openInterfaceType)
				return type;
			else
				return type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == openInterfaceType);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetInterfaceFirstArgumentType(this Type type, Type openInterfaceType) {
			return type.GetInterface(openInterfaceType).GetGenericArguments()[0];
		}

		public static Type GetArrayElementType(this Type arrayType) {
			var rank = arrayType.GetArrayRank();
			if(rank == 1)
				return arrayType.GetElementType();
			else
				return arrayType.GetElementType().MakeArrayType(rank - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEnumerable(this Type type) {
			return type.HasInterface(typeof(IEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetEnumerableElementType(this Type type) {
			return type.GetInterfaceFirstArgumentType(typeof(IEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAsyncEnumerable(this Type type) {
			return type.HasInterface(typeof(IAsyncEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetAsyncEnumerableElementType(this Type type) {
			return type.GetInterfaceFirstArgumentType(typeof(IAsyncEnumerable<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsCollection(this Type type) {
			return type.HasInterface(typeof(ICollection<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Type GetCollectionElementType(this Type type) {
			return type.GetInterfaceFirstArgumentType(typeof(ICollection<>));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckObjectType(object obj, Type type, string argument = null) {
			if(obj != null ? !type.IsAssignableFrom(obj.GetType()) : (type.IsValueType && Nullable.GetUnderlyingType(type) == null)) {
				var message = (obj != null ? $"Object of type {obj.GetType().FullName ?? obj.GetType().Name}" : "null") + " " +
					$"is not assignable to type {type.FullName ?? type.Name}.";
				throw argument != null ? (Exception)new ArgumentException(message) : new InvalidOperationException(message);
			}
		}

		public static bool IsCollectionReadonly(object collection) {
			if(collection == null)
				return false;

			// Just in case https://stackoverflow.com/questions/4482557/what-interfaces-do-all-arrays-implement-in-c#comment4902688_4482567
			var collectionType = collection.GetType();
			if(collectionType.IsArray)
				return true;

			return ICollection_IsReadOnlyCache.GetOrAdd(collectionType.GetCollectionElementType(), type => {
				var collectionInterfaceType = typeof(ICollection<>).MakeGenericType(type);
				var param = Expression.Parameter(typeof(object), "collection");
				// ((ICollection<Type>)collection).IsReadOnly
				var body = Expression.Property(Expression.Convert(param, collectionInterfaceType), collectionInterfaceType.GetProperty(nameof(ICollection<object>.IsReadOnly)));
				return Expression.Lambda<Func<object, bool>>(body, param).Compile();
			}).Invoke(collection);
		}


		private static readonly MethodInfo this_ConvertTask = typeof(TypeUtils).GetMethod(nameof(ConvertTask), BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("Could not find ConvertTask<TSource, TDestination>(task)");
		private static async Task<TDestination> ConvertTask<TSource, TDestination>(Task<TSource> task) where TSource : TDestination {
			var result = await task;
			return result;
		}
		public static TDelegate MethodToDelegate<TDelegate>(MethodInfo method, params string[] parameterNames) where TDelegate : Delegate {
			var delegateArguments = typeof(TDelegate).GetGenericArguments();

			// Create arguments matching provided delegate type and cast them to types of the method
			var parameterExpressions = delegateArguments
				.Zip(parameterNames, (p1, p2) => (p1, p2))
				.Select(parameters => Expression.Parameter(parameters.p1, parameters.p2))
				.ToArray();
			var parametersList = parameterExpressions
				.Zip(method.GetParameters(), (p1, p2) => (p1, p2))
				.Select(parameters => {
					if (parameters.p1.Type == parameters.p2.ParameterType)
						return parameters.p1;
					else
						return (Expression)Expression.Convert(parameters.p1, parameters.p2.ParameterType);
				});

			// Method((Type1)arg1, (Type2)arg2, ...)
			Expression body;
			if (method.IsStatic)
				body = Expression.Call(method, parametersList);
			else {
				body = Expression.Call(Expression.Constant(ObjectFactory.GetOrCreateCached(method.DeclaringType)),
					method, parametersList);
			}

			// Cast return type if needed (we check for not equal instead of IsAssignableFrom
			// because Expressions require convert even for implicit casts)
			if (method.ReturnType != typeof(void) && method.ReturnType != delegateArguments[delegateArguments.Length - 1]) {
				// (Destination)(await task) or
				// (Destination)result
				if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
					body = Expression.Call(this_ConvertTask.MakeGenericMethod(method.ReturnType.GetGenericArguments()[0], delegateArguments[delegateArguments.Length - 1].GetGenericArguments()[0]), body);
				else
					body = Expression.Convert(body, delegateArguments[delegateArguments.Length - 1]);
			}

			return Expression.Lambda<TDelegate>(body, parameterExpressions).Compile();
		}
	}
}
