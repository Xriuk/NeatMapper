---
layout: default
title: "Mappers"
nav_order: 1
parent: "Built-in"
---

# CustomMapper

Maps objects by using `INewMap<TSource, TDestination>`, `IMergeMap<TSource, TDestination>` (and their static counterparts) and any [additional map](/advanced-options/configuration#custommaptypeadditionalmapsoptions).
Also supports [conditional mapping](/customization/conditional-mapping).

```csharp
// Implement the interface(s)
public class MyMaps :
	INewMap<Product, ProductDto>
{
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

...

// Create the mapper and retrieve the maps from the given types
var mapper = new CustomMapper(new CustomMapsOptions{ Types = [ typeof(MyMaps) ] });

...

// Create a new object (source type is auto-inferred)
var myProductDto = mapper.Map<ProductDto>(myProduct);
```

# CollectionMapper

Maps collections by using another `IMapper` to map elements. Supports many types including interfaces and concrete types derived from `IEnumerable<T>` and `ICollection<T>`.
Also supports merging two collections by matching elements together and optionally removing unmatched ones.
See also [Collections mapping and projection](/advanced-options/collection-mapping-and-projection).

```csharp
// Create the mapper
var mapper = new CollectionMapper(new CustomMapper(...)); // Use any mapper

...

// Convert a collection
var myProductDtos = mapper.Map<IEnumerable<Product>, IList<ProductDto>>(myProducts);
```

# CompositeMapper

Delegates mapping to other `IMapper`s. All the mappers are tried in the provided order, for new maps, if no mapper can map the types a destination object is created and merge maps are tried.

```csharp
// Create the mapper
var mapper = new CompositeMapper(
	new CustomMapper(...),
	new CollectionMapper(new CustomMapper(...)),
	...
);

...

// Map objects
var myProductDto = mapper.Map<Product, ProductDto>(myProduct);
var myProductDtos = mapper.Map<IEnumerable<Product>, IList<ProductDto>>(myProducts);
```

# CopyMapper

Maps objects by copying all supported properties/fields between source and destination (can also copy private ones). Supports derived and base types (non-abstract), and deep copies. Same references are mapped to the same objects, to avoid duplicates and handle recursion.

```csharp
// Create the mapper
var mapper = new CopyMapper();

...

// Copy object
var myProductCopy = mapper.Map<Product, Product>(myProduct);

// Copy object to derived class
var myLimitedProduct = mapper.Map<Product, LimitedProduct>(myProduct);

// Copy object to parent class
var myBaseProduct = mapper.Map<Product, BaseProduct>(myProduct);
```

# EnumMapper

Maps enums to and from their underlying numeric types, strings and other enums. Supports only new maps.

```csharp
// Declare the enums
public enum Enum1 {
	A,
	B,
	C
}

public enum Enum2 : ushort { // Change the underlying type
	A1,
	[EnumMember(Value = "Second value")]
	B2,
	[Display(Name = "Third value")]
	C3
}

...

// Create the mapper
var mapper = new EnumMapper();

...

// Map the enums
mapper.Map<int>(Enum1.B); // 2
mapper.Map<ushort>(Enum2.C3); // 3

mapper.Map<string>(Enum2.A1); // "A1"
mapper.Map<string>(Enum2.B2); // "Second value"
mapper.Map<string>(Enum2.C3); // "Third value"

mapper.Map<Enum2>(Enum.C); // Enum2.C3
mapper.Map<Enum1>(Enum2.A1); // Enum1.A
```

# NullableMapper

Maps `Nullable<T>` and its underlying types by using another `IMapper`. Supports only new maps.

```csharp
// Implement the interface(s)
public class MyMaps :
	INewMap<DateTime, string>
{
	public string? Map(DateTime source, MappingContext context){
		return source.ToString("O"); // ISO format, never null
	}
}

...

// Create the mapper and retrieve the maps from the given types
var mapper = new NullableMapper(new CustomMapper(new CustomMapsOptions{ Types = [ typeof(MyMaps) ] }));

...

// Create a new object (source type is auto-inferred)
var myDate = mapper.Map<DateTime, string>(new DateTime(2025, 08, 31, 19, 21, 16, DateTimeKind.Utc)); // "2025-08-31T19:21:16Z"
var myNullableDate1 = mapper.Map<DateTime?, string>(new DateTime(2025, 08, 31, 19, 21, 16, DateTimeKind.Utc)); // "2025-08-31T19:21:16Z", same as above
var myNullableDate2 = mapper.Map<DateTime?, string>(null); // null
```

# ProjectionMapper

Maps objects by using an `IProjector` to retrieve mapping expressions which then get compiled and cached into delegates. Supports only new maps and not merge maps.

```csharp
// Implement the interface(s)
public class MyMaps :
	IProjectionMap<Product, ProductDto>
{
	public Expression<Func<Product, ProductDto>> Map(ProjectionContext context){
		return p => new ProductDto{
			Code = p.Code,
			...
		};
	}
}

...

// Create the mapper and retrieve the maps from the given types
var mapper = new ProjectionMapper(new CustomProjector(new CustomMapsOptions{ Types = [ typeof(MyMaps) ] }));

...

// Create a new object (source type is auto-inferred)
var myProductDto = mapper.Map<ProductDto>(myProduct);
```