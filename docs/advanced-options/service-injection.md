---
layout: default
title: "Service injection"
nav_order: 5
parent: "Advanced options"
---

You will be able to retrieve services from your maps, this could allow you to query objects from the database or from external APIs for example.

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
```