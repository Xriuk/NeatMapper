using System.Collections.Generic;
using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to mappers/matchers/projectors with user-defined mappings, which allow to specify types
	/// to scan for custom maps.
	/// </summary>
	public sealed class CustomMapsOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CustomMapsOptions"/>.
		/// </summary>
		public CustomMapsOptions() {
			TypesToScan = [];
		}
		/// <summary>
		/// Creates a new instance of <see cref="CustomMapsOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CustomMapsOptions(CustomMapsOptions options) {
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			TypesToScan = new List<Type>(options.TypesToScan ?? throw new InvalidOperationException("TypesToScan cannot be null"));
		}


		/// <summary>
		/// Types which to scan for custom maps.
		/// </summary>
		public ICollection<Type> TypesToScan { get; set; }
	}
}
