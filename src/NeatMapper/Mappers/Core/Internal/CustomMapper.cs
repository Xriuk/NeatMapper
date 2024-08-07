﻿#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	/// <summary>
	/// Base class for mappers which use user-defined maps to map types.
	/// Internal class.
	/// </summary>
	public abstract class CustomMapper : IMapper {
		/// <summary>
		/// Configuration for class and additional maps for the mapper.
		/// </summary>
		internal readonly CustomMapsConfiguration _configuration;

		/// <summary>
		/// Service provider available in the created <see cref="MappingContext"/>s.
		/// </summary>
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingContext"/>.
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingContext> _contextsCache =
			new ConcurrentDictionary<MappingOptions, MappingContext>();

		/// <summary>
		/// Cached output <see cref="MappingContext"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/>,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingContext _contextsCacheNull;


		internal CustomMapper(CustomMapsConfiguration configuration, IServiceProvider serviceProvider = null) {
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_serviceProvider = serviceProvider ?? EmptyServiceProvider.Instance;
			_contextsCacheNull = CreateMappingContext(MappingOptions.Empty);
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		protected MappingContext GetOrCreateMappingContext(MappingOptions options) {
			if(options == null || options == MappingOptions.Empty)
				return _contextsCacheNull;
			else if(options.Cached)
				return _contextsCache.GetOrAdd(options, CreateMappingContext);
			else
				return CreateMappingContext(options);
		}

		private MappingContext CreateMappingContext(MappingOptions options) {
			var overrideOptions = options.GetOptions<MapperOverrideMappingOptions>();
			return new MappingContext(
				overrideOptions?.ServiceProvider ?? _serviceProvider,
				overrideOptions?.Mapper ?? this,
				this,
				options
			);
		}
	}
}
