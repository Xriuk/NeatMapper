﻿using System.Reflection;

namespace NeatMapper.Core.Configuration {
	public class GenericMap {
		public required Type From { get; init; }

		public required Type To { get; init; }

		public required Type Class { get; init; }

		public required RuntimeMethodHandle Method { get; init; }
	}

	public interface IMapperConfiguration {
		/// <summary>
		/// <see cref="INewMap{TSource, TDestination}.Map(TSource, MappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> NewMaps { get; }

		/// <summary>
		/// <see cref="IMergeMap{TSource, TDestination}.Map(TSource, TDestination, MappingContext)"/>
		/// </summary>
		public IReadOnlyDictionary<(Type From, Type To), MethodInfo> MergeMaps { get; }

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
	}
}
