using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeatMapper.Tests.Mapping.Async;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Extensions {
	[TestClass]
	public class AsyncMapperExtensionsTests {
		// No need to test because it is a compile-time issue
		public static async Task ShouldNotHaveAmbiguousInvocations() {
			IAsyncMapper mapper =
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

			// NewMap
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					await mapper.MapAsync<string, int>("Test");

					// MappingOptions
					await mapper.MapAsync<string, int>("Test", options);

					// IEnumerable
					await mapper.MapAsync<string, int>("Test", enumerable1);
					await mapper.MapAsync<string, int>("Test", enumerable2);
				}

				// Explicit destination, inferred source
				{
					// No parameters (MappingOptions with default value overload)
					await mapper.MapAsync<int>("Test");
					await mapper.MapAsync<int>(strNonNull);

					// MappingOptions
					await mapper.MapAsync<int>("Test", options);
					await mapper.MapAsync<int>(strNonNull, options);

					// IEnumerable
					await mapper.MapAsync<int>("Test", enumerable1);
					await mapper.MapAsync<int>(strNonNull, enumerable1);
					await mapper.MapAsync<int>("Test", enumerable2);
					await mapper.MapAsync<int>(strNonNull, enumerable2);

				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					await mapper.MapAsync("Test", typeof(string), typeof(int));
					await mapper.MapAsync(str, typeof(string), typeof(int));
					await mapper.MapAsync(strNonNull, typeof(string), typeof(int));

					// MappingOptions
					await mapper.MapAsync("Test", typeof(string), typeof(int), options);
					await mapper.MapAsync(str, typeof(string), typeof(int), options);
					await mapper.MapAsync(strNonNull, typeof(string), typeof(int), options);

					// IEnumerable
					await mapper.MapAsync("Test", typeof(string), typeof(int), enumerable1);
					await mapper.MapAsync(str, typeof(string), typeof(int), enumerable1);
					await mapper.MapAsync(strNonNull, typeof(string), typeof(int), enumerable1);
					await mapper.MapAsync("Test", typeof(string), typeof(int), enumerable2);
					await mapper.MapAsync(str, typeof(string), typeof(int), enumerable2);
					await mapper.MapAsync(strNonNull, typeof(string), typeof(int), enumerable2);
				}
			}

			// MergeMap
			{
				// Collection
				{
					// No parameters (MappingOptions with default value overload)
					await mapper.MapAsync(strColl, intColl, (s, i, c) => true);
					await mapper.MapAsync(strCollNonNull, intColl, (s, i, c) => true);
					await mapper.MapAsync(strCollElementNonNull, intColl, (s, i, c) => true);
					await mapper.MapAsync(strCollNonNullElementNonNull, intColl, (s, i, c) => true);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					await mapper.MapAsync<string?, int>(strColl, intColl, (s, i, c) => true);
					await mapper.MapAsync<string?, int>(strCollNonNull, intColl, (s, i, c) => true);
#else
					await mapper.MapAsync<string, int>(strColl, intColl, (s, i, c) => true);
					await mapper.MapAsync<string, int>(strCollNonNull, intColl, (s, i, c) => true);
#endif
					await mapper.MapAsync<string, int>(strCollElementNonNull, intColl, (s, i, c) => true);
					await mapper.MapAsync<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true);

					// MappingOptions
					await mapper.MapAsync(strColl, intColl, (s, i, c) => true, options);
					await mapper.MapAsync(strCollNonNull, intColl, (s, i, c) => true, options);
					await mapper.MapAsync(strCollElementNonNull, intColl, (s, i, c) => true, options);
					await mapper.MapAsync(strCollNonNullElementNonNull, intColl, (s, i, c) => true, options);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					await mapper.MapAsync<string?, int>(strColl, intColl, (s, i, c) => true, options);
					await mapper.MapAsync<string?, int>(strCollNonNull, intColl, (s, i, c) => true, options);
#else
					await mapper.MapAsync<string, int>(strColl, intColl, (s, i, c) => true, options);
					await mapper.MapAsync<string, int>(strCollNonNull, intColl, (s, i, c) => true, options);
#endif
					await mapper.MapAsync<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, options);
					await mapper.MapAsync<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, options);

					// IEnumerable
					await mapper.MapAsync(strColl, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync(strCollNonNull, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync(strCollElementNonNull, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable1);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					await mapper.MapAsync<string?, int>(strColl, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync<string?, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable1);
#else
					await mapper.MapAsync<string, int>(strColl, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync<string, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable1);
#endif
					await mapper.MapAsync<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable1);
					await mapper.MapAsync(strColl, intColl, (s, i, c) => true, enumerable2);
					await mapper.MapAsync(strCollNonNull, intColl, (s, i, c) => true, enumerable2);
					await mapper.MapAsync(strCollElementNonNull, intColl, (s, i, c) => true, enumerable2);
					await mapper.MapAsync(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable2);
#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
					await mapper.MapAsync<string?, int>(strColl, intColl, (s, i, c) => true, enumerable2);
					await mapper.MapAsync<string?, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable2);
#else
					await mapper.MapAsync<string, int>(strColl, intColl, (s, i, c) => true, enumerable2);
					await mapper.MapAsync<string, int>(strCollNonNull, intColl, (s, i, c) => true, enumerable2);
#endif
					await mapper.MapAsync<string, int>(strCollElementNonNull, intColl, (s, i, c) => true, enumerable2);
					await mapper.MapAsync<string, int>(strCollNonNullElementNonNull, intColl, (s, i, c) => true, enumerable2);
				}

				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					await mapper.MapAsync("Test", 2);
					await mapper.MapAsync<string, int>("Test", 2);

					// MappingOptions
					await mapper.MapAsync("Test", 2, options);
					await mapper.MapAsync<string, int>("Test", 2, options);

					// IEnumerable
					await mapper.MapAsync("Test", 2, enumerable1);
					await mapper.MapAsync<string, int>("Test", 2, enumerable1);
					await mapper.MapAsync("Test", 2, enumerable2);
					await mapper.MapAsync<string, int>("Test", 2, enumerable2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					await mapper.MapAsync("Test", typeof(string), 2, typeof(int));
					await mapper.MapAsync(str, typeof(string), 2, typeof(int));
					await mapper.MapAsync(strNonNull, typeof(string), 2, typeof(int));

					// MappingOptions
					await mapper.MapAsync("Test", typeof(string), 2, typeof(int), options);
					await mapper.MapAsync(str, typeof(string), 2, typeof(int), options);
					await mapper.MapAsync(strNonNull, typeof(string), 2, typeof(int), options);

					// IEnumerable
					await mapper.MapAsync("Test", typeof(string), 2, typeof(int), enumerable1);
					await mapper.MapAsync(str, typeof(string), 2, typeof(int), enumerable1);
					await mapper.MapAsync(strNonNull, typeof(string), 2, typeof(int), enumerable1);
					await mapper.MapAsync("Test", typeof(string), 2, typeof(int), enumerable2);
					await mapper.MapAsync(str, typeof(string), 2, typeof(int), enumerable2);
					await mapper.MapAsync(strNonNull, typeof(string), 2, typeof(int), enumerable2);
				}
			}


			// CanMapAsyncNew
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapAsyncNew<string, int>();

					// MappingOptions
					mapper.CanMapAsyncNew<string, int>(options);

					// IEnumerable
					mapper.CanMapAsyncNew<string, int>(enumerable1);
					mapper.CanMapAsyncNew<string, int>(enumerable2);

					// Params
					mapper.CanMapAsyncNew<string, int>(option1);
					mapper.CanMapAsyncNew<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapAsyncNew(typeof(string), typeof(int));

					// MappingOptions
					mapper.CanMapAsyncNew(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.CanMapAsyncNew(typeof(string), typeof(int), enumerable1);
					mapper.CanMapAsyncNew(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.CanMapAsyncNew(typeof(string), typeof(int), option1);
					mapper.CanMapAsyncNew(typeof(string), typeof(int), option1, option2);
				}
			}

			// CanMapAsyncMerge
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapAsyncMerge<string, int>();

					// MappingOptions
					mapper.CanMapAsyncMerge<string, int>(options);

					// IEnumerable
					mapper.CanMapAsyncMerge<string, int>(enumerable1);
					mapper.CanMapAsyncMerge<string, int>(enumerable2);

					// Params
					mapper.CanMapAsyncMerge<string, int>(option1);
					mapper.CanMapAsyncMerge<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.CanMapAsyncMerge(typeof(string), typeof(int));

					// MappingOptions
					mapper.CanMapAsyncMerge(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.CanMapAsyncMerge(typeof(string), typeof(int), enumerable1);
					mapper.CanMapAsyncMerge(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.CanMapAsyncMerge(typeof(string), typeof(int), option1);
					mapper.CanMapAsyncMerge(typeof(string), typeof(int), option1, option2);
				}
			}


			// MapNewFactory
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapAsyncNewFactory<string, int>();

					// MappingOptions
					mapper.MapAsyncNewFactory<string, int>(options);

					// IEnumerable
					mapper.MapAsyncNewFactory<string, int>(enumerable1);
					mapper.MapAsyncNewFactory<string, int>(enumerable2);

					// Params
					mapper.MapAsyncNewFactory<string, int>(option1);
					mapper.MapAsyncNewFactory<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapAsyncNewFactory(typeof(string), typeof(int));

					// MappingOptions
					mapper.MapAsyncNewFactory(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.MapAsyncNewFactory(typeof(string), typeof(int), enumerable1);
					mapper.MapAsyncNewFactory(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.MapAsyncNewFactory(typeof(string), typeof(int), option1);
					mapper.MapAsyncNewFactory(typeof(string), typeof(int), option1, option2);
				}
			}

			// MapMergeFactory
			{
				// Explicit source and destination
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapAsyncMergeFactory<string, int>();

					// MappingOptions
					mapper.MapAsyncMergeFactory<string, int>(options);

					// IEnumerable
					mapper.MapAsyncMergeFactory<string, int>(enumerable1);
					mapper.MapAsyncMergeFactory<string, int>(enumerable2);

					// Params
					mapper.MapAsyncMergeFactory<string, int>(option1);
					mapper.MapAsyncMergeFactory<string, int>(option1, option2);
				}

				// Runtime
				{
					// No parameters (MappingOptions with default value overload)
					mapper.MapAsyncMergeFactory(typeof(string), typeof(int));

					// MappingOptions
					mapper.MapAsyncMergeFactory(typeof(string), typeof(int), options);

					// IEnumerable
					mapper.MapAsyncMergeFactory(typeof(string), typeof(int), enumerable1);
					mapper.MapAsyncMergeFactory(typeof(string), typeof(int), enumerable2);

					// Params
					mapper.MapAsyncMergeFactory(typeof(string), typeof(int), option1);
					mapper.MapAsyncMergeFactory(typeof(string), typeof(int), option1, option2);
				}
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif
		}

		[TestMethod]
		public async Task ShouldRespectMapRequired() {
			var additionalMaps = new CustomAsyncNewAdditionalMapsOptions();
			additionalMaps.AddMap<int?, string>((_, __) => Task.FromResult<string>(null));
			IAsyncMapper mapper = new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps) }
			}, additionalMaps);


			// Returns null only if null
			{
				Assert.IsNull(await mapper.MapRequiredAsync<Product, ProductDto>(null));
				Assert.IsNotNull(await mapper.MapRequiredAsync<Product, ProductDto>(new Product {
					Code = "Test"
				}));
			}

			// Never returns null
			{
				Assert.IsNotNull(await mapper.MapRequiredAsync<string, ClassWithoutParameterlessConstructor>(null));
				Assert.IsNotNull(await mapper.MapRequiredAsync<string, ClassWithoutParameterlessConstructor>("Test"));
			}

			// Always returns null
			{
				Assert.IsNull(await mapper.MapRequiredAsync<int?, string>(null));
				await Assert.ThrowsExceptionAsync<NullReferenceException>(() => mapper.MapRequiredAsync<int?, string>(2));
			}
		}
	}
}
