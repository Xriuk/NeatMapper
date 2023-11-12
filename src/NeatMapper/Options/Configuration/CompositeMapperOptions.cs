using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IMapper"/>s to use for <see cref="CompositeMapper"/>
	/// </summary>
	/// <remarks>
	/// Configuration should be done by using <see cref="Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions.ConfigureAll{TOptions}"/>
	/// or <see cref="Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions.PostConfigureAll{TOptions}"/>
	/// </remarks>
	public sealed class CompositeMapperOptions {
		/// <summary>
		/// Named <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/> which is used to configure any mapper
		/// which requires an <see cref="IMapper"/> itself, these do not include <see cref="CollectionMapper"/>s
		/// </summary>
		public const string Base = "Base";


		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CompositeMapperOptions() {
			Mappers = new List<IMapper>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CompositeMapperOptions(CompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IMapper>(options.Mappers);
		}


		public IList<IMapper> Mappers { get; set; }
	}
}
