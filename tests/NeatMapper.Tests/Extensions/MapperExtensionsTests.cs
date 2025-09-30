using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class MapperExtensionsTests {
		// No need to test because it is a compile-time issue
		public static void ShouldNotHaveAmbiguousInvocations() {
			IMapper mapper =
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				null!;
#else
				null;
#endif

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable enable

			MappingOptions? options = null;
			IEnumerable? enumerable1 = null;
			IEnumerable<string>? enumerable2 = null;
			object? option1 = null;
			object? option2 = null;
			object? str = null;
			object strNonNull = null!;
			IEnumerable<string?>? strColl = null;
			IEnumerable<string?> strCollNonNull = null!;
			IEnumerable<string>? strCollElementNonNull = null;
			IEnumerable<string> strCollNonNullElementNonNull = null!;
			ICollection<int> intColl = null!;
#else
			MappingOptions options = null;
			IEnumerable enumerable1 = null;
			IEnumerable<string> enumerable2 = null;
			object option1 = null;
			object option2 = null;
			object str = "Test";
			object strNonNull = null;
			IEnumerable<string> strColl = null;
			IEnumerable<string> strCollNonNull = null;
			IEnumerable<string> strCollElementNonNull = null;
			IEnumerable<string> strCollNonNullElementNonNull = null;
			ICollection<int> intColl = null;
#endif

			// Map (new)
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.Map<string, int>("Test");

					// MappingOptions
					mapper.Map<string, int>("Test", options);

					// IEnumerable
					mapper.Map<string, int>("Test", enumerable1);
					mapper.Map<string, int>("Test", enumerable2);

					// Params (no more params)
					//mapper.Map<string, int>("Test", option1);
					//mapper.Map<string, int>("Test", option1, option2);
				}

				// Explicit destination, inferred source
				{
					// No parameters (MappingOptions with default value overload)
					mapper.Map<int>("Test");
					mapper.Map<int>(strNonNull);

					// MappingOptions
					mapper.Map<int>("Test", options);
					mapper.Map<int>(strNonNull, options);

					// IEnumerable
					mapper.Map<int>("Test", enumerable1);
					mapper.Map<int>(strNonNull, enumerable1);
					mapper.Map<int>("Test", enumerable2);
					mapper.Map<int>(strNonNull, enumerable2);

					// Params (no more params)
					//mapper.Map<int>("Test", option1);
					//mapper.Map<int>(strNonNull, option1);
					//mapper.Map<int>("Test", option1, option2);
					//mapper.Map<int>(strNonNull, option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.Map("Test", typeof(string), typeof(int));
					mapper.Map(str, typeof(string), typeof(int));
					mapper.Map(strNonNull, typeof(string), typeof(int));

					// MappingOptions
					mapper.Map("Test", typeof(string), typeof(int), options);
					mapper.Map(str, typeof(string), typeof(int), options);
					mapper.Map(strNonNull, typeof(string), typeof(int), options);

					// IEnumerable
					mapper.Map("Test", typeof(string), typeof(int), enumerable1);
					mapper.Map(str, typeof(string), typeof(int), enumerable1);
					mapper.Map(strNonNull, typeof(string), typeof(int), enumerable1);
					mapper.Map("Test", typeof(string), typeof(int), enumerable2);
					mapper.Map(str, typeof(string), typeof(int), enumerable2);
					mapper.Map(strNonNull, typeof(string), typeof(int), enumerable2);

					// Params
					mapper.Map("Test", typeof(string), typeof(int), option1);
					mapper.Map(str, typeof(string), typeof(int), option1);
					mapper.Map(strNonNull, typeof(string), typeof(int), option1);
					mapper.Map("Test", typeof(string), typeof(int), option1, option2);
					mapper.Map(str, typeof(string), typeof(int), option1, option2);
					mapper.Map(strNonNull, typeof(string), typeof(int), option1, option2);
				}
			}

			// Map (merge)
			{
				// Collection
				{
					// No parameters (MappingOptions with default value overload)
					mapper.Map(strColl, intColl, (s, i, c) => true);
					mapper.Map(strCollNonNull, intColl, (s, i, c) => true);
					mapper.Map(strCollElementNonNull, intColl, (s, i, c) => true);
					mapper.Map(strCollNonNullElementNonNull, intColl, (s, i, c) => true);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					mapper.Map<string?, int>(strColl, intColl, (s, i, c) => true);
					mapper.Map<string?, int>(strCollNonNull, intColl, (s, i, c) => true);
#else
					mapper.Map<string, int>(strColl, intColl, (s, i, c) => true);
					mapper.Map<string, int>(strCollNonNull, intColl, (s, i, c) => true);
#endif
					mapper.Map<string, int>(strCollElementNonNull, intColl, (s, i, c) => true);
					mapper.Map<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true);

					// MappingOptions
					mapper.Map(strColl, intColl, (s, i, c) => true, options);
					mapper.Map(strCollNonNull, intColl, (s, i, c) => true, options);
					mapper.Map(strCollElementNonNull, intColl, (s, i, c) => true, options);
					mapper.Map(strCollNonNullElementNonNull, intColl, (s, i, c) => true, options);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					mapper.Map<string?, int>(strColl, intColl, (s, i, c) => true, options);
					mapper.Map<string?, int>(strCollNonNull, intColl, (s, i, c) => true, options);
#else
					mapper.Map<string, int>(strColl, intColl, (s, i, c) => true, options);
					mapper.Map<string, int>(strCollNonNull, intColl, (s, i, c) => true, options);
#endif
					mapper.Map<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, options);
					mapper.Map<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, options);

					// IEnumerable
					mapper.Map(strColl, intColl, (s, i, c) => true, enumerable1);
					mapper.Map(strCollNonNull, intColl, (s, i, c) => true, enumerable1);
					mapper.Map(strCollElementNonNull, intColl, (s, i, c) => true, enumerable1);
					mapper.Map(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable1);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					mapper.Map<string?, int>(strColl, intColl, (s, i, c) => true, enumerable1);
					mapper.Map<string?, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable1);
#else
					mapper.Map<string, int>(strColl, intColl, (s, i, c) => true, enumerable1);
					mapper.Map<string, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable1);
#endif
					mapper.Map<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, enumerable1);
					mapper.Map<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable1);
					mapper.Map(strColl, intColl, (s, i, c) => true, enumerable2);
					mapper.Map(strCollNonNull, intColl, (s, i, c) => true, enumerable2);
					mapper.Map(strCollElementNonNull, intColl, (s, i, c) => true, enumerable2);
					mapper.Map(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable2);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					mapper.Map<string?, int>(strColl, intColl, (s, i, c) => true, enumerable2);
					mapper.Map<string?, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable2);
#else
					mapper.Map<string, int>(strColl, intColl, (s, i, c) => true, enumerable2);
					mapper.Map<string, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable2);
#endif
					mapper.Map<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, enumerable2);
					mapper.Map<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable2);

					// Params
					mapper.Map(strColl, intColl, (s, i, c) => true, option1);
					mapper.Map(strCollNonNull, intColl, (s, i, c) => true, option1);
					mapper.Map(strCollElementNonNull, intColl, (s, i, c) => true, option1);
					mapper.Map(strCollNonNullElementNonNull, intColl, (s, i, c) => true, option1);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					mapper.Map<string?, int>(strColl, intColl, (s, i, c) => true, option1);
					mapper.Map<string?, int>(strCollNonNull, intColl, (s, i, c) => true, option1);
#else
					mapper.Map<string, int>(strColl, intColl, (s, i, c) => true, option1);
					mapper.Map<string, int>(strCollNonNull, intColl, (s, i, c) => true, option1);
#endif
					mapper.Map<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, option1);
					mapper.Map<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, option1);
					mapper.Map(strColl, intColl, (s, i, c) => true, option1, option2);
					mapper.Map(strCollNonNull, intColl, (s, i, c) => true, option1, option2);
					mapper.Map(strCollElementNonNull, intColl, (s, i, c) => true, option1, option2);
					mapper.Map(strCollNonNullElementNonNull, intColl, (s, i, c) => true, option1, option2);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					mapper.Map<string?, int>(strColl, intColl, (s, i, c) => true, option1, option2);
					mapper.Map<string?, int>(strCollNonNull, intColl, (s, i, c) => true, option1, option2);
#else
					mapper.Map<string, int>(strColl, intColl, (s, i, c) => true, option1, option2);
					mapper.Map<string, int>(strCollNonNull, intColl, (s, i, c) => true, option1, option2);
#endif
					mapper.Map<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, option1, option2);
					mapper.Map<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, option1, option2);
				}

				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.Map("Test", 2);
					mapper.Map<string, int>("Test", 2);

					// MappingOptions
					mapper.Map("Test", 2, options);
					mapper.Map<string, int>("Test", 2, options);

					// IEnumerable
					mapper.Map("Test", 2, enumerable1);
					mapper.Map<string, int>("Test", 2, enumerable1);
					mapper.Map("Test", 2, enumerable2);
					mapper.Map<string, int>("Test", 2, enumerable2);

					// Params (causes ambiguity with Runtime NewMap/MergeMap)
					//mapper.Map("Test", 2, option1);
					//mapper.Map<string, int>("Test", 2, option1);
					//mapper.Map("Test", 2, option1, option2);
					//mapper.Map<string, int>("Test", 2, option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.Map("Test", typeof(string), 2, typeof(int));
					mapper.Map(str, typeof(string), 2, typeof(int));
					mapper.Map(strNonNull, typeof(string), 2, typeof(int));

					// MappingOptions
					mapper.Map("Test", typeof(string), 2, typeof(int), options);
					mapper.Map(str, typeof(string), 2, typeof(int), options);
					mapper.Map(strNonNull, typeof(string), 2, typeof(int), options);

					// IEnumerable
					mapper.Map("Test", typeof(string), 2, typeof(int), enumerable1);
					mapper.Map(str, typeof(string), 2, typeof(int), enumerable1);
					mapper.Map(strNonNull, typeof(string), 2, typeof(int), enumerable1);
					mapper.Map("Test", typeof(string), 2, typeof(int), enumerable2);
					mapper.Map(str, typeof(string), 2, typeof(int), enumerable2);
					mapper.Map(strNonNull, typeof(string), 2, typeof(int), enumerable2);

					// Params
					mapper.Map("Test", typeof(string), 2, typeof(int), option1);
					mapper.Map(str, typeof(string), 2, typeof(int), option1);
					mapper.Map(strNonNull, typeof(string), 2, typeof(int), option1);
					mapper.Map("Test", typeof(string), 2, typeof(int), option1, option2);
					mapper.Map(str, typeof(string), 2, typeof(int), option1, option2);
					mapper.Map(strNonNull, typeof(string), 2, typeof(int), option1, option2);
				}
			}


			// CanMapNew
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapNew<string, int>();

					// MappingOptions
					mapper.CanMapNew<string, int>(options);

					// IEnumerable
					mapper.CanMapNew<string, int>(enumerable1);
					mapper.CanMapNew<string, int>(enumerable2);

					// Params
					mapper.CanMapNew<string, int>(option1);
					mapper.CanMapNew<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapNew(typeof(string), typeof(int));

					// MappingOptions
					mapper.CanMapNew(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.CanMapNew(typeof(string), typeof(int), enumerable1);
					mapper.CanMapNew(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.CanMapNew(typeof(string), typeof(int), option1);
					mapper.CanMapNew(typeof(string), typeof(int), option1, option2);
				}
			}

			// CanMapMerge
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapMerge<string, int>();

					// MappingOptions
					mapper.CanMapMerge<string, int>(options);

					// IEnumerable
					mapper.CanMapMerge<string, int>(enumerable1);
					mapper.CanMapMerge<string, int>(enumerable2);

					// Params
					mapper.CanMapMerge<string, int>(option1);
					mapper.CanMapMerge<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapMerge(typeof(string), typeof(int));

					// MappingOptions
					mapper.CanMapMerge(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.CanMapMerge(typeof(string), typeof(int), enumerable1);
					mapper.CanMapMerge(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.CanMapMerge(typeof(string), typeof(int), option1);
					mapper.CanMapMerge(typeof(string), typeof(int), option1, option2);
				}
			}


			// MapNewFactory
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapNewFactory<string, int>();

					// MappingOptions
					mapper.MapNewFactory<string, int>(options);

					// IEnumerable
					mapper.MapNewFactory<string, int>(enumerable1);
					mapper.MapNewFactory<string, int>(enumerable2);

					// Params
					mapper.MapNewFactory<string, int>(option1);
					mapper.MapNewFactory<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapNewFactory(typeof(string), typeof(int));

					// MappingOptions
					mapper.MapNewFactory(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.MapNewFactory(typeof(string), typeof(int), enumerable1);
					mapper.MapNewFactory(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.MapNewFactory(typeof(string), typeof(int), option1);
					mapper.MapNewFactory(typeof(string), typeof(int), option1, option2);
				}
			}

			// MapMergeFactory
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapMergeFactory<string, int>();

					// MappingOptions
					mapper.MapMergeFactory<string, int>(options);

					// IEnumerable
					mapper.MapMergeFactory<string, int>(enumerable1);
					mapper.MapMergeFactory<string, int>(enumerable2);

					// Params
					mapper.MapMergeFactory<string, int>(option1);
					mapper.MapMergeFactory<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapMergeFactory(typeof(string), typeof(int));

					// MappingOptions
					mapper.MapMergeFactory(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.MapMergeFactory(typeof(string), typeof(int), enumerable1);
					mapper.MapMergeFactory(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.MapMergeFactory(typeof(string), typeof(int), option1);
					mapper.MapMergeFactory(typeof(string), typeof(int), option1, option2);
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}

		[TestMethod]
		public void ShouldRespectMapRequired() {
			var additionalMaps = new CustomNewAdditionalMapsOptions();
			additionalMaps.AddMap<int?, string>((_, __) => null);
			IMapper mapper = new CustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(NewMapsTests.Maps) }
			}, additionalMaps);


			// Returns null only if null
			{
				Assert.IsNull(mapper.MapRequired<Product, ProductDto>(null));
				Assert.IsNotNull(mapper.MapRequired<Product, ProductDto>(new Product {
					Code = "Test"
				}));
			}

			// Never returns null
			{
				Assert.IsNotNull(mapper.MapRequired<string, ClassWithoutParameterlessConstructor>(null));
				Assert.IsNotNull(mapper.MapRequired<string, ClassWithoutParameterlessConstructor>("Test"));
			}

			// Always returns null
			{
				Assert.IsNull(mapper.MapRequired<int?, string>(null));
				Assert.ThrowsException<NullReferenceException>(() => mapper.MapRequired<int?, string>(2));
			}
		}
	}
}
