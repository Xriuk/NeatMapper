#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Collections.Concurrent;

namespace NeatMapper {
	// Adapted from https://gist.github.com/eknowledger/caa7baa8f6cc846a90525912aaceeb0a
	internal class ObjectPool<T> {
		private readonly ConcurrentBag<T> _objects;
		private readonly Func<T> _objectGenerator;

		public ObjectPool(Func<T> objectGenerator) {
			if (objectGenerator == null)
				throw new ArgumentNullException(nameof(objectGenerator));
			_objects = new ConcurrentBag<T>();
			_objectGenerator = objectGenerator;
		}


		public T Get() {
			if (_objects.TryTake(out var item))
				return item;
			return _objectGenerator();
		}

		public void Return(T item) {
			_objects.Add(item);
		}
	}
}
