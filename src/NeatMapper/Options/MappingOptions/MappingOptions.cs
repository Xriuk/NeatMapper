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
		/// <summary>
		/// Empty instance with no options inside
		/// </summary>
		public static readonly MappingOptions Empty = new MappingOptions(null);


		private readonly IDictionary<Type, object> options;


		public MappingOptions(
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			IEnumerable?
#else
			IEnumerable
#endif
			options) {

			if(options != null) {
				options = options.Cast<object>().Where(o => o != null);
				if(options.Cast<object>().GroupBy(o => o.GetType()).Any(g => g.Count() > 1))
					throw new ArgumentException("Options of the same type must be grouped together");

				this.options = options.Cast<object>().ToDictionary(o => o.GetType(), o => o);
			}
			else
				this.options = new Dictionary<Type, object>();
		}


		public
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			GetOptions(Type optionsType){

			if(optionsType == null)
				throw new ArgumentNullException(nameof(optionsType));

			if(this.options.TryGetValue(optionsType, out var options))
				return options;
			else
				return null;
		}

		public MappingOptions Replace(IDictionary<Type, Func<object,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			>> factories) {

			// DEV: run factories, and if all options are the same do not create new object

			if (factories == null)
				throw new ArgumentNullException(nameof(factories));
			if(factories.Count == 0)
				return Empty;

			return new MappingOptions(
				AsEnumerable()
				.Select(o => {
					if(factories.TryGetValue(o.GetType(), out var factory))
						return factory.Invoke(o);
					else
						return o;
				})
				.Where(o => o != null)
			);
		}


		public MappingOptions ReplaceOrAdd(IDictionary<Type, Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			object?
#else
			object
#endif
			>> factories) {

			// DEV: run factories, and if all options are the same do not create new object

			if (factories == null)
				throw new ArgumentNullException(nameof(factories));
			if (factories.Count == 0)
				return Empty;

			return new MappingOptions(factories
				.Select(f => f.Value.Invoke(GetOptions(f.Key)))
				.Where(o => o != null)
				.Concat(options
					.Where(o => !factories.ContainsKey(o.Key))
					.Select(o => o.Value))
			);
		}

		public IEnumerable<object> AsEnumerable() {
			return options.Values;
		}
	}
}
