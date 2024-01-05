using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	public static class EnumerableExtensions {
		/// <summary>
		/// <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_Select = typeof(Enumerable).GetMethods().Single(m => {
			if (m.Name != nameof(Enumerable.Select))
				return false;
			var parameters = m.GetParameters();
			return (parameters.Length == 2 && parameters[1].ParameterType.IsGenericType && parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));
		});

		private static readonly MethodInfo MapperExtensions_MapNewFactory = typeof(MapperExtensions).GetMethod(nameof(MapperExtensions.MapNewFactory), new[] { typeof(IMapper), typeof(MappingOptions) })
			?? throw new InvalidOperationException("Could not find MapperExtensions.MapNewFactory method");

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		private static Func<TSource, TDestination> GetFactory<TSource, TDestination>(IMapper mapper, MappingOptions mappingOptions = null) {
			return mapper.MapNewFactory<TSource, TDestination>(mappingOptions);
		}
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif


		#region Project
		#region Runtime destination
		/// <summary>
		/// Projects an enumerable into another one lazily. The source type will be inferred from the enumerable itself
		/// (so it CANNOT be null, if it implements <see cref="IEnumerable{T}"/> it will be the type argument,
		/// otherwise it will be <see cref="object"/>).
		/// </summary>
		/// <param name="mapper">Mapper to use.</param>
		/// <param name="destinationElementType">
		/// Type of the destination element, used to retrieve the available maps.
		/// </param>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>
		/// The projected <see cref="IEnumerable{T}"/> with element type equal to <paramref name="destinationElementType"/>.
		/// The actual elements may be null.
		/// </returns>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/exception"/>
		public static IEnumerable Project(this IEnumerable enumerable,
			IMapper mapper,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));

			Type sourceElementType;
			try {
				sourceElementType = enumerable.GetType().GetEnumerableElementType();
			}
			catch {
				sourceElementType = typeof(object);
			}

			return enumerable.Project(mapper, sourceElementType, destinationElementType, mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, MappingOptions)"/>
		public static IEnumerable Project(this IEnumerable enumerable,
			IMapper mapper,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return enumerable.Project(mapper, destinationElementType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, MappingOptions)"/>
		public static IEnumerable Project(this IEnumerable enumerable,
			IMapper mapper, Type destinationElementType, params object[] mappingOptions) {

			return enumerable.Project(mapper, destinationElementType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Runtime source and destination
		/// <summary>
		/// Projects an enumerable into another one lazily.
		/// </summary>
		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, MappingOptions)"/>
		/// <param name="sourceElementType">Type of the source element, used to retrieve the available maps.</param>
		public static IEnumerable Project(this IEnumerable enumerable,
			IMapper mapper,
			Type sourceElementType,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));
			if (!typeof(IEnumerable<>).MakeGenericType(sourceElementType).IsAssignableFrom(enumerable.GetType()))
				throw new ArgumentException("Source element type is not assignable to enumerable element type", nameof(sourceElementType));
			if (mapper == null)
				throw new ArgumentNullException(nameof(mapper));
			if (sourceElementType == null)
				throw new ArgumentNullException(nameof(sourceElementType));
			if (destinationElementType == null)
				throw new ArgumentNullException(nameof(destinationElementType));

			return (IEnumerable)Enumerable_Select.MakeGenericMethod(sourceElementType, destinationElementType)
				.Invoke(null, new object[] {
					enumerable,
					MapperExtensions_MapNewFactory.MakeGenericMethod(sourceElementType, destinationElementType).Invoke(null, new object[]{ mapper, mappingOptions })
				});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, Type, MappingOptions)"/>
		public static IEnumerable Project(this IEnumerable enumerable,
			IMapper mapper,
			Type sourceElementType,
			Type destinationElementType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return enumerable.Project(mapper, sourceElementType, destinationElementType, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, Type, MappingOptions)"/>
		public static IEnumerable Project(this IEnumerable enumerable,
			IMapper mapper, Type sourceElementType, Type destinationElementType, params object[] mappingOptions) {

			return enumerable.Project(mapper, sourceElementType, destinationElementType, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit destination, inferred source
		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TDestination"><inheritdoc cref="Project(IEnumerable, IMapper, Type, Type, MappingOptions)" path="/param[@name='destinationElementType']"/></typeparam>
		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, MappingOptions)" path="/param[@name='mapper']"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <returns>The projected enumerable, the actual elements may be null.</returns>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/exception"/>
		public static IEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TDestination>(this IEnumerable enumerable,
			IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));

			Type sourceElementType;
			try {
				sourceElementType = enumerable.GetType().GetEnumerableElementType();
			}
			catch {
				sourceElementType = typeof(object);
			}

			return (IEnumerable<TDestination>)enumerable.Project(mapper, sourceElementType, typeof(TDestination), mappingOptions);
		}

		/// <inheritdoc cref="Project{TDestination}(IEnumerable, IMapper, MappingOptions)"/>
		public static IEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TDestination>(this IEnumerable enumerable,
			IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return enumerable.Project<TDestination>(mapper, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project{TDestination}(IEnumerable, IMapper, MappingOptions)"/>
		public static IEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TDestination>(this IEnumerable enumerable, IMapper mapper, params object[] mappingOptions) {

			return enumerable.Project<TDestination>(mapper, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion

		#region Explicit source and destination
		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, Type, MappingOptions)" path="/summary"/>
		/// <typeparam name="TSource"><inheritdoc cref="Project(IEnumerable, IMapper, Type, Type, MappingOptions)" path="/param[@name='sourceElementType']"/></typeparam>
		/// <inheritdoc cref="Project{TDestination}(IEnumerable, IMapper, MappingOptions)" path="/typeparam[@name='TDestination']"/>
		/// <inheritdoc cref="Project(IEnumerable, IMapper, Type, MappingOptions)" path="/param[@name='mapper']"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/param[@name='mappingOptions']"/>
		/// <inheritdoc cref="Project{TDestination}(IEnumerable, IMapper, MappingOptions)" path="/returns"/>
		/// <inheritdoc cref="IMapper.Map(object, Type, Type, MappingOptions)" path="/exception"/>
		public static IEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
#pragma warning disable CS1712
			> Project<TSource, TDestination>(this IEnumerable<
#pragma warning restore CS1712
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> enumerable,
			IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			return (IEnumerable<TDestination>)enumerable.Project(mapper, typeof(TSource), typeof(TDestination), mappingOptions);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IEnumerable{TSource}, IMapper, MappingOptions)"/>
		public static IEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TSource, TDestination>(this IEnumerable<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> enumerable,
			IMapper mapper,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			mappingOptions) {

			return enumerable.Project<TSource, TDestination>(mapper, mappingOptions != null ? new MappingOptions(mappingOptions) : null);
		}

		/// <inheritdoc cref="Project{TSource, TDestination}(IEnumerable{TSource}, IMapper, MappingOptions)"/>
		public static IEnumerable<
#if NET5_0_OR_GREATER
			TDestination?
#else
			TDestination
#endif
			> Project<TSource, TDestination>(this IEnumerable<
#if NET5_0_OR_GREATER
			TSource?
#else
			TSource
#endif
			> enumerable,
			IMapper mapper, params object[] mappingOptions) {

			return enumerable.Project<TSource, TDestination>(mapper, mappingOptions?.Length > 0 ? new MappingOptions(mappingOptions) : null);
		}
		#endregion
		#endregion
	}
}
