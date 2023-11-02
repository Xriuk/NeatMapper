using System.Collections.Generic;

namespace NeatMapper.EntityFrameworkCore.Tests {
	public class StringKey {
		public string Id { get; set; }

		public ICollection<OwnedEntity> Entities { get; set; }
	}
}
