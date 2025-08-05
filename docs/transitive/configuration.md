---
layout: default
title: "Configuration"
nav_order: 2
parent: "Transitive"
---

# TransitiveOptions

{: .important }
Can be overridden during mapping with `TransitiveMappingOptions`.

## Max chain length

You can limit the maximum length of the chain of maps. If you specify a length and no map shorter or equal is found the types cannot be mapped.

```csharp
services.Configure<TransitiveOptions>(o => o.MaxChainLength = 5);

...

Product product = ...;

mapper.Map<ProductDto2>(product, new object[]{
	new TransitiveMappingOptions { MaxChainLength = 5 }
});
```