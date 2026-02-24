using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to projectors.<br/>
	/// Can be overridden during mapping with <see cref="ProjectorsMappingOptions"/>.
	/// </summary>
	public sealed class ProjectorsOptions {
		/// <summary>
		/// Creates a new instance of <see cref="ProjectorsOptions"/>.
		/// </summary>
		public ProjectorsOptions() {
			NullChecks = true;
		}
		/// <summary>
		/// Creates a new instance of <see cref="ProjectorsOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public ProjectorsOptions(ProjectorsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			NullChecks = options.NullChecks;
		}


		/// <summary>
		/// If <see langword="true"/> will add null checks for <see cref="Nullable{T}"/>
		/// and nullable reference types (NRTs) otherwise will leave the generated expressions
		/// as they are. Should be used if any library handles null values already like EF Core
		/// for example.
		/// </summary>
		/// <remarks>Defaults to <see langword="true"/>.</remarks>
		public bool NullChecks { get; set; }
	}
}
