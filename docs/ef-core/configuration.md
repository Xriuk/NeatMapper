---
layout: default
title: "Configuration"
nav_order: 2
parent: "Entity Framework Core"
---

# EntityFrameworkCoreOptions

{: .important }
Can be overridden during mapping with `EntityFrameworkCoreMappingOptions`.

## Entities retrieval mode

Entities can be retrieved (or created) in different ways.

- `Local`: the entities will be queried only locally in the entities already tracked by the context, no calls to the db will be made, if the entities are not found `null` will be returned.
- `LocalOrAttach`: the entities will be queried locally like above, and if not found new entities will be created (or the provided entities will be used) and attached to the context with just their keys set and in `Unchanged` state. No calls to the db will be made and no null entities will be returned. This is useful when you need to update entities without querying them, this way you can set the properties you need and only they will be sent to the db to be updated.
- `LocalOrRemote` (the default): the entities will be queried locally like above, and if not found they will be queried on the db, a single db call will be made, if the entities are not found `null` will be returned.
- `Remote`: the entities will be queried only remotely, the context will then handle merging them with already tracked entities, a single db call will be made, if the entities are not found `null` will be returned.

```csharp
services.Configure<EntityFrameworkCoreOptions>(o => o.EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrAttach);

...

mapper.Map<MyEntity>(2, new object[]{
    new EntityFrameworkCoreMappingOptions { EntitiesRetrievalMode = EntitiesRetrievalMode.LocalOrAttach }
});
```

## Duplicates merging

When merging entities (or collections of them), if a destination is provided and it does not match the entity retrieved from the context (maybe because the provided destination comes from another context or it was detached previously), the entity from the context will be returned by default. If that's not the behaviour you need you might want to throw an exception instead and handle it from there, by setting the options below a `DuplicateEntityException` will be thrown when encountering a duplicate entity.

```csharp
services.Configure<EntityFrameworkCoreOptions>(o => o.ThrowOnDuplicateEntity = true);

...

mapper.Map<IEnumerable<int>, MyEntity[]>(new []{ 2, 3 }, myEntities, new object[]{
    new EntityFrameworkCoreMappingOptions { ThrowOnDuplicateEntity = true }
});
```