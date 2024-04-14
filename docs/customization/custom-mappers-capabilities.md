---
layout: default
title: "Custom mappers capabilities"
nav_order: 2
parent: "Customization"
---

To allow discovering if your custom mapper/matcher/projector supports two given types you can implement the optional interfaces below:
- `IMapperCanMap` for `IMapper`
- `IAsyncMapperCanMap` for `IAsyncMapper`
- `IMatcherCanMatch` for `IMatcher`
- `IProjectorCanProject` for `IProjector`

{: .highlight }
The methods of the interfaces should be [thread-safe](/advanced-options/thread-safety).

See also [Advanced options > Mappers capabilities](/advanced-options/mappers-capabilities).