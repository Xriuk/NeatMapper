# .NEaT Mapper - Entity Framework Core

[![NuGet](https://img.shields.io/nuget/vpre/NeatMapper.EntityFrameworkCore.svg?label=NuGet)](https://www.nuget.org/packages/NeatMapper.EntityFrameworkCore)

## What is this package

Entity Framework Core maps for [NeatMapper](https://www.nuget.org/packages/NeatMapper).

Creates automatic maps between entities and their keys, supports normal maps and asynchronous ones, also supports collections (not nested).

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
// Map a key to an entity
var entity = mapper.Map<MyEntity>(2);
var entity = await mapper.MapAsync<MyEntity>(2);

// Map a composite key to an entity with tuples (System.Tuple or System.ValueTuple)
var entity = await mapper.MapAsync<MyEntityWithCompositeKey>((2, "StringKey"));

// Map multiple keys to their respective entities
var entities = await mapper.MapAsync<MyEntity[]>(new int[]{ 2, 3, ... });


// Map entity to key(s)
(int MyIntKey, string MyStringKey) = mapper.Map<(int, string)>(myEntity);
```

## Advanced options

Find more advanced use cases in the [wiki](https://github.com/Xriuk/NeatMapper/wiki/Entity-Framework-Core) or in the extended [tests project](https://github.com/Xriuk/NeatMapper/tree/main/tests/NeatMapper.EntityFrameworkCore.Tests).

## License

[Read the license here](https://github.com/Xriuk/NeatMapper/blob/main/LICENSE.md)