#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
#nullable disable
#endif

using System;
using System.Linq.Expressions;

namespace NeatMapper.EntityFrameworkCore {
	internal sealed class EntityMappingInfo {
		public object LocalEntity { get; set; }

		public LambdaExpression Expression { get; set; }

		public Delegate Delegate { get; set; }
	}
}
