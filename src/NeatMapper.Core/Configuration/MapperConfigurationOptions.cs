﻿namespace NeatMapper.Core.Configuration {
	/// <summary>
	/// Options applied to automatic collections mapping via <see cref="IMergeMap{TSource, TDestination}"/>
	/// </summary>
	public sealed class MergeMapsCollectionsOptions {
		public MergeMapsCollectionsOptions() { }
		public MergeMapsCollectionsOptions(MergeMapsCollectionsOptions options) {
			RemoveNotMatchedDestinationElements = options.RemoveNotMatchedDestinationElements;
		}


		/// <summary>
		/// If true, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="ICollectionElementComparer{TSource, TDestination}"/>
		/// </summary>
		/// <remarks>Defaults to true</remarks>
		public bool RemoveNotMatchedDestinationElements { get; set; } = true;
	}

	/// <summary>
	/// Global mapping options for an <see cref="IMapper"/>
	/// </summary>
	public sealed class MapperConfigurationOptions {
		public MapperConfigurationOptions() { }
		public MapperConfigurationOptions(MapperConfigurationOptions options) {
			ScanTypes = options.ScanTypes.ToList();
			MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions(options.MergeMapsCollectionsOptions);
		}


		/// <summary>
		/// Types which to scan for:<br/>
		/// <see cref="INewMap{TSource, TDestination}"/><br/>
		/// <see cref="IMergeMap{TSource, TDestination}"/><br/>
		/// <see cref="IAsyncNewMap{TSource, TDestination}"/><br/>
		/// <see cref="IAsyncMergeMap{TSource, TDestination}"/><br/>
		/// <see cref="ICollectionElementComparer{TSource, TDestination}"/>
		/// </summary>
		public ICollection<Type> ScanTypes { get; set; } = new List<Type>();

		/// <summary>
		/// Options applied to automatic collections mapping via <see cref="IMergeMap{TSource, TDestination}"/> and <see cref="IAsyncMergeMap{TSource, TDestination}"/>
		/// </summary>
		public MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; set; } = new MergeMapsCollectionsOptions();
	}
}