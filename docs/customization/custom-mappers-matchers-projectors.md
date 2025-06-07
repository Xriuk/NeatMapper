---
layout: default
title: "Custom mappers, matchers and projectors"
nav_order: 1
parent: "Customization"
---

Custom mappers, async mappers, matchers and projectors can be created by implementing the appropriate interface:
- `IMapper`
- `IAsyncMapper`
- `IMatcher`
- `IProjector`

```csharp
public class MyCustomMapper : IMapper{
    ...
}
```

{: .highlight }
The methods of the interfaces should be [thread-safe](/advanced-options/thread-safety).

The custom classes then should be registered in the DI container to allow injecting them into the default instance of the service.

```csharp
services.AddNeatMapper();

// Insert the custom mapper in the first position so that built-in mappers are invoked after it
services.Configure<CompositeMapperOptions>(o => o.Mappers.Insert(0, new MyCustomMapper()));
```

The options which can be configured this way are the following:
- `CompositeMapperOptions`
- `CompositeMatcherOptions`
- `AsyncCompositeMapperOptions`
- `CompositeProjectorOptions`

See also [Advanced options > Configuration](/advanced-options/configuration#compositemappertypeoptions).