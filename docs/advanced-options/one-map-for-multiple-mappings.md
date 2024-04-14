---
layout: default
title: "One map for multiple mappings"
nav_order: 2
parent: "Advanced options"
---

# MergeMap to NewMap

If you only define an `IMergeMap<TSource, TDestination>` for two given types it can also be used when creating a new object instead of defining a separate `INewMap<TSource, TDestination>` for the same types.

{: .important }
In this case a destination object will be created automatically, so for this to work a parameterless constructor is required.

```csharp
CategoryDto? IMergeMap<Category, CategoryDto>.Map(Category? source, CategoryDto? destination, MappingContext context){
    if(source != null){
        destination ??= new CategoryDto();
        destination.Id = source.Id;
        ...
    }
    return destination;
}


// Map to an existing object
mapper.Map<Category, CategoryDto>(myCategory, myCategoryDto);

// Or create a new object
var myProductDto = mapper.Map<Category, CategoryDto>(myCategory);
```

You can still create both maps for the given types if you need specific behaviour in one case or the other.

# ProjectionMap to NewMap

If you only define an `IProjectionMap<TSource, TDestination>` for two given types it can also be used to map them: the generated expression can be compiled into a delegate, which can be used to create a new object from an existing one, instead of defining a separate `INewMap<TSource, TDestination>` for the same types.

```csharp
Expression<Func<Category?, CategoryDto?>> IProjectionMap<Category, CategoryDto>.Project(ProjectionContext context){
    return source => source == null ? null : new CategoryDto { ... };
}


// Create a projection expression
var expr = projector.Project<Category, CategoryDto>();

// Or create a new object
var myCategoryDto = mapper.Map<Category, CategoryDto>(myCategory);
```