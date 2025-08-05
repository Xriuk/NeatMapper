---
layout: default
title: "Getting started"
nav_order: 1
parent: "Entity Framework Core"
---

# Installation

You can install the package directly from Nuget [NeatMapper.EntityFrameworkCore](https://www.nuget.org/packages/NeatMapper.EntityFrameworkCore).

# Configuring the services

While configuring your services simply add:

```csharp
services.AddDbContext<TestContext>();
services.AddNeatMapper();

// This configures everything needed
services.AddNeatMapperEntityFrameworkCore<TestContext>();
```

# Mapping objects

```csharp
var mapper = serviceProvider.GetRequiredService<IMapper>();
var asyncMapper = serviceProvider.GetRequiredService<IMapper>();
var projector = serviceProvider.GetRequiredService<IProjector>();


// Map a key to its entity
var entity = mapper.Map<MyEntity>(2);
var entity = await asyncMapper.MapAsync<MyEntity>(2);

// Map a composite key to an entity with tuples (System.Tuple or System.ValueTuple),
// notice the double parentheses
var entity = await asyncMapper.MapAsync<MyEntityWithCompositeKey>((2, "StringKey"));

// Map multiple keys to their respective entities
var entities = await asyncMapper.MapAsync<MyEntity[]>(new int[]{ 2, 3, ... });


// Map an entity to its key(s) (the order is important!)
(int MyIntKey, string MyStringKey) = mapper.Map<(int, string)>(myEntity);


// Project an entity into its key
var myEntitiesKeys = db.Set<MyEntity>()
	.Project<int>(projector)
	.ToArray();


// Create a filtering expression from key(s)
var expr1 = mapper.Map<Expression<Func<MyEntity, bool>>>(2);
// entity => entity.Id == 2

var expr2 = mapper.Map<Expression<Func<MyEntity, bool>>>(new int[]{ 2, 3, ... });
// entity => new int[]{ 2, 3, ... }.Contains(entity.Id)

var expr3 = mapper.Map<Expression<Func<MyEntityWithCompositeKey, bool>>>(new []{ (2, "StringKey1"), (3, "StringKey2"), ... });
// entity => (entity.MyIntKey == 2 && entity.MyStringKey == "StringKey1") ||
//           (entity.MyIntKey == 3 && entity.MyStringKey == "StringKey2") || ...
```