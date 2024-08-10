using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Transitive {
	public static class MergeMapFactoryExtensions {
		/// <summary>
		/// Retrieves the type chain of the current factory, if the factory is <see cref="ITransitiveMergeMapFactories"/>
		/// the returned types will be retrieved from the underlying <see cref="ITransitiveMergeMapFactories.Factories"/>,
		/// if the factory is <see cref="ITransitiveMergeMapFactory"/> the returned types will be
		/// <see cref="ITransitiveMergeMapFactory.Types"/> otherwise they will be <see cref="IMergeMapFactory.SourceType"/>
		/// and <see cref="IMergeMapFactory.DestinationType"/>.
		/// </summary>
		/// <returns>
		/// A collection of chains of type maps which could be used to map the given types.
		/// The chains will always begin with the provided <see cref="IMergeMapFactory.SourceType"/> and will end with
		/// the provided <see cref="IMergeMapFactory.DestinationType"/>, so they will always have at least 2 elements.
		/// </returns>
		public static IEnumerable<IList<Type>> GetMapTypes(this IMergeMapFactory factory) {
			if(factory == null)
				throw new ArgumentNullException(nameof(factory));

			if(factory is ITransitiveMergeMapFactories transFactories)
				return transFactories.Factories.Select(f => f.Types);
			if (factory is ITransitiveMergeMapFactory transFactory)
				return new []{ transFactory.Types };
			else
				return new []{ new [] { factory.SourceType, factory.DestinationType } };
		}
	}
}
