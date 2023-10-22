namespace NeatMapper.Tests {
	public class GenericClass<T> {
		public int Id { get; set; }

		public T Value { get; set; }
	}

	public class GenericClassDto<T> {
		public int Id { get; set; }

		public T Value { get; set; }
	}
}
