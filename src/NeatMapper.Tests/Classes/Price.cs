namespace NeatMapper.Tests.Classes {
	public class Price {
		public decimal Amount { get; set; }

		public string Currency { get; set; } = null!;
	}

	public class PriceFloat {
		public float Amount { get; set; }

		public string Currency { get; set; } = null!;
	}
}
