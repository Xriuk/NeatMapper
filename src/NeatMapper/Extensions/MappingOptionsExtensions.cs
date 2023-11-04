using System;
using System.Collections.Generic;

namespace NeatMapper {
	public static class MappingOptionsExtensions {
		public static
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TOptions?
#else
			TOptions
#endif
			GetOptions<TOptions>(this MappingOptions options) where TOptions : class {

			if (options == null)
				throw new ArgumentNullException(nameof(options));
			return options.GetOptions(typeof(TOptions)) as TOptions;
		}


		public static MappingOptions Replace<TOptions>(this MappingOptions options, Func<TOptions,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			TOptions?
#else
			TOptions
#endif
			> factory) where TOptions : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.Replace(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions), o => o is TOptions opts ? (object)factory.Invoke(opts) : null }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions Replace<TOptions1, TOptions2>(this MappingOptions options,
			Func<TOptions1,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<TOptions2,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2)
				where TOptions1 : class
				where TOptions2 : class{

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.Replace(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? (object)factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? (object)factory2.Invoke(opts) : null }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions Replace<TOptions1, TOptions2, TOptions3>(this MappingOptions options,
			Func<TOptions1,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<TOptions2,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2,
			Func<TOptions3,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				> factory3)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.Replace(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? (object)factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? (object)factory2.Invoke(opts) : null },
				{ typeof(TOptions3), o => o is TOptions3 opts ? (object)factory3.Invoke(opts) : null }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions Replace<TOptions1, TOptions2, TOptions3, TOptions4>(this MappingOptions options,
			Func<TOptions1,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<TOptions2,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2,
			Func<TOptions3,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				> factory3,
			Func<TOptions4,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions4?
#else
				TOptions4
#endif
				> factory4)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.Replace(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? (object)factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? (object)factory2.Invoke(opts) : null },
				{ typeof(TOptions3), o => o is TOptions3 opts ? (object)factory3.Invoke(opts) : null },
				{ typeof(TOptions4), o => o is TOptions4 opts ? (object)factory4.Invoke(opts) : null }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions Replace<TOptions1, TOptions2, TOptions3, TOptions4, TOptions5>(this MappingOptions options,
			Func<TOptions1,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<TOptions2,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2,
			Func<TOptions3,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				> factory3,
			Func<TOptions4,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions4?
#else
				TOptions4
#endif
				> factory4,
			Func<TOptions5,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions5?
#else
				TOptions5
#endif
				> factory5)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class
				where TOptions5 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.Replace(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => o is TOptions1 opts ? (object)factory1.Invoke(opts) : null },
				{ typeof(TOptions2), o => o is TOptions2 opts ? (object)factory2.Invoke(opts) : null },
				{ typeof(TOptions3), o => o is TOptions3 opts ? (object)factory3.Invoke(opts) : null },
				{ typeof(TOptions4), o => o is TOptions4 opts ? (object)factory4.Invoke(opts) : null },
				{ typeof(TOptions5), o => o is TOptions5 opts ? (object)factory5.Invoke(opts) : null }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}


		public static MappingOptions ReplaceOrAdd<TOptions>(this MappingOptions options, Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions?
#else
				TOptions
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions?
#else
				TOptions
#endif
				> factory) where TOptions : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions), o => factory.Invoke(o as TOptions) }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2>(this MappingOptions options,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2)
				where TOptions1 : class
				where TOptions2 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2, TOptions3>(this MappingOptions options,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				> factory3)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) },
				{ typeof(TOptions3), o => factory3.Invoke(o as TOptions3) }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2, TOptions3, TOptions4>(this MappingOptions options,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				> factory3,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions4?
#else
				TOptions4
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions4?
#else
				TOptions4
#endif
				> factory4)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) },
				{ typeof(TOptions3), o => factory3.Invoke(o as TOptions3) },
				{ typeof(TOptions4), o => factory4.Invoke(o as TOptions4) }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		public static MappingOptions ReplaceOrAdd<TOptions1, TOptions2, TOptions3, TOptions4, TOptions5>(this MappingOptions options,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions1?
#else
				TOptions1
#endif
				> factory1,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions2?
#else
				TOptions2
#endif
				> factory2,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions3?
#else
				TOptions3
#endif
				> factory3,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions4?
#else
				TOptions4
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions4?
#else
				TOptions4
#endif
				> factory4,
			Func<
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions5?
#else
				TOptions5
#endif
				,
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				TOptions5?
#else
				TOptions5
#endif
				> factory5)
				where TOptions1 : class
				where TOptions2 : class
				where TOptions3 : class
				where TOptions4 : class
				where TOptions5 : class {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

			if (options == null)
				throw new ArgumentNullException(nameof(options));

			return options.ReplaceOrAdd(new Dictionary<Type, Func<object, object>> {
				{ typeof(TOptions1), o => factory1.Invoke(o as TOptions1) },
				{ typeof(TOptions2), o => factory2.Invoke(o as TOptions2) },
				{ typeof(TOptions3), o => factory3.Invoke(o as TOptions3) },
				{ typeof(TOptions4), o => factory4.Invoke(o as TOptions4) },
				{ typeof(TOptions5), o => factory5.Invoke(o as TOptions5) }
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
