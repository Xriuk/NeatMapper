using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NeatMapper.Configuration {
	/// <summary>
	/// Specific options for <see cref="Mapper"/>
	/// </summary>
	public sealed class MapperOptions : IAdditionalMapsOptions {
		public MapperOptions() {}
		public MapperOptions(MapperOptions options) {
			_newMaps = new Dictionary<(Type From, Type To), AdditionalMap>(options._newMaps);
			_mergeMaps = new Dictionary<(Type From, Type To), AdditionalMap>(options._mergeMaps);
			_matchMaps = new Dictionary<(Type From, Type To), AdditionalMap>(options._matchMaps);
		}

		private Dictionary<(Type From, Type To), AdditionalMap> _newMaps = new Dictionary<(Type From, Type To), AdditionalMap>();
		private Dictionary<(Type From, Type To), AdditionalMap> _mergeMaps = new Dictionary<(Type From, Type To), AdditionalMap>();
		private Dictionary<(Type From, Type To), AdditionalMap> _matchMaps = new Dictionary<(Type From, Type To), AdditionalMap>();

		IReadOnlyDictionary<(Type From, Type To), AdditionalMap> IAdditionalMapsOptions.NewMaps => _newMaps;
		IReadOnlyDictionary<(Type From, Type To), AdditionalMap> IAdditionalMapsOptions.MergeMaps => _mergeMaps;
		IReadOnlyDictionary<(Type From, Type To), AdditionalMap> IAdditionalMapsOptions.MatchMaps => _matchMaps;


		/// <summary>
		/// Adds an additional NewMap after the ones discovered in <see cref="MapperConfigurationOptions"/>,
		/// will throw if the types are already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="newMapDelegate">mapping method</param>
		public void AddNewMap<TSource, TDestination>(NewMapDelegate<TSource, TDestination> newMapDelegate) {
			AddNewMapInternal(newMapDelegate, false);
		}

		/// <summary>
		/// Adds an additional NewMap after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// if the specified types are not already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="newMapDelegate">mapping method</param>
		public void TryAddNewMap<TSource, TDestination>(NewMapDelegate<TSource, TDestination> newMapDelegate) {
			AddNewMapInternal(newMapDelegate, true);
		}

		private void AddNewMapInternal<TSource, TDestination>(NewMapDelegate<TSource, TDestination> newMapDelegate, bool ignoreIfAdded) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (newMapDelegate == null)
				throw new ArgumentNullException(nameof(newMapDelegate));

			var map = new AdditionalMap { IgnoreIfAlreadyAdded = ignoreIfAdded };
			if (IsRuntime(newMapDelegate)) {
				NewMapDelegate<TSource, TDestination> method = newMapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = newMapDelegate;
			}
			else
				map.Method = newMapDelegate.Method;
			_newMaps.Add((typeof(TSource), typeof(TDestination)), map);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Adds an additional MergeMap after the ones discovered in <see cref="MapperConfigurationOptions"/>,
		/// will throw if the types are already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="mergeMapDelegate">mapping method</param>
		public void AddMergeMap<TSource, TDestination>(MergeMapDelegate<TSource, TDestination> mergeMapDelegate) {
			AddMergeMapInternal(mergeMapDelegate, false);
		}

		/// <summary>
		/// Adds an additional MergeMap after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// if the specified types are not already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="mergeMapDelegate">mapping method</param>
		public void TryAddMergeMap<TSource, TDestination>(MergeMapDelegate<TSource, TDestination> mergeMapDelegate) {
			AddMergeMapInternal(mergeMapDelegate, true);
		}

		private void AddMergeMapInternal<TSource, TDestination>(MergeMapDelegate<TSource, TDestination> mergeMapDelegate, bool ignoreIfAdded) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (mergeMapDelegate == null)
				throw new ArgumentNullException(nameof(mergeMapDelegate));

			var map = new AdditionalMap { IgnoreIfAlreadyAdded = ignoreIfAdded };
			if (IsRuntime(mergeMapDelegate)) {
				MergeMapDelegate<TSource, TDestination> method = mergeMapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = mergeMapDelegate;
			}
			else
				map.Method = mergeMapDelegate.Method;
			_mergeMaps.Add((typeof(TSource), typeof(TDestination)), map);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Adds an additional MatchMap after the ones discovered in <see cref="MapperConfigurationOptions"/>,
		/// will throw if the types are already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="matchMapDelegate">mapping method</param>
		public void AddMatchMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> matchMapDelegate) {
			AddMatchMapInternal(matchMapDelegate, false);
		}

		/// <summary>
		/// Adds an additional MatchMap after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// if the specified types are not already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="matchMapDelegate">mapping method</param>
		public void TryAddMatchMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> matchMapDelegate) {
			AddMatchMapInternal(matchMapDelegate, true);
		}

		private void AddMatchMapInternal<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> matchMapDelegate, bool ignoreIfAdded) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (matchMapDelegate == null)
				throw new ArgumentNullException(nameof(matchMapDelegate));

			var map = new AdditionalMap { IgnoreIfAlreadyAdded = ignoreIfAdded };
			if (IsRuntime(matchMapDelegate)) {
				MatchMapDelegate<TSource, TDestination> method = matchMapDelegate.Invoke;
				map.Method = method.Method;
				map.Instance = matchMapDelegate;
			}
			else
				map.Method = matchMapDelegate.Method;
			_matchMaps.Add((typeof(TSource), typeof(TDestination)), map);

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsRuntime(Delegate del) {
			return (del.Method.GetType().FullName?.StartsWith("System.Reflection.Emit.DynamicMethod") == true);
		}
	}
}
