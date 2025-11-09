---
layout: default
title: "One map for multiple mappings"
nav_order: 2
parent: "Advanced options"
---

# MergeMap to NewMap

If you only define an `IMergeMap<TSource, TDestination>` for two given types it can also be used when creating a new object instead of defining a separate `INewMap<TSource, TDestination>` for the same types.

{: .important }
In this case a destination object will be created automatically and provided to the map, so for this to work a parameterless constructor is required for the type.

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

// Or create a new object (a destination object will be created automatically
// and provided to the map)
var myProductDto = mapper.Map<Category, CategoryDto>(myCategory);
```

You can still create both maps for the given types if you need specific behaviour in one case or the other.

# ProjectionMap to NewMap

If you only define an `IProjectionMap<TSource, TDestination>` for two given types it can also be used to map them: the generated expression can be compiled into a delegate, which can be used to create a new object from an existing one, instead of defining a separate `INewMap<TSource, TDestination>` for the same types.

{: .important }
C# has some [limitations regarding expression trees](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/expression-tree-restrictions), so you may not be able to use all of the features.

```csharp
Expression<Func<Category, CategoryDto>> IProjectionMap<Category, CategoryDto>.Project(ProjectionContext context){
	return source => new CategoryDto { ... };
}


// Create a projection expression
var expr = projector.Project<Category, CategoryDto>();

// Or create a new object
var myCategoryDto = mapper.Map<Category, CategoryDto>(myCategory);
```

{: .highlight }
All projection maps can be compiled by default, if your maps are not suitable for compilation (for example they use methods which can only be converted to other languages by LINQ providers) you can check if your expression is about to be compiled by checking `ProjectionCompilationContext` inside `MappingOptions` of the `ProjectionContext`.

```csharp
bool ICanProject<Category, CategoryDto>.CanProject(ProjectionContext context){
	// Refuse compilation
	return !context.MappingOptions.HasOptions<ProjectionCompilationContext>();
}

Expression<Func<Category, CategoryDto>> IProjectionMap<Category, CategoryDto>.Project(ProjectionContext context){
	return source => new CategoryDto { ... };
}
```