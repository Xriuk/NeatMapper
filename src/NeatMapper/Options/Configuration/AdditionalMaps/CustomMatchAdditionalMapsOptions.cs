// This code was generated by a tool (CustomAdditionalMapsOptions.tt).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.

using System;
using System.Collections.Generic;

namespace NeatMapper {
  /// <summary>
	/// Options which allow to specify additional user-defined mappings (and optionally checks for them), to be added to maps found
	/// in types in <see cref="CustomMapsOptions"/>.
	/// </summary>
	public sealed class CustomMatchAdditionalMapsOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CustomMatchAdditionalMapsOptions"/>.
		/// </summary>
		public CustomMatchAdditionalMapsOptions() {
			_maps = [];
		}
		/// <summary>
		/// Creates a new instance of <see cref="CustomMatchAdditionalMapsOptions"/>
		/// by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CustomMatchAdditionalMapsOptions(CustomMatchAdditionalMapsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>(options._maps);
		}

		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _maps = [];
		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _canMaps = [];


		/// <summary>
		/// Adds a custom map, this will later throw if another map for the same types was defined
		/// in one of the types in <see cref="CustomMapsOptions"/>.
		/// </summary>
		/// <param name="mapDelegate">Mapping method to use.</param>
		public void AddMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, null, true);
		}
		/// <summary>
		/// Adds a custom map, this will later throw if another map for the same types was defined
		/// in one of the types in <see cref="CustomMapsOptions"/>.
		/// </summary>
		/// <param name="mapDelegate">Mapping method to use.</param>
		/// <param name="canMapDelegate">Optional check method which will be called before the mapping method.</param>
		public void AddMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate, CanMatchDelegate<TSource, TDestination>? canMapDelegate) {
			AddMapInternal(mapDelegate, canMapDelegate, true);
		}

		/// <summary>
		/// Adds a custom map, this won't throw if another map for the same types was defined
		/// in one of the types in <see cref="CustomMapsOptions"/>, the map will be ignored.
		/// </summary>
		/// <param name="mapDelegate">Mapping method to use.</param>
		public void TryAddMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, null, false);
		}
		/// <summary>
		/// Adds a custom map, this won't throw if another map for the same types was defined
		/// in one of the types in <see cref="CustomMapsOptions"/>, the map will be ignored.
		/// </summary>
		/// <param name="mapDelegate">Mapping method to use.</param>
		/// <param name="canMapDelegate">Optional check method which will be called before the mapping method.</param>
		public void TryAddMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate, CanMatchDelegate<TSource, TDestination>? canMapDelegate) {
			AddMapInternal(mapDelegate, canMapDelegate, false);
		}


		private void AddMapInternal<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate, CanMatchDelegate<TSource, TDestination>? canMapDelegate, bool throwOnDuplicate) {
			if (mapDelegate == null)
				throw new ArgumentNullException(nameof(mapDelegate));

			if (_maps.ContainsKey((typeof(TSource), typeof(TDestination))) || (canMapDelegate != null && _canMaps.ContainsKey((typeof(TSource), typeof(TDestination)))))
				throw new ArgumentException($"Duplicate map for types {typeof(TSource).FullName ?? typeof(TSource).Name} -> {typeof(TDestination).FullName ?? typeof(TDestination).Name}");

			var map = new CustomAdditionalMap {
				From = typeof(TSource),
				To = typeof(TDestination),
				ThrowOnDuplicate = throwOnDuplicate
			};
			if ((mapDelegate.Method.GetType().FullName?.StartsWith("System.Reflection.Emit.DynamicMethod") == true)) {
				MatchMapDelegate<TSource, TDestination> method = mapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = mapDelegate;
			}
			else
				map.Method = mapDelegate.Method;
			_maps.Add((typeof(TSource), typeof(TDestination)), map);

			if(canMapDelegate != null){
				map = new CustomAdditionalMap {
					From = typeof(TSource),
					To = typeof(TDestination),
					ThrowOnDuplicate = throwOnDuplicate
				};
				if ((canMapDelegate.Method.GetType().FullName?.StartsWith("System.Reflection.Emit.DynamicMethod") == true)) {
					CanMatchDelegate<TSource, TDestination> method = canMapDelegate.Invoke;
					map.Method = method.Method;
					map.Instance = canMapDelegate;
				}
				else
					map.Method = canMapDelegate.Method;
				_canMaps.Add((typeof(TSource), typeof(TDestination)), map);
			}
		}
	}
}
