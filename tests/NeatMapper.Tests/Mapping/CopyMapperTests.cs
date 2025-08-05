using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace NeatMapper.Tests.Mapping {
	[TestClass]
	public class CopyMapperTests {
		private abstract class AbstractClass { }

		private class ConcreteClass : AbstractClass { }

		public class FieldsClass {
			public int id;
		}

		public class PrivateFields {
			private int id;


			public int GetId() {
				return id;
			}

			public void SetId(int value) {
				id = value;
			}
		}

		public class PrivateProperties{
			public string Test { get; private set; }


			public void SetTest(string value) {
				Test = value;
			}
		}


		IMapper _mapper = null;

		[TestInitialize]
		public void Initialize() {
			_mapper = new CompositeMapper(new CollectionMapper(EmptyMapper.Instance), new CopyMapper());
		}


		[TestMethod]
		public void ShouldMapSimpleObjects() {
			Assert.IsTrue(_mapper.CanMapNew<Price, Price>());
			Assert.IsTrue(_mapper.CanMapMerge<Price, Price>());

			// New
			var source = new Price {
				Amount = 12.34m,
				Currency = "EUR"
			};
			var result = _mapper.Map<Price>(source);
			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.Amount, result.Amount);
			Assert.AreEqual(source.Currency, result.Currency);

			// Merge
			var destination = new Price();
			result = _mapper.Map(source, destination);
			Assert.AreNotSame(source, result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(source.Amount, result.Amount);
			Assert.AreEqual(source.Currency, result.Currency);
		}

		[TestMethod]
		public void ShouldMapComplexObjects() {
			var category = new Category {
				Id = 1
			};
			var source = new Product {
				Code = "TEST",
				Categories = new List<Category> { category }
			};
			category.Products = new List<Product> { source };

			Assert.AreSame(source, source.Categories.First().Products.First());

			var result = _mapper.Map<Product>(source);
			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.Code, result.Code);
			Assert.AreSame(source.Categories, result.Categories);
			Assert.AreNotSame(result, result.Categories.First().Products.First());
		}

		[TestMethod]
		public void ShouldMapDeep() {
			var category = new Category {
				Id = 1
			};
			var source = new Product {
				Code = "TEST",
				Categories = new List<Category> { category }
			};
			category.Products = new List<Product> { source };

			Assert.AreSame(source, source.Categories.First().Products.First());

			var result = _mapper.Map<Product>(source, new MappingOptions(new CopyMapperMappingOptions(deepCopy: DeepCopyFlags.DeepMap)));
			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.Code, result.Code);
			Assert.AreNotSame(source.Categories, result.Categories);
			Assert.AreEqual(source.Categories.Count, result.Categories.Count);
			Assert.AreNotSame(source.Categories.First(), result.Categories.First());
			Assert.AreNotSame(category, result.Categories.First());
			Assert.AreEqual(source.Categories.First().Id, result.Categories.First().Id);
			Assert.AreSame(source.Categories.First().Parent, result.Categories.First().Parent);
			Assert.AreSame(result, result.Categories.First().Products.First());
		}

		[TestMethod]
		public void ShouldMergeDeep() {
			var source = new Category {
				Id = 1,
				Parent = new Category {
					Id = 2,
					Products = new List<Product> {
						new Product {
							Code = "TEST"
						}
					}
				}
			};

			var category = new Category {
				Id = 3
			};
			var destination = new Category {
				Id = 4,
				Parent = category
			};
			var result = _mapper.Map(source, destination, new MappingOptions(new CopyMapperMappingOptions(deepCopy: DeepCopyFlags.DeepMap)));
			Assert.AreNotSame(source, result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(source.Id, result.Id);
			Assert.AreSame(category, result.Parent);
			Assert.AreEqual(source.Parent.Id, result.Parent.Id);
			Assert.AreNotSame(source.Parent.Products, result.Parent.Products);
			Assert.AreEqual(source.Parent.Products.Count, result.Parent.Products.Count);
		}

		[TestMethod]
		public void ShouldOverrideDeep() {
			var source = new Category {
				Id = 1,
				Parent = new Category {
					Id = 2,
					Products = new List<Product> {
						new Product {
							Code = "TEST"
						}
					}
				}
			};

			var category = new Category {
				Id = 3
			};
			var destination = new Category {
				Id = 4,
				Parent = category
			};
			var result = _mapper.Map(source, destination, new MappingOptions(new CopyMapperMappingOptions(deepCopy: DeepCopyFlags.DeepMap | DeepCopyFlags.OverrideInstance)));
			Assert.AreNotSame(source, result);
			Assert.AreSame(destination, result);
			Assert.AreEqual(source.Id, result.Id);
			Assert.AreNotSame(category, result.Parent);
			Assert.AreEqual(source.Parent.Id, result.Parent.Id);
			Assert.AreNotSame(source.Parent.Products, result.Parent.Products);
			Assert.AreEqual(source.Parent.Products.Count, result.Parent.Products.Count);
		}

		[TestMethod]
		public void ShouldMapBaseToDerived() {
			var source = new Product {
				Code = "TEST",
				Categories = new List<Category> {
					new Category {
						Id = 1
					}
				}
			};

			var result = _mapper.Map<LimitedProduct>(source);

			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.Code, result.Code);
			Assert.AreSame(source.Categories, result.Categories);
			Assert.AreEqual(0, result.Copies); // default
		}

		[TestMethod]
		public void ShouldMapDerivedToBase() {
			var source = new LimitedProduct {
				Code = "TEST",
				Categories = new List<Category> {
					new Category {
						Id = 1
					}
				},
				Copies = 4
			};

			var result = _mapper.Map<Product>(source);

			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.Code, result.Code);
			Assert.AreSame(source.Categories, result.Categories);
		}

		[TestMethod]
		public void ShouldNotMapAbstractClasses() {
			Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<AbstractClass>(new ConcreteClass()));
		}

		[TestMethod]
		public void ShouldNotMapClassesWithoutParameterlessConstructor() {
			Assert.ThrowsException<MapNotFoundException>(() => _mapper.Map<ClassWithoutParameterlessConstructor>(new ClassWithoutParameterlessConstructor("test")));
		}

		[TestMethod]
		public void ShouldMapFields() {
			var source = new FieldsClass {
				id = 2
			};

			var result = _mapper.Map<FieldsClass>(source);

			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.id, result.id);
		}

		[TestMethod]
		public void ShouldNotMapPrivateFieldsByDefault() {
			var source = new PrivateFields();
			source.SetId(2);

			var result = _mapper.Map<PrivateFields>(source);

			Assert.AreNotSame(source, result);
			Assert.AreNotEqual(source.GetId(), result.GetId());
		}

		[TestMethod]
		public void ShouldMapPrivateFields() {
			var source = new PrivateFields();
			source.SetId(2);

			var result = _mapper.Map<PrivateFields>(source, new MappingOptions(new CopyMapperMappingOptions(fieldsToMap: MemberVisibilityFlags.Public | MemberVisibilityFlags.NonPublic)));

			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.GetId(), result.GetId());
		}

		[TestMethod]
		public void ShouldNotMapPrivatePropertiesByDefault() {
			var source = new PrivateProperties();
			source.SetTest("test");

			var result = _mapper.Map<PrivateProperties>(source);

			Assert.AreNotSame(source, result);
			Assert.AreNotEqual(source.Test, result.Test);
		}

		[TestMethod]
		public void ShouldMapPrivateProperties() {
			var source = new PrivateProperties();
			source.SetTest("test");

			var result = _mapper.Map<PrivateProperties>(source, new MappingOptions(new CopyMapperMappingOptions(propertiesToMap: MemberVisibilityFlags.Public | MemberVisibilityFlags.NonPublic)));

			Assert.AreNotSame(source, result);
			Assert.AreEqual(source.Test, result.Test);
		}
	}
}
