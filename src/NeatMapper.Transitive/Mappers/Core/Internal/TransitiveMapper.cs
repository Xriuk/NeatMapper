#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace NeatMapper.Transitive {
	/// <summary>
	/// <see cref="IMapper"/> which maps types transitively by retrieving all the available maps from another
	/// <see cref="IMapper"/> (via <see cref="IMapperMaps"/> interface), the retrieved maps are combined into
	/// a graph and the shortest path is solved by using the Dijkstra algorithm.<br/>
	/// Internal class.
	/// </summary>
	public abstract class TransitiveMapper : IMapper {
		/// <summary>
		/// <see cref="IMapper"/> which is used to map the types, will be also provided as a nested mapper
		/// in <see cref="MapperOverrideMappingOptions"/> (if not already present).
		/// </summary>
		protected readonly IMapper _mapper;

		/// <summary>
		/// Options to apply when mapping types.
		/// </summary>
		protected readonly TransitiveOptions _transitiveOptions;

		/// <summary>
		/// Cached nested context with no parents.
		/// </summary>
		private readonly NestedMappingContext _nestedMappingContext;

		/// <summary>
		/// Cached input <see cref="MappingOptions"/> (only if <see cref="MappingOptions.Cached"/> is
		/// <see langword="true"/>) and output <see cref="MappingOptions"/> (with 
		///	<see cref="MappingOptions.Cached"/> also set to <see langword="true"/>).
		/// </summary>
		private readonly ConcurrentDictionary<MappingOptions, MappingOptions> _optionsCache =
			new ConcurrentDictionary<MappingOptions, MappingOptions>();

		/// <summary>
		/// Cached output <see cref="MappingOptions"/> for <see langword="null"/> <see cref="MappingOptions"/>
		/// (since a dictionary can't have null keys) and <see cref="MappingOptions.Empty"/> inputs,
		/// also provides faster access since locking isn't needed for thread-safety.
		/// </summary>
		private readonly MappingOptions _optionsCacheNull;


		internal TransitiveMapper(IMapper mapper, TransitiveOptions transitiveOptions) {
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_transitiveOptions = transitiveOptions ?? new TransitiveOptions();

			_nestedMappingContext = new NestedMappingContext(this);
			_optionsCacheNull = MergeMappingOptions(MappingOptions.Empty);
		}


		public abstract object Map(object source, Type sourceType, Type destinationType, MappingOptions mappingOptions = null);
		public abstract object Map(object source, Type sourceType, object destination, Type destinationType, MappingOptions mappingOptions = null);


		protected MappingOptions MergeOrCreateMappingOptions(MappingOptions options) {
			if (options == null || options == MappingOptions.Empty)
				return _optionsCacheNull;
			else if (options.Cached)
				return _optionsCache.GetOrAdd(options, MergeMappingOptions);
			else
				return MergeMappingOptions(options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private MappingOptions MergeMappingOptions(MappingOptions options) {
			return options.ReplaceOrAdd<NestedMappingContext>(
				n => n != null ? new NestedMappingContext(_nestedMappingContext.ParentMapper, n) : _nestedMappingContext, options.Cached);
		}
	}
}
