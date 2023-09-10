namespace NeatMapper.Core {
	public interface IMapper {
		public object? Map(object? source, Type sourceType, Type destinationType);

		public object? Map(object? source, Type sourceType, object? destination, Type destinationType);
	}
}
