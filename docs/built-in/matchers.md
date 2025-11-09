---
layout: default
title: "Matchers"
nav_order: 2
parent: "Built-in"
---

# CustomMatcher

Maps objects by using `IMatchMap<TSource, TDestination>` (and its static counterpart) and any [additional map](/advanced-options/configuration#custommaptypeadditionalmapsoptions).
Also supports [conditional matching](/customization/conditional-mapping).

```csharp
// Implement the interface(s)
public class MyMaps :
	IMatchMap<Product, ProductDto>
{
	public bool Match(Product? source, ProductDto? destination, MatchingContext context){
		return source?.Id == destination?.Id;
	}
}

...

// Create the matcher and retrieve the maps from the given types
var matcher = new CustomMatcher(new CustomMapsOptions{ Types = [ typeof(MyMaps) ] });

...

// Match two objects
if(matcher.Match(myProduct, myProductDto)){
	...
}
```

# HierarchyCustomMatcher

Maps objects by using `IHierarchyMatchMap<TSource, TDestination>` (and its static counterpart) and any [additional map](/advanced-options/configuration#custommaptypeadditionalmapsoptions).
Also supports [conditional matching](/customization/conditional-mapping).

```csharp
// Implement the interface(s)
public class MyMaps :
	IHierarchyMatchMap<Product, ProductDto>
{
	public bool Match(Product? source, ProductDto? destination, MatchingContext context){
		return source?.Id == destination?.Id;
	}
}

...

// Create the matcher and retrieve the maps from the given types
var matcher = new HierarchyCustomMatcher(new CustomMapsOptions{ Types = [ typeof(MyMaps) ] });

...

// Match two objects (same type or derived/parent)
if(matcher.Match<Product, ProductDto>(myProduct, myProductDto)){
	...
}
if(matcher.Match<LimitedProduct, ProductDto>(myLimitedProduct, myProductDto)){
	...
}
if(matcher.Match<Product, LimitedProductDto>(myProduct, myLimitedProductDto)){
	...
}
```

# CollectionMatcher

Matchee collections by using another `IMatcher` to match elements. Different matching options are available, defined in `CollectionMatchersOptions`.
See also [CollectionMatchersOptions](/advanced-options/configuration#collectionmatchersoptions).

```csharp
// Create the matcher
var matcher = new CollectionMatcher(new CustomMatcher(...)); // Use any matcher

...

// Match two collections
IEnumerable<Product> myProducts = ...;
IList<ProductDto> myProductDtos = ...;
if(matcher.Match(myProducts, myProductDtos)){
	...
}
```

# CompositeMatcher

Delegates matching to other `IMatcher`s. All the matchers are tried in the provided order.

```csharp
// Create the matcher
var matcher = new CompositeMatcher(
	new CustomMatcherr(...),
	new CollectionMatcher(new CustomMatcher(...)),
	...
);

...

// Match objects
if(matcher.Match(myProduct, myProductDto)){
	...
}
if(matcher.Match(myProducts, myProductDtos)){
	...
}
```

# EquatableMatcher

Matches classes implementing `IEquatable<T>`. The types are matched in the provided order: source type is checked for the interface.
Singleton matcher.

```csharp
if(EqualityOperatorsMatcher.Instance.Match<TimeOnly, TimeOnly>(myTime1, myTime2)){
	...
}
```

# StructuralEquatableMatcher

Matches classes implementing `IStructuralEquatable`. Objects need to be of the same type (or derived).
Singleton matcher.

```csharp
// Match two tuples
if(StructuralEquatableMatcher.Instance.Match<(int, bool), (int, bool)>((1, true), (1, true))){
	...
}
```

# ObjectEqualsMatcher

Matches by invoking `Object.Equals(object, object)` (and overloads).
Singleton matcher.

```csharp
if(ObjectEqualsMatcher.Instance.Match<TimeOnly, TimeOnly>(myTime1, myTime2)){
	...
}
```

# EqualityOperatorsMatcher (.NET 7+)

Matches classes implementing `IEqualityOperators<TSelf, TOther, bool>`. The types are matched in the provided order: source type is checked for the interface.
Singleton matcher.

```csharp
if(EqualityOperatorsMatcher.Instance.Match<TimeOnly, TimeOnly>(myTime1, myTime2)){
	...
}
```

# NullableMatcher

Matches `Nullable<T>` and its underlying types by using another `IMatcher`.

```csharp
// Implement the interface(s)
public class MyMaps :
	IMatchMap<DateTime, string>
{
	public bool Match(DateTime source, string? destination, MatchingContext context){
		return source.ToString("O") == destination;
	}
}

...

// Create the matcher and retrieve the maps from the given types
var matcher = new NullableMatcher(new CustomMatcher(new CustomMapsOptions{ Types = [ typeof(MyMaps) ] }));

...

// Match two objects (they all return true)
if(matcher.Match<DateTime, string>(new DateTime(2025, 08, 31, 19, 21, 16, DateTimeKind.Utc), "2025-08-31T19:21:16Z")){
	...
}
if(matcher.Match<DateTime?, string>(new DateTime(2025, 08, 31, 19, 21, 16, DateTimeKind.Utc), "2025-08-31T19:21:16Z")){
	...
}
if(matcher.Match<DateTime?, string>(null, null)){
	...
}
```

# DelegateMatcher

Matches by invoking a delegate (used to define custom matchers for collections for example).

```csharp
// Create the matcher
var matcher = DelegateMatcher.Create<Product, ProductDto>((s, d, c) => s?.Id == d?.Id);

...

// Match objects
if(matcher.Match(myProduct, myProductDto)){
	...
}
```

# EqualityComparerMatcher

Matches by invoking an `IEqualityComparer<T>`.

```csharp
// Create the matcher
var matcher = EqualityComparerMatcher.Create<Product>(myComparer);

...

// Match objects
if(matcher.Match<Product, Product>(myProduct1, myProduct2)){
	...
}
```