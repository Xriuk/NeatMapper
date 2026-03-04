using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="CompositeMapper"/>.<br/>
	/// Can be overridden during mapping with <see cref=CompositeMapperMappingOptions"/>.
	/// </summary>
	public sealed class CompositeMapperOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapperOptions"/>.
		/// </summary>
		public CompositeMapperOptions() {
			Mappers = [];
			MergeMapsHandling = MergeMapsHandling.CreateDestination;
		}
		/// <summary>
		/// Creates a new instance of <see cref="CompositeMapperOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CompositeMapperOptions(CompositeMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			Mappers = new List<IMapper>(options.Mappers ?? throw new InvalidOperationException("Mappers cannot be null"));
			MergeMapsHandling = options.MergeMapsHandling;
		}


		/// <summary>
		/// Ordered list of mappers, each mapper will be tried and the first one to succeed will map the types.
		/// </summary>
		public IList<IMapper> Mappers { get; set; }

		/// <summary>
		/// Specifies how to forward new maps to merge maps.
		/// </summary>
		/// <remarks>Defaults to <see cref="MergeMapsHandling.CreateDestination"/>.</remarks>
		public MergeMapsHandling MergeMapsHandling { get; set; }
	}
}
