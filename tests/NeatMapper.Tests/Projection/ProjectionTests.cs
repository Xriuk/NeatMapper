using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NeatMapper.Tests.Projection {
	[TestClass]
	public class ProjectionTests {
		public class Maps :
#if NET7_0_OR_GREATER
			IProjectionMapStatic<int, string>,
			IProjectionMapStatic<Price, decimal>,
			IProjectionMapStatic<Product, ProductDto>,
			IProjectionMapStatic<LimitedProduct, LimitedProductDto>,
			IProjectionMapStatic<Category, int?>,
			IProjectionMapStatic<Category, CategoryDto>,
			IProjectionMapStatic<float, int>,
			IProjectionMapStatic<string, KeyValuePair<string, int>>,
			IProjectionMapStatic<int, char>,
			IProjectionMapStatic<char, float>,
			IProjectionMapStatic<float, double>
#else
			IProjectionMap<int, string>,
			IProjectionMap<Price, decimal>,
			IProjectionMap<Product, ProductDto>,
			IProjectionMap<LimitedProduct, LimitedProductDto>,
			IProjectionMap<Category, int?>,
			IProjectionMap<Category, CategoryDto>,
			IProjectionMap<float, int>,
			IProjectionMap<string, KeyValuePair<string, int>>,
			IProjectionMap<int, char>,
			IProjectionMap<char, float>,
			IProjectionMap<float, double>
#endif
			{

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<int, string>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<int, string>
#else
				IProjectionMap<int, string>
#endif
				.Project(ProjectionContext context) {

				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				return source => (source * 2).ToString();
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Price, decimal>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Price, decimal>
#else
				IProjectionMap<Price, decimal>
#endif
				.Project(ProjectionContext context) {

				return source => source != null ? source.Amount : 0m;
			}

			public static int? GetCategoryId(CategoryDto cat) {
				return cat?.Id;
			}

			// Nested map + method
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Product, ProductDto>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Product, ProductDto>
#else
				IProjectionMap<Product, ProductDto>
#endif
				.Project(ProjectionContext context) {

				return source => source == null ?
					null :
					new ProductDto {
						Code = source.Code,
						Categories = source.Categories != null ?
							source.Categories
								.Select(c => GetCategoryId(context.Projector.Project<CategoryDto>(c, context.MappingOptions)))
								.Where(i => i != null)
								.Cast<int>()
								.ToList() :
							new List<int>()
					};
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<LimitedProduct, LimitedProductDto>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<LimitedProduct, LimitedProductDto>
#else
				IProjectionMap<LimitedProduct, LimitedProductDto>
#endif
				.Project(ProjectionContext context) {

				return source => source == null ?
					null :
					new LimitedProductDto {
						Code = source.Code,
						Categories = source.Categories != null ?
							source.Categories
								.Select(c => context.Projector.Project<Category, int?>(c, context.MappingOptions))
								.Where(i => i != null)
								.Cast<int>()
								.ToList() :
							new List<int>(),
						Copies = source.Copies
					};
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Category, int?>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Category, int?>
#else
				IProjectionMap<Category, int?>
#endif
			.Project(ProjectionContext context) {

				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				return source => source != null ? (int?)source.Id : null;
			}

			// Nested map
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<Category, CategoryDto>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<Category, CategoryDto>
#else
				IProjectionMap<Category, CategoryDto>
#endif
				.Project(ProjectionContext context) {

				MappingOptionsUtils.options = context.MappingOptions.GetOptions<TestOptions>();
				return source => source == null ?
					null :
					new CategoryDto {
						Id = source.Id,
						Parent = context.Projector.Project<int?>(source.Parent, MappingOptionsUtils.options)
					};
			}

			// Throws exception
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<float, int>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<float, int>
#else
				IProjectionMap<float, int>
#endif
				.Project(ProjectionContext context) {

				throw new NotImplementedException();
			}
			
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<string, KeyValuePair<string, int>>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<string, KeyValuePair<string, int>>
#else
				IProjectionMap<string, KeyValuePair<string, int>>
#endif
				.Project(ProjectionContext context) {

				return source => new KeyValuePair<string, int>(source, source.Length);
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<int, char>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<int, char>
#else
				IProjectionMap<int, char>
#endif
				.Project(ProjectionContext context) {

				return source => (char)source;
			}

#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<char, float>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<char, float>
#else
				IProjectionMap<char, float>
#endif
				.Project(ProjectionContext context) {

				return source => (float)source;
			}

			// Rejects itself
#if NET7_0_OR_GREATER
			static
#endif
			Expression<Func<float, double>>
#if NET7_0_OR_GREATER
				IProjectionMapStatic<float, double>
#else
				IProjectionMap<float, double>
#endif
				.Project(ProjectionContext context) {

				throw new MapNotFoundException((typeof(float), typeof(double)));
			}
		}

		IProjector _projector = null;

		[TestInitialize]
		public void Initialize() {
			_projector = new CustomProjector(new CustomMapsOptions {
				TypesToScan = new[] { typeof(Maps) }
			});
		}


		[TestMethod]
		public void ShouldProjectPrimitives() {
			Assert.IsTrue(_projector.CanProject<int, string>());

			Expression<Func<int, string>> expr = source => (source * 2).ToString();
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<int, string>());
		}

		[TestMethod]
		public void ShouldProjectClasses() {
			Assert.IsTrue(_projector.CanProject<Price, decimal>());

			Expression<Func<Price, decimal>> expr = source => source != null ? source.Amount : 0m;
			TestUtils.AssertExpressionsEqual(expr, _projector.Project<Price, decimal>());
		}

		[TestMethod]
		public void ShouldNotProjectWithoutMap() {
			Assert.IsFalse(_projector.CanProject<bool, int>());

			TestUtils.AssertMapNotFound(() => _projector.Project<bool, int>());
		}

		[TestMethod]
		public void ShouldProjectNested() {
			{
				Assert.IsTrue(_projector.CanProject<Product, ProductDto>());

				// Should forward options to nested map
				MappingOptionsUtils.options = null;

				var value = new Maps();
				Expression<Func<Product, ProductDto>> expr = source => source == null ?
					null :
					new ProductDto {
						Code = source.Code,
						Categories = source.Categories != null ?
						source.Categories
								.Select(c => Maps.GetCategoryId(c == null ?
									null :
									new CategoryDto {
										Id = c.Id,
										Parent = c.Parent != null ? (int?)c.Parent.Id : null
									}))
								.Where(i => i != null)
								.Cast<int>()
								.ToList() :
							new List<int>()
					};
				var options = new TestOptions();
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<Product, ProductDto>(options));

				Assert.AreSame(MappingOptionsUtils.options, options);
			}

			{
				Assert.IsTrue(_projector.CanProject<Category, CategoryDto>());

				Expression<Func<Category, CategoryDto>> expr = source => source == null ?
					null :
					new CategoryDto {
						Id = source.Id,
						Parent = source.Parent != null ? (int?)source.Parent.Id : null
					};
				TestUtils.AssertExpressionsEqual(expr, _projector.Project<Category, CategoryDto>());
			}
		}

		[TestMethod]
		public void ShouldCatchExceptionsInProjectionss() {
			var exc = Assert.ThrowsException<ProjectionException>(() => _projector.Project<float, int>());
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
		}

		[TestMethod]
		public void ShouldProjectWithAdditionalMaps() {
			var options = new CustomProjectionAdditionalMapsOptions();
			options.AddMap<string, int>(c => s => s != null ? s.Length : 0);
			var projector = new CustomProjector(null, options);

			Assert.IsTrue(projector.CanProject<string, int>());

			Expression<Func<string, int>> expr = s => s != null ? s.Length : 0;
			TestUtils.AssertExpressionsEqual(expr, projector.Project<string, int>());
		}

		[TestMethod]
		public void ShouldCheckCanProjectWithAdditionalMaps() {
			var options = new CustomProjectionAdditionalMapsOptions();
			options.AddMap<string, int>(c => s => s != null ? s.Length : 0, c => c.MappingOptions.GetOptions<ProjectionCompilationContext>() == null);
			var projector = new CustomProjector(null, options);

			Assert.IsTrue(projector.CanProject<string, int>());
			Assert.IsFalse(projector.CanProject<string, int>(ProjectionCompilationContext.Instance));

			Expression<Func<string, int>> expr = s => s != null ? s.Length : 0;
			TestUtils.AssertExpressionsEqual(expr, projector.Project<string, int>());
			TestUtils.AssertMapNotFound(() => projector.Project<string, int>(ProjectionCompilationContext.Instance));
		}
	}
}
