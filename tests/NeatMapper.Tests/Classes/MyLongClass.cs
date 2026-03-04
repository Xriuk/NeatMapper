using System;

namespace NeatMapper.Tests {
	public class MyLongClass {
		public int Id { get; set; }

		public string Name { get; set; }

		public DateTime Creation { get; set; }

		public bool Active { get; set; }
	}

	public interface IMyInterface {
		bool EntityActive { get; set; }
	}

	public class MyInterfaceClassDto : IMyInterface {
		public string EntityName { get; set; }

		public bool EntityActive { get; set; }
	}

	public class MyLongClassDto : IMyInterface {
		public MyLongClassDto() { }
		public MyLongClassDto(int id) {
			EntityId = id;
		}


		public int EntityId { get; set; }

		public string EntityName { get; set; }

		public DateTime EntityCreation { get; set; }

		public bool EntityActive { get; set; }
	}
}
