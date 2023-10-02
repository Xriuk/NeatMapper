using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Configuration;
using NeatMapper.Tests.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using static NeatMapper.Tests.Configuration.ConfigurationsTests;

namespace NeatMapper.Tests.Configuration {
	[TestClass]
	public class ConfigurationsTests {
		public class Map1 : IMatchMap<string, int> {
			bool IMatchMap<string, int>.Match(string source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class Map2 : IMatchMap<string, int> {
			bool IMatchMap<string, int>.Match(string source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class NotParameterlessClass : IMatchMap<string, int> {
			public NotParameterlessClass(string test) { }

			bool IMatchMap<string, int>.Match(string source, int destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap1<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap2<T1, T2> :
			IMatchMap<IList<T1>, IEnumerable<T2>> {

			bool IMatchMap<IList<T1>, IEnumerable<T2>>.Match(IList<T1> source, IEnumerable<T2> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap3<T1> :
			IMatchMap<IList<T1>, IEnumerable<int>> {

			bool IMatchMap<IList<T1>, IEnumerable<int>>.Match(IList<T1> source, IEnumerable<int> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericMap4<T1, T2> :
			IMatchMap<IList<T1>, IEnumerable<T1>> {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}

		}

		public class NotParameterlessGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> {

			public NotParameterlessGenericMap(string test) { }

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}

		}

		public class ClassConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : class {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class StructConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : struct {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class NewConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : new() {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class UnmanagedConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : unmanaged {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class BaseClassConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : Product {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class DerivedClassMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : LimitedProduct {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class NotDerivedClassMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : Category {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class InterfaceConstraintGenericMap<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : IDisposable {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class DisposableTest : IDisposable {
			public void Dispose() {
				throw new NotImplementedException();
			}
		}

		public class InterfaceImplementingClass<T1> :
			IMatchMap<IList<T1>, IEnumerable<T1>> where T1 : DisposableTest {

			bool IMatchMap<IList<T1>, IEnumerable<T1>>.Match(IList<T1> source, IEnumerable<T1> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericTypeParameterConstraintGenericMap1<T1, T2> :
			IMatchMap<IList<T1>, IEnumerable<T2>> where T1 : T2 {

			bool IMatchMap<IList<T1>, IEnumerable<T2>>.Match(IList<T1> source, IEnumerable<T2> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericTypeParameterConstraintGenericMap2<T1, T2> :
			IMatchMap<IList<T1>, IEnumerable<T2>> where T1 : List<T2> {

			bool IMatchMap<IList<T1>, IEnumerable<T2>>.Match(IList<T1> source, IEnumerable<T2> destination, MatchingContext context) {
				throw new NotImplementedException();
			}
		}

		public class GenericNestedMap<T1, T2> {
			public class Map1 : IMatchMap<string, int> {
				bool IMatchMap<string, int>.Match(string source, int destination, MatchingContext context) {
					throw new NotImplementedException();
				}
			}

			public class GenericMap2 :
			IMatchMap<IList<T1>, IEnumerable<T2>> {

				bool IMatchMap<IList<T1>, IEnumerable<T2>>.Match(IList<T1> source, IEnumerable<T2> destination, MatchingContext context) {
					throw new NotImplementedException();
				}
			}

			public class GenericMap1<T3> :
			IMatchMap<IList<T3>, IEnumerable<T3>> {

				bool IMatchMap<IList<T3>, IEnumerable<T3>>.Match(IList<T3> source, IEnumerable<T3> destination, MatchingContext context) {
					throw new NotImplementedException();
				}
			}
		}

		internal static MapperConfiguration Configure(MapperConfigurationOptions options) {
			// MatchMap is always configured
			return new MapperConfiguration(i => false, i => false, options);
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
			TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
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
			TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(GenericMap2<,>) }
			}));
		}

		[TestMethod]
		public void ShouldNotAllowOverlappingGenericConstraints() {
			// No constraint - struct
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(StructConstraintGenericMap<>) }
				}));
			}

			// No constraint - class
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(ClassConstraintGenericMap<>) }
				}));
			}

			// No constraint - unmanaged
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(UnmanagedConstraintGenericMap<>) }
				}));
			}

			// No constraint - new()
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// No constraint - base class
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(BaseClassConstraintGenericMap<>) }
				}));
			}

			// No constraint - interface
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(InterfaceConstraintGenericMap<>) }
				}));
			}

			// No constraint - generic type parameter
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericMap1<>), typeof(GenericTypeParameterConstraintGenericMap1<,>) }
				}));
			}

			// new() (counts as no constraint) - struct
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(StructConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() (counts as no constraint) - class
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() (counts as no constraint) - unmanaged
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(UnmanagedConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() (counts as no constraint) - base class
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(BaseClassConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() (counts as no constraint) - interface
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(InterfaceConstraintGenericMap<>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// new() (counts as no constraint) - generic type parameter
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(NewConstraintGenericMap<>) }
				}));
			}

			// unmanaged - struct
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(UnmanagedConstraintGenericMap<>), typeof(StructConstraintGenericMap<>) }
				}));
			}

			// base class - derived class
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(BaseClassConstraintGenericMap<>), typeof(DerivedClassMap<>) }
				}));
			}

			// interface - implementing class
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(InterfaceConstraintGenericMap<>), typeof(InterfaceImplementingClass<>) }
				}));
			}

			// generic type parameter - struct (too complex to check)
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(StructConstraintGenericMap<>) }
				}));
			}

			// generic type parameter - class (too complex to check)
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(ClassConstraintGenericMap<>) }
				}));
			}

			// generic type parameter - unmanaged (too complex to check)
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(UnmanagedConstraintGenericMap<>) }
				}));
			}

			// generic type parameter - base class (too complex to check)
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(BaseClassConstraintGenericMap<>) }
				}));
			}

			// generic type parameter - interface (too complex to check)
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(InterfaceConstraintGenericMap<>) }
				}));
			}

			// generic type parameter - generic type parameter (too complex to check)
			{
				TestUtils.AssertDuplicateMap(() => Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(GenericTypeParameterConstraintGenericMap1<,>), typeof(GenericTypeParameterConstraintGenericMap2<,>) }
				}));
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
			// class - struct
			{ 
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(StructConstraintGenericMap<>) }
				});
			}

			// class - unmanaged
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(UnmanagedConstraintGenericMap<>) }
				});
			}

			// class, new() - struct
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(StructConstraintGenericMap<>) }
				});
			}

			// class, new() - unmanaged
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(UnmanagedConstraintGenericMap<>) }
				});
			}

			// class - unmanaged, new()
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(UnmanagedConstraintGenericMap<>) }
				});
			}
			// class - struct, new()
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(ClassConstraintGenericMap<>), typeof(StructConstraintGenericMap<>) }
				});
			}

			// base class - not derived class
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(BaseClassConstraintGenericMap<>), typeof(NotDerivedClassMap<>) }
				});
			}

			// interface - not implementing class
			{
				Configure(new MapperConfigurationOptions {
					ScanTypes = new List<Type> { typeof(InterfaceConstraintGenericMap<>), typeof(NotDerivedClassMap<>) }
				});
			}
		}

		[TestMethod]
		public void ShouldNotScanNestedClassesInGenericTypes() {
			var config = Configure(new MapperConfigurationOptions {
				ScanTypes = new List<Type> { typeof(GenericNestedMap<,>) }
			});

			Assert.AreEqual(0, config.Matchers.Count);
			Assert.AreEqual(0, config.GenericMatchers.Count());
		}
	}
}
