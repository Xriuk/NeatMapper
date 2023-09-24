using System.Reflection;

namespace NeatMapper.Core.Configuration {
	/// <summary>
	/// Configuration info for a generic:<br/>
	/// <see cref="INewMap{TSource, TDestination}"/><br/>
	/// <see cref="IMergeMap{TSource, TDestination}"/><br/>
	/// <see cref="IAsyncNewMap{TSource, TDestination}"/><br/>
	/// <see cref="IAsyncMergeMap{TSource, TDestination}"/><br/>
	/// <see cref="ICollectionElementComparer{TSource, TDestination}"/>
	/// </summary>
	/// <remarks>At least one of <see cref="From"/> or <see cref="To"/> is an open generic type</remarks>
	public class GenericMap {
		/// <summary>
		/// Source type, may be a generic open type
		/// </summary>
		public required Type From { get; init; }

		/// <summary>
		/// Destination type, may be a generic open type
		/// </summary>
		public required Type To { get; init; }

		/// <summary>
		/// Declaring class of the <see cref="Method"/>
		/// </summary>
		public required Type Class { get; init; }

		/// <summary>
		/// Handle of the generic method, used with GetMethodFromHandle with generated concrete type during mapping
		/// </summary>
		public required RuntimeMethodHandle Method { get; init; }
	}

	/// <summary>
	/// Generated configuration for an <see cref="IMapper"/>
	/// </summary>
	public interface IMapperConfiguration {
		#region Maps
		/// <summary>
		/// <see cref="INewMap{TSource, TDestination}.Map(TSource, MappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> NewMaps { get; }

		/// <summary>
		/// <see cref="IMergeMap{TSource, TDestination}.Map(TSource, TDestination, MappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> MergeMaps { get; }

		/// <summary>
		/// <see cref="IAsyncNewMap{TSource, TDestination}.MapAsync(TSource, AsyncMappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> AsyncNewMaps { get; }

		/// <summary>
		/// <see cref="IAsyncMergeMap{TSource, TDestination}.MapAsync(TSource, TDestination, AsyncMappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> AsyncMergeMaps { get; }

		/// <summary>
		/// One or two open types which contain <see cref="INewMap{TSource, TDestination}.Map(TSource, MappingContext)"/> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		public IEnumerable<GenericMap> GenericNewMaps { get; }

		/// <summary>
		/// One or two open types which contain <see cref="IMergeMap{TSource, TDestination}.Map(TSource, TDestination, MappingContext)"/> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		public IEnumerable<GenericMap> GenericMergeMaps { get; }

		/// <summary>
		/// One or two open types which contain <see cref="IAsyncNewMap{TSource, TDestination}.MapAsync(TSource, AsyncMappingContext)"/> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		public IEnumerable<GenericMap> AsyncGenericNewMaps { get; }

		/// <summary>
		/// One or two open types which contain <see cref="IAsyncMergeMap{TSource, TDestination}.MapAsync(TSource, TDestination, AsyncMappingContext)"/> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		public IEnumerable<GenericMap> AsyncGenericMergeMaps { get; }
		#endregion

		#region MergeMap additional options
		/// <summary>
		/// <see cref="ICollectionElementComparer{TSource, TDestination}.Match(TSource, TDestination, MappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> CollectionElementComparers { get; }

		/// <summary>
		/// One or two open types which contain <see cref="ICollectionElementComparer{TSource, TDestination}.Match(TSource, TDestination, MappingContext)"/> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		public IEnumerable<GenericMap> GenericCollectionElementComparers { get; }

		/// <summary>
		/// Options applied to automatic collections mapping via <see cref="IMergeMap{TSource, TDestination}"/> and <see cref="IAsyncMergeMap{TSource, TDestination}"/>
		/// </summary>
		public MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; } 
		#endregion
	}
}
