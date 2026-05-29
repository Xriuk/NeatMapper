using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;

namespace NeatMapper.Tests {
	[TestClass]
	public class RecursiveTypesMappingContextTests {
		[TestMethod]
		public void ShouldReturnFullTypesChain() {
			var context = new RecursiveTypesMappingContext(typeof(string), typeof(int),
				new RecursiveTypesMappingContext(typeof(char), typeof(float),
					new RecursiveTypesMappingContext(typeof(bool), typeof(decimal))));

			// Types are reversed
			var fullTypeChain = context.GetFullTypesChain();
			Assert.AreEqual(3, fullTypeChain.Count());

			var first = fullTypeChain.ElementAt(0);
			Debug.Assert(first.SourceType == typeof(bool));
			Debug.Assert(first.DestinationType == typeof(decimal));

			var second = fullTypeChain.ElementAt(1);
			Debug.Assert(second.SourceType == typeof(char));
			Debug.Assert(second.DestinationType == typeof(float));

			var third = fullTypeChain.ElementAt(2);
			Debug.Assert(third.SourceType == typeof(string));
			Debug.Assert(third.DestinationType == typeof(int));
		}

		[TestMethod]
		public void ShouldDetectNotRecursion() {
			var context = new RecursiveTypesMappingContext(typeof(string), typeof(int),
				new RecursiveTypesMappingContext(typeof(char), typeof(float),
					new RecursiveTypesMappingContext(typeof(bool), typeof(decimal))));

			var recursion = context.GetRecursingTypesChain(out var recursingTypes);
			Assert.AreEqual(1, recursion);
			Assert.AreEqual(3, recursingTypes.Count());

			var first = recursingTypes.ElementAt(0);
			Debug.Assert(first.SourceType == typeof(bool));
			Debug.Assert(first.DestinationType == typeof(decimal));

			var second = recursingTypes.ElementAt(1);
			Debug.Assert(second.SourceType == typeof(char));
			Debug.Assert(second.DestinationType == typeof(float));

			var third = recursingTypes.ElementAt(2);
			Debug.Assert(third.SourceType == typeof(string));
			Debug.Assert(third.DestinationType == typeof(int));
		}

		[TestMethod]
		public void ShouldDetectRecursion() {
			var context = new RecursiveTypesMappingContext(typeof(string), typeof(int),
				new RecursiveTypesMappingContext(typeof(char), typeof(float),
					new RecursiveTypesMappingContext(typeof(string), typeof(int),
						new RecursiveTypesMappingContext(typeof(char), typeof(float)))));

			var recursion = context.GetRecursingTypesChain(out var recursingTypes);
			Assert.AreEqual(2, recursion);
			Assert.AreEqual(2, recursingTypes.Count());

			var first = recursingTypes.ElementAt(0);
			Debug.Assert(first.SourceType == typeof(char));
			Debug.Assert(first.DestinationType == typeof(float));

			var second = recursingTypes.ElementAt(1);
			Debug.Assert(second.SourceType == typeof(string));
			Debug.Assert(second.DestinationType == typeof(int));
		}

		[TestMethod]
		public void ShouldDetectRecursionOffset() {
			var context = new RecursiveTypesMappingContext(typeof(string), typeof(int),
				new RecursiveTypesMappingContext(typeof(char), typeof(float),
					new RecursiveTypesMappingContext(typeof(string), typeof(int),
						new RecursiveTypesMappingContext(typeof(char), typeof(float),
							new RecursiveTypesMappingContext(typeof(string), typeof(float),
								new RecursiveTypesMappingContext(typeof(bool), typeof(float)))))));

			var recursion = context.GetRecursingTypesChain(out var recursingTypes);
			Assert.AreEqual(2, recursion);
			Assert.AreEqual(2, recursingTypes.Count());

			var first = recursingTypes.ElementAt(0);
			Debug.Assert(first.SourceType == typeof(char));
			Debug.Assert(first.DestinationType == typeof(float));

			var second = recursingTypes.ElementAt(1);
			Debug.Assert(second.SourceType == typeof(string));
			Debug.Assert(second.DestinationType == typeof(int));
		}
	}
}
