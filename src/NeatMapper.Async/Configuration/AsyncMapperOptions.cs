using NeatMapper.Async;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;

namespace NeatMapper.Configuration {
	/// <summary>
	/// Specific options for <see cref="AsyncMapper"/>
	/// </summary>
	public sealed class AsyncMapperOptions : IAdditionalMapsOptions {
		public AsyncMapperOptions() {}
		public AsyncMapperOptions(AsyncMapperOptions options) {
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
		public void AddNewMap<TSource, TDestination>(AsyncNewMapDelegate<TSource, TDestination> newMapDelegate) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (newMapDelegate == null)
				throw new ArgumentNullException(nameof(newMapDelegate));

			_newMaps.Add((typeof(TSource), typeof(TDestination)), new AdditionalMap {
				Method = newMapDelegate.Method,
				IgnoreIfAlreadyAdded = false
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Adds an additional NewMap after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// if the specified types are not already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="newMapDelegate">mapping method</param>
		public void TryAddNewMap<TSource, TDestination>(AsyncNewMapDelegate<TSource, TDestination> newMapDelegate) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (newMapDelegate == null)
				throw new ArgumentNullException(nameof(newMapDelegate));

			_newMaps.Add((typeof(TSource), typeof(TDestination)), new AdditionalMap {
				Method = newMapDelegate.Method,
				IgnoreIfAlreadyAdded = true
			});

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
		public void AddMergeMap<TSource, TDestination>(AsyncMergeMapDelegate<TSource, TDestination> mergeMapDelegate) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (mergeMapDelegate == null)
				throw new ArgumentNullException(nameof(mergeMapDelegate));

			_mergeMaps.Add((typeof(TSource), typeof(TDestination)), new AdditionalMap {
				Method = mergeMapDelegate.Method,
				IgnoreIfAlreadyAdded = false
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Adds an additional MergeMap after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// if the specified types are not already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="mergeMapDelegate">mapping method</param>
		public void TryAddMergeMap<TSource, TDestination>(AsyncMergeMapDelegate<TSource, TDestination> mergeMapDelegate) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (mergeMapDelegate == null)
				throw new ArgumentNullException(nameof(mergeMapDelegate));

			_mergeMaps.Add((typeof(TSource), typeof(TDestination)), new AdditionalMap {
				Method = mergeMapDelegate.Method,
				IgnoreIfAlreadyAdded = true
			});

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

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (matchMapDelegate == null)
				throw new ArgumentNullException(nameof(matchMapDelegate));

			_matchMaps.Add((typeof(TSource), typeof(TDestination)), new AdditionalMap {
				Method = matchMapDelegate.Method,
				IgnoreIfAlreadyAdded = false
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}

		/// <summary>
		/// Adds an additional MatchMap after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// if the specified types are not already mapped
		/// </summary>
		/// <typeparam name="TSource">Source type</typeparam>
		/// <typeparam name="TDestination">Destination type</typeparam>
		/// <param name="matchMapDelegate">mapping method</param>
		public void TryAddMatchMap<TSource, TDestination>(MatchMapDelegate<TSource, TDestination> matchMapDelegate) {

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
			if (matchMapDelegate == null)
				throw new ArgumentNullException(nameof(matchMapDelegate));

			_matchMaps.Add((typeof(TSource), typeof(TDestination)), new AdditionalMap {
				Method = matchMapDelegate.Method,
				IgnoreIfAlreadyAdded = true
			});

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable
#endif
		}
	}
}
