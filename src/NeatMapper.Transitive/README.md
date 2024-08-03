# .NEaT Mapper - Transitive

[![NuGet](https://img.shields.io/nuget/v/NeatMapper.Transitive.svg?label=NuGet)](https://www.nuget.org/packages/NeatMapper.Transitive)

## What is this package

Transitive maps for [NeatMapper](https://www.nuget.org/packages/NeatMapper).

Allows mapping types by automatically chaining maps together, eg: If you have maps for types A -> B and B -> C you can also map A -> C by chaining A -> B -> C, supports normal maps and asynchronous ones, also supports collections.

## How to install

You can find all the other packages on Nuget https://www.nuget.org/profiles/xriuk

You can install this package directly from Nuget https://www.nuget.org/packages/NeatMapper.Transitive

## How to use

While configuring your services simply add

```csharp
services.AddNeatMapper();

services.AddNeatMapperTransitive();
```

// DEV: add docs

## Advanced options

Find more advanced use cases in the [website](https://www.neatmapper.org/transitive/configuration) or in the extended [tests project](https://github.com/Xriuk/NeatMapper/tree/main/tests/NeatMapper.Transitive.Tests).

## License

[Read the license here](https://www.neatmapper.org/license)
