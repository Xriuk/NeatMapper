using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options used to define a list of <see cref="IAsyncMapper"/>s to use for <see cref="AsyncCompositeMapper"/>
	/// </summary>
	/// <remarks>
	/// Configuration should be done by using <see cref="Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions.ConfigureAll{TOptions}"/>
	/// or <see cref="Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions.PostConfigureAll{TOptions}"/>
	/// </remarks>
	public sealed class AsyncCompositeMapperOptions {
		/// <summary>
		/// Named <see cref="Microsoft.Extensions.Options.IOptions{TOptions}"/> which is used to configure any mapper
		/// which requires an <see cref="IAsyncMapper"/> itself, these do not include <see cref="AsyncCollectionMapper"/>s
		/// </summary>
		public const string Base = "Base";


		/// <summary>
		/// Creates a new instance
		/// </summary>
		public AsyncCompositeMapperOptions() {
			Mappers = new List<IAsyncMapper>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public AsyncCompositeMapperOptions(AsyncCompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IAsyncMapper>(options.Mappers);
		}


		public IList<IAsyncMapper> Mappers { get; set; }
	}
}
