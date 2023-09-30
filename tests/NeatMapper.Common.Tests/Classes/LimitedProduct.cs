namespace NeatMapper.Tests.Classes {
	public class LimitedProduct : Product {
		public int Copies { get; set; }
	}

	public class LimitedProductDto : ProductDto {
		public int Copies { get; set; }
	}
}
