using System.Collections.Generic;

namespace NeatMapper.EntityFrameworkCore.Tests {
	public class IntKey {
		public int Id { get; set; }

		public OwnedEntity1 Entity { get; set; }

		public ICollection<OwnedEntity2> NewEntities { get; set; }
	}

	public class IntFieldKey {
		public int Id;
	}
}
