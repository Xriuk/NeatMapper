using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NeatMapper {
	// https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-create-an-object-pool
	internal sealed class ObjectPool<T> {
		private readonly ConcurrentBag<T> _objects = [];
		private readonly Func<T> _generator;
		private readonly Action<T>? _reset;

		public ObjectPool(Func<T> generator, Action<T>? reset = null) {
			_generator = generator ?? throw new ArgumentNullException(nameof(generator));
			_reset = reset;
		}


		public T Get() {
			return _objects.TryTake(out var item) ? item : _generator();
		}

		public void Return(T item){
			_reset?.Invoke(item);
			_objects.Add(item);
		}
	}

	internal static class ObjectPool {
		public static readonly ObjectPool<List<object?>> Lists =
			new ObjectPool<List<object?>>(
				() => [],
				l => l.Clear());
	}
}
