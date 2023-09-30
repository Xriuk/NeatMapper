namespace NeatMapper.Tests.Classes {
	public class GenericClass<T> {
		public int Id { get; set; }

		public T Value { get; set; } = default!;
	}

	public class GenericClassDto<T> {
		public int Id { get; set; }

		public T Value { get; set; } = default!;
	}
}
