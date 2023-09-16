using System.Reflection;

namespace NeatMapper.Core.Configuration {
	public class GenericMap {
		public required Type From { get; init; }

		public required Type To { get; init; }

		public required Type Class { get; init; }

		public required RuntimeMethodHandle Method { get; init; }
	}

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
	}
}
