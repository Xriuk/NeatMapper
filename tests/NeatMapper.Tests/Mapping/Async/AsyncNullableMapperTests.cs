using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeatMapper.Tests.Mapping.Async {
	[TestClass]
	public class AsyncNullableMapperTests {
		IAsyncMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new AsyncNullableMapper(new AsyncCustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps) }
			}));
		}


		[TestMethod]
		public async Task ShouldMapValueTypeToNullable() {
			Assert.IsTrue(_mapper.CanMapAsyncNew<int, char?>());

			var result = await _mapper.MapAsync<int, char?>(122);
			Assert.IsNotNull(result);
			Assert.AreEqual('z', result.Value);

			using(var factory = _mapper.MapAsyncNewFactory<int, char?>()){
				result = await factory.Invoke(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('z', result.Value);
			}
		}

		[TestMethod]
		public async Task ShouldMapReferenceTypeToNullable() {
			{ 
				Assert.IsTrue(_mapper.CanMapAsyncNew<string, int?>());

				var result = await _mapper.MapAsync<string, int?>("ciao");
				Assert.IsNotNull(result);
				Assert.AreEqual(4, result.Value);

				result = await _mapper.MapAsync<string, int?>(null);
				Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
				Assert.AreEqual(-1, result.Value);

				using (var factory = _mapper.MapAsyncNewFactory<string, int?>()) {
					result = await factory.Invoke("ciao");
					Assert.IsNotNull(result);
					Assert.AreEqual(4, result.Value);

					result = await factory.Invoke(null);
					Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
					Assert.AreEqual(-1, result.Value);
				}
			}

			{
				Assert.IsTrue(_mapper.CanMapAsyncNew<string, KeyValuePair<string, int>?>());

				var result = await _mapper.MapAsync<string, KeyValuePair<string, int>?>("ciao");
				Assert.IsNotNull(result);
				Assert.AreEqual("ciao", result.Value.Key);
				Assert.AreEqual(4, result.Value.Value);

				result = await _mapper.MapAsync<string, KeyValuePair<string, int>?>(null);
				Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
				Assert.IsNull(result.Value.Key);
				Assert.AreEqual(-1, result.Value.Value);

				using (var factory = _mapper.MapAsyncNewFactory<string, KeyValuePair<string, int>?>()) {
					result = await factory.Invoke("ciao");
					Assert.IsNotNull(result);
					Assert.AreEqual("ciao", result.Value.Key);
					Assert.AreEqual(4, result.Value.Value);

					result = await factory.Invoke(null);
					Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
					Assert.IsNull(result.Value.Key);
					Assert.AreEqual(-1, result.Value.Value);
				}
			}
		}


		[TestMethod]
		public async Task ShouldMapNullableToValueType() {
			Assert.IsTrue(_mapper.CanMapAsyncNew<int?, char>());

			Assert.AreEqual('z', await _mapper.MapAsync<int?, char>(122));

			await Assert.ThrowsExceptionAsync<MappingException>(() => _mapper.MapAsync<int?, char>(null));

			using (var factory = _mapper.MapAsyncNewFactory<int?, char>()) {
				Assert.AreEqual('z', await factory.Invoke(122));

				await Assert.ThrowsExceptionAsync<MappingException>(() => factory.Invoke(null));
			}
		}

		[TestMethod]
		public async Task ShouldMapNullableToReferenceType() {
			Assert.IsTrue(_mapper.CanMapAsyncNew<int?, string>());

			Assert.AreEqual("4", await _mapper.MapAsync<int?, string>(2));

			Assert.IsNull(await _mapper.MapAsync<int?, string>(null));

			using (var factory = _mapper.MapAsyncNewFactory<int?, string>()) {
				Assert.AreEqual("4", await factory.Invoke(2));

				Assert.IsNull(await factory.Invoke(null));
			}
		}


		[TestMethod]
		public async Task ShouldMapNullableToNullable() {
			// int? -> char
			{
				var additionalMaps = new CustomAsyncNewAdditionalMapsOptions();
				additionalMaps.AddMap<int?, char>((n, c) => Task.FromResult(n != null ? (char)(n.Value + 2) : '\0'));
				var mapper = new AsyncNullableMapper(new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps) }
				}, additionalMaps));


				Assert.IsTrue(mapper.CanMapAsyncNew<int?, char?>());

				var result = await mapper.MapAsync<int?, char?>(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('|', result.Value);

				result = await mapper.MapAsync<int?, char?>(null);
				Assert.IsNotNull(result);
				Assert.AreEqual('\0', result.Value);

				using (var factory = mapper.MapAsyncNewFactory<int?, char?>()) {
					result = await factory.Invoke(122);
					Assert.IsNotNull(result);
					Assert.AreEqual('|', result.Value);

					result = await factory.Invoke(null);
					Assert.IsNotNull(result);
					Assert.AreEqual('\0', result.Value);
				}
			}

			// int -> char?
			{
				var additionalMaps = new CustomAsyncNewAdditionalMapsOptions();
				additionalMaps.AddMap<int, char?>((n, c) => Task.FromResult(n == 0 ? null : (char?)(n - 2)));
				var mapper = new AsyncNullableMapper(new AsyncCustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(AsyncNewMapsTests.Maps) }
				}, additionalMaps));


				Assert.IsTrue(mapper.CanMapAsyncNew<int?, char?>());

				var result = await mapper.MapAsync<int?, char?>(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('x', result.Value);

				Assert.IsNull(await mapper.MapAsync<int?, char?>(null)); // Shortcut
				Assert.IsNull(await mapper.MapAsync<int?, char?>(0)); // Map

				using (var factory = mapper.MapAsyncNewFactory<int?, char?>()) {
					result = await factory.Invoke(122);
					Assert.IsNotNull(result);
					Assert.AreEqual('x', result.Value);

					Assert.IsNull(await factory.Invoke(null)); // Shortcut
					Assert.IsNull(await factory.Invoke(0)); // Map
				}
			}

			// int -> char
			{ 
				Assert.IsTrue(_mapper.CanMapAsyncNew<int?, char?>());

				var result = await _mapper.MapAsync<int?, char?>(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('z', result.Value);

				Assert.IsNull(await _mapper.MapAsync<int?, char?>(null));

				using (var factory = _mapper.MapAsyncNewFactory<int?, char?>()) {
					result = await factory.Invoke(122);
					Assert.IsNotNull(result);
					Assert.AreEqual('z', result.Value);

					Assert.IsNull(await factory.Invoke(null));
				}
			}
		}


		[TestMethod]
		public void ShouldReturnNullifiedTypes() {
			var maps = _mapper.GetAsyncNewMaps();

			Assert.IsTrue(maps.Contains((typeof(int), typeof(char))));
			Assert.IsTrue(maps.Contains((typeof(int?), typeof(char))));
			Assert.IsTrue(maps.Contains((typeof(int), typeof(char?))));
			Assert.IsTrue(maps.Contains((typeof(int?), typeof(char?))));

			Assert.IsTrue(maps.Contains((typeof(string), typeof(int))));
			Assert.IsTrue(maps.Contains((typeof(string), typeof(int?))));
		}
	}
}
