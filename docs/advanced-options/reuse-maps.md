---
layout: default
title: "Reuse maps"
nav_order: 3
parent: "Advanced options"
---

You can map derived or nested objects (even recursively) by reusing existing maps to keep your code DRY.

# Nested maps

For this you can use the `IMapper` (or `IAsyncMapper`) instance you will find in the mapping context.

{: .important }
In case of async maps you should also pass down the provided `CancellationToken` to allow interrupting async operations if needed.

```csharp
public class MyMaps :
    INewMap<Product, ProductDto>,
    IAsyncMergeMap<Category, CategoryDto>
{
    ProductDto? INewMap<Product, ProductDto>.Map(Product? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new ProductDto{
                Code = source.Code,
                Category = context.Mapper.Map<Category, CategoryDto>(source.Category),
                ...
            };
        }
    }

    async Task<CategoryDto?> IMergeMap<Category, CategoryDto>.MapAsync(Category? source, CategoryDto? destination, AsyncMappingContext context){
        if(source != null){
            destination ??= new CategoryDto();
            destination.Id = source.Id;
            destination.Parent = await context.Mapper.MapAsync<Category, CategoryDto>(source.Parent, destination.Parent, context.CancellationToken);
            ...
        }
        return destination;
    }
}
```

# Hierarchy maps

You could even map hierarchies by reusing existing maps.

```csharp
public class MyMaps :
    IMergeMap<Product, ProductDto>,
    IMergeMap<LimitedProduct, LimitedProductDto>
{
    ProductDto? IMergeMap<Product, ProductDto>.Map(Product? source, ProductDto?, MappingContext context){
        if(source != null){
            destination ??= new ProductDto();
            destination.Code = source.Code;
            ...
        }
        return destination;
    }

    LimitedProductDto? IMergeMap<LimitedProduct, LimitedProductDto>.Map(LimitedProduct? source, LimitedProductDto?, MappingContext context){
        // Needed to prevent constructing ProductDto in parent map
        destination ??= new LimitedProductDto();
        
        // Map parents (returned destination may be null, depending on the mapping)
        destination = context.Mapper.Map<Product, ProductDto>(source, destination) as LimitedProductDto;
        
        // Map child properties
        if(source != null){
            destination ??= new LimitedProductDto();
            destination.LimitedProp = source.LimitedProp;
            ...
        }
        return destination;
    }
}
```

# Nested projections

When creating projection expression you can reuse existing maps by using the `NestedProjector` instance you will find in the projection context.

When the final map will be created the nested projector map will be replaced with the actual nested expression inline.

```csharp
public class MyMaps :
    IProjectionMap<Category, CategoryDto>,
    IProjectionMap<Product, ProductDto>
{

    Expression<Func<Category?, CategoryDto?>> IProjectionMap<Category, CategoryDto>.Project(ProjectionContext context){
        return source => source == null ?
            null : 
            new CategoryDto{
                Id = source.Id,
                ...
            };
    }

    Expression<Func<Product?, ProductDto?> IProjectionMap<Product, ProductDto>.Project(ProjectionContext context){
        return source => source == null ?
            null : 
            new ProductDto{
                Code = source.Code,
                Category = context.Projector.Project<Category, CategoryDto>(source.Category),
                ...
            };
        }
    }
}
```

The compiled expression will look like this:

```csharp
source => source == null ?
    null : 
    new ProductDto{
        Code = source.Code,
        Category = source.Category == null ?
            null : 
            new CategoryDto{
                Id = source.Category.Id,
                ...
            },
        ...
    };
}
```

{: .highlight }
Be careful with nesting projections as this can lead to complex evaluations when the expression will be parsed/translated.