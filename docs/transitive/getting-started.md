---
layout: default
title: "Getting started"
nav_order: 1
parent: "Transitive"
---

# Installation

You can install the package directly from Nuget [NeatMapper.Transitive](https://www.nuget.org/packages/NeatMapper.Transitive).

# Creating maps

Create maps for your entities like shown in the [NeatMapper core package](/getting-started).

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

# Configuring the services

While configuring your services simply add:

```csharp
services.AddNeatMapper();

// This configures everything needed
services.AddNeatMapperTransitive();
```

# Mapping objects

```csharp
var mapper = serviceProvider.GetRequiredService<IMapper>();


Product product = ...;

// Transitively maps Product to ProductDto1 and then to ProductDto2
ProductDto2 productDto2 = mapper.Map<ProductDto2>(product);
```
