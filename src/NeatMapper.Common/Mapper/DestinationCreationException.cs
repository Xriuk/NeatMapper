using System;

namespace NeatMapper.Common.Mapper {
	internal class DestinationCreationException : InvalidOperationException {
		public DestinationCreationException(Type destination, Exception exception) :
			base($"Could not create destination object for type {destination.Name} ({destination.FullName})", exception) { }
	}
}
