# .NEaT Mapper

## What is this package

Dependency Injection extensions for [NeatMapper](https://www.nuget.org/packages/NeatMapper)

## How to install

You can find all the other packages on Nuget https://www.nuget.org/profiles/xriuk

You can install this package directly from Nuget https://www.nuget.org/packages/NeatMapper.DependencyInjection

## How to use

While configuring your services simply add

```csharp
services.Configure<MapperConfigurationOptions>(o => o.ScanTypes = Assembly.GetExecutingAssembly().GetTypes().ToList());
services.AddNeatMapper();
```

## License

[Read the license here](https://github.com/Xriuk/NeatMapper/tree/main/src/NeatMapper.DependencyInjection/LICENSE.md)