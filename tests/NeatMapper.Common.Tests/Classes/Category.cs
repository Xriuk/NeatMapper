using System.Collections.Generic;

namespace NeatMapper.Tests.Classes {
	public class Category {
		public int Id { get; set; }

		public Category Parent { get; set; }

		public ICollection<Product> Products { get; set; }
	}

	public class CategoryDto {
		public int Id { get; set; }

		public int? Parent { get; set; }
	}

	public class CategoryProducts {
		public int Id { get; set; }

		public ICollection<string> Products { get; set; }
	}
}
