# .NEaT Mapper

[![NuGet](https://img.shields.io/nuget/v/NeatMapper.svg?label=NuGet)](https://www.nuget.org/packages/NeatMapper)

## What is this package

.NEaT Mapper is a .NET object mapper (and now projector too!), with configurable and reusable mappings.

This allows you to create mappings (and projections) between different types (even generic ones), combine them, nest them and reuse them, making your code DRY. Map once, use everywhere.

All of this is achieved with explicit strongly typed maps, easily debuggable, no implicit mappings, no conventions, no compilation into obscure expressions, no black boxes.

It also supports asynchronous maps and services via Dependency Injection (DI).

## How to install

You can install this package directly from Nuget https://www.nuget.org/packages/NeatMapper

## How to use

### 1. Create mapping classes

You have to create one or more classes implementing one of the following mapping interfaces:
- `INewMap<TSource, TDestination>` to map existing objects to new ones
- `IMergeMap<TSource, TDestination>` to map and merge an object with another one
- `IProjectionMap<TSource, TDestination>` to project an object into another one, this will create an expression which can be used with LINQ or compiled into a NewMap

You can also create `async` maps by implementing the interfaces:
- `IAsyncNewMap<TSource, TDestination>`
- `IAsyncMergeMap<TSource, TDestination>`

If you are on .NET 7 or greater you can use the `static` versions of the interfaces above: `INewMapStatic<TSource, TDestination>`, `IMergeMapStatic<TSource, TDestination>`, `IProjectionMapStatic<TSource, TDestination>`, `IAsyncNewMapStatic<TSource, TDestination>`, `IAsyncMergeMapStatic<TSource, TDestination>`.

If you create a class with more than 1 mapping of the same interface you must implement them explicitly like below.

```csharp
public class MyMaps :
    INewMap<Product, ProductDto>,
    IAsyncMergeMap<Category, CategoryDto>,
    IProjectionMap<Book, BookDto>
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

    async Task<CategoryDto?> IAsyncMergeMap<Category, CategoryDto>.MapAsync(Category? source, CategoryDto? destination, AsyncMappingContext context){
        if(source != null){
            destination ??= new CategoryDto();
            destination.Id = source.Id;
            destination.Parent = await context.Mapper.MapAsync(source.Parent, destination.Parent, context.CancellationToken);
            ...
        }
        return destination;
    }

    Expression<Func<Book, BookDto>> IProjectionMap<Book, BookDto>.Project(ProjectionContext context){
        return source => source == null ? null : new BookDto{ ... };
    }
}
```

### 2. Configure the services

The easiest way to create a mapper is to use Dependency Injection (DI), which will handle all the configuration for you.

```csharp
// Add all available maps
services.Configure<CustomMapsOptions>(o => o.TypesToScan = Assembly.GetExecutingAssembly().GetTypes().ToList() );
// Or add specific maps
//services.Configure<CustomMapsOptions>(o => o.TypesToScan = new List<Type>{ typeof(MyMaps), ... });

services.AddNeatMapper();

...

IMapper mapper = serviceProvider.GetRequiredService<IMapper>();
IAsyncMapper asyncMapper = serviceProvider.GetRequiredService<IAsyncMapper>();
IProjector projector = serviceProvider.GetRequiredService<IProjector>();
```

### 3. Map your types

Map your types by invoking the generic methods available.

Note that mapping matches types exactly, so parent or derived classes won't work.

```csharp
// Create a new object
var myProductDto = mapper.Map<ProductDto>(myProduct);

// Map to an existing object asynchronously (the types are auto-inferred)
await asyncMapper.MapAsync(myCategory, myCategoryDto);

// Create a projection to use in a LINQ query
var myBookDtos = db.Set<Book>()
    .Project<BookDto>(projector)
    .ToArray();
```

## Advanced options

Find more advanced use cases in the [website](https://www.neatmapper.org/advanced-options/) or in the extended [tests project](https://github.com/Xriuk/NeatMapper/tree/main/tests/NeatMapper.Tests).

## License

[Read the license here](https://www.neatmapper.org/license)
