using NeatMapper.Common.Mapper;
using NeatMapper.Configuration;

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


	}
}
