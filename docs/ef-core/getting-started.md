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


// Map an entity to its key(s)
(int MyIntKey, string MyStringKey) = mapper.Map<(int, string)>(myEntity);


// Project an entity into its key
var myEntitiesKeys = db.Set<MyEntity>()
    .Project<int>(projector)
    .ToArray();
```
