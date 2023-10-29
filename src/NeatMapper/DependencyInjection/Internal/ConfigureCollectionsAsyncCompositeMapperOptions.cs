using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace NeatMapper.DependencyInjection.Internal {
	internal class ConfigureCollectionsAsyncCompositeMapperOptions : IConfigureOptions<AsyncCompositeMapperOptions> {
		private IServiceProvider _serviceProvider;

		public ConfigureCollectionsAsyncCompositeMapperOptions(IServiceProvider serviceProvider) {
			_serviceProvider = serviceProvider;
		}

		public void Configure(AsyncCompositeMapperOptions options) {
			options.Mappers.Add(_serviceProvider.GetRequiredService<AsyncNewCollectionMapper>());
			options.Mappers.Add(_serviceProvider.GetRequiredService<AsyncMergeCollectionMapper>());
		}
	}
}
