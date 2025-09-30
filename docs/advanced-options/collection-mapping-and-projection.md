---
layout: default
title: "Collections mapping and projection"
nav_order: 4
parent: "Advanced options"
---

# Automatic collection maps

When you create a map, you can also map collections of the types, even nested, automatically. This applies for normal maps, async maps (including `IAsyncEnumerable<T>`) and projection maps.

```csharp
// Create a new list
var myProductDtoList = mapper.Map<IEnumerable<Product>, List<ProductDto>>(myProducts);

// Create a new list of lists
var myProductDtoList = mapper.Map<IEnumerable<IEnumerable<Product>>, List<List<ProductDto>>>(myProductss);

// Map to an existing collection
mapper.Map<IList<Category>, ICollection<CategoryDto>>(myCategories, myCategoryDtos);
```

This works with (mostly) all collections, interfaces, even **read-only** and **custom ones** (provided that they have a parameterless constructor for mapping, or a constructor which accepts an IEnumerable<T> for projections).

The only limitation is that you <u>cannot map</u> to an existing read-only collection or a `IAsyncEnumerable<T>` (so no merge maps, but you can create it with a new map).

If you specify an explicit map for two collections this map will be used instead, so you will be in charge of everything.

```csharp
public class MyMaps :
	INewMap<Product, ProductDto>,
	INewMap<IEnumerable<Product>, List<ProductDto>>
{
	ProductDto? INewMap<Product, ProductDto>.Map(Product? source, MappingContext context){
		if(source == null)
			return null;
		else{
			return new ProductDto{
				Code = source.Code,
				...
			};
		}
	}

	List<ProductDto>? INewMap<IEnumerable<Product>, List<ProductDto>>.Map(IEnumerable<Product>? source, MappingContext context){
		return source?.
			.Select(s => new ProductDto{
				...
			})
			.ToList();
	}

	...

	// Create a new list using your explicit map instead of automatic collection mapping
	var myProductDtoList = mapper.Map<IEnumerable<Product>, List<ProductDto>>(myProducts);
}
```

# Match elements in collections

{: .highlight }
The section below **does not apply** to projectors.

When merging to an existing collection, by default all the object present are removed and new ones are mapped and added (by using `INewMap<TSource, TDestination>` or `IMergeMap<TSource, TDestination>` in this order).

If you need to match elements and merge them you can implement an `IMatchMap<TSource, TDestination>` (or `IMatchMapStatic<TSource, TDestination>` if you're on .NET 7 or greater) or specify a matching method or passing an `IEqualityComparer<T>` when mapping.

This way each element is matched with a corresponding element of the destination collection, if found and a `IMergeMap<TSource, TDestination>` is defined it is merged together, otherwise a new element is added to the collection using `INewMap<TSource, TDestination>`.

```csharp
public class MyMaps :
	IMergeMap<Category, CategoryDto>,
	IMatchMap<Category, CategoryDto>,
	IMergeMap<Product, ProductDto>
{
	CategoryDto? IMergeMap<Category, CategoryDto>.Map(Category? source, CategoryDto? destination, MappingContext context){
		if(source != null){
			destination ??= new CategoryDto();
			destination.Id = source.Id;
			...
		}
		return destination;
	}

	bool IMatchMap<Category, CategoryDto>.Match(Category? source, CategoryDto? destination, MatchingContext context){
		return source?.Id == destination?.Id;
	}

	ProductDto? IMergeMap<Product, ProductDto>.Map(Product? source, ProductDto? destination, MappingContext context){
		if(source != null){
			destination ??= new ProductDto();
			destination.Code = source.Code;
			...
		}
		return destination;
	}
}

...

// Map to an existing collection using the match map
mapper.Map<IList<Category>, ICollection<CategoryDto>>(myCategories, myCategoryDtos);

// Map to an existing collection using a custom matching method (used for all types matching
// the provided types, in case of nested collections)
mapper.Map(myCategories, myCategoryDtos, (source, destination, context) => source?.Code == destination?.Code);

// Map to an existing collection using a custom IEqualityComparer<T> (only for matching
// the same type, used for all types matching the provided types, in case of nested collections)
mapper.Map<IList<Category>, ICollection<Category>>(myCategories1, myCategories2, myEqualityComparer);
```

You can also match whole hierarchies by creating a `IHierarchyMatchMap<TSource, TDestination>` (or `IHierarchyMatchMapStatic<TSource, TDestination>` if you're on .NET 7 or greater), this will be applied to the specified types and all types derived from them.

# Destination collection cleanup

{: .highlight }
The section below **does not apply** to projectors.

Any element in the destination collection which does not have a corresponding element in the source collection is removed by default, you can disable this (if you need to create an add or update collection for example) via global settings or specific for each mapping.

```csharp
// Global settings via DI
services.Configure<MergeCollectionsOptions>(o => RemoveNotMatchedDestinationElements = false);

// Single mapping override
mapper.Map<IList<Category>, ICollection<CategoryDto>>(myCategories, myCategoryDtos, new MergeCollectionsMappingOptions{
	RemoveNotMatchedDestinationElements = false
});
```

# Lazy collection mapping

In all of the above maps collections are mapped fully, but you may also map only the elements you need while you enumerate the destination collection, for this you can use the `Project` extension methods.

{: .important }
This works only for maps to new collections.

```csharp
// Create a new lazy-projected enumerable
var myProductDtoEnumerable = myProducts.Project<ProductDto>(mapper);

// Map the first 3 elements into an array
var myProductDtoArray = myProductDtoEnumerable.Take(3).ToArray();

// Loop through the projected elements by mapping them as needed
// (the first 3 elements get mapped again, because they are not cached)
foreach(var myProductDto in myProductDtoEnumerable){
	// Here you may exit based on conditions, thus not mapping the whole collection
}
```

Like for every IEnumerable, keep in mind that everytime you enumerate it, it gets mapped again.

# Async parallelization

Generally, whenever possible a specific async collection map should be created to handle parallel operations.

However, when this is not possible, async automatic collection maps can be parallelized to improve performance for long-running tasks.

This can be done by configuring `AsyncCollectionMappersOptions` like below or specified for individual maps.

```csharp
// Global settings via DI
services.Configure<AsyncCollectionMappersOptions>(o => MaxParallelMappings = 10);

// Single mapping override
await asyncMapper.MapAsync<IList<Entity>, List<EntityDto>>(myEntities, new object[]{
	new AsyncCollectionMappersMappingOptions{
		MaxParallelMappings = 10
	}
});
```