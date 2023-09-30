namespace NeatMapper.Tests.Classes {
	public class MyClassString {
		public string MyString { get; set; } = null!;
	}

	public class MyClassStringWithKey : MyClassString {
		public int Id { get; set; }
	}
}
