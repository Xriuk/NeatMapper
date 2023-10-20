﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace NeatMapper.Configuration {
	/// <summary>
	/// Options applied to mappers with user-defined mappings
	/// </summary>
	public sealed class CustomMapsOptions {
		/// <summary>
		/// Creates a new instance
		/// </summary>
		public CustomMapsOptions() {
			TypesToScan = new List<Type>();
		}
		/// <summary>
		/// Creates a new instance by copying options from another instance
		/// </summary>
		/// <param name="options">Options to copy from</param>
		public CustomMapsOptions(CustomMapsOptions options) {
			if(options == null)
				throw new ArgumentNullException(nameof(options));

			TypesToScan = options.TypesToScan.ToList();
		}


		/// <summary>
		/// Types which to scan for custom maps
		/// </summary>
		public ICollection<Type> TypesToScan { get; set; }
	}
}
