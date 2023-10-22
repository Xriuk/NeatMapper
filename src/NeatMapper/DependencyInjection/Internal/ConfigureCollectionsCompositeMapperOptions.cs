using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace NeatMapper.DependencyInjection.Internal {
	internal class ConfigureCollectionsCompositeMapperOptions : IConfigureOptions<CompositeMapperOptions> {
		private IServiceProvider _serviceProvider;

		public ConfigureCollectionsCompositeMapperOptions(IServiceProvider serviceProvider) {
			_serviceProvider = serviceProvider;
		}

		public void Configure(CompositeMapperOptions options) {
			options.Mappers.Add(_serviceProvider.GetRequiredService<NewCollectionMapper>());
			options.Mappers.Add(_serviceProvider.GetRequiredService<MergeCollectionMapper>());
		}
	}
}
