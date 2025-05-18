# .NEaT Mapper - Entity Framework Core

[![NuGet](https://img.shields.io/nuget/v/NeatMapper.EntityFrameworkCore.svg?label=NuGet)](https://www.nuget.org/packages/NeatMapper.EntityFrameworkCore)

## What is this package

Entity Framework Core maps for [NeatMapper](https://www.nuget.org/packages/NeatMapper).

Creates automatic maps and projections between entities and their keys (even composite and shadow keys), supports normal maps and asynchronous ones, also supports collections (not nested).

It can also map keys to predicates (`Expression<Func<Entity, bool>>`) for filtering.

## How to install

You can find all the other packages on Nuget https://www.nuget.org/profiles/xriuk

You can install this package directly from Nuget https://www.nuget.org/packages/NeatMapper.EntityFrameworkCore

## How to use

While configuring your services simply add

```csharp
services.AddDbContext<TestContext>();
services.AddNeatMapper();

services.AddNeatMapperEntityFrameworkCore<TestContext>();
```

And you are ready to map your entities

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


// Project an entity into its key (even shadow)
var myEntitiesKeys = db.Set<MyEntity>()
    .Project<int>(projector)
    .ToArray();


// Create a filtering expression from key(s)
var expr1 = mapper.Map<Expression<Func<MyEntity, bool>>>(2);
// entity => entity.Id == 2

var expr2 = mapper.Map<Expression<Func<MyEntity, bool>>>(new int[]{ 2, 3, ... });
// entity => new int[]{ 2, 3, ... }.Contains(entity.Id)

var expr3 = mapper.Map<Expression<Func<MyEntityWithCompositeKey, bool>>>(new []{ (2, "StringKey1"), (3, "StringKey2"), ... });
// entity => (entity.MyIntKey == 2 && entity.MyStringKey == "StringKey1") || (entity.MyIntKey == 3 && entity.MyStringKey == "StringKey2") || ...
```

## Advanced options

Find more advanced use cases in the [website](https://www.neatmapper.org/ef-core/configuration) or in the extended [tests project](https://github.com/Xriuk/NeatMapper/tree/main/tests/NeatMapper.EntityFrameworkCore.Tests).

## License

[Read the license here](https://www.neatmapper.org/license)
