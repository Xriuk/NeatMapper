using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class AsyncMergeMapFactoryExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IAsyncMergeMapFactory factory =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif
			AsyncMergeMapFactory<string, int> genericFactory1 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
			null;
#endif
			AsyncMergeMapFactory<string, string> genericFactory2 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
			null;
#endif

			// MapNewFactory
			{
				// Explicit source and destination
				{
					AsyncNewMapFactory<string, int> f1 = genericFactory1.MapAsyncNewFactory();
					AsyncNewMapFactory<string, string> f2 = genericFactory2.MapAsyncNewFactory();
				}

				// Runtime
				{
					factory.MapAsyncNewFactory();
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}


		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncMergeMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Mapping.Async.AsyncMergeMapperTests.Maps) }
			});
		}

		[TestMethod]
		public async Task MapAsyncNewFactoryShouldWork() {
			// Runtime
			using (var factory = _mapper.MapAsyncMergeFactory(typeof(float), typeof(string)).MapAsyncNewFactory()) {
				Assert.AreEqual("6", await factory.Invoke(2f));
			}

			// Explicit source and destination
			using (var factory = _mapper.MapAsyncMergeFactory<float, string>().MapAsyncNewFactory()) {
				Assert.AreEqual("6", await factory.Invoke(2f));
			}
		}

		[TestMethod]
		public async Task MapAsyncNewFactoryShouldDisposeProvidedFactoryOnDispose() {
			var factory = _mapper.MapAsyncMergeFactory<float, string>();

			// Should not be disposed
			await factory.Invoke(2f, "");

			// Should dispose both
			using (factory.MapAsyncNewFactory()) { }
			await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => factory.Invoke(2f, ""));


			factory = _mapper.MapAsyncMergeFactory<float, string>();

			// Should not be disposed
			await factory.Invoke(2f, "");

			// Should dispose only created
			using (factory.MapAsyncNewFactory(false)) { }
			await factory.Invoke(2f, "");
		}

		[TestMethod]
		public async Task MapAsyncNewFactoryShouldDisposeProvidedFactoryOnException() {
			var factory = _mapper.MapAsyncMergeFactory<string, ClassWithoutParameterlessConstructor>();

			// Should not be disposed
			await factory.Invoke("", new ClassWithoutParameterlessConstructor(""));

			// Should fail and dispose
			Assert.ThrowsException<MapNotFoundException>(() => factory.MapAsyncNewFactory());
			await Assert.ThrowsExceptionAsync<ObjectDisposedException>(() => factory.Invoke("", new ClassWithoutParameterlessConstructor("")));


			factory = _mapper.MapAsyncMergeFactory<string, ClassWithoutParameterlessConstructor>();

			// Should not be disposed
			await factory.Invoke("", new ClassWithoutParameterlessConstructor(""));

			// Should fail and not dispose
			Assert.ThrowsException<MapNotFoundException>(() => factory.MapAsyncNewFactory(false));
			await factory.Invoke("", new ClassWithoutParameterlessConstructor(""));
		}
	}
}
