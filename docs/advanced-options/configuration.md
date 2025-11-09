---
layout: default
title: "Configuration"
nav_order: 1
parent: "Advanced options"
---

# Options

The mappers/matchers/projectors can be configured on creation to specify global behaviour.

Some options can also be overridden during mapping, to apply some behaviour just for a single map.

## CustomMapsOptions

Options applied to mappers/matchers/projectors with user-defined mappings, which allow to specify types to scan for custom maps.

```csharp
// Add all available maps
services.Configure<CustomMapsOptions>(o => o.TypesToScan = Assembly.GetExecutingAssembly().GetTypes().ToList() );

// Or add specific maps
services.Configure<CustomMapsOptions>(o => o.TypesToScan = new List<Type>{ typeof(MyMaps), ... });
services.Configure<CustomMapsOptions>(o => o.TypesToScan.Add(typeof(MyMaps)));
```

## Custom{MapType}AdditionalMapsOptions

Options which allow to specify additional inline user-defined mappings, to be added to maps found in types in `CustomMapsOptions`.

Various options exist for different maps:
- `CustomNewAdditionalMapsOptions`: for `INewMap`
- `CustomMergeAdditionalMapsOptions`: for `IMergeMap`
- `CustomMatchAdditionalMapsOptions`: for `IMatchMap`
- `CustomHierarchyMatchAdditionalMapsOptions`: for `IHierarchyMatchMap`
- `CustomAsyncNewAdditionalMapsOptions`: for `IAsyncNewMap`
- `CustomAsyncMergeAdditionalMapsOptions`: for `IAsyncMergeMap`
- `CustomProjectionAdditionalMapsOptions`: for `IProjectionMap`

```csharp
services.Configure<CustomNewAdditionalMapsOptions>(o => o.AddMap<Product, ProductDto>((p, c) => ...));
```

You can also add the corresponding Can* function to check if your map can be invoked safely without throwing, if it returns false the map won't be invoked.

```csharp
// Execute our custom map only if we have MyOptions inside the provided MappingOptions
services.Configure<CustomNewAdditionalMapsOptions>(o => o.AddMap<Product, ProductDto>((p, c) => ..., (c) => c.MappingOptions.HasOptions<MyOptions>()));
```

## MergeCollectionsOptions

Options applied to automatic collections mapping.

{: .important }
Can be overridden during mapping with `MergeCollectionsMappingOptions`.

```csharp
// Configure globally
services.Configure<MergeCollectionsOptions>(o => o.RemoveNotMatchedDestinationElements = false);

// Override for specific maps via MappingOptions parameter
mapper.Map(products, productDtos, new object[]{ new MergeCollectionsMappingOptions{ RemoveNotMatchedDestinationElements = true } });
```

## AsyncCollectionMappersOptions

Options applied to automatic asynchronous collections mapping.

{: .important }
Can be overridden during mapping with `AsyncCollectionMappersMappingOptions`.

```csharp
// Configure globally
services.Configure<AsyncCollectionMappersOptions>(o => o.MaxParallelMappings = 1);

// Override for specific maps via MappingOptions parameter
asyncMapper.MapAsync<ProductDto[]>(products, new object[]{ new AsyncCollectionMappersMappingOptions{ MaxParallelMappings = 3 } });
```

## CopyMapperOptions

Options applied to `CopyMapper`.

{: .important }
Can be overridden during mapping with `CopyMapperMappingOptions`.

## EnumMapperOptions

Options applied to `EnumMapper`.

{: .important }
Can be overridden during mapping with `EnumMapperMappingOptions`.

## CollectionMatchersOptions

Options applied to `CollectionMatcher`.

{: .important }
Can be overridden during mapping with `CollectionMatchersMappingOptions`.

# Dependency Injection (DI) Configuration

When using Dependency Injection (DI), additional options can be configured to change how some mappers are created.

## Composite{MapperType}Options

Options used to define a list of mappers/matchers/projectors to use to create the final `IMapper`/`IAsyncMapper`/`IMatcher`/`IProjector`. This should be used to add custom mappers/matchers/projectors.

Various options exist for different services:
- `CompositeMapperOptions`: for `IMapper`
- `AsyncCompositeMapperOptions`: for `IAsyncMapper`
- `CompositeMatcherOptions`: for `IMatcher`
- `CompositeProjectorOptions`: for `IProjector`

```csharp
// Create a new instance manually
services.Configure<CompositeMapperOptions>(o => o.Mappers.Add(new MyMapper(...)));

// Or by injecting additional services via Dependency Injection (DI)
services.AddOptions<CompositeMapperOptions>()
	.Configure<MyService>((options, myService) => options.Mappers.Add(new MyMapper(myService, ...)));
```

# Mapping options

In addition to options which can be used to override [global options](#options), it is also possible to specify other options to change the behaviour of the mappers/maps at runtime, these can be passed directly to the `Map()`/`MapAsync()`/`Match()`/`Project()` methods, and also to their "discovery" counterparts `CanMapNew()`/`CanMapMerge()`/`CanMapAsyncNew()`/`CanMapAsyncMerge()`/`CanMatch()`/`CanProject()`.

## {MapperType}OverrideMappingOptions

Options which allow to override the service provider and mapper/matcher/projector to use for nested maps.

```csharp
IServiceProvider mySpecialServiceProvider = new ...;
mapper.Map(products, productDtos,
	new object[]{ new MapperOverrideMappingOptions{ ServiceProvider = mySpecialServiceProvider } });
```

## MergeCollectionsMappingOptions

In addition to overriding global options for [MergeCollectionsOptions](#mergecollectionsoptions) allows to override/provide the matcher used to match elements in merge collection maps.

```csharp
mapper.Map(products, productDtos,
	new object[]{ new MergeCollectionsMappingOptions{ Matcher = DelegateMatcher.Create<Product, ProductDto>((p1, p2, c) => p1?.Code == p2?.Code) } });
```

## Nested{MapperType}Context

These are not options, but contexts created by mappers/matchers/projectors which use other mappers/matchers/projectors to perform their duties (like Composite and Collection), so that those mappers can check this information and act accordingly whether they are used as standalone or by other mappers.

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

## ProjectionCompilationContext

This is not an option, but a context created by mappers which use projectors to compile projection expressions into maps (delegates), so that those projectors can check this information and act accordingly whether they are used as standalone or by other mappers.

```csharp
public class MyMaps :
	ICanProject<Product, ProductDto>,
	IProjectionMap<Product, ProductDto>
{
	// EF.Property below projects the shadow key directly from the DB,
	// so it cannot be compiled into a delegate because the function 
	// cannot be invoked but must be converted into an SQL query,
	// so the method invocation actually throws
	public bool CanProject(ProjectionContext context){
		return !context.MappingOptions.HasOptions<ProjectionCompilationContext>();
	}

	public Expression<Func<Book, BookDto>> Project(ProjectionContext context){
		return source => new BookDto{
			Id = EF.Property<int>(source, "Id")
		};
	}
}
```

## {CustomOptions}

Additional options can be created by the users by using custom classes and checked for in their own maps/mappers via `context.MappingOptions.GetOptions<MyOptions>()` (or `context.MappingOptions.HasOptions<MyOptions>()`), they can be passed along other options.

Custom option classes must be immutable.