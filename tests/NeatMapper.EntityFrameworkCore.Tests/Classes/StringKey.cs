using System.Collections.Generic;

namespace NeatMapper.EntityFrameworkCore.Tests {
	public class StringKey {
		public string Id { get; set; }

		public ICollection<OwnedEntity1> Entities { get; set; }

		public ICollection<OwnedEntity2> NewEntities { get; set; }
	}

	public class StringFieldKey {
		public string Id;
	}
}
