using System;
using System.Collections.Generic;

namespace NeatMapper{
    public sealed class CustomAsyncMergeAdditionalMapsOptions {
		public CustomAsyncMergeAdditionalMapsOptions() {
			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();
		}
		public CustomAsyncMergeAdditionalMapsOptions(CustomAsyncMergeAdditionalMapsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>(options._maps);
		}


		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();


		public void AddMap<TSource, TDestination>(AsyncMergeMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, true);
		}

		public void TryAddMap<TSource, TDestination>(AsyncMergeMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, false);
		}


		private void AddMapInternal<TSource, TDestination>(AsyncMergeMapDelegate<TSource, TDestination> mapDelegate, bool throwOnDuplicate) {
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
				AsyncMergeMapDelegate<TSource, TDestination> method = mapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = mapDelegate;
			}
			else
				map.Method = mapDelegate.Method;
			_maps.Add((typeof(TSource), typeof(TDestination)), map);
		}
	}
}
