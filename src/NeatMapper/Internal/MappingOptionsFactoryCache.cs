using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	public sealed class MappingOptionsFactoryCache<TOutput> {
		/// <summary>
		/// Factory used to process mapping options, will not receive null values
		/// (only <see cref="MappingOptions.Empty"/>).
		/// </summary>
		private readonly Func<MappingOptions, TOutput> _factory;

		// DEV: maybe convert to weak-map?
		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output objects.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, TOutput> _optionsCache =
			new ConcurrentDictionary<MappingOptions, TOutput>();

		/// <summary>
		/// Cached output object for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly TOutput _optionsCacheNull;


		internal MappingOptionsFactoryCache(Func<MappingOptions, TOutput> factory) {
			_factory = factory;
			_optionsCacheNull = _factory.Invoke(MappingOptions.Empty);
		}


		public TOutput GetOrCreate(MappingOptions? options) {
			if (options == null || options == MappingOptions.Empty)
				return _optionsCacheNull;
			else if (options.Cached)
				return _optionsCache.GetOrAdd(options, _factory);
			else
				return _factory.Invoke(options);
		}
	}
}
