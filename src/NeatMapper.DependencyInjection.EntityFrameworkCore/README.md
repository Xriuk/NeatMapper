# .NEaT Mapper - Dependency Injection 

[![NuGet](https://img.shields.io/nuget/vpre/NeatMapper.DependencyInjection.svg?label=NuGet)](https://www.nuget.org/packages/NeatMapper.DependencyInjection)

## What is this package

Dependency Injection extensions for [NeatMapper](https://www.nuget.org/packages/NeatMapper)

## How to install

You can find all the other packages on Nuget https://www.nuget.org/profiles/xriuk

You can install this package directly from Nuget https://www.nuget.org/packages/NeatMapper.DependencyInjection

## How to use

While configuring your services simply add

```csharp
servicesCollection.Configure<MapperConfigurationOptions>(o => o.ScanTypes = Assembly.GetExecutingAssembly().GetTypes().ToList());
//servicesCollection.Configure<MapperOptions>(o => ...);
servicesCollection.AddNeatMapper();

...

var mapper = serviceProvider.GetRequiredService<IMapper>();
mapper.Map<Foo, Bar>(...);
```

For information on how to use the Mapper, check [the main package README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md).

## License

[Read the license here](https://github.com/Xriuk/NeatMapper/blob/main/LICENSE.md)