﻿using System;

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
		public AsyncMapperOverrideMappingOptions(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IAsyncMapper?
#else
			IAsyncMapper
#endif
			mapper = null,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) {

			Mapper = mapper;
			ServiceProvider = serviceProvider;
		}


		/// <summary>
		/// Mapper to be used for nested maps.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent mapper.</remarks>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IAsyncMapper?
#else
			IAsyncMapper
#endif
			Mapper {
				get;
#if NET5_0_OR_GREATER
				init;
#endif
		}

		/// <summary>
		/// Service provider to use for the maps.
		/// </summary>
		/// <remarks><see langword="null"/> to use the one provided by the parent mapper.</remarks>
		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			ServiceProvider {
				get;
#if NET5_0_OR_GREATER
				init;
#endif
		}
	}
}
