namespace NeatMapper.Tests.Classes {
	public class Product {
		public string Code { get; set; } = null!;

		public IEnumerable<Category> Categories { get; set; } = null!;
	}

	public class ProductDto {
		public string Code { get; set; } = null!;

		public IEnumerable<int> Categories { get; set; } = null!;
	}
}
