using System;

namespace NeatMapper {
    internal class ObjectCreationException : InvalidOperationException{
        public ObjectCreationException(Type destination, Exception exception) :
            base($"Could not create object for type {destination.Name} ({destination.FullName})", exception)
        { }
    }
}
