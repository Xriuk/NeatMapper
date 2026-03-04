namespace NeatMapper.Tests {
	public class MyDerivedLongClass : MyLongClass {
	}

	public class MyDerivedLongClassDto : MyLongClassDto {
		public MyDerivedLongClassDto() { }
		public MyDerivedLongClassDto(int id) {
			EntityId = id;
		}
	}
}
