#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NeatMapper.Configuration {
	internal class AdditionalMap {
		public MethodInfo Method { get; set; }

		public bool IgnoreIfAlreadyAdded { get; set; }
	}

	internal interface IAdditionalMapsOptions {
		/// <summary>
		/// Additional NewMaps to be added after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// </summary>
		/// <remarks>Should not edit directly but use typed extensions</remarks>
		IReadOnlyDictionary<(Type From, Type To), AdditionalMap> NewMaps { get; }

		/// <summary>
		/// Additional MergeMaps to be added after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// </summary>
		/// <remarks>Should not edit directly but use typed extensions</remarks>
		IReadOnlyDictionary<(Type From, Type To), AdditionalMap> MergeMaps { get; }

		/// <summary>
		/// Additional MatchMaps to be added after the ones discovered in <see cref="MapperConfigurationOptions"/>
		/// </summary>
		/// <remarks>Should not edit directly but use typed extensions</remarks>
		IReadOnlyDictionary<(Type From, Type To), AdditionalMap> MatchMaps { get; }
	}
}
