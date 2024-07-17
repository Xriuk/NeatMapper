# Changelog

## [4.0.0] - 2024-07-16

### Changed

- `IAsyncNewMapFactory` and `IAsyncMergeMapFactory`'s `Invoke(...)` method can now receive a `CancellationToken` parameter, which was moved from the factory creation to invocation, to better handle cancellability of parallel tasks in case of exceptions.
- Generic versions of `INewMapFactory`, `IMergeMapFactory`, `IMatchMapFactory`, `IAsyncNewMapFactory` and `IAsyncMergeMapFactory` have been converted to classes (same names without the "I") to explicitly implement `Invoke` methods of non-generic parent interfaces, this was done to hide the parent method in the generic implementation, because it could have caused mistakes as it would allow to pass any variables in any order, which is not type-safe.
- `AsyncMappingContext` was converted to a value type to reduce allocations when forwarding the `CancellationToken` parameter while invoking async factories, this allows to separate the provided async context (which can be cached if needed) from the cancellation token (which could change between mappings).
- `MappingOptions` are now not cached by default, but require to be initialized with cached = true, in this case they will be cached (and all of their variations) in the mapping pipeline where supported.
- `CompositeMatcher` constructor changed signature to accept a single parameter of type `CompositeMatcherOptions`.
- `CompositeMatcher` now matches the given types in any order, the exact one is tried first, then the types are reverted. This behaviour can be configured with `CompositeMatcherOptions` (and `CompositeMatcherMappingOptions`).

### Added

- Analyzers and code fixers to detect when `CancellationToken` from `AsyncMappingContext` is not forwarded to async methods, works like [CA2016](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca2016).
- Added overloads with `params object[]` mapping options for:
   - `IMapper` extensions (`Map`, `CanMapNew`, `CanMapMerge`, `MapNewFactory` and `MapMergeFactory`).
   - `IAsyncMapper` extensions (`MapAsyncNewFactory` and `MapAsyncMergeFactory`).
   - `IMatcher` extensions (`Match`, `CanMatch`, and `MatchFactory`).
- `IMatcher` extension method `Predicate`, which allows to compare a single provided value of a given type with other values of a different type repeatedly by returning a predicate (`Func<T, bool>`) which can be used in Linq methods like `Where`, `First`, `Count`, ...
- Implicit conversions from generic factories (`NewMapFactory`, `MergeMapFactory`, `MatchMapFactory`, `AsyncNewMapFactory` and `AsyncMergeMapFactory`) to corresponding delegates (`Func<...>`).
- `IAsyncEnumerable<T>` support for `AsyncNewCollectionMapper` for both source and destination and for `AsyncMergeCollectionMapper` only for source.
- AsyncMappers `IAsyncEnumerable` extension methods `Project`, which creates a lazy projection (just like Linq.Async method `Select()`).
- `IMapper` extension method `MapMergeFactory`, and `IAsyncMapper` extension method `MapAsyncMergeFactory` to create factories for collections.

### Fixed

- `IProjector` extensions overloads with `params object[]`, nullability is now correct.
- Generic overload of `IMapper`'s extensions `MapMergeFactory` now correctly returns a generic factory.


## [3.1.0] - 2024-04-26

### Added

- .NET 8.0 support.

### Fixed

- Dependency Injection (DI) now uses `IOptionSnapshot` instead of `IOptions` to respect different lifetimes of mappers/matchers/projectors, previously `IOptions` forced Singleton instead of the specified lifetime.
- `CanMap*`/`CanMatch`/`CanProject` extension methods now wrap exceptions thrown by the mapper/matcher/projector, while invoking the corresponding `Map*`/`Match`/`Project` methods. An `InvalidOperationException` with the inner exception wrapped will be thrown instead, signaling that the mapper/matcher/projector cannot determine if the two types are supported.
- Instead of `TaskCanceledException` which were caught and re-thrown directly by maps and mappers (instead of being wrapped in `MappingException` like the others) now `OperationCanceledException` are caught and re-thrown, this is backwards compatible, since `TaskCanceledException` is derived from it, but now other exceptions can be caught too.


## [3.0.0] - 2024-03-28

### Removed

- Removed classes and methods previously marked as Obsolete.
- `FactoryContext` was removed, all maps should assume they will be included in factories and should optimize themselves.

### Changed

- `IMapperFactory`, `IAsyncMapperFactory` and `IMatcherFactory` interfaces were changed to return custom factory interfaces instead of plain `Delegate`s, this was done to allow factories to dispose unmanaged resources via `IDisposable`. All core classes (including extension methods) are now updated, and implementers must do the same.
- (Async)Mappers/matchers/projectors are now 40 to 90% faster as they cache various data, from maps to `MappingOptions` and others.
- `AsyncCompositeMapper`/`CompositeMapper`'s `MapInternal` methods visibility has been changed from public (mistakenly) to private.
- All interfaces (async and not) now require their implementing classes to be thread-safe (including returned factories), core types were adjusted to be compliant.
- All options passed as `MappingOptions` are now required to be (and assumed to be) immutable, for performance reasons.
- Improved performances for nested (async)mappings/matchings, nested (async)mappers/matchers will be created lazily only if a nested map is performed.

### Fixed

- `MappingContext` nested Mapper creation fixes.
- Custom maps now correctly throw `MapNotFoundException` if the types matches the current mapped types or wraps it in `MappingException`/`MatcherException`/`ProjectionException`.
- Fixed various memory leaks, especially in AsyncMappers (whooops).
- Composite (Async)Mapper/Matcher will now correctly fallback to the next mapper/matcher in the list if the factory returned by one of the previous ones fails after being invoked.
- Added/improved docs.


## [2.2.0] - 2024-02-03

### Changed

- `MappingContext`/`MatchingContext` now automatically enforce `NestedMappingContext`/`NestedMatchingContext` on nested maps and has now an additional constructor which allows to specify the parent mapper/matcher too. It also allows automatic optimization for nested maps. It also automatically forwards `FactoryContext` if present.
- `DelegateMatcher.CanMatch()` is now marked as obsolete, and will be removed in the next major version.
- `DelegateMatcher` should now throw `MapNotFoundException` when matching two incompatible types.

### Added

- Projectors/Mappers `IQueryable`/`IEnumerable` extension methods `Project`, which creates a lazy projection (by using `Select()`).
- New optional interfaces `IMapperFactory`/`IAsyncMapperFactory`/`IMatcherFactory` which allows to create mapping/matching factories instead of mapping/matching directly, to map/match multiple elements of two given types faster.
- Extension methods `MapNewFactory` and `MapMergeFactory` for any `IMapper` which will fallback to default `Map()` if `IMapperFactory` is not implemented.
- New options `FactoryContext` which allows to discover if a given map will be part of a factory.
- All core mappers now implement `IMapperFactory`.
- Extension methods `MapAsyncNewFactory` and `MapAsyncMergeFactory` for any `IAsyncMapper` which will fallback to default `MapAsync()` if `IAsyncMapperFactory` is not implemented.
- All async mappers now implement `IAsyncMapperFactory`.
- Extension method `MatchFactory` for any `IMatcher` which will fallback to default `Match()` if `IMatcherFactory` is not implemented.
- All matchers now implement `IMatcherFactory`.


## [2.1.0] - 2023-12-05

### Changed

- `MapNotFoundException` now inherits from `Exception` instead of `ArgumentException`.
- `NewCollectionMapper` and `AsyncNewCollectionMapper` constructors with `IServiceProvider` are now marked as obsolete, since the parameter was not used, and will be removed in the next major version.
- `MatcherNotFound` is now marked as obsolete since it was not used (correctly), and will be removed in the next major version.
- Fixed DI creation for collection mappers and marked some public constants as obsolete, now `(Async)CompositeMapperOptions` can be configured in any way (and not just with `ConfigureAll` or `PostConfigureAll`).

### Added

- `IProjectionMap` (and `IProjectionMapStatic` in .NET 7+), `IProjector`, `IProjectorCanProject` interfaces to create projections (expressions) between types.
- Various `IProjector` default implementations:
   - `CustomProjector`: projects using `IProjectionMap`.
   - `CollectionProjector`: projects collections by using another `IProjector`.
   - `CompositeProjector`: combines one or more `IProjector` of the above in a defined order and tries them all, the first one to succeeds projects the objects.
- Projection interfaces and options added to Dependency Injection (DI).
- `ProjectionMapper`, an `IMapper` which uses an `IProjector` to map types by compiling and caching expressions into delegates.
- `From` and `To` properties on `MapNotFoundException`.
- `CompositeMapper` and `AsyncCompositeMapper` now forward new maps to merge maps if not found, by creating a destination object.

### Fixed

- All the mappers now should not wrap `TaskCanceledException` but throw it directly instead.
- Maps are now able to reject themselves (maybe based on their Mapping/Matching/ProjectionContext?) by throwing `MapNotFoundException`, which won't be wrapped by the mapper/matcher/projector (it could be replaced by any parent mapper/matcher/projector with another exception of the same type).
- Arrays, as generic maps type parameters, are now correctly recognized.
- Collection mappers now throw correctly `MapNotFoundException` for their collection types and not element ones.


## [2.0.0] - 2023-11-12

### Removed

- `Mapper` which was replaced by the separate mappers below in the **Added** section.
- `CollectionMappingException` which was replaced by `MappingException` (nested).

### Changed

- Every namespace was renamed to `NeatMapper`.
- `IMapper` added optional `mappingOptions` parameter to override settings for specific mappings, support and options depends on the mappers and/or the maps, added also to extension methods.
- `MergeMapsCollectionsOptions` renamed in `MergeCollectionsMappingOptions`.
- NeatMapper.Common package embedded.

### Added

- `BaseMapper` from NeatMapper.Common was split into different mappers which can be used separately or combined together (check the [README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md) for instructions). Also each mapper has a corresponding `Async` version:
   - `NewMapper` (`AsyncNewMapper`): maps only `INewMap` (`IAsyncNewMap`).
   - `MergeMapper` (`AsyncMergeMapper`): maps only `IMergeMap` (`IAsyncMergeMap`).
   - `NewCollectionMapper` (`AsyncNewCollectionMapper`): creates new collections by using another `IMapper` (`IAsyncMapper`) to map elements.
   - `MergeCollectionMapper` (`AsyncMergeCollectionMapper`): merges collections by using another `IMapper` (`IAsyncMapper`) to map elements.
   - `CompositeMapper` (`AsyncCompositeMapper`): combines one or more `IMapper`s of the above in a defined order and tries them all, the first one to succeeds maps the objects.
- `Custom{New|Merge|Match|AsyncNew|AsyncMerge}AdditionalMapsOptions` which can be used to specify additional mapping methods like delegates/lambdas or compiled ones like expressions.
- `MapperOverrideMappingOptions`, used to override `IServiceProvider` and/or `IMapper` in the `MappingContext` created by the mapper, implementation depends on the mapper itself. Also the async version is available: `AsyncMapperOverriddeMappingOptions`.
- NeatMapper.DependencyInjection package embedded, now the core package allows injecting `IMapper` (and also `IAsyncMapper`) and `IMatcher` and configuring them via `IOptions` (check the [README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md) for instructions).
- `IHierarchyMatchMap<TSource, TDestination>` (and its .NET 7+ static counterpart `IHierarchyMatchMapStatic<TSource, TDestination>`) which allows matching two types as well as derived types, will be automatically used when merging collections.
- Various `IMatcher` default implementations:
   - `CustomMatcher`: matches using `IMatchMap`.
   - `HierachyCustomMatcher`: matches using `IHierarchyMatchMap`.
   - `EmptyMatcher`: passthrough matcher which returns false for every match.
   - `SafeMatcher`: wraps another `IMatcher` and returns false in case it cannot match the given types.
   - `DelegateMatcher`: matches using a custom delegate.
   - `CompositeMatcher`: combines one or more `IMatcher`s of the above in a defined order and tries them all, the first one to succeeds matches the objects.
- `MatcherOverrideMappingOptions`, used to override `IServiceProvider` and/or `IMatcher` in the `MatchingContext` created by the mapper, implementation depends on the matcher itself.
- `AsyncCollectionMappersOptions` (and override `AsyncCollectionMappersMappingOptions`) which allows to specify settings for parallel async maps in collections.


## [1.1.0] - 2023-10-02

### Deprecated

- Mapper empty constructor.

### Added

- Support for:
  - .NET Framework 4.7, 4.8
  - .NET Standard 2.1
  - .NET Core 3.1
  - .NET 5.0


## [1.0.2] - 2023-10-02

### Changed

- Previous versions deprecated.

### Fixed

- Dependencies versions.


## [1.0.1] - 2023-10-02

### Fixed

- Dependencies versions.


## [1.0.0] - 2023-10-01

- Initial version.
