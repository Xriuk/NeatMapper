#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using NeatMapper.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace NeatMapper.Mappers {
	/// <summary>
	/// Base class for mappers which map collections by mapping elements with another <see cref="IMapper"/>
	/// </summary>
	public abstract class CustomCollectionMapper : IMapper {
		protected readonly IMapper _elementsMapper;
		protected readonly IServiceProvider _serviceProvider;

		public CustomCollectionMapper(IMapper elementsMapper, IServiceProvider serviceProvider = null) {
			_elementsMapper = elementsMapper ?? throw new ArgumentNullException(nameof(elementsMapper));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, IEnumerable mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, IEnumerable mappingOptions = null);


		protected MappingContext CreateMappingContext(IEnumerable mappingOptions) {
			var options = new MappingOptions(mappingOptions);
			var overrideOptions = options.GetOptions<MapperOverrideOptions>();
			return new MappingContext {
				Mapper = overrideOptions.Mapper ?? this, // DEV: provide CombinedMapper with this and _elementsMapper
				ServiceProvider = overrideOptions.ServiceProvider ?? _serviceProvider,
				MappingOptions = new MappingOptions(mappingOptions)
			};
		}

		protected static bool CanCreateCollection(Type destination) {
			if (destination.IsArray)
				return true;
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>) ||
					collectionDefinition == typeof(ReadOnlyDictionary<,>) ||
					collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {

					return true;
				}
			}

			if (destination == typeof(string))
				return true;
			else if (destination.IsInterface && destination.IsGenericType) {
				var interfaceDefinition = destination.GetGenericTypeDefinition();
				if (interfaceDefinition == typeof(IEnumerable<>) || interfaceDefinition == typeof(IList<>) || interfaceDefinition == typeof(ICollection<>) ||
					interfaceDefinition == typeof(IReadOnlyList<>) || interfaceDefinition == typeof(IReadOnlyCollection<>) ||
					interfaceDefinition == typeof(IDictionary<,>) || interfaceDefinition == typeof(IReadOnlyDictionary<,>) ||
					interfaceDefinition == typeof(ISet<>)
#if NET5_0_OR_GREATER
					|| interfaceDefinition == typeof(IReadOnlySet<>)
#endif
					) {

					return true;
				}
			}

			return ObjectFactory.CanCreate(destination);
		}

		// Create a non-readonly collection which could be later converted to the given type
		protected static object CreateCollection(Type destination) {
			if (destination.IsArray)
				destination = typeof(List<>).MakeGenericType(destination.GetElementType());
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>))
					destination = typeof(List<>).MakeGenericType(destination.GetGenericArguments());
				else if (collectionDefinition == typeof(ReadOnlyDictionary<,>))
					destination = typeof(Dictionary<,>).MakeGenericType(destination.GetGenericArguments());
				else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>))
					destination = typeof(ObservableCollection<>).MakeGenericType(destination.GetGenericArguments());
			}

			return ObjectFactory.Create(destination);
		}

		// Returns an instance method which can be invoked with a single parameter to be added to the collection
		protected static MethodInfo GetCollectionAddMethod(object collection) {
			var collectionInstanceType = collection.GetType();
			var collectionInterface = collectionInstanceType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
			if (collectionInterface != null)
				return collectionInstanceType.GetInterfaceMap(collectionInterface).TargetMethods.First(m => m.Name.EndsWith(nameof(ICollection<object>.Add)));
			else if (collectionInstanceType.IsGenericType) {
				var collectionGenericType = collectionInstanceType.GetGenericTypeDefinition();
				if (collectionGenericType == typeof(Queue<>)) {
					return collectionInstanceType.GetMethod(nameof(Queue<object>.Enqueue))
						?? throw new InvalidOperationException($"Cannot find method {nameof(Queue)}.{nameof(Queue<object>.ToArray)}");
				}
				else if (collectionGenericType == typeof(Stack<>)) {
					return collectionInstanceType.GetMethod(nameof(Stack<object>.Push))
						?? throw new InvalidOperationException($"Cannot find method {nameof(Queue)}.{nameof(Queue<object>.ToArray)}");
				}
			}

			throw new InvalidOperationException("Invalid collection"); // Should not happen
		}

		// T[] Enumerable.ToArray(this IEnumerable<T> source);
		private static readonly MethodInfo Enumerable_ToArray = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray))
			?? throw new InvalidOperationException($"Cannot find method {nameof(Enumerable)}.{nameof(Enumerable.ToArray)}");
		protected static object ConvertCollectionToType(object collection, Type destination) {
			if (destination.IsArray)
				return Enumerable_ToArray.MakeGenericMethod(destination.GetElementType()).Invoke(null, new object[] { collection });
			else if (destination.IsGenericType) {
				var collectionDefinition = destination.GetGenericTypeDefinition();
				if (collectionDefinition == typeof(ReadOnlyCollection<>)) {
					return typeof(ReadOnlyCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IList<>);
						}).Invoke(new object[] { collection });
				}
				else if (collectionDefinition == typeof(ReadOnlyDictionary<,>)) {
					return typeof(ReadOnlyDictionary<,>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>);
						}).Invoke(new object[] { collection });
				}
				else if (collectionDefinition == typeof(ReadOnlyObservableCollection<>)) {
					return typeof(ReadOnlyObservableCollection<>).MakeGenericType(destination.GetGenericArguments()).GetConstructors()
						.First(c => {
							var param = c.GetParameters().Single();
							return param.ParameterType.IsGenericType && param.ParameterType.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
						}).Invoke(new object[] { collection });
				}
			}

			return collection;
		}
	}
}
