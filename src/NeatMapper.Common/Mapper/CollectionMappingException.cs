using NeatMapper.Common.Mapper;
using System;

namespace NeatMapper {
	/// <summary>
	/// Exception thrown when an exception was thrown while mapping two collections without an explicit map but only a map for their elements
	/// </summary>
	public class CollectionMappingException : TypesException {
		public CollectionMappingException(Exception exception, (Type From, Type To) types) :
			base($"An exception was thrown while mapping the collections: {types.From.Name} -> {types.To.Name}\n" +
			$"{types.From.FullName} -> {types.To.FullName}\n" +
			$"Check the inner exception for details", exception) { }
	}
}
