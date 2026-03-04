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
				// Use the nested map for Category
				Category = context.Mapper.Map<Category, CategoryDto>(source.Category),
				...
			};
		}
	}

	async Task<CategoryDto?> IMergeMap<Category, CategoryDto>.MapAsync(Category? source, CategoryDto? destination, AsyncMappingContext context){
		if(source != null){
			destination ??= new CategoryDto();
			destination.Id = source.Id;
			// Use the async nested map to map the parent category (remember to pass down
			// the CancellationToken)
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
// LimitedProduct derives from Product
public class MyMaps :
	IMergeMap<Product, ProductDto>,
	IMergeMap<LimitedProduct, LimitedProductDto>
{
	ProductDto? IMergeMap<Product, ProductDto>.Map(Product? source, ProductDto? destination, MappingContext context){
		if(source != null){
			destination ??= new ProductDto();
			destination.Code = source.Code;
			...
		}
		return destination;
	}

	LimitedProductDto? IMergeMap<LimitedProduct, LimitedProductDto>.Map(LimitedProduct? source, LimitedProductDto? destination, MappingContext context){
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

When the final map will be created the nested projector map will be replaced with the actual nested expression inline with the arguments replaced.

```csharp
public class MyMaps :
	IProjectionMap<Category, CategoryDto>,
	IProjectionMap<Product, ProductDto>
{

	Expression<Func<Category, CategoryDto>> IProjectionMap<Category, CategoryDto>.Project(ProjectionContext context){
		return source => new CategoryDto{
			Id = source.Id,
			...
		};
	}

	Expression<Func<Product, ProductDto> IProjectionMap<Product, ProductDto>.Project(ProjectionContext context){
		return source => new ProductDto{
			Code = source.Code,
			Category = source.Category != null ? context.Projector.Project<Category, CategoryDto>(source.Category) : null,
			...
		};
	}
}
```

The resulting expression will look like this:

```csharp
source => new ProductDto{
	Code = source.Code,
	Category = source.Category != null ? new CategoryDto{
		Id = source.Category.Id,
		...
	} : null,
	...
};
```

{: .highlight }
Be careful with nesting projections as this can lead to complex evaluations when the expression will be parsed/translated.

# Inline expressions

When creating projection expression you can reuse existing `Expression<Func<...>>` by using the `NestedProjector` instance you will find in the projection context.

When the final map will be created the inline Expression will be replaced with the actual expression with the arguments replaced.

Up to 16 arguments are supported, just like the `System.Func<...>` delegate.

```csharp
public class MyMaps :
	IProjectionMap<Product, ProductDto>
{

	static readonly Expression<Func<Category, CategoryDto>> MyInlineExpression = source => new CategoryDto{
		Id = source.Id,
		...
	};

	Expression<Func<Product, ProductDto> IProjectionMap<Product, ProductDto>.Project(ProjectionContext context){
		return source => new ProductDto{
			Code = source.Code,
			Category = context.Projector.Inline(MyInlineExpression, source.Category),
			...
		};
	}
}
```

The resulting expression will look like this:

```csharp
source => new ProductDto{
	Code = source.Code,
	Category = new CategoryDto{
		Id = source.Category.Id,
		...
	},
	...
};
```

# Extend projections

In addition to using nested projections you can also extend existing projections by using the `NestedProjector` instance you will find in the projection context. For example derived classes could reuse parents' projections to avoid redeclaring members.

```csharp
public class Product : BaseProduct { ... }

public class ProductDto : BaseProductDto { ... }

public class MyMaps :
	IProjectionMap<BaseProduct, BaseProductDto>,
	IProjectionMap<Product, ProductDto>
{

	Expression<Func<BaseProduct, BaseProductDto>> IProjectionMap<BaseProduct, BaseProductDto>.Project(ProjectionContext context){
		return source => new BaseProductDto{
			Id = source.Id,
			...
		};
	}

	Expression<Func<Product, ProductDto> IProjectionMap<Product, ProductDto>.Project(ProjectionContext context){
		// The first type argument is the resulting type, the second is the base type for assignability
		return source => context.Projector.Merge<ProductDto, BaseProductDto>(
			context.Projector.Project<BaseProduct, BaseProductDto>(source), // source is a Product, so it is also a BaseProduct
			// Declare the other members
			new ProductDto{
				Code = source.Code,
				...
			});
	}
}
```

The resulting expression will look like this:

```csharp
source => new ProductDto{
	// Members from BaseProduct, BaseProductDto
	Id = source.Id,
	...
	// Members from Product, ProductDto
	Code = source.Code,
	...
};
```

You can also use reuse existing maps as interfaces for maximum extendability, 

```csharp
public class Product { ... }

public class RegularProductDto : IMyInterface { ... }

public class LimitedProductDto : IMyInterface { ... }

public class MyMaps :
	IProjectionMap<Product, RegularProductDto>,
	IProjectionMap<Product, ProductDto>
{

	Expression<Func<Product, RegularProductDto>> IProjectionMap<Product, RegularProductDto>.Project(ProjectionContext context){
		return source => new RegularProductDto{
			Id = source.Id,
			...
		};
	}

	Expression<Func<Product, LimitedProductDto> IProjectionMap<Product, LimitedProductDto>.Project(ProjectionContext context){
		// The first type argument is the resulting type, the second is the base type for assignability
		return source => context.Projector.Merge<LimitedProductDto, IMyInterface>(
			// The cast is necessary to only consider members of the interface (otherwise unrelated members from RegularProductDto
			// could be included triggering errors, since RegularProductDto and LimitedProductDto are unrelated, they just share an interface)
			(IMyInterface)context.Projector.Project<Product, RegularProductDto>(source),

			// Declare the other members, no cast needed here since we are not considering the interface but the whole class
			new LimitedProductDto{
				Code = source.Code,
				...
			});
	}
}
```