using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class NullableMapperTests {
		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new NullableMapper(new CustomMapper(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(NewMapsTests.Maps) }
			}));
		}


		[TestMethod]
		public void ShouldMapValueTypeToNullable() {
			Assert.IsTrue(_mapper.CanMapNew<int, char?>());

			var result = _mapper.Map<int, char?>(122);
			Assert.IsNotNull(result);
			Assert.AreEqual('z', result.Value);

			using(var factory = _mapper.MapNewFactory<int, char?>()){
				result = factory.Invoke(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('z', result.Value);
			}
		}

		[TestMethod]
		public void ShouldMapReferenceTypeToNullable() {
			{ 
				Assert.IsTrue(_mapper.CanMapNew<string, int?>());

				var result = _mapper.Map<string, int?>("ciao");
				Assert.IsNotNull(result);
				Assert.AreEqual(4, result.Value);

				result = _mapper.Map<string, int?>(null);
				Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
				Assert.AreEqual(-1, result.Value);

				using (var factory = _mapper.MapNewFactory<string, int?>()) {
					result = factory.Invoke("ciao");
					Assert.IsNotNull(result);
					Assert.AreEqual(4, result.Value);

					result = factory.Invoke(null);
					Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
					Assert.AreEqual(-1, result.Value);
				}
			}

			{
				Assert.IsTrue(_mapper.CanMapNew<string, KeyValuePair<string, int>?>());

				var result = _mapper.Map<string, KeyValuePair<string, int>?>("ciao");
				Assert.IsNotNull(result);
				Assert.AreEqual("ciao", result.Value.Key);
				Assert.AreEqual(4, result.Value.Value);

				result = _mapper.Map<string, KeyValuePair<string, int>?>(null);
				Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
				Assert.IsNull(result.Value.Key);
				Assert.AreEqual(-1, result.Value.Value);

				using (var factory = _mapper.MapNewFactory<string, KeyValuePair<string, int>?>()) {
					result = factory.Invoke("ciao");
					Assert.IsNotNull(result);
					Assert.AreEqual("ciao", result.Value.Key);
					Assert.AreEqual(4, result.Value.Value);

					result = factory.Invoke(null);
					Assert.IsNotNull(result); // NullableMapper does not short-circuit here, and the original map is still invoked
					Assert.IsNull(result.Value.Key);
					Assert.AreEqual(-1, result.Value.Value);
				}
			}
		}


		[TestMethod]
		public void ShouldMapNullableToValueType() {
			Assert.IsTrue(_mapper.CanMapNew<int?, char>());

			Assert.AreEqual('z', _mapper.Map<int?, char>(122));

			Assert.ThrowsException<MappingException>(() => _mapper.Map<int?, char>(null));

			using (var factory = _mapper.MapNewFactory<int?, char>()) {
				Assert.AreEqual('z', factory.Invoke(122));

				Assert.ThrowsException<MappingException>(() => factory.Invoke(null));
			}
		}

		[TestMethod]
		public void ShouldMapNullableToReferenceType() {
			Assert.IsTrue(_mapper.CanMapNew<int?, string>());

			Assert.AreEqual("4", _mapper.Map<int?, string>(2));

			Assert.IsNull(_mapper.Map<int?, string>(null));

			using (var factory = _mapper.MapNewFactory<int?, string>()) {
				Assert.AreEqual("4", factory.Invoke(2));

				Assert.IsNull(factory.Invoke(null));
			}
		}


		[TestMethod]
		public void ShouldMapNullableToNullable() {
			// int? -> char
			{
				var additionalMaps = new CustomNewAdditionalMapsOptions();
				additionalMaps.AddMap<int?, char>((n, c) => n != null ? (char)(n.Value + 2) : '\0');
				var mapper = new NullableMapper(new CustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(NewMapsTests.Maps) }
				}, additionalMaps));


				Assert.IsTrue(mapper.CanMapNew<int?, char?>());

				var result = mapper.Map<int?, char?>(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('|', result.Value);

				result = mapper.Map<int?, char?>(null);
				Assert.IsNotNull(result);
				Assert.AreEqual('\0', result.Value);

				using (var factory = mapper.MapNewFactory<int?, char?>()) {
					result = factory.Invoke(122);
					Assert.IsNotNull(result);
					Assert.AreEqual('|', result.Value);

					result = factory.Invoke(null);
					Assert.IsNotNull(result);
					Assert.AreEqual('\0', result.Value);
				}
			}

			// int -> char?
			{
				var additionalMaps = new CustomNewAdditionalMapsOptions();
				additionalMaps.AddMap<int, char?>((n, c) => n == 0 ? null : (char?)(n - 2));
				var mapper = new NullableMapper(new CustomMapper(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(NewMapsTests.Maps) }
				}, additionalMaps));


				Assert.IsTrue(mapper.CanMapNew<int?, char?>());

				var result = mapper.Map<int?, char?>(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('x', result.Value);

				Assert.IsNull(mapper.Map<int?, char?>(null)); // Shortcut
				Assert.IsNull(mapper.Map<int?, char?>(0)); // Map

				using (var factory = mapper.MapNewFactory<int?, char?>()) {
					result = factory.Invoke(122);
					Assert.IsNotNull(result);
					Assert.AreEqual('x', result.Value);

					Assert.IsNull(factory.Invoke(null)); // Shortcut
					Assert.IsNull(factory.Invoke(0)); // Map
				}
			}

			// int -> char
			{ 
				Assert.IsTrue(_mapper.CanMapNew<int?, char?>());

				var result = _mapper.Map<int?, char?>(122);
				Assert.IsNotNull(result);
				Assert.AreEqual('z', result.Value);

				Assert.IsNull(_mapper.Map<int?, char?>(null));

				using (var factory = _mapper.MapNewFactory<int?, char?>()) {
					result = factory.Invoke(122);
					Assert.IsNotNull(result);
					Assert.AreEqual('z', result.Value);

					Assert.IsNull(factory.Invoke(null));
				}
			}
		}


		[TestMethod]
		public void ShouldCheckButNotMapOpenNullable() {
			{ 
				Assert.IsTrue(_mapper.CanMapNew(typeof(int), typeof(Nullable<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(1, typeof(int), typeof(Nullable<>)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(Nullable<>), typeof(int)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(1, typeof(Nullable<>), typeof(int)));
			}

			{
				Assert.IsTrue(_mapper.CanMapNew(typeof(Nullable<>), typeof(Nullable<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map(1, typeof(Nullable<>), typeof(Nullable<>)));
			}
		}


		[TestMethod]
		public void ShouldReturnNullifiedTypes() {
			var maps = _mapper.GetNewMaps();

			Assert.IsTrue(maps.Contains((typeof(int), typeof(char))));
			Assert.IsTrue(maps.Contains((typeof(int?), typeof(char))));
			Assert.IsTrue(maps.Contains((typeof(int), typeof(char?))));
			Assert.IsTrue(maps.Contains((typeof(int?), typeof(char?))));

			Assert.IsTrue(maps.Contains((typeof(string), typeof(int))));
			Assert.IsTrue(maps.Contains((typeof(string), typeof(int?))));
		}
	}
}
