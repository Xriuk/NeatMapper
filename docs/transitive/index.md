---
layout: default
title: "Transitive"
nav_order: 6
has_children: true
---

<img src="/assets/images/icon.png" alt="drawing" width="200"/>

# .NEaT Mapper - Transitive

[Download on NuGet](https://www.nuget.org/packages/NeatMapper.Transitive){: .btn .btn-primary .fs-5 .mb-4 .mb-md-0 .mr-2 }
[Get started](/transitive/getting-started){: .btn .fs-5 .mb-4 .mb-md-0 }

Transitive maps for NeatMapper.

Allows mapping types by automatically chaining maps together, eg: If you have maps for types A -> B and B -> C you can also map A -> C by chaining A -> B -> C, supports normal maps and asynchronous ones, also supports collections.

New maps are mapped in sequence, while for merge maps, new maps are applied until the last two, which are merged together:
A -> C
A -> B (new)
B -> C (merge)