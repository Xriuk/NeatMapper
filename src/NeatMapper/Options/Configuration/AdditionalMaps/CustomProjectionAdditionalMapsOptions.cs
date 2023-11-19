using System;
using System.Collections.Generic;

namespace NeatMapper {
	public sealed class CustomProjectionAdditionalMapsOptions {
		public CustomProjectionAdditionalMapsOptions() {
			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();
		}
		public CustomProjectionAdditionalMapsOptions(CustomProjectionAdditionalMapsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>(options._maps);
		}


		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();


		public void AddMap<TSource, TDestination>(ProjectionMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, true);
		}

		public void TryAddMap<TSource, TDestination>(ProjectionMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, false);
		}


		private void AddMapInternal<TSource, TDestination>(ProjectionMapDelegate<TSource, TDestination> mapDelegate, bool throwOnDuplicate) {
			if (mapDelegate == null)
				throw new ArgumentNullException(nameof(mapDelegate));

			var map = new CustomAdditionalMap {
				From = typeof(TSource),
				To = typeof(TDestination),
				ThrowOnDuplicate = throwOnDuplicate
			};
			if ((mapDelegate.Method.GetType().FullName?.StartsWith("System.Reflection.Emit.DynamicMethod") == true)) {
				ProjectionMapDelegate<TSource, TDestination> method = mapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = mapDelegate;
			}
			else
				map.Method = mapDelegate.Method;
			_maps.Add((typeof(TSource), typeof(TDestination)), map);
		}
	}
}
