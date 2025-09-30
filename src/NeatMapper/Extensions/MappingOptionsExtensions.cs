using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NeatMapper {
	public static class MappingOptionsExtensions {
		/// <summary>
		/// Retrieves options of type <typeparamref name="TOptions"/> from the current mapping options.
		/// </summary>
		/// <typeparam name="TOptions">Type of the options to retrieve.</typeparam>
		/// <returns>The retrieved options, <see langword="null"/> if not found.</returns>
		public static TOptions? GetOptions<TOptions>(this MappingOptions options) where TOptions : class {
			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.GetOptions(typeof(TOptions)) as TOptions;
		}

		/// <summary>
		/// Checks if options of type <typeparamref name="TOptions"/> are present in the current mapping options.
		/// </summary>
		/// <typeparam name="TOptions">Type of the options to retrieve.</typeparam>
		/// <returns>True if the options exist, false otherwise.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasOptions<TOptions>(this MappingOptions options) where TOptions : class {
			return options.GetOptions<TOptions>() != null;
		}

		#region Replace
		/// <summary>
		/// Creates a new instance of <see cref="MappingOptions"/> by copying options from the current instance
		/// and replaces (or removes) options of type <typeparamref name="TOptions"/> (only if found)
		/// with a new option from the specified factory.
		/// </summary>
		/// <typeparam name="TOptions">Type of the options to replace.</typeparam>
		/// <param name="factory">
		/// Factory used to create the new options, will be invoked only if an option of type
		/// <typeparamref name="TOptions"/> exists in the current mapping options, will receive it as a parameter.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied into the new instance.
		/// </param>
		/// <param name="cached">
		/// True if the new options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <returns>
		/// The new instance of <see cref="MappingOptions"/> with the options copied from the current instance and
		/// replaced with the provided factory. Or the same instance if no options were changed.
		/// </returns>
		public static MappingOptions Replace<TOptions>(this MappingOptions options,
			Func<TOptions, TOptions?> factory,
			bool cached = false) where TOptions : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			return options.Replace(new Dictionary<Type, Func<object, object?>> {
				{ typeof(TOptions), o => o is TOptions opts ? factory.Invoke(opts) : null }
			}, cached);
		}

		/// <inheritdoc cref="Replace{TOptions1, TOptions2, TOptions3, TOptions4, TOptions5}(MappingOptions, Func{TOptions1, TOptions1}, Func{TOptions2, TOptions2}, Func{TOptions3, TOptions3}, Func{TOptions4, TOptions4}, Func{TOptions5, TOptions5}, bool)"/>
		public static MappingOptions Replace<TOptions1, TOptions2>(this MappingOptions options,
			Func<TOptions1, TOptions1?> factory1,
			Func<TOptions2, TOptions2?> factory2,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));

			return options.Replace(new Dictionary<Type, Func<object, object?>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? factory2.Invoke(opts) : null }
			}, cached);
		}

		/// <inheritdoc cref="Replace{TOptions1, TOptions2, TOptions3, TOptions4, TOptions5}(MappingOptions, Func{TOptions1, TOptions1}, Func{TOptions2, TOptions2}, Func{TOptions3, TOptions3}, Func{TOptions4, TOptions4}, Func{TOptions5, TOptions5}, bool)"/>
		public static MappingOptions Replace<TOptions1, TOptions2, TOptions3>(this MappingOptions options,
			Func<TOptions1, TOptions1?> factory1,
			Func<TOptions2, TOptions2?> factory2,
			Func<TOptions3, TOptions3?> factory3,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));
			if (factory3 == null)
				throw new ArgumentNullException(nameof(factory3));

			return options.Replace(new Dictionary<Type, Func<object, object?>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? factory2.Invoke(opts) : null },
				{ typeof(TOptions3), o => o is TOptions3 opts ? factory3.Invoke(opts) : null }
			}, cached);
		}

		/// <inheritdoc cref="Replace{TOptions1, TOptions2, TOptions3, TOptions4, TOptions5}(MappingOptions, Func{TOptions1, TOptions1}, Func{TOptions2, TOptions2}, Func{TOptions3, TOptions3}, Func{TOptions4, TOptions4}, Func{TOptions5, TOptions5}, bool)"/>
		public static MappingOptions Replace<TOptions1, TOptions2, TOptions3, TOptions4>(this MappingOptions options,
			Func<TOptions1, TOptions1?> factory1,
			Func<TOptions2, TOptions2?> factory2,
			Func<TOptions3, TOptions3?> factory3,
			Func<TOptions4, TOptions4?> factory4,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));
			if (factory3 == null)
				throw new ArgumentNullException(nameof(factory3));
			if (factory4 == null)
				throw new ArgumentNullException(nameof(factory4));

			return options.Replace(new Dictionary<Type, Func<object, object?>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? factory2.Invoke(opts) : null },
				{ typeof(TOptions3), o => o is TOptions3 opts ? factory3.Invoke(opts) : null },
				{ typeof(TOptions4), o => o is TOptions4 opts ? factory4.Invoke(opts) : null }
			}, cached);
		}

		/// <summary>
		/// Creates a new instance of <see cref="MappingOptions"/> by copying options from the current instance
		/// and replaces (or removes) the specified option types (only if found)
		/// with new options from the specified factories.
		/// </summary>
		/// <typeparam name="TOptions1">Type of the first options to replace.</typeparam>
		/// <typeparam name="TOptions2">Type of the second options to replace.</typeparam>
		/// <typeparam name="TOptions3">Type of the third options to replace.</typeparam>
		/// <typeparam name="TOptions4">Type of the fourth options to replace.</typeparam>
		/// <typeparam name="TOptions5">Type of the fifth options to replace.</typeparam>
		/// <param name="factory1">
		/// Factory used to create the new options, will be invoked only if an option of type
		/// <typeparamref name="TOptions1"/> exists in the current mapping options, will receive it as a parameter.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied into the new instance.
		/// </param>
		/// <param name="factory2">
		/// Factory used to create the new options, will be invoked only if an option of type
		/// <typeparamref name="TOptions2"/> exists in the current mapping options, will receive it as a parameter.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied into the new instance.
		/// </param>
		/// <param name="factory3">
		/// Factory used to create the new options, will be invoked only if an option of type
		/// <typeparamref name="TOptions3"/> exists in the current mapping options, will receive it as a parameter.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied into the new instance.
		/// </param>
		/// <param name="factory4">
		/// Factory used to create the new options, will be invoked only if an option of type
		/// <typeparamref name="TOptions4"/> exists in the current mapping options, will receive it as a parameter.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied into the new instance.
		/// </param>
		/// <param name="factory5">
		/// Factory used to create the new options, will be invoked only if an option of type
		/// <typeparamref name="TOptions5"/> exists in the current mapping options, will receive it as a parameter.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied into the new instance.
		/// </param>
		/// <param name="cached">
		/// True if the new options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <returns>
		/// The new instance of <see cref="MappingOptions"/> with the options copied from the current instance and
		/// replaced with the provided factories. Or the same instances if no options were changed.
		/// </returns>
		public static MappingOptions Replace<TOptions1, TOptions2, TOptions3, TOptions4, TOptions5>(this MappingOptions options,
			Func<TOptions1, TOptions1?> factory1,
			Func<TOptions2, TOptions2?> factory2,
			Func<TOptions3, TOptions3?> factory3,
			Func<TOptions4, TOptions4?> factory4,
			Func<TOptions5, TOptions5?> factory5,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class
				where TOptions5 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));
			if (factory3 == null)
				throw new ArgumentNullException(nameof(factory3));
			if (factory4 == null)
				throw new ArgumentNullException(nameof(factory4));
			if (factory5 == null)
				throw new ArgumentNullException(nameof(factory5));

			return options.Replace(new Dictionary<Type, Func<object, object?>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? factory2.Invoke(opts) : null },
				{ typeof(TOptions3), o => o is TOptions3 opts ? factory3.Invoke(opts) : null },
				{ typeof(TOptions4), o => o is TOptions4 opts ? factory4.Invoke(opts) : null },
				{ typeof(TOptions5), o => o is TOptions5 opts ? factory5.Invoke(opts) : null }
			}, cached);
		}
		#endregion

		#region ReplaceOrAdd
		/// <summary>
		/// Creates a new <see cref="MappingOptions"/> object with all the options copied from the current instance
		/// and replaces (or removes) or adds the specified option types with new options from the specified factories.
		/// </summary>
		/// <typeparam name="TOptions">Type of the first options to replace or add.</typeparam>
		/// <param name="factory">
		/// Factory used to create the new options, will receive as a parameter the options of type
		/// <typeparamref name="TOptions"/> if they exist in the current mapping options,
		/// will receive null otherwise.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied/added into the new instance.
		/// </param>
		/// <param name="cached">
		/// True if the new options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <returns>The new generated options.</returns>
		public static MappingOptions ReplaceOrAdd<TOptions>(this MappingOptions options,
			Func<TOptions?, TOptions?> factory,
			bool cached = false) where TOptions : class {
			 
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory == null)
				throw new ArgumentNullException(nameof(factory));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object?, object?>> {
				{ typeof(TOptions), o => factory.Invoke(o as TOptions) }
			}, cached);
		}

		/// <inheritdoc cref="ReplaceOrAdd{TOptions1, TOptions2, TOptions3, TOptions4, TOptions5}(MappingOptions, Func{TOptions1, TOptions1}, Func{TOptions2, TOptions2}, Func{TOptions3, TOptions3}, Func{TOptions4, TOptions4}, Func{TOptions5, TOptions5}, bool)"/>
		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2>(this MappingOptions options,
			Func<TOptions1?, TOptions1?> factory1,
			Func<TOptions2?, TOptions2?> factory2,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object?, object?>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) }
			}, cached);
		}

		/// <inheritdoc cref="ReplaceOrAdd{TOptions1, TOptions2, TOptions3, TOptions4, TOptions5}(MappingOptions, Func{TOptions1, TOptions1}, Func{TOptions2, TOptions2}, Func{TOptions3, TOptions3}, Func{TOptions4, TOptions4}, Func{TOptions5, TOptions5}, bool)"/>
		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2, TOptions3>(this MappingOptions options,
			Func<TOptions1?, TOptions1?> factory1,
			Func<TOptions2?, TOptions2?> factory2,
			Func<TOptions3?, TOptions3?> factory3,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));
			if (factory3 == null)
				throw new ArgumentNullException(nameof(factory3));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object?, object?>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) },
				{ typeof(TOptions3), o => factory3.Invoke(o as TOptions3) }
			}, cached);
		}

		/// <inheritdoc cref="ReplaceOrAdd{TOptions1, TOptions2, TOptions3, TOptions4, TOptions5}(MappingOptions, Func{TOptions1, TOptions1}, Func{TOptions2, TOptions2}, Func{TOptions3, TOptions3}, Func{TOptions4, TOptions4}, Func{TOptions5, TOptions5}, bool)"/>
		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2, TOptions3, TOptions4>(this MappingOptions options,
			Func<TOptions1?, TOptions1?> factory1,
			Func<TOptions2?, TOptions2?> factory2,
			Func<TOptions3?, TOptions3?> factory3,
			Func<TOptions4?, TOptions4?> factory4,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));
			if (factory3 == null)
				throw new ArgumentNullException(nameof(factory3));
			if (factory4 == null)
				throw new ArgumentNullException(nameof(factory4));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object?, object?>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) },
				{ typeof(TOptions3), o => factory3.Invoke(o as TOptions3) },
				{ typeof(TOptions4), o => factory4.Invoke(o as TOptions4) }
			}, cached);
		}

		/// <summary>
		/// Creates a new <see cref="MappingOptions"/> object with all the options copied from the current instance
		/// and replaces (or removes) or adds the specified option types with new options from the specified factories.
		/// </summary>
		/// <typeparam name="TOptions1">Type of the first options to replace or add.</typeparam>
		/// <typeparam name="TOptions2">Type of the second options to replace or add.</typeparam>
		/// <typeparam name="TOptions3">Type of the third options to replace or add.</typeparam>
		/// <typeparam name="TOptions4">Type of the fourth options to replace or add.</typeparam>
		/// <typeparam name="TOptions5">Type of the fifth options to replace or add.</typeparam>
		/// <param name="factory1">
		/// Factory used to create the new options, will receive as a parameter the options of type
		/// <typeparamref name="TOptions1"/> if they exist in the current mapping options,
		/// will receive null otherwise.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied/added into the new instance.
		/// </param>
		/// <param name="factory2">
		/// Factory used to create the new options, will receive as a parameter the options of type
		/// <typeparamref name="TOptions2"/> if they exist in the current mapping options,
		/// will receive null otherwise.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied/added into the new instance.
		/// </param>
		/// <param name="factory3">
		/// Factory used to create the new options, will receive as a parameter the options of type
		/// <typeparamref name="TOptions3"/> if they exist in the current mapping options,
		/// will receive null otherwise.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied/added into the new instance.
		/// </param>
		/// <param name="factory4">
		/// Factory used to create the new options, will receive as a parameter the options of type
		/// <typeparamref name="TOptions4"/> if they exist in the current mapping options,
		/// will receive null otherwise.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied/added into the new instance.
		/// </param>
		/// <param name="factory5">
		/// Factory used to create the new options, will receive as a parameter the options of type
		/// <typeparamref name="TOptions5"/> if they exist in the current mapping options,
		/// will receive null otherwise.<br/>
		/// If it returns <see langword="null"/> no options of that type will be copied/added into the new instance.
		/// </param>
		/// <param name="cached">
		/// True if the new options created will be reused, and thus can be cached in the mapping chain,
		/// to avoid recomputing them.
		/// </param>
		/// <returns>The new generated options.</returns>
		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2, TOptions3, TOptions4, TOptions5>(this MappingOptions options,
			Func<TOptions1?, TOptions1?> factory1,
			Func<TOptions2?, TOptions2?> factory2,
			Func<TOptions3?, TOptions3?> factory3,
			Func<TOptions4?, TOptions4?> factory4,
			Func<TOptions5?, TOptions5?> factory5,
			bool cached = false)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class
				where TOptions5 : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (factory1 == null)
				throw new ArgumentNullException(nameof(factory1));
			if (factory2 == null)
				throw new ArgumentNullException(nameof(factory2));
			if (factory3 == null)
				throw new ArgumentNullException(nameof(factory3));
			if (factory4 == null)
				throw new ArgumentNullException(nameof(factory4));
			if (factory5 == null)
				throw new ArgumentNullException(nameof(factory5));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object?, object?>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) },
				{ typeof(TOptions3), o => factory3.Invoke(o as TOptions3) },
				{ typeof(TOptions4), o => factory4.Invoke(o as TOptions4) },
				{ typeof(TOptions5), o => factory5.Invoke(o as TOptions5) }
			}, cached);
		}
		#endregion

		/// <summary>
		/// Creates a new <see cref="MappingOptions"/> object with all the options copied from the current instance
		/// and adds (or sets) the specified matchers to be used to match elements when merging collections.
		/// </summary>
		/// <param name="matchers">Matchers to add to existing ones (or to set if no matcher exists).</param>
		/// <returns>The new generated options.</returns>
		public static MappingOptions AddMergeCollectionMatchers(this MappingOptions options, params IMatcher[] matchers) {
			if (options == null)
				throw new ArgumentNullException(nameof(options));
			if (matchers == null)
				throw new ArgumentNullException(nameof(matchers));

			if (matchers.Length > 0) {
				return options.ReplaceOrAdd<MergeCollectionsMappingOptions>(m => {
					// Avoid wrapping the matcher in CompositeMatcher if it's only one
					if (m?.Matcher != null || matchers.Length > 1) { 
						var opts = new CompositeMatcherOptions();
						if(m?.Matcher != null)
							opts.Matchers.Add(m.Matcher);
						foreach(var matcher in matchers) {
							if(matcher != null)
								opts.Matchers.Add(matcher);
						}

						return new MergeCollectionsMappingOptions(m?.RemoveNotMatchedDestinationElements, new CompositeMatcher(opts));
					}
					else
						return new MergeCollectionsMappingOptions(m?.RemoveNotMatchedDestinationElements, matchers[0]);
				});
			}
			else
				return options;
		}
	}
}
