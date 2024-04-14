---
layout: default
title: "Mappers capabilities"
nav_order: 7
parent: "Advanced options"
---

You can check if a mapper/matcher/projector supports a map for two given types by using the extension methods.

`IMapper`:

- `.CanMapNew<Source, Destination>()`
- `.CanMapMerge<Source, Destination>()`

`IAsyncMapper`:
- `.CanMapAsyncNew<Source, Destination>()`
- `.CanMapAsyncMerge<Source, Destination>()`

`IMatcher`:
- `.CanMatch<Source, Destination>()`

`IProjector`:
- `.CanProject<Source, Destination>()`

This will check and will return true if the mapper/matcher **could** potentially map the two given types (actual mapping can still fail for various reasons).

{: .highlight }
It may throw an InvalidOperationException in the case where it cannot verify if two types can be mapped (currently in case of some interfaces because some details depends on their runtime implementation), so you should also keep that in mind.