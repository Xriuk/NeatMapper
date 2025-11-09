---
layout: default
title: "Conditional mapping"
nav_order: 1
parent: "Customization"
---

# Interfaces

Maps can map/match/project types based on certain conditions of the context, to achieve this you can implement the corresponding optional interfaces for your maps:
- `ICanMapNew<TSource, TDestination>` for `INewMap<TSource, TDestination>`
- `ICanMapMerge<TSource, TDestination>` for `IMergeMap<TSource, TDestination>`
- `ICanMatch<TSource, TDestination>` for `IMatchMap<TSource, TDestination>`
- `ICanMatchHierarchy<TSource, TDestination>` for `IHierarchyMatchMap<TSource, TDestination>`
- `ICanProject<TSource, TDestination>` for `IProjectionMap<TSource, TDestination>`
- `ICanMapAsyncNew<TSource, TDestination>` for `IAsyncNewMap<TSource, TDestination>`
- `ICanMapAsyncMerge<TSource, TDestination>` for `IAsyncMergeMap<TSource, TDestination>`

If you are on .NET 7 or greater you can use the `static` versions of the interfaces above instead:
- `ICanMapNewStatic<TSource, TDestination>`
- `ICanMapMergeStatic<TSource, TDestination>`
- `ICanMatchStatic<TSource, TDestination>`
- `ICanMatchHierarchyStatic<TSource, TDestination>`
- `ICanProjectStatic<TSource, TDestination>`
- `ICanMapAsyncNewStatic<TSource, TDestination>`
- `ICanMapAsyncMergeStatic<TSource, TDestination>`

This allows you to check condition before invoking your maps, if you return false your map will be ignored, and depending on the mapper another map/mapper may be executed or the mapper might throw `MapNotFoundException`.

{: .highlight }
Maps themselves cannot throw `MapNotFoundException` to map conditionally, for performance reasons. All the exceptions thrown inside the maps will be wrapped in `MappingException`/`MatcherException`/`ProjectionException` (except `OperationCanceledException` to allow `CancellationToken` cancellation propagation).

```csharp
public class MyMaps :
	ICanMapNew<Product, ProductDto>,
	INewMap<Product, ProductDto>
{
	public bool CanMapNew(MappingContext context){
		// Prevent the map from being used by a CollectionMapper because
		// it might be inefficient to do so (simplified check)
		return context.MappingOptions.GetOptions<NestedMappingContext>()?.ParentMapper is not CollectionMapper;
	}

	public ProductDto? Map(Product? source, MappingContext context){
		if(source == null)
			return null;
		else{
			return new ProductDto{
				Code = source.Code,
				...
			};
		}
	}
}
```

# Additional maps

Conditional mapping is also available for additional maps, simply use the overload which accepts two methods:

```csharp
var additionalNewMaps = new CustomNewAdditionalMapsOptions();
additionalNewMaps.AddMap<Product, ProductDto>(
	(s, c) => ...,
	c => c.MappingOptions.GetOptions<NestedMappingContext>()?.ParentMapper is not CollectionMapper);
```