using System;
using System.Collections.Generic;

namespace NeatMapper{
    public sealed class CustomAsyncNewAdditionalMapsOptions {
		public CustomAsyncNewAdditionalMapsOptions() {
			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();
		}
		public CustomAsyncNewAdditionalMapsOptions(CustomAsyncNewAdditionalMapsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>(options._maps);
		}


		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();


		public void AddMap<TSource, TDestination>(AsyncNewMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, true);
		}

		public void TryAddMap<TSource, TDestination>(AsyncNewMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, false);
		}


		private void AddMapInternal<TSource, TDestination>(AsyncNewMapDelegate<TSource, TDestination> mapDelegate, bool throwOnDuplicate) {
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
				AsyncNewMapDelegate<TSource, TDestination> method = mapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = mapDelegate;
			}
			else
				map.Method = mapDelegate.Method;
			_maps.Add((typeof(TSource), typeof(TDestination)), map);
		}
	}
}
