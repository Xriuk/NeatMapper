using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace NeatMapper.Async {
	public class AsyncMapper : BaseMapper, IAsyncMapper {
		protected override MatchingContext MatchingContext { get; }

		public AsyncMapper(MapperConfigurationOptions configuration,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IServiceProvider?
#else
			IServiceProvider
#endif
			serviceProvider = null) :
				base(new MapperConfiguration(i => i == typeof(IAsyncNewMap<,>)
#if NET7_0_OR_GREATER
					|| i == typeof(IAsyncNewMapStatic<,>)
#endif
					,
					i => i == typeof(IAsyncMergeMap<,>)
#if NET7_0_OR_GREATER
					|| i == typeof(IAsyncMergeMapStatic<,>)
#endif
					, configuration ?? new MapperConfigurationOptions()), serviceProvider) {

			MatchingContext = new MatchingContext {
				ServiceProvider = serviceProvider,
				Matcher = this
			};
		}

		public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				object?
#else
				object
#endif
			> MapAsync(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
			Type destinationType,
			CancellationToken cancellationToken = default) {


		}

		public Task<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			> MapAsync(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			source,
			Type sourceType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			destination,
			Type destinationType,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			MappingOptions?
#else
			MappingOptions
#endif
			mappingOptions = null,
			CancellationToken cancellationToken = default) {


		}
	}
}
