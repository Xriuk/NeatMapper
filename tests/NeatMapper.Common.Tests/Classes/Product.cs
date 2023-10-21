using System.Collections.Generic;

namespace NeatMapper.Tests {
	public class Product {
		public string Code { get; set; }

		public ICollection<Category> Categories { get; set; }
	}

	public class ProductDto {
		public string Code { get; set; }

		public ICollection<int> Categories { get; set; }
	}
}
