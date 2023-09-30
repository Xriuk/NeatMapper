using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;
using static NeatMapper.Common.Tests.Configuration.ConfigurationsTests;

namespace NeatMapper.Common.Tests.Configuration {
	[TestClass]
	public class ConfigurationsTests {
		public class Map1 : IMatchMap<string, int> {
			bool IMatchMap<string, int>.Match(string? source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class Map2 : IMatchMap<string, int> {
			bool IMatchMap<string, int>.Match(string? source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class NotParameterlessClass : IMatchMap<string, int> {
			public NotParameterlessClass(string test) { }

			bool IMatchMap<string, int>.Match(string? source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap1<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1>? source, IEnumerable<T1>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap2<T1, T2> :
			IMatchMap<IList<T1>, IEnumerable<T2>> {

			bool IMatchMap<IList<T1>, IEnumerable<T2>>.Match(IList<T1>? source, IEnumerable<T2>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap3<T1> :
			IMatchMap<IList<T1>, IEnumerable<int>> {

			bool IMatchMap<IList<T1>, IEnumerable<int>>.Match(IList<T1>? source, IEnumerable<int>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap4<T1, T2> :
			IMatchMap<IList<T1>, IEnumerable<T1>> {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1>? source, IEnumerable<T1>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}

		}

		public class NotParameterlessGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> {

			public NotParameterlessGenericMap(string test) { }

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1>? source, IEnumerable<T1>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}

		}

		public class ClassConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : class {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1>? source, IEnumerable<T1>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class StructConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : struct {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1>? source, IEnumerable<T1>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class NewConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : new() {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1>? source, IEnumerable<T1>? destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		protected static void Configure(MapperConfigurationOptions options) {
			new MapperConfiguration(i => i == typeof(IMatchMap<,>), i => false, options);
		}

		protected static void AssertDuplicateMap(Action action) {
			var exc = Assert.ThrowsException<InvalidOperationException>(action);

			Assert.IsTrue(exc.Message.StartsWith("Duplicate interface"));
		}


		[TestMethod]
		public void ShouldNotAllowClassesWithoutParameterlessConstructor() {
			var exc = Assert.ThrowsException<InvalidOperationException>(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(NotParameterlessClass) }
			}));

			Assert.IsTrue(exc.Message.Contains("cannot be instantiated because the class which implements the non-static interface has no parameterless constructor"));
		}

		[TestMethod]
		public void ShouldNotAllowDuplicateMaps() {
			AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(Map1), typeof(Map2) }
			}));
		}

		[TestMethod]
		public void ShouldNotAllowGenericClassesWithMoreGenericArgumentsThanMap() {
			var exc = Assert.ThrowsException<InvalidOperationException>(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(GenericMap4<,>) }
			}));

			Assert.IsTrue(exc.Message.Contains("cannot be instantiated because the generic arguments of the interface do not fully cover the generic arguments of the class"));
		}

		[TestMethod]
		public void ShouldNotAllowGenericClassesWithoutParameterlessConstructor() {
			var exc = Assert.ThrowsException<InvalidOperationException>(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(NotParameterlessGenericMap<>) }
			}));

			Assert.IsTrue(exc.Message.Contains("cannot be instantiated because the class which implements the non-static interface has no parameterless constructor"));
		}

		[TestMethod]
		public void ShouldNotAllowDuplicateGenericMaps() {
			AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(GenericMap2<,>) }
			}));
		}

		[TestMethod]
		public void ShouldNotAllowOverlappingGenericConstraints() {
			// No constraint - struct
			{
				AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(StructConstraintGenericMap<>) }
				}));
			}

			// No constraint - class
			{
				AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(ClassConstraintGenericMap<>) }
				}));
			}

			// No constraint - unmanaged
			{

			}

			// No constraint - new()
			{
				AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// No constraint - base class
			{

			}

			// No constraint - interface
			{

			}

			// No constraint - generic type parameter
			{

			}

			// new() - struct
			{
				AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(StructConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() - class
			{
				AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() - unmanaged
			{

			}

			// new() - base class
			{

			}

			// new() - interface
			{

			}

			// new() - generic type parameter
			{

			}

			// unmanaged - struct
			{

			}

			// base class - derived class
			{

			}

			// class - interface
			{

			}
		}

		[TestMethod]
		[DataRow(true)]
		[DataRow(false)]
		public void ShouldNotConsiderSpecificGenericMapsAsDuplicates(bool genericMap1) {
			Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { genericMap1 ? typeof(GenericMap1<>) : typeof(GenericMap2<,>), typeof(GenericMap3<>) }
			});
		}

		[TestMethod]
		public void ShouldNotConsiderDifferentGenericConstraintsAsDuplicates() {
			Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(StructConstraintGenericMap<>) }
			});
		}
	}
}
