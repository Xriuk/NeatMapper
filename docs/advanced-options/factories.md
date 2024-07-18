---
layout: default
title: "Factories"
nav_order: 8
parent: "Advanced options"
---

(Async)Mappers and matchers also allow creating factories for mapping two given types, instead of mapping them directly.

This allows for a greater performance for multiple subsequent maps for the same types, like in collections.

To create a factory you can use the following extension methods:

- `IMapper`
  - `mapper.MapNewFactory<Source, Destination>()`
  - `mapper.MapMergeFactory<Source, Destination>()`
- `IAsyncMapper`
  - `asyncMapper.MapAsyncNewFactory<Source, Destination>()`
  - `asyncMapper.MapAsyncMergeFactory<Source, Destination>()`
- `IMatcher`
  - `matcher.MatchFactory<Source, Destination>()`

Each method accepts the types to map as parameters and returns a factory.

Optionally you can specify other `MappingOptions`, and they are applied to the factory **before** creation, so that all the invocations share the same `(Async)Mapping/Matching Context`.

{: .highlight }
This is important for async maps, since the returned factory has a single `CancellationToken` tied to it, so it should be used for a short period of time (eg: a single web request or scope).

To use the factory you just have to invoke the method `Invoke()` and pass the related object(s) to it.

{: .highlight }
Factories will need to be disposed at the end by invoking the `Dispose()` method (or wrapping them in an `using` block since they implement `IDisposable`).

```csharp
// Retrieve a factory and wrap it in an using block to dispose it correctly after use
using(var factory = mapper.MapNewFactory<Product, ProductDto>()){
    // Perform single maps
    var myProductDto = factory.Invoke(myProduct); // Or just factory(myProduct)

    // Map collections
    var myProductsDto = myProducts.Select(factory).ToArray(); // Implicit cast to Func<Product, ProductDto>

    ...
}
```

Factories are thread-safe as all the [other classes](/advanced-options/thread-safety), so they can be used even in parallel if needed.