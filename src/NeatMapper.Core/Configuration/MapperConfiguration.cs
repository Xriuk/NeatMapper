using System.Reflection;

namespace NeatMapper.Core.Configuration {
	internal sealed class MapperConfiguration : IMapperConfiguration {
		

		public IDictionary<(Type From, Type To), MethodInfo> Maps => throw new NotImplementedException();

		public IDictionary<(Type From, Type To), MethodInfo> MergeMaps => throw new NotImplementedException();
	}
}
