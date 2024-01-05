// This code was generated by a tool (CustomAdditionalMapsOptions.tt).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.

using System;
using System.Collections.Generic;

namespace NeatMapper {
  /// <summary>
	/// Options which allow to specify additional user-defined mappings, to be added to maps found
	/// in types in <see cref="CustomMapsOptions"/>.
	/// </summary>
	public sealed class CustomHierarchyMatchAdditionalMapsOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CustomHierarchyMatchAdditionalMapsOptions"/>.
		/// </summary>
		public CustomHierarchyMatchAdditionalMapsOptions() {
			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();
		}
		/// <summary>
		/// Creates a new instance of <see cref="CustomHierarchyMatchAdditionalMapsOptions"/>
		/// by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CustomHierarchyMatchAdditionalMapsOptions(CustomHierarchyMatchAdditionalMapsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>(options._maps);
		}

		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();


		/// <summary>
		/// Adds a custom map, this will later throw if another map for the same types was defined
		/// in one of the types in <see cref="CustomMapsOptions"/>.
		/// </summary>
		/// <param name="mapDelegate">Mapping method to use.</param>
		public void AddMap<TSource, TDestination>(HierarchyMatchMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, true);
		}

		/// <summary>
		/// Adds a custom map, this won't throw if another map for the same types was defined
		/// in one of the types in <see cref="CustomMapsOptions"/>, the map will be ignored.
		/// </summary>
		/// <param name="mapDelegate">Mapping method to use.</param>
		public void TryAddMap<TSource, TDestination>(HierarchyMatchMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, false);
		}


		private void AddMapInternal<TSource, TDestination>(HierarchyMatchMapDelegate<TSource, TDestination> mapDelegate, bool throwOnDuplicate) {
			if (mapDelegate == null)
				throw new ArgumentNullException(nameof(mapDelegate));

			if (_maps.ContainsKey((typeof(TSource), typeof(TDestination))))
				throw new ArgumentException($"Duplicate map for types {typeof(TSource).FullName ?? typeof(TSource).Name} -> {typeof(TDestination).FullName ?? typeof(TDestination).Name}");

			var map = new CustomAdditionalMap {
				From = typeof(TSource),
				To = typeof(TDestination),
				ThrowOnDuplicate = throwOnDuplicate
			};
			if ((mapDelegate.Method.GetType().FullName?.StartsWith("System.Reflection.Emit.DynamicMethod") == true)) {
				HierarchyMatchMapDelegate<TSource, TDestination> method = mapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = mapDelegate;
			}
			else
				map.Method = mapDelegate.Method;
			_maps.Add((typeof(TSource), typeof(TDestination)), map);
		}
	}
}
