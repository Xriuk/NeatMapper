using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NeatMapper {
	/// <summary>
	/// <see cref="IProjector"/> which projects an <see cref="IEnumerable{T}"/> (even nested) to a new
	/// <see cref="IEnumerable{T}"/> by projecting elements with another <see cref="IProjector"/>.
	/// Also supports mapping <see cref="IQueryable{T}"/>s between them.
	/// </summary>
	public sealed class CollectionProjector : IProjector {
		/// <summary>
		/// <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})"/>
		/// </summary>
		private static readonly MethodInfo Queryable_Select = TypeUtils.GetMethod(() => default(IQueryable<object>)!.Select(q => q));
		/// <summary>
		/// <see cref="Enumerable.Select{TSource, TResult}(IEnumerable{TSource}, Func{TSource, TResult})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_Select = TypeUtils.GetMethod(() => default(IEnumerable<object>)!.Select(e => e));
		/// <summary>
		/// <see cref="Enumerable.ToArray{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToArray = TypeUtils.GetMethod(() => default(IEnumerable<object>)!.ToArray());
		/// <summary>
		/// <see cref="Enumerable.ToList{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToList = TypeUtils.GetMethod(() => default(IEnumerable<object>)!.ToList());
#if !NETCOREAPP3_1_OR_GREATER && !NET5_0_OR_GREATER
		/// <summary>
		/// <see cref="Enumerable.ToDictionary{TSource, TKey, TElement}(IEnumerable{TSource}, Func{TSource, TKey}, Func{TSource, TElement})"/>
		/// </summary>
		private static readonly MethodInfo Enumerable_ToDictionary = TypeUtils.GetMethod(() => default(IEnumerable<object>)!.ToDictionary(e => e, e => e));
#endif
		/// <summary>
		/// <see cref="string.String(char[])"/>
		/// </summary>
		private static readonly ConstructorInfo string_charArray = typeof(string).GetConstructor([ typeof(char[]) ])
			?? throw new InvalidOperationException("Could not find new string(char[])");

		// Also supports open generics
		private static bool CanProjectCollection(Type type) {
			if (type == typeof(string))
				return true;
			else if (type.IsArray)
				return true;
			else if (type.IsGenericType) {
				var genericDefinition = type.GetGenericTypeDefinition();
				if (type.IsInterface) {
					if (genericDefinition == typeof(IEnumerable<>) ||
						genericDefinition == typeof(ICollection<>) || genericDefinition == typeof(IReadOnlyCollection<>) ||
						genericDefinition == typeof(IList<>) || genericDefinition == typeof(IReadOnlyList<>) ||
						genericDefinition == typeof(IDictionary<,>) || genericDefinition == typeof(IReadOnlyDictionary<,>) ||
						genericDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
						|| genericDefinition == typeof(IReadOnlySet<>)
#endif
						) {

						return true;
					}
					else
						return false;
				}
				else if (genericDefinition == typeof(ReadOnlyCollection<>) ||
					genericDefinition == typeof(ReadOnlyDictionary<,>) ||
					genericDefinition == typeof(Dictionary<,>) ||
					genericDefinition == typeof(ReadOnlyObservableCollection<>) ||
					genericDefinition == typeof(SortedList<,>)) {

					return true;
				}
			}

			// Otherwise a collection (even custom) can be projected only if it has a constructor which accepts an IEnumerable<T>
			if(type.IsGenericTypeDefinition)
				return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Any(c => c.GetParameters().ElementAtOrDefault(0)?.ParameterType.IsEnumerable() == true);
			else
				return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [ typeof(IEnumerable<>).MakeGenericType(type.GetEnumerableElementType()) ], null) != null;
		}


		/// <summary>
		/// <see cref="IProjector"/> which is used to project the elements of the collections, will be also provided
		/// as a nested projector in <see cref="ProjectorOverrideMappingOptions"/> (if not already present).
		/// </summary>
		private readonly IProjector _elementsProjector;

		/// <summary>
		/// Cached input and output <see cref="MappingOptions"/>.
		/// </summary>
		private readonly MappingOptionsFactoryCache<MappingOptions> _optionsCache;


		/// <summary>
		/// Creates a new instance of <see cref="CollectionProjector"/>.
		/// </summary>
		/// <param name="elementsProjector">
		/// <see cref="IProjector"/> to use to project collection elements.<br/>
		/// Can be overridden during projection with <see cref="ProjectorOverrideMappingOptions"/>.
		/// </param>
		public CollectionProjector(IProjector elementsProjector) {
			_elementsProjector = new CompositeProjector(elementsProjector ?? throw new ArgumentNullException(nameof(elementsProjector)), this);
			var nestedProjectionContext = new NestedProjectionContext(this);
			_optionsCache = new MappingOptionsFactoryCache<MappingOptions>(options => options.ReplaceOrAdd<ProjectorOverrideMappingOptions, NestedProjectionContext>(
				p => p?.Projector != null ? p : new ProjectorOverrideMappingOptions(_elementsProjector, p?.ServiceProvider),
				n => n != null ? new NestedProjectionContext(this, n) : nestedProjectionContext, options.Cached));
		}


		public bool CanProject(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			return CanProjectInternal(sourceType, destinationType, ref mappingOptions, out _, out _, out _);
		}

		public LambdaExpression Project(Type sourceType, Type destinationType, MappingOptions? mappingOptions = null) {
			(Type From, Type To) types = (sourceType, destinationType);

			if (!CanProjectInternal(sourceType, destinationType, ref mappingOptions, out var elementTypes, out var isQueryable, out var elementsProjector) || elementsProjector == null)
				throw new MapNotFoundException(types);

			Expression elementProjection;
			try { 
				elementProjection = elementsProjector.Project(elementTypes.From, elementTypes.To, mappingOptions);
			}
			catch (MapNotFoundException) {
				throw new MapNotFoundException(types);
			}

			var source = Expression.Parameter(types.From, "source");

			// source.Select(PROJECTION)
			// If we have an IQueryable we quote the element projection expression as it needs to stay an Expression<Func<..., ...>>
			Expression body = Expression.Call(
				null,
				(isQueryable ? Queryable_Select : Enumerable_Select).MakeGenericMethod(elementTypes.From, elementTypes.To),
				source,
				(isQueryable ? Expression.Quote(elementProjection) : elementProjection));

			// Create the final built-in collection for non queryables
			if (!isQueryable) {
				Expression? collection = null;
				if (!types.To.IsGenericType || types.To.GetGenericTypeDefinition() != typeof(IEnumerable<>)) { 
					if (types.To == typeof(string)){
						// new string(PROJECTION.ToArray())
						collection = Expression.New(string_charArray, Expression.Call(null, Enumerable_ToArray.MakeGenericMethod(elementTypes.To), body));
					}
					else if (types.To.IsArray) {
						// PROJECTION.ToArray()
						collection = Expression.Call(null, Enumerable_ToArray.MakeGenericMethod(elementTypes.To), body);
					}
					else if (types.To.IsGenericType) {
						var genericDefinition = types.To.GetGenericTypeDefinition();
						if (types.To.IsInterface) {
							if (genericDefinition == typeof(IList<>) || genericDefinition == typeof(ICollection<>) ||
								genericDefinition == typeof(IReadOnlyList<>) || genericDefinition == typeof(IReadOnlyCollection<>)){

								// PROJECTION.ToList()
								collection = Expression.Call(null, Enumerable_ToList.MakeGenericMethod(elementTypes.To), body);
							}
							else if(genericDefinition == typeof(IDictionary<,>) || genericDefinition == typeof(IReadOnlyDictionary<,>)){
								// DICTIONARY(PROJECTION)
								collection = CreateDictionary(body, elementTypes.To);
							}
							else if(genericDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
							|| genericDefinition == typeof(IReadOnlySet<>)
#endif
								) {

								// new HashSet(PROJECTION)
								Expression.New(
									typeof(HashSet<>).MakeGenericType(types.To.GetGenericArguments())
										.GetConstructor([ typeof(IEnumerable<>).MakeGenericType(elementTypes.To) ])!,
									body
								);
							}
						}
						else if (genericDefinition == typeof(ReadOnlyCollection<>)){
							// new ReadOnlyCollection(PROJECTION.ToList())
							collection = Expression.New(
								types.To.GetConstructors().Single(),
								Expression.Call(null, Enumerable_ToList.MakeGenericMethod(elementTypes.To), body)
							);
						}
						else if(genericDefinition == typeof(ReadOnlyDictionary<,>)) {
							// new ReadOnlyDictionary(DICTIONARY(PROJECTION))
							collection = Expression.New(
								types.To.GetConstructors().Single(),
								CreateDictionary(body, elementTypes.To)
							);
						}
						else if (genericDefinition == typeof(Dictionary<,>)) {
							// DICTIONARY(PROJECTION)
							collection = CreateDictionary(body, elementTypes.To);
						}
						else if(genericDefinition == typeof(ReadOnlyObservableCollection<>)) {
							// new ReadOnlyObservableCollection(new ObservableCollection(PROJECTION))
							collection = Expression.New(
								types.To.GetConstructors().Single(),
								Expression.New(
									typeof(ObservableCollection<>).MakeGenericType(types.To.GetGenericArguments())
										.GetConstructor([ typeof(IEnumerable<>).MakeGenericType(elementTypes.To) ])!,
									body
								)
							);
						}
						else if (genericDefinition == typeof(SortedList<,>)) {
							// new SortedList(DICTIONARY(PROJECTION))
							collection = Expression.New(
								types.To.GetConstructor([ typeof(IDictionary<,>).MakeGenericType(elementTypes.To.GetGenericArguments()) ])!,
								CreateDictionary(body, elementTypes.To)
							);
						}
					}

					// new CustomCollection(PROJECTION)
					collection ??= Expression.New(types.To.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [ typeof(IEnumerable<>).MakeGenericType(elementTypes.To) ], null)!, body);
				}
				else
					collection = body;

				// source == null ? null : PROJECTION
				body = Expression.Condition(Expression.Equal(source, Expression.Constant(null, types.From)), Expression.Constant(null, collection.Type), collection);
			}

			return Expression.Lambda(typeof(Func<,>).MakeGenericType(types.From, types.To), body, source);


#if !NETCOREAPP3_1_OR_GREATER && !NET5_0_OR_GREATER
			static
#endif
				Expression CreateDictionary(Expression projection, Type elementType) {
#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
				// new Dictionary(PROJECTION)
				return Expression.New(
					typeof(Dictionary<,>).MakeGenericType(types.To.GetGenericArguments())
						.GetConstructor([ typeof(IEnumerable<>).MakeGenericType(elementType) ])!,
					projection
				);
#else
				ParameterExpression param1 = Expression.Parameter(elementType, "p");

				// p => p.Key
				Expression keyProperty = Expression.Property(param1, nameof(KeyValuePair<object, object>.Key));

				ParameterExpression param2 = Expression.Parameter(elementType, "p");

				// p => p.Value
				Expression valueProperty = Expression.Property(param2, nameof(KeyValuePair<object, object>.Value));

				// PROJECTION.ToDictionary(p => p.Key, p => p.Value)
				return Expression.Call(
					null,
					Enumerable_ToDictionary.MakeGenericMethod(elementType, elementType.GetGenericArguments()[0], elementType.GetGenericArguments()[1]),
					projection,
					Expression.Lambda(typeof(Func<,>).MakeGenericType(elementType, elementType.GetGenericArguments()[0]), keyProperty, param1),
					Expression.Lambda(typeof(Func<,>).MakeGenericType(elementType, elementType.GetGenericArguments()[1]), valueProperty, param2));
#endif
			}
		}


		private bool CanProjectInternal(
			Type sourceType,
			Type destinationType,
			[NotNullWhen(true)] ref MappingOptions? mappingOptions,
			out (Type From, Type To) elementTypes,
			out bool isQueryable,
			out IProjector elementsProjector) {

			if (sourceType == null)
				throw new ArgumentNullException(nameof(sourceType));
			if (destinationType == null)
				throw new ArgumentNullException(nameof(destinationType));

			if (sourceType.IsEnumerable() && destinationType.IsEnumerable()) {
				// Check if we are mapping IQueryable<T>
				isQueryable = sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == typeof(IQueryable<>) &&
					destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(IQueryable<>);
				if (isQueryable || CanProjectCollection(destinationType)) {
					if (sourceType.IsGenericTypeDefinition || destinationType.IsGenericTypeDefinition) {
						elementTypes = default;
						elementsProjector = null!;
						mappingOptions = null!;

						return true;
					}
					else { 
						elementTypes = (From: sourceType.GetEnumerableElementType(), To: destinationType.GetEnumerableElementType());

						mappingOptions = _optionsCache.GetOrCreate(mappingOptions);
						elementsProjector = mappingOptions.GetOptions<ProjectorOverrideMappingOptions>()?.Projector
							?? _elementsProjector;

						return elementsProjector.CanProject(elementTypes.From, elementTypes.To, mappingOptions);
					}
				}
				else {
					elementTypes = default;
					elementsProjector = null!;

					return false;
				}
			}
			else {
				elementTypes = default;
				elementsProjector = null!;
				isQueryable = false;

				return false;
			}
		}
	}
}
