using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class MergeMapFactoryExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IMergeMapFactory factory =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
# endif
			MergeMapFactory<string, int> genericFactory1 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
			null;
#endif
			MergeMapFactory<string, string> genericFactory2 =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
			null;
#endif

			// MapNewFactory
			{
				// Explicit source and destination
				{
					NewMapFactory<string, int> f1 = genericFactory1.MapNewFactory();
					NewMapFactory<string, string> f2 = genericFactory2.MapNewFactory();
				}

				// Runtime
				{
					factory.MapNewFactory();
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}


		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new CustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(Mapping.MergeMapsTests.Maps) }
			});
		}

		[TestMethod]
		public void MapNewFactoryShouldWork() {
			// Runtime
			using (var factory = _mapper.MapMergeFactory(typeof(float), typeof(string)).MapNewFactory()) {
				Assert.AreEqual("6", factory.Invoke(2f));
			}

			// Explicit source and destination
			using (var factory = _mapper.MapMergeFactory<float, string>().MapNewFactory()) {
				Assert.AreEqual("6", factory.Invoke(2f));
			}
		}

		[TestMethod]
		public void MapNewFactoryShouldDisposeProvidedFactoryOnDispose() {
			var factory = _mapper.MapMergeFactory<float, string>();

			// Should not be disposed
			factory.Invoke(2f, "");

			// Should dispose both
			using (factory.MapNewFactory()) { }
			Assert.ThrowsException<ObjectDisposedException>(() => factory.Invoke(2f, ""));


			factory = _mapper.MapMergeFactory<float, string>();

			// Should not be disposed
			factory.Invoke(2f, "");

			// Should dispose only created
			using (factory.MapNewFactory(false)) { }
			factory.Invoke(2f, "");
		}

		[TestMethod]
		public void MapNewFactoryShouldDisposeProvidedFactoryOnException() {
			var factory = _mapper.MapMergeFactory<string, ClassWithoutParameterlessConstructor>();

			// Should not be disposed
			factory.Invoke("", new ClassWithoutParameterlessConstructor(""));

			// Should fail and dispose
			Assert.ThrowsException<MapNotFoundException>(() => factory.MapNewFactory());
			Assert.ThrowsException<ObjectDisposedException>(() => factory.Invoke("", new ClassWithoutParameterlessConstructor("")));


			factory = _mapper.MapMergeFactory<string, ClassWithoutParameterlessConstructor>();

			// Should not be disposed
			factory.Invoke("", new ClassWithoutParameterlessConstructor(""));

			// Should fail and not dispose
			Assert.ThrowsException<MapNotFoundException>(() => factory.MapNewFactory(false));
			factory.Invoke("", new ClassWithoutParameterlessConstructor(""));
		}
	}
}
