using System;

namespace NeatMapper {
	/// <summary>
	/// Options for <see cref="IAsyncMapper"/> which allow to override mapper and service provider
	/// inside the created <see cref="AsyncMappingContext"/>.
	/// </summary>
	public sealed class AsyncMapperOverrideMappingOptions {
		/// <summary>
		/// Creates a new instance of <see cref="AsyncMapperOverrideMappingOptions"/>.
		/// </summary>
		/// <param name="mapper">
		/// <inheritdoc cref="Mapper" path="/summary"/><inheritdoc cref="Mapper" path="/remarks"/>
		/// </param>
		/// <param name="serviceProvider">
		/// <inheritdoc cref="ServiceProvider" path="/summary"/><inheritdoc cref="ServiceProvider" path="/remarks"/>
		/// </param>
		public AsyncMapperOverrideMappingOptions(IAsyncMapper? mapper = null, IServiceProvider? serviceProvider = null) {
			Mapper = mapper;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Mapper to be used for nested maps.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent mapper.</remarks>
		public IAsyncMapper? Mapper { get; init; }

		/// <summary>
		/// Service provider to use for the maps.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent mapper.</remarks>
		public IServiceProvider? ServiceProvider { get; init; }
	}
}
