﻿using System;
using System.Linq.Expressions;

namespace NeatMapper {
	/// <summary>
	/// Delegate which allows projecting an object to another one, used to add custom
	/// <see cref="IProjectionMap{TSource, TDestination}"/>.
	/// </summary>
	/// <typeparam name="TSource">Source type.</typeparam>
	/// <typeparam name="TDestination">Destination type.</typeparam>
	/// <param name="context">Projection context, which allows nested projections, services retrieval via DI, ....</param>
	/// <returns>The newly created object, may be null.</returns>
	public delegate Expression<Func<TSource?, TDestination?>> ProjectionMapDelegate<TSource, TDestination>(ProjectionContext context);
}
