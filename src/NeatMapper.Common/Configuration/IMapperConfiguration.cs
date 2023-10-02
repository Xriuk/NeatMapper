#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeatMapper.Configuration {
	/// <summary>
	/// Configuration info for a:<br/>
	/// NewMap<br/>
	/// MergeMap<br/>
	/// <see cref="Core.IMatchMap{TSource, TDestination}"/>
	/// </summary>
	internal class Map {
		/// <summary>
		/// Declaring class of the <see cref="Method"/>
		/// </summary>
		public Type Class { get; set; }

		/// <summary>
		/// Method to invoke
		/// </summary>
		/// <remarks>May be instance or static</remarks>
		public MethodInfo Method { get; set; }
	}

	/// <summary>
	/// Configuration info for a generic:<br/>
	/// NewMap<br/>
	/// MergeMap<br/>
	/// <see cref="Core.IMatchMap{TSource, TDestination}"/>
	/// </summary>
	/// <remarks>At least one of <see cref="From"/> or <see cref="To"/> is an open generic type</remarks>
	internal class GenericMap {
		/// <summary>
		/// Source type, may be a generic open type
		/// </summary>
		public Type From { get; set; }

		/// <summary>
		/// Destination type, may be a generic open type
		/// </summary>
		public Type To { get; set; }

		/// <summary>
		/// Declaring class of the <see cref="Method"/>
		/// </summary>
		public Type Class { get; set; }

		/// <summary>
		/// Handle of the generic method, used with GetMethodFromHandle with generated concrete type during mapping
		/// </summary>
		/// <remarks>May be instance or static</remarks>
		public RuntimeMethodHandle Method { get; set; }
	}

	/// <summary>
	/// Generated configuration for a mapper
	/// </summary>
	internal interface IMapperConfiguration {
		#region Maps
		/// <summary>
		/// (source, context) => destination
		/// </summary>
		/// <remarks>Method may be instance or static</remarks>
		IReadOnlyDictionary<(Type From, Type To), Map> NewMaps { get; }

		/// <summary>
		/// (source, destination, context) => destination
		/// </summary>
		/// <remarks>Method may be instance or static</remarks>
		IReadOnlyDictionary<(Type From, Type To), Map> MergeMaps { get; }

		/// <summary>
		/// One or two open types which contain (source, context) => destination for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		IEnumerable<GenericMap> GenericNewMaps { get; }

		/// <summary>
		/// One or two open types which contain (source, destination, context) => destination> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		IEnumerable<GenericMap> GenericMergeMaps { get; }
		#endregion

		#region MergeMap additional options
		/// <summary>
		/// <see cref="Core.IMatchMap{TSource, TDestination}.Match"/>
		/// </summary>
		/// <remarks>Method may be instance or static</remarks>
		IReadOnlyDictionary<(Type From, Type To), Map> Matchers { get; }

		/// <summary>
		/// One or two open types which contain <see cref="Core.IMatchMap{TSource, TDestination}.Match"/> for the given open types.<br/>
		/// (IEnumerable&lt;TSource&gt;, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TSource, TDestination&gt;<br/>
		/// (IEnumerable&lt;TSource&gt;, MyNonGenericClass) =&gt; MyClass&lt;TSource&gt;<br/>
		/// (MyNonGenericClass, IList&lt;TDestination&gt;) =&gt; MyClass&lt;TDestination&gt;
		/// </summary>
		IEnumerable<GenericMap> GenericMatchers { get; }

		/// <summary>
		/// Options applied to automatic collections mapping via <see cref="MergeMaps"/> or <see cref="GenericMergeMaps"/>
		/// </summary>
		MergeMapsCollectionsOptions MergeMapsCollectionsOptions { get; } 
		#endregion
	}
}
