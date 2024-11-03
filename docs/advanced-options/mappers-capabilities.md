---
layout: default
title: "Mappers capabilities"
nav_order: 7
parent: "Advanced options"
---

# Test types

You can check if a mapper/matcher/projector supports a map for two given types by using the methods.

`IMapper`:

- `mapper.CanMapNew<Source, Destination>()`
- `mapper.CanMapMerge<Source, Destination>()`

`IAsyncMapper`:
- `asyncMapper.CanMapAsyncNew<Source, Destination>()`
- `asyncMapper.CanMapAsyncMerge<Source, Destination>()`

`IMatcher`:
- `matcher.CanMatch<Source, Destination>()`

`IProjector`:
- `projector.CanProject<Source, Destination>()`

This will check and will return true if the mapper/matcher can map the two given types.

# List types

You can also retrieve a list of all available maps for a given mapper/projector (if supported by it), by using the extension methods.

`IMapper`:

- `mapper.GetNewMaps()`
- `mapper.GetMergeMaps()`

`IAsyncMapper`:
- `asyncMapper.GetAsyncNewMaps()`
- `asyncMapper.GetAsyncMergeMaps()`

`IProjector`:
- `projector.GetMaps()`

This should return a list of types (source and destination) which can be mapped by the mapper.
It will be empty for dynamic mappers which map based on rules.
For performance reasons duplicate types may be returned.