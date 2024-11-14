using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper {
	/// <summary>
	/// Additional options, contains multiple options of different types,
	/// each mapper/map should try to retrieve its options and use them.
	/// </summary>
	/// <remarks>All options inside are assumed to be immutable.</remarks>
	public sealed class MappingOptions {
		private static readonly IReadOnlyDictionary<Type, object> emptyDictionary = new Dictionary<Type, object>();

		/// <summary>
		/// Empty instance with no options inside.
		/// </summary>
		public static readonly MappingOptions Empty = new MappingOptions((IEnumerable?)null, true);


		/// <summary>
		/// Types and instances of the present options
		/// </summary>
		private readonly IReadOnlyDictionary<Type, object> options;

		internal bool Cached { get; }


		/// <summary>
		/// Creates a new instance of <see cref="MappingOptions"/>.
		/// </summary>
		/// <param name="options">Objects to add to options, must be unique by type.</param>
		/// <param name="cached">
		/// True if the options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <exception cref="ArgumentException">
		/// <paramref name="options"/> have more than one instance for the same type.
		/// </exception>
		public MappingOptions(IEnumerable? options,bool cached = false) {
			if(options != null) {
				options = options.Cast<object>().Where(o => o != null);

				try { 
					this.options = options.Cast<object>().ToDictionary(o => o.GetType(), o => o);
				}
				catch {
					throw new ArgumentException("Options of the same type must be grouped together", nameof(options));
				}
			}
			else
				this.options = emptyDictionary;

			Cached = cached;
		}

		/// <inheritdoc cref="MappingOptions(IEnumerable, bool)"/>
		public MappingOptions(params object?[]? options) : this(options?.Length > 0 ? (IEnumerable)options : null) { }

		// Internal faster constructor
		internal MappingOptions(IEnumerable<KeyValuePair<Type, object>> options,
			bool cached = false) {

#if NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
			this.options = new Dictionary<Type, object>(options);
#else
			this.options = options.ToDictionary(o => o.Key, o => o.Value);
#endif

			Cached = cached;
		}


		/// <summary>
		/// Retrieves an option.
		/// </summary>
		/// <param name="optionsType">Type of the option to retrieve.</param>
		/// <returns>The retrieved option if found or <see langword="null"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="optionsType"/> was null.</exception>
		public object? GetOptions(Type optionsType){

			if(optionsType == null)
				throw new ArgumentNullException(nameof(optionsType));

			if(this.options.TryGetValue(optionsType, out var options))
				return options;
			else
				return null;
		}

		/// <summary>
		/// Creates a new instance of <see cref="MappingOptions"/> by copying options from the current instance and
		/// replacing the specified options (only if found) with new options from the provided factories.
		/// </summary>
		/// <param name="factories">
		/// Factories to invoke, at most one for each option type, if the option type is found
		/// the corresponding factory will be invoked with the retrieved option and the result will be added
		/// to the new options (if non-<see langword="null"/>) or removed (if <see langword="null"/>).<br/>
		/// If no factories are provided the current instance will be returned unaltered.
		/// </param>
		/// <param name="cached">
		/// True if the new options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <returns>
		/// The new instance of <see cref="MappingOptions"/> with the options copied from the current instance and
		/// replaced with the provided factories.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="factories"/> was null.</exception>
		public MappingOptions Replace(IDictionary<Type, Func<object, object?>> factories, bool cached = false) {
			if (factories == null)
				throw new ArgumentNullException(nameof(factories));
			if(factories.Count == 0)
				return this;

			// Run factories, and if all options are the same do not create new object
			bool changed = false;
			var newValues = factories
				.Select(f => {
					var opt = GetOptions(f.Key);
					if(opt == null)
						return new KeyValuePair<Type, object>(f.Key, opt!);

					var ret = f.Value.Invoke(opt);
					if(!changed && !object.Equals(ret, opt))
						changed = true;
					return new KeyValuePair<Type, object>(f.Key, ret!);
				})
				.Where(o => o.Value != null)
				.ToList();

			if(!changed)
				return this;

			return new MappingOptions(newValues.Concat(options.Where(o => !factories.ContainsKey(o.Key))), cached);
		}

		/// <summary>
		/// Creates a new instance of <see cref="MappingOptions"/> by copying options from the current instance and
		/// replacing (or adding) the specified options (even if not found) with new options from the provided factories.
		/// </summary>
		/// <param name="factories">
		/// Factories to invoke, at most one for each option type, the corresponding factory will be invoked
		/// with the retrieved option of the given type (if found, or <see langword="null"/> will be passed otherwise)
		/// and the result will be added to the new options (if non-<see langword="null"/>) or removed
		/// (if <see langword="null"/>).<br/>
		/// If no factories are provided the current instance will be returned unaltered.
		/// </param>
		/// <param name="cached">
		/// True if the new options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <returns>
		/// The new instance of <see cref="MappingOptions"/> with the options copied from the current instance and
		/// replaced/added with the provided factories.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="factories"/> was null.</exception>
		public MappingOptions ReplaceOrAdd(IDictionary<Type, Func<object?, object?>> factories, bool cached = false) {
			if (factories == null)
				throw new ArgumentNullException(nameof(factories));
			if (factories.Count == 0)
				return this;

			// Run factories, and if all options are the same do not create new object
			bool changed = false;
			var newValues = factories
				.Select(f => {
					var opt = GetOptions(f.Key);
					var ret = f.Value.Invoke(opt);
					if (!object.Equals(ret, opt))
						changed = true;
					return new KeyValuePair<Type, object>(f.Key, ret!);
				})
				.Where(o => o.Value != null)
				.ToList();

			if (!changed)
				return this;

			return new MappingOptions(newValues.Concat(options.Where(o => !factories.ContainsKey(o.Key))), cached);
		}

		/// <summary>
		/// Returns an enumeration of the options present.
		/// </summary>
		/// <returns>Instances of the options present.</returns>
		public IEnumerable<object> AsEnumerable() {
			return options.Values;
		}
	}
}
