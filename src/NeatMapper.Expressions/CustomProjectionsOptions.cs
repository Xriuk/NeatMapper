namespace NeatMapper.Expressions {
	public sealed class CustomProjectionsOptions {
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CustomProjectionsOptions() {
			TypesToScan = new List<Type>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CustomProjectionsOptions(CustomProjectionsOptions options) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			TypesToScan = new List<Type>(options.TypesToScan);
		}


		/// <summary>
		/// Types which to scan for custom projection maps
		/// </summary>
		public ICollection<Type> TypesToScan { get; set; }
	}
}
