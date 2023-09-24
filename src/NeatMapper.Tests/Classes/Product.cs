namespace NeatMapper.Tests.Classes {
	public class Product {
		public string Code { get; set; } = null!;

		public ICollection<Category> Categories { get; set; } = null!;
	}

	public class ProductDto {
		public string Code { get; set; } = null!;

		public ICollection<int> Categories { get; set; } = null!;
	}
}
