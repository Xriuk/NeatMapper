using System.Collections.Generic;
using System;

namespace NeatMapper.Configuration{
    public sealed class CustomMatchAdditionalMapsOptions {
		public CustomMatchAdditionalMapsOptions() {
			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();
		}
		public CustomMatchAdditionalMapsOptions(CustomMatchAdditionalMapsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			_maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>(options._maps);
		}


		internal readonly Dictionary<(Type From, Type To), CustomAdditionalMap> _maps = new Dictionary<(Type From, Type To), CustomAdditionalMap>();


		public void AddMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, true);
		}

		public void TryAddMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate) {
			AddMapInternal(mapDelegate, false);
		}


		private void AddMapInternal<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> mapDelegate, bool throwOnDuplicate) {
			if (mapDelegate == null)
				throw new ArgumentNullException(nameof(mapDelegate));

			var map = new CustomAdditionalMap {
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
		}
	}
}
