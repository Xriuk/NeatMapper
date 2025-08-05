---
layout: default
title: "Generic maps"
nav_order: 6
parent: "Advanced options"
---

You can also create mappings between generic types, and they will be automatically mapped for any type (which may even not have a map so T1 can be any type), cool, isn't it?

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

// If you add all available maps the generic types will be automatically included
services.Configure<CustomMapsOptions>(o => o.TypesToScan = Assembly.GetExecutingAssembly().GetTypes().ToList() );

// If you add specific maps you can add the generic types by specifying
// the open generic type of the map (without arguments)
services.Configure<CustomMapsOptions>(o => o.TypesToScan = new List<Type>{ typeof(MyGenericMaps<>), ... });

// Map any type
var myGenericClassDto1 = mapper.Map<MyGenericClass<int>, MyGenericClassDto<int>>(myGenericClass1);
var myGenericClassDto2 = mapper.Map<MyGenericClass<string>, MyGenericClassDto<string>>(myGenericClass2);
var myGenericClassDto3 = mapper.Map<MyGenericClass<Product>, MyGenericClassDto<Product>>(myGenericClass3);
```

{: .important }
You can create types with as many generic parameters as you want, with the only condition that the parameters must be all present in the map definition.

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

You can also specify any [supported generic constraint](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters) (except `default` and `allows ref struct`) to specialize your generic maps.

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

services.Configure<CustomMapsOptions>(o => o.TypesToScan = new List<Type>{ typeof(MyGenericMapsClass<>), typeof(MyGenericMapsStruct<>), ... });

...

// Map with struct (or value type)
var myGenericClassDto1 = mapper.Map<MyGenericClass<int>, MyGenericClassDto<int>>(myGenericClass1);

// Map with class
var myGenericClassDto3 = mapper.Map<MyGenericClass<Product>, MyGenericClassDto<Product>>(myGenericClass3);
```

Generic types also supports [automatic collection maps](/advanced-options/collection-mapping-and-projection#automatic-collection-maps) and [matchers](/advanced-options/collection-mapping-and-projection#match-elements-in-collections) (which can be generic or explicit too).

If you specify an explicit map for two generic types this map will be used instead, this allows to define specific mapping for specific types.

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

// Specific int mapping
var myGenericClassDto1 = mapper.Map<MyGenericClass<int>, MyGenericClassDto<int>>(myGenericClass1);

// Generic mapping
var myGenericClassDto3 = mapper.Map<MyGenericClass<Product>, MyGenericClassDto<Product>>(myGenericClass3);
```