---
layout: default
title: "Custom factories"
nav_order: 2
parent: "Customization"
---

To improve performances for subsequent mappings/matches (like collections) you can implement the optional interfaces below:
- `IMapperFactory` for `IMapper`
- `IAsyncMapperFactory` for `IAsyncMapper`
- `IMatcherFactory` for `IMatcher`

{: .highlight }
The methods of the interfaces should be [thread-safe](/advanced-options/thread-safety).

See also [Advanced options > Factories](/advanced-options/factories).