namespace NeatMapper.Tests.Classes {
	public class Category {
		public int Id { get; set; }

		public Category Parent { get; set; } = null!;

		public IEnumerable<Product> Products { get; set; } = null!;
	}

	public class CategoryDto {
		public int Id { get; set; }

		public int Parent { get; set; }
	}

	public class CategoryProducts {
		public int Id { get; set; }

		public IEnumerable<string> Products { get; set; } = null!;
	}
}
