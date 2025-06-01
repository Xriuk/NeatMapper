namespace NeatMapper {
	/// <summary>
	/// Singleton <see cref="IAsyncMapper"/> which returns the provided source element (for both new and merge maps).
	/// Supports only the same source/destination types. Can be used to merge collections of elements of the same type.
	/// </summary>
	public static class AsyncIdentityMapper{
		/// <summary>
		/// Singleton instance of the mapper.
		/// </summary>
		public static readonly IAsyncMapper Instance = new AsyncIMapperWrapperMapper(IdentityMapper.Instance);
	}
}
