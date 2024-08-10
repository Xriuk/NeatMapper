using System;
using System.Collections.Generic;

namespace NeatMapper.Transitive {
	public static class NewMapFactoryExtensions {
		/// <summary>
		/// Retrieves the type chain of the current factory, if the factory is <see cref="ITransitiveNewMapFactory"/>
		/// the returned types will be <see cref="ITransitiveNewMapFactory.Types"/> otherwise they will be
		/// <see cref="INewMapFactory.SourceType"/> and <see cref="INewMapFactory.DestinationType"/>.
		/// </summary>
		/// <returns>
		/// A chain of type maps which could be used to map the given types.
		/// The chain will always begin with the provided <see cref="INewMapFactory.SourceType"/> and will end with
		/// the provided <see cref="INewMapFactory.DestinationType"/>, so it will always have at least 2 elements.
		/// </returns>
		public static IList<Type> GetMapTypes(this INewMapFactory factory) {
			if(factory == null)
				throw new ArgumentNullException(nameof(factory));

			if (factory is ITransitiveNewMapFactory transFactory)
				return transFactory.Types;
			else
				return new[] { factory.SourceType, factory.DestinationType };
		}
	}
}
