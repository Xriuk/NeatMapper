using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using NeatMapper.Tests.Projection;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class ProjectionMapperTests {
		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			var projector = new CustomProjector(new CustomMapsOptions {
				TypesToScan = new List<Type> { typeof(ProjectionTests.Maps) }
			});
			_mapper = new ProjectionMapper(projector);
		}


		[TestMethod]
		public void ShouldMapPrimitives() {
			Assert.IsTrue(_mapper.CanMapNew<int, string>());

			Assert.AreEqual("4", _mapper.Map<string>(2));
			Assert.AreEqual("-6", _mapper.Map<string>(-3));
			Assert.AreEqual("0", _mapper.Map<string>(0));
		}

		[TestMethod]
		public void ShouldMapClasses() {
			Assert.IsTrue(_mapper.CanMapNew<Price, decimal>());

			Assert.AreEqual(20.00m, _mapper.Map<decimal>(new Price {
				Amount = 20.00m,
				Currency = "EUR"
			}));
		}

		[TestMethod]
		public void ShouldMapChildClassAsParent() {
			Assert.IsTrue(_mapper.CanMapNew<Product, ProductDto>());

			var result = _mapper.Map<Product, ProductDto>(new LimitedProduct {
				Code = "Test",
				Categories = new List<Category> {
					new Category {
						Id = 2
					}
				},
				Copies = 3
			});
			Assert.IsNotNull(result);
			Assert.IsTrue(result.GetType() == typeof(ProductDto));
		}

		[TestMethod]
		public void ShouldNotMapWithoutMap() {
			Assert.IsFalse(_mapper.CanMapNew<bool, int>());

			TestUtils.AssertMapNotFound(() => _mapper.Map<int>(false));
		}

		[TestMethod]
		public void ShouldMapNested() {
			{
				var result = _mapper.Map<Product, ProductDto>(new Product {
					Code = "Test",
					Categories = new List<Category> {
						new Category {
							Id = 2
						}
					}
				});
				Assert.IsNotNull(result);
				Assert.AreEqual("Test", result.Code);
				Assert.IsNotNull(result.Categories);
				Assert.AreEqual(1, result.Categories.Count());
				Assert.AreEqual(2, result.Categories.Single());
			}

			{
				var result = _mapper.Map<Category, CategoryDto>(new Category {
					Id = 2
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Id);
				Assert.IsNull(result.Parent);
			}

			{
				var result = _mapper.Map<Category, CategoryDto>(new Category {
					Id = 2,
					Parent = new Category { Id = 3 }
				});
				Assert.IsNotNull(result);
				Assert.AreEqual(2, result.Id);
				Assert.AreEqual(3, result.Parent);
			}
		}

		[TestMethod]
		public void ShouldCatchExceptionsInMaps() {
			var exc = Assert.ThrowsException<MappingException>(() => _mapper.Map<int>(2f));
			Assert.IsInstanceOfType(exc.InnerException, typeof(NotImplementedException));
		}
	}
}
