---
layout: default
title: "Getting started"
nav_order: 2
---

# Installation

You can install the core package directly from Nuget [NeatMapper](https://www.nuget.org/packages/NeatMapper).

# Creating maps

You have to create one or more classes implementing one of the following mapping interfaces:
- `INewMap<TSource, TDestination>` to map existing objects to new ones
- `IMergeMap<TSource, TDestination>` to map and merge objects with others
- `IMatchMap<TSource, TDestination>` to match objects together (used to merge collections)
- `IHierarchyMatchMap<TSource, TDestination>` like `IMatchMap<TSource, TDestination>` but matches both exact types and derived ones
- `IProjectionMap<TSource, TDestination>` to project objects into new ones, this will create an expression which can be used with LINQ or compiled into a NewMap

You can also create `async` maps by implementing the interfaces:
- `IAsyncNewMap<TSource, TDestination>`
- `IAsyncMergeMap<TSource, TDestination>`

If you are on .NET 7 or greater you can use the `static` versions of the interfaces above instead:
- `INewMapStatic<TSource, TDestination>`
- `IMergeMapStatic<TSource, TDestination>`
- `IMatchMapStatic<TSource, TDestination>`
- `IHierarchyMatchMapStatic<TSource, TDestination>`
- `IProjectionMapStatic<TSource, TDestination>`
- `IAsyncNewMapStatic<TSource, TDestination>`
- `IAsyncMergeMapStatic<TSource, TDestination>`

{: .highlight }
If you create a class with more than 1 implementation of the same interface you must implement them [explicitly](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation), like below.

```csharp
public class MyMaps :
	INewMap<Product, ProductDto>,
	IAsyncMergeMap<Category, CategoryDto>,
	IProjectionMap<Book, BookDto>
{
	// A map which creates a new object of type ProductDto from an existing object of type Product
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

	// An asynchronous map which merges two existing objects of type Category and CategoryDto
	// into a third object of type CategoryDto (which can be the exiting object or a mix of the two)
	async Task<CategoryDto?> IAsyncMergeMap<Category, CategoryDto>.MapAsync(Category? source, CategoryDto? destination, AsyncMappingContext context){
		if(source != null){
			destination ??= new CategoryDto();
			destination.Id = source.Id;
			// Nested maps allow to reuse existing maps by avoiding repeating code
			destination.Parent = await context.Mapper.MapAsync(source.Parent, destination.Parent, context.CancellationToken);
			...
		}
		return destination;
	}

	// A projection Expression to convert an object of type Book into an object of type BookDto,
	// which can be translated (by other libraries like Entity Framework) or compiled into delegates.
	// In expressions nullability is respected and not enforced like in maps
	Expression<Func<Book, BookDto>> IProjectionMap<Book, BookDto>.Project(ProjectionContext context){
		return source => new BookDto{ ... };
	}
}
```

# Configuring the services

The easiest way to create a mapper is to use [Dependency Injection (DI)](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection), which will handle all the configuration for you.

```csharp
// Add all available maps
services.Configure<CustomMapsOptions>(o => o.TypesToScan = Assembly.GetExecutingAssembly().GetTypes().ToList() );
// Or add specific maps
services.Configure<CustomMapsOptions>(o => o.TypesToScan = new List<Type>{ typeof(MyMaps), ... });

services.AddNeatMapper();

...

IMapper mapper = serviceProvider.GetRequiredService<IMapper>();
IAsyncMapper asyncMapper = serviceProvider.GetRequiredService<IAsyncMapper>();
IProjector projector = serviceProvider.GetRequiredService<IProjector>();
```

# Mapping objects

Map your objects by invoking the generic (extension) methods available:
- `IMapper`
  - `mapper.Map<Destination>(source)`
  - `mapper.Map<Source, Destination>(source)`
  - `mapper.Map(source, destination)`
  - `mapper.Map<Source, Destination>(source, destination)`
  - `IEnumerable`/`IEnumerable<T>` extension methods:
    - `enumerable.Project<Destination>(mapper)`
	- `enumerable.Project<Source, Destination>(mapper)`
- `IAsyncMapper`
  - `await asyncMapper.MapAsync<Destination>(source)`
  - `await asyncMapper.MapAsync<Source, Destination>(source)`
  - `await asyncMapper.MapAsync(source, destination)`
  - `await asyncMapper.MapAsync<Source, Destination>(source, destination)`
  - `IAsyncEnumerable<T>` extension method `asyncEnumerable.Project<Source, Destination>(asyncMapper)`
- `IProjector`
  - `projector.Project<Source, Destination>()`
  - `IQueryable`/`IQueryable<T>` extension methods:
    - `queryable.Project<Destination>(projector)`
    - `queryable.Project<Source, Destination>(projector)`

{: .highlight }
Note that mapping matches types exactly, so parent or derived classes won't work.

```csharp
// Create a new object (source type is auto-inferred)
var myProductDto = mapper.Map<ProductDto>(myProduct);

// Map to an existing object asynchronously (both types are auto-inferred)
await asyncMapper.MapAsync(myCategory, myCategoryDto);

// Create a projection to use in a LINQ query
var myBookDtos = db.Set<Book>()
	.Project<BookDto>(projector)
	.ToArray();
```
