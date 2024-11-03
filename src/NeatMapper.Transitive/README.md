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

Then define your maps

```csharp
public class MyMaps :
    INewMap<Product, ProductDto1>,
    INewMap<ProductDto1, ProductDto2>
{
    ProductDto1? INewMap<Product, ProductDto1>.Map(Product? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new ProductDto1{
                Code = source.Code,
                ...
            };
        }
    }

    ProductDto2? INewMap<ProductDto1, ProductDto2>.Map(ProductDto1? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new ProductDto2{
                Code = source.Code,
                ...
            };
        }
    }
}
```

And you are ready to map your entities

```csharp
var mapper = serviceProvider.GetRequiredService<IMapper>();


Product product = ...;

// Transitively maps Product to ProductDto1 and then to ProductDto2
ProductDto2 productDto2 = mapper.Map<ProductDto2>(product);
```

## Advanced options

Find more advanced use cases in the [website](https://www.neatmapper.org/transitive/configuration) or in the extended [tests project](https://github.com/Xriuk/NeatMapper/tree/main/tests/NeatMapper.Transitive.Tests).

## License

[Read the license here](https://www.neatmapper.org/license)
