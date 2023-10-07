﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Configuration {
	/// <summary>
	/// Options applied to automatic collections mapping via <see cref="MapperConfiguration.MergeMaps"/> or <see cref="MapperConfiguration.GenericMergeMaps"/>
	/// </summary>
	public sealed class MergeMapsCollectionsOptions {
		public MergeMapsCollectionsOptions() { }
		public MergeMapsCollectionsOptions(MergeMapsCollectionsOptions options) {
			RemoveNotMatchedDestinationElements = options.RemoveNotMatchedDestinationElements;
		}


		/// <summary>
		/// If true, will remove all the elements from destination which do not have a corresponding element in source,
		/// matched with <see cref="IMatchMap{TSource, TDestination}"/>
		/// </summary>
		/// <remarks>Defaults to true</remarks>
		public bool RemoveNotMatchedDestinationElements { get; set; } = true;
	}

	/// <summary>
	/// Global mapping options for a mapper
	/// </summary>
	public sealed class MapperConfigurationOptions {
		public MapperConfigurationOptions() { }
		public MapperConfigurationOptions(MapperConfigurationOptions options) {
			ScanTypes = options.ScanTypes.ToList();
			MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions(options.MergeMapsCollectionsOptions);
		}


		/// <summary>
		/// Types which to scan for:<br/>
		/// - NewMap<br/>
		/// - MergeMap<br/>
		/// - MatchMap<br/>
		/// - Generic versions of the above
		/// </summary>
		public ICollection<Type> ScanTypes { get; set; } = new List<Type>();

		/// <summary>
		/// Options applied to automatic collections mapping via <see cref="MapperConfiguration.MergeMaps"/> or <see cref="MapperConfiguration.GenericMergeMaps"/>
		/// </summary>
		public MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; set; } = new MergeMapsCollectionsOptions();
	}
}
