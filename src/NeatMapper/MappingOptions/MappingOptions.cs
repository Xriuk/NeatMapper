using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// Additional options, contains multiple options of different types,
	/// each mapper/map should try to retrieve its options and use them
	/// </summary>
	public sealed class MappingOptions {
		private readonly IDictionary<Type, object> options;

		public MappingOptions(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			options) {

			if(options != null) { 
				if(options.Cast<object>().GroupBy(o => o.GetType()).Any(g => g.Count() > 1))
					throw new ArgumentException("Options of the same type must be grouped together");

				this.options = options.Cast<object>().ToDictionary(o => o.GetType(), o => o);
			}
			else
				this.options = new Dictionary<Type, object>();
		}


		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TOptions?
#else
			TOptions
#endif
			GetOptions<TOptions>() where TOptions : class {

			if(this.options.TryGetValue(typeof(TOptions), out var options))
				return options as TOptions;
			else
				return null;
		}

		public IEnumerable<object> AsEnumerable() {
			return options.Values;
		}
	}
}
