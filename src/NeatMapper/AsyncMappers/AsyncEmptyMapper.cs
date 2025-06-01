namespace NeatMapper{
	/// <summary>
	/// Singleton <see cref="IAsyncMapper"/> which cannot map any type.
	/// </summary>
	public static class AsyncEmptyMapper {
		/// <summary>
		/// Singleton instance of the mapper
		/// </summary>
		public static readonly IAsyncMapper Instance = new AsyncIMapperWrapperMapper(EmptyMapper.Instance);
	}
}
