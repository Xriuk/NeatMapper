# .NEaT Mapper

## What is this package

.NEaT Mapper is an object mapper, with configurable and reusable mappings.

This allows you to create mappings between different types (even generic ones),
combine them, nest them and reuse them, making your code DRY. Map once, use everywhere.

All of this is achieved with strongly typed maps, easily debuggable,
no compilation into obscure expressions, no black boxes.

## How to install

You can find all the other packages on Nuget https://www.nuget.org/profiles/xriuk

You can install this package directly from Nuget https://www.nuget.org/packages/NeatMapper

## How to use

### 1. Create mapping classes

You have to create one or more classes implementing one of the 2 mapping interfaces:
- `INewMap<TSource, TDestination>` to map existing objects to new ones
- `IMergeMap<TSource, TDestination>` to map and merge an object with another one,
but it can also be used to map an object to a new one (see [One map to rule them all](#one-map-to-rule-them-all-use-one-map-for-multiple-mappings) below)

If you are on .NET 7 or greater you can use the `static` versions of these interfaces:
`INewMapStatic<TSource, TDestination>` and `IMergeMapStatic<TSource, TDestination>`.

If you create a class with more than 1 mapping of the same interface you must implement
them explicitly like below.

```csharp
public class MyMaps :
    INewMap<Product, ProductDto>,
    IMergeMap<Category, CategoryDto>
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

    CategoryDto? IMergeMap<Category, CategoryDto>.Map(Category? source, CategoryDto? destination, MappingContext context){
        if(source != null){
            destination ??= new CategoryDto();
            destination.Id = source.Id;
            ...
        }
        return destination;
    }
}
```

### 2. Configure and create a mapper

Once you have all your map classes you can create instances of the mapper and you can
even load different maps into different mappers.

```csharp
// Specify classes
IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { typeof(Maps), ... }
});

// Or scan all the assembly
IMapper mapper = new Mapper();
```

### 3. Map your types

Map your types by invoking the generic methods available.

Note that mapping matches types exactly (except for advanced cases described below),
so parent or derived classes won't work, but you may reuse mappings for them
(keep reading below).

```csharp
// Create a new object
var myProductDto = mapper.Map<Product, ProductDto>(myProduct);

// Map to an existing object
mapper.Map<Category, CategoryDto>(myCategory, myCategoryDto);
```

## Advanced options

### One map to rule them all (use one map for multiple mappings)

If you only define an `IMergeMap<TSource, TDestination>` for two given types
it can also be used when creating a new object instead of defining a separate
`INewMap<TSource, TDestination>` for the same types.

In this case a destination object will be created automatically,
so for this to work a parameterless constructor is required.

```csharp
// Map to an existing object
mapper.Map<Category, CategoryDto>(myCategory, myCategoryDto);

// Create a new object
var myProductDto = mapper.Map<Category, CategoryDto>(myProduct);
```

You can still create both maps for the given types if you need specific behaviour
in one case or the other.

### Deep down (nested mapping)

You can map derived or nested objects (even recursively) by reusing existing maps to keep your code DRY.

For this you can use the `IMapper` instance you will find in the mapping context.

```csharp
public class MyMaps :
    INewMap<Product, ProductDto>,
    IMergeMap<Category, CategoryDto>
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

    CategoryDto? IMergeMap<Category, CategoryDto>.Map(Category? source, CategoryDto? destination, MappingContext context){
        if(source != null){
            destination ??= new CategoryDto();
            destination.Id = source.Id;
            destination.Parent = context.Mapper.Map<Category, CategoryDto>(source.Parent, destination.Parent);
            ...
        }
        return destination;
    }
}
```

### Gotta collect them all (map collections)

When you create a map you can also map collections of the types, even nested, automatically.

```csharp
// Create a new list
var myProductDtoList = mapper.Map<IEnumerable<Product>, List<ProductDto>>(myProducts);

// Create a new list of lists
var myProductDtoList = mapper.Map<IEnumerable<IEnumerable<Product>>, List<List<ProductDto>>>(myProductss);

// Map to an existing collection
mapper.Map<IList<Category>, ICollection<CategoryDto>>(myCategories, myCategoryDtos);
```

This works with (mostly) all collections, interfaces, even read-only and custom ones.

The only limitation is that you cannot map to an existing read-only collection.

If you specify an explicit map for two collections this map will be used instead,
so you will be in charge of everything.

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

Read the section below too for more informations about collections.

### Guess who (match elements in collections)

When merging to an existing collection, by default all the object present are removed
and new ones are mapped and added (by using `INewMap<TSource, TDestination>` or
`IMergeMap<TSource, TDestination>` in this order).

If you need to match elements and merge them you can implement an
`IMatchMap<TSource, TDestination>` (or `IMatchMapStatic<TSource, TDestination>`
if you're on .NET 7 or greater) or specify a matching method when mapping.

This way each element is matched with a corresponding element of the destination collection,
if found and a `IMergeMap<TSource, TDestination>` is defined it is merged together,
otherwise it is removed and a new element is added to the collection using
`INewMap<TSource, TDestination>`.

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

IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { typeof(Maps), ... }
});

// Map to an existing collection using the match map
mapper.Map<IList<Category>, ICollection<CategoryDto>>(myCategories, myCategoryDtos);

// Map to an existing collection using a custom matching method
mapper.Map(myCategories, myCategoryDtos, (source, destination, context) => source?.Code == destination?.Code);
```

Any element in the destination collection which do not have a corresponding element
in the source collection is removed by default, you can disable this
(if you need to create an add or update collection for example) via global settings
or specific for each mapping.

```csharp
// Global settings
IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { typeof(Maps), ... },
    MergeMapsCollectionsOptions = new MergeMapsCollectionsOptions{
        RemoveNotMatchedDestinationElements = false
    }
});

// Single mapping override
mapper.Map<IList<Category>, ICollection<CategoryDto>>(myCategories, myCategoryDtos, new MappingOptions{
    CollectionRemoveNotMatchedDestinationElements = false
});
```

### Generally speaking (generic types)

You can also create mappings between generic types, and they will be automatically mapped
for any type (which may even not have a map), cool, isn't it?

```csharp
public class MyGenericMaps<T1> :
    INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>,
    IMergeMap<MyGenericClass<T1>, MyGenericClassDto<T1>>
{
    MyGenericClassDto<T1>? INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>.Map(MyGenericClass<T1>? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new MyGenericClassDto<T1>{
                ...
            };
        }
    }

    MyGenericClassDto<T1>? IMergeMap<MyGenericClass<T1>, MyGenericClassDto<T1>>.Map(MyGenericClass<T1>? source, MyGenericClassDto<T1>? destination, MappingContext context){
        if(source != null){
            destination ??= new MyGenericClassDto<T1>();
            ...
        }
        return destination;
    }
}

...

// Create a mapper by passing an open generic type of the class with the maps
IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { typeof(MyGenericMaps<>), ... }
});

// Map any type
var myGenericClassDto1 = mapper.Map<MyGenericClass<int>, MyGenericClassDto<int>>(myGenericClass1);
var myGenericClassDto2 = mapper.Map<MyGenericClass<string>, MyGenericClassDto<string>>(myGenericClass2);
var myGenericClassDto3 = mapper.Map<MyGenericClass<Product>, MyGenericClassDto<Product>>(myGenericClass3);
```

You can create types with as many generic parameters as you want, with the only condition
that the parameters must be all present in the map definition.

```csharp
public class MyGenericMaps<T1, T2> :
    INewMap<MyGenericClass<T1>, MyGenericClassDto<T2>>
    //, IMergeMap<MyGenericClass<T1>, MyGenericClassDto<T1>> // not valid since it uses only T1
{
    MyGenericClassDto<T2>? INewMap<MyGenericClass<T1>, MyGenericClassDto<T2>>.Map(MyGenericClass<T1>? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new MyGenericClassDto<T2>{
                ...
            };
        }
    }
}
```

You can also specify any supported generic constraint to specialize your generic maps.

```csharp
public class MyGenericMapsClass<T1> :
    INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>
    where T1 : class
{
    MyGenericClassDto<T1>? INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>.Map(MyGenericClass<T1>? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new MyGenericClassDto<T1>{
                // Different mapping for classes
                ...
            };
        }
    }
}

public class MyGenericMapsStruct<T1> :
    INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>
    where T1 : struct
{
    MyGenericClassDto<T1>? INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>.Map(MyGenericClass<T1>? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new MyGenericClassDto<T1>{
                // Different mapping for structs
                ...
            };
        }
    }
}

...

IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { typeof(MyGenericMapsClass<>), typeof(MyGenericMapsStruct<>), ... }
});

// Map with struct
var myGenericClassDto1 = mapper.Map<MyGenericClass<int>, MyGenericClassDto<int>>(myGenericClass1);

// Map with class
var myGenericClassDto3 = mapper.Map<MyGenericClass<Product>, MyGenericClassDto<Product>>(myGenericClass3);
```

Generic types also supports [automatic collection maps](#gotta-collect-them-all-map-collections) and [matchers](#guess-who-match-elements-in-collections) (which can be generic or explicit too).

If you specify an explicit map for two generic types this map will be used instead,
this allows to define specific mapping for specific types.

```csharp
public class MyGenericMaps<T1> :
    INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>
{
    MyGenericClassDto<T1>? INewMap<MyGenericClass<T1>, MyGenericClassDto<T1>>.Map(MyGenericClass<T1>? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new MyGenericClassDto<T1>{
                ...
            };
        }
    }
}

public class MyMaps :
    INewMap<MyGenericClass<int>, MyGenericClassDto<int>>
{
    MyGenericClassDto<int>? INewMap<MyGenericClass<int>, MyGenericClassDto<int>>.Map(MyGenericClass<int>? source, MappingContext context){
        if(source == null)
            return null;
        else{
            return new MyGenericClassDto<int>{
                // Specific int mapping
                ...
            };
        }
    }
}

...

IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { typeof(MyGenericMaps<>), typeof(MyMaps), ... }
});

// Map with struct
var myGenericClassDto1 = mapper.Map<MyGenericClass<int>, MyGenericClassDto<int>>(myGenericClass1);

// Map with class
var myGenericClassDto3 = mapper.Map<MyGenericClass<Product>, MyGenericClassDto<Product>>(myGenericClass3);
```

### At your service (dependency injection - DI, service providers)

If you pass a `IServiceProvider` to the Mapper constructor you will be able to retrieve services from your maps, this could allow you to query objects from the database or from external APIs for example.

```csharp
public class MyMaps :
    INewMap<Product, ProductDto>,
    IMergeMap<Category, CategoryDto>
{
    ProductDto? INewMap<Product, ProductDto>.Map(Product? source, MappingContext context){
        if(source == null)
            return null;
        else{
            var product = context.ServiceProvider.GetRequiredService<MyDatabase>().Find<Product>(source.Code);

            return new ProductDto{
                Code = source.Code,
                Name = product.Name,
                ...
            };
        }
    }

    CategoryDto? IMergeMap<Category, CategoryDto>.Map(Category? source, CategoryDto? destination, MappingContext context){
        if(source != null){
            var category = context.ServiceProvider.GetRequiredService<MyAPI>().GetCategoryParent(source.Id);

            destination ??= new CategoryDto();
            destination.Id = source.Id;
            destination.Parent = category?.Id;
            ...
        }
        return destination;
    }
}

...

IMapper mapper = new Mapper(new MapperConfigurationOptions {
    ScanTypes = new List<Type> { ... }
}, myServiceprovider);
```

If you are using `Microsoft.Extensions.DependencyInjection` you may want to install [NeatMapper.DependencyInjection](https://www.nuget.org/packages/NeatMapper.DependencyInjection) so that everything will be automatically configured.

## License

[Read the license here](https://github.com/Xriuk/NeatMapper/tree/main/src/NeatMapper/LICENSE.md)