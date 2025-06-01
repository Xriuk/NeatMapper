using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace NeatMapper.Tests {
	[TestClass]
	public class IDisposableTests {
		private class DisposableClass : IDisposable {
			public void Dispose() {}
		}


		
		[TestMethod]
		public void DisposableAsyncNewMapFactoryIdempotency() {
			IDisposable factory = new DisposableAsyncNewMapFactory(typeof(int), typeof(string), (s, c) => Task.FromResult<object>(""), new[] { new DisposableClass() });

			factory.Dispose();
			factory.Dispose();
		}

		[TestMethod]
		public void DisposableAsyncMergeMapFactoryIdempotency() {
			IDisposable factory = new DisposableAsyncMergeMapFactory(typeof(int), typeof(string), (s, d, c) => Task.FromResult<object>(""), new[] { new DisposableClass() });

			factory.Dispose();
			factory.Dispose();
		}


		[TestMethod]
		public void DisposableNewMapFactoryIdempotency() {
			IDisposable factory = new DisposableNewMapFactory(typeof(int), typeof(string), s => "", new[] { new DisposableClass() });

			factory.Dispose();
			factory.Dispose();
		}

		[TestMethod]
		public void DisposableMergeMapFactoryIdempotency() {
			IDisposable factory = new DisposableMergeMapFactory(typeof(int), typeof(string), (s, d) => "", new[] { new DisposableClass() });

			factory.Dispose();
			factory.Dispose();
		}


		[TestMethod]
		public void DisposableMatchMapFactoryIdempotency() {
			IDisposable factory = new DisposableMatchMapFactory(typeof(int), typeof(string), (s, d) => false, new[] { new DisposableClass() });

			factory.Dispose();
			factory.Dispose();
		}


		[TestMethod]
		public void DisposablePredicateFactoryIdempotency() {
			IDisposable factory = new DisposablePredicateFactory(2, typeof(int), typeof(string), c => false, new[] { new DisposableClass() });

			factory.Dispose();
			factory.Dispose();
		}
	}
}
