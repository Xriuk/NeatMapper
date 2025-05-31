using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.Tests.Projection {
	[TestClass]
	public class NullableProjectorTests {
		IProjector _projector = null;

		[TestInitialize]
		public void Initialize() {
			_projector = new NullableProjector(new CustomProjector(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(ProjectionTests.Maps) }
			}));
		}


		[TestMethod]
		public void ShouldProjectValueTypeToNullable() {
			Assert.IsTrue(_projector.CanProject<int, char?>());

			Expression<Func<int, char?>> expr = source => (char?)(char)source;
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<int, char?>());
		}

		[TestMethod]
		public void ShouldProjectReferenceTypeToNullable() {
			{
				Assert.IsTrue(_projector.CanProject<string, int?>());

				Expression<Func<string, int?>> expr = source => (int?)(source != null ? source.Length : -1);
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<string, int?>());
			}

			{
				Assert.IsTrue(_projector.CanProject<string, KeyValuePair<string, int>?>());

				Expression<Func<string, KeyValuePair<string, int>?>> expr = source => (KeyValuePair<string, int>?)(new KeyValuePair<string, int>(source, source.Length));
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<string, KeyValuePair<string, int>?>());
			}
		}


		[TestMethod]
		public void ShouldProjectNullableToValueType() {
			Assert.IsTrue(_projector.CanProject<int?, char>());

			// source => source == null ?
			//     throw new ProjectionException(new InvalidOperationException("Cannot map null value because the destination type is not nullable.")) :
			//     (char)source.Value
			var source = Expression.Parameter(typeof(int?), "source");
			var body = Expression.Condition(
				Expression.Equal(source, Expression.Constant(null, typeof(int?))),
					Expression.Throw(
						Expression.New(
							typeof(ProjectionException).GetConstructor(new []{ typeof(Exception), typeof((Type, Type)) }),
							Expression.New(
								typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }),
								Expression.Constant("Cannot map null value because the destination type is not nullable.")),
							Expression.Constant((typeof(int?), typeof(char)))),
						typeof(char)),
					Expression.Convert(Expression.Property(source, nameof(Nullable<int>.Value)), typeof(char)));
			TestUtils.AssertExpressionsEqual(Expression.Lambda(body, source), _projector.Project<int?, char>());
		}

		[TestMethod]
		public void ShouldProjectNullableToReferenceType() {
			Assert.IsTrue(_projector.CanProject<int?, string>());

			Expression<Func<int?, string>> expr = source => source == null ? null : (source.Value * 2).ToString();
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<int?, string>());
		}


		[TestMethod]
		public void ShouldProjectNullableToNullable() {
			// int? -> char
			{
				var additionalMaps = new CustomProjectionAdditionalMapsOptions();
				additionalMaps.AddMap<int?, char>(c => n => n != null ? (char)(n.Value + 2) : '\0');
				var projector = new NullableProjector(new CustomProjector(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(ProjectionTests.Maps) }
				}, additionalMaps));


				Assert.IsTrue(projector.CanProject<int?, char?>());

				Expression<Func<int?, char?>> expr = n => (char?)(n != null ? (char)(n.Value + 2) : '\0');
				TestUtils.AssertExpressionsEqual(expr, projector.Project<int?, char?>());
			}

			// int -> char?
			{
				var additionalMaps = new CustomProjectionAdditionalMapsOptions();
				additionalMaps.AddMap<int, char?>(c => n => n == 0 ? null : (char?)(n - 2));
				var projector = new NullableProjector(new CustomProjector(new CustomMapsOptions {
					TypesToScan = new List<Type> { typeof(ProjectionTests.Maps) }
				}, additionalMaps));


				Assert.IsTrue(projector.CanProject<int?, char?>());

				Expression<Func<int?, char?>> expr = n => n == null ? null : (n.Value == 0 ? null : (char?)(n.Value - 2));
				TestUtils.AssertExpressionsEqual(expr, projector.Project<int?, char?>());
			}

			// int -> char
			{
				Assert.IsTrue(_projector.CanProject<int?, char?>());

				Expression<Func<int?, char?>> expr = n => n == null ? null : (char?)(char)n.Value;
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<int?, char?>());
			}
		}


		[TestMethod]
		public void ShouldCheckButNotProjectOpenNullable() {
			{
				Assert.IsTrue(_projector.CanProject(typeof(int), typeof(Nullable<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _projector.Project(typeof(int), typeof(Nullable<>)));
			}

			{
				Assert.IsTrue(_projector.CanProject(typeof(Nullable<>), typeof(int)));

				Assert.ThrowsException<MapNotFoundException>(() => _projector.Project(typeof(Nullable<>), typeof(int)));
			}

			{
				Assert.IsTrue(_projector.CanProject(typeof(Nullable<>), typeof(Nullable<>)));

				Assert.ThrowsException<MapNotFoundException>(() => _projector.Project(typeof(Nullable<>), typeof(Nullable<>)));
			}
		}


		[TestMethod]
		public void ShouldReturnNullifiedTypes() {
			var maps = _projector.GetMaps();

			Assert.IsTrue(maps.Contains((typeof(int), typeof(char))));
			Assert.IsTrue(maps.Contains((typeof(int?), typeof(char))));
			Assert.IsTrue(maps.Contains((typeof(int), typeof(char?))));
			Assert.IsTrue(maps.Contains((typeof(int?), typeof(char?))));

			Assert.IsTrue(maps.Contains((typeof(string), typeof(int))));
			Assert.IsTrue(maps.Contains((typeof(string), typeof(int?))));
		}
	}
}
