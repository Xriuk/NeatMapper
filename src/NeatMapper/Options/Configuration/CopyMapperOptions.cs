using System;

namespace NeatMapper {
	/// <summary>
	/// Options applied to <see cref="CopyMapper"/>.<br/>
	/// Can be overridden during mapping with <see cref="CopyMapperMappingOptions"/>.
	/// </summary>
	public sealed class CopyMapperOptions {
		/// <summary>
		/// Creates a new instance of <see cref="CopyMapperOptions"/>.
		/// </summary>
		public CopyMapperOptions() {
			PropertiesToMap = MemberVisibilityFlags.Public;
			FieldsToMap = MemberVisibilityFlags.Public;
			DeepCopy = DeepCopyFlags.None;
		}
		/// <summary>
		/// Creates a new instance of <see cref="CopyMapperOptions"/> by copying options from another instance.
		/// </summary>
		/// <param name="options">Options to copy from.</param>
		public CopyMapperOptions(CopyMapperOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			PropertiesToMap = options.PropertiesToMap;
			FieldsToMap = options.FieldsToMap;
			DeepCopy = options.DeepCopy;
		}


		/// <summary>
		/// Specifies which properties to map, all properties will need to be readable and writable.
		/// </summary>
		/// <remarks>Defaults to <see cref="MemberVisibilityFlags.Public"/>.</remarks>
		public MemberVisibilityFlags PropertiesToMap { get; set; }

		/// <summary>
		/// Specifies which fields to map.
		/// </summary>
		/// <remarks>Defaults to <see cref="MemberVisibilityFlags.Public"/>.</remarks>
		public MemberVisibilityFlags FieldsToMap { get; set; }

		/// <summary>
		/// Specifies how deep copy will be handled.
		/// </summary>
		/// <remarks>Defaults to <see cref="DeepCopyFlags.None"/>.</remarks>
		public DeepCopyFlags DeepCopy { get; set; }
	}
}
