using System.Collections.Generic;

namespace NeatMapper.Tests.Classes {
	internal class CustomCollectionComplex<TType> {
		private List<TType> list = new List<TType>();

		public IEnumerable<TType> Elements => list;

		public void Add(TType item) {
			list.Add(item);
		}

		public void Remove(TType item) {
			list.Remove(item);
		}
	}
}
