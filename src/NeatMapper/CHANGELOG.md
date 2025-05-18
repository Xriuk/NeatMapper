# Changelog

## [5.1.0] - 2025-05-18

### Changed

- Deprecated `IMatcher` and `IMatchMapFactory` `Predicate()` extension methods with destination to avoid ambiguities with the same types. New extension methods `PredicateDestination()` should be used instead.
- Deprecated `(Async)EmptyMapper` `I(Async)MapperFactory` implementation in favor of the corresponding `I(Async)Mapper` extension methods which now check for `CanMap(Async)*()` methods and throw accordingly.
- Typed implementations of factories (`NewMapFactory<TSource, TDestination>`, ...) now have type members as virtual instead of abstract, and correspond to the type arguments by default where available.

### Added

- New optional interfaces `ICanMapNew`, `ICanMapMerge`, `ICanMapAsyncNew`, `ICanMapAsyncMerge`, `ICanMatch`, `ICanMatchHierarchy` and `ICanProject` (and their static counterparts) which can be implemented to validate the corresponding custom maps, this should be especially useful for generic custom maps, because it allows to validate the generic type arguments (as a map can no longer reject itself by throwing `MapNotFoundException`, which now gets wrapped in a `MappingException`/`MatchingException`/`ProjectionException`).
- `IMatcher` and `IMatchMapFactory` `PredicateDestination()` extension methods to create `Predicate()` factories by passing the destination instead of source and inferring its type.
- `ObjectEqualsMatcher` now implements `IMatcherFactory`.
- `CanProject` method to `NestedProjector` (which can be used outside expressions unlike `Project`), which just forwards the calls to the underlying `IProjector`.
- `CollectionMatcher` which allows matching collections by matching elements inside, supports ordered matches and not.
- `MergeCollectionsOptions` `RecreateReadonlyDestination` which allows merge mapping to readonly collections (normal and async) by recreating them, all the merged elements will be copied from the original destination and new ones added.
- `NullableMapper`/`AsyncNullableMapper`/`NullableMatcher` which allows mapping/matching `Nullable<T>` types by mapping/matching underlying types.
- `NestedMappingContext`/`AsyncNestedMappingContext`/`NestedMatchingContext`/`NestedProjectionContext` method `CheckRecursive` which allows to check if the context itself or any of its parent contexts matches a given predicate.
- `AsyncIMapperWrapperMapper` which allows wrapping an `IMapper` for async calls.

### Fixed

- Collection mappers for `string`s (which use `StringBuilder` as their backing collection) now handle correctly `null` chars which result in adding the null char (`'\0'`).
- `ObjectEqualsMatcher` source and destination typechecking.
- `AsyncCollectionMappersOptions`, `AsyncCompositeMapperOptions`, `CompositeMapperOptions`, `CompositeProjectorOptions` and `MergeCollectionsOptions` copy constructors added null checks.
- `TypeConverterMapper` `IMapperFactory` missing implementation.
- `AsyncCollectionMapper` mapper removed parallel mentions from merge maps docs, as it is not supported.
- Source/destination type checks for `EqualityComparerMatcher` and `EqualityOperatorsMatcher`.
- `EquatableMatcher`, `EqualityComparerMatcher` and `EqualityOperatorsMatcher` exceptions handling.
- Mappers and matchers added type checks for non-generic factories.
- Internal code cleanup


## [5.0.0] - 2024-11-03

### Removed

- `IServiceProvider` parameter from `MergeCollectionMapper` and `AsyncMergeCollectionMapper` constructors.
- Some ambiguous `params object[]` overloads for `IMapper` `Map` extension methods.

### Changed

- Internal optimizations and pooling have allowed an additional increase in performance of more than 50%.
- New and Merge mappers (normal, collection and async ones) were merged together into a custom one.
- `IMapperCanMap`/`IAsyncMapperCanMap`/`IMatcherCanMatch`/`IProjectorCanProject` interfaces were integrated into their parent interfaces (`IMapper`/`IAsyncMapper`/`IMatcher`/`IProjector`) and removed, all implementing classes and extension methods were adjusted.
- `IAsyncMapper` `CanMapAsync*` methods now return `bool` instead of `Task<bool>` and the `CancellationToken` parameter has been removed.
- `CanMap*`/`CanMapAsync*`/`CanMatch`/`CanProject` methods now should never throw, if an interface type can be mapped true will be returned, and if the object provided does not respect expectations (eg: passing a readonly collection) a mapping exception will be thrown.
- `DelegateMatcher` constructor was made private and it now can be created only via the type-safe static method `Create`.
- `MergeCollectionsMappingOptions` Matcher was changed from `MatchMapDelegate` to the safer `IMatcher` (which can also be a `CompositeMatcher` to support multiple maps). `IMapper`/`IAsyncMapper` extensions were updated.
- Extension method `Predicate` for `IMatchMapFactory`/`MatchMapFactory<TSource, TDestination>` now have an optional parameter which allows to dispose the provided factory together with the returned one (or in case of exceptions), this allows to create factories from other factories directly. The parameter is true by default, meaning that the provided factory will be disposed.
- Improved and cleanup extension method `Predicate` for `IMatcher`.
- Extension method `Predicate` for `IMatcher`/`IMatchMapFactory`/`MatchMapFactory<TSource, TDestination>` now return `IPredicateFactory`/`PredicateFactory<T>`.
- `CompositeMapper` now returns both new factories and merge factories for `MapNewFactory()` (like `CompositeMatcher` does for exact and reverse maps), the same applies to `AsyncCompositeMapper` too.
- Refactored internal reflection usage to create and cache delegates instead of direct invocations for increased performance.
- All delegates and interfaces (where applicable) now have the correct co/contra-variance specified on their parameters.
- One-liner extension methods should now be inlined.
- `AsyncCollectionMappersMappingOptions` `MaxParallelMappings` are now also supported for `IAsyncEnumerable` too.
- `AsyncMergeCollectionMapper` does not support anymore `AsyncCollectionMappersMappingOptions` `MaxParallelMappings`, constructor has been updated to remove the parameter.
- `MapNotFoundException` can no longer be thrown from mapper/maps on its own, a map cannot refuse itself based on provided objects.
- `CompositeMapper` and `AsyncCompositeMapper` will not try following mappers anymore on `MapNotFoundException`s.
- `CompositeMapperOptions` collection mapper has been moved to `PostConfigure()`.
- `AsyncCompositeMapperOptions` collection mapper has been moved to `PostConfigure()`.
- `CompositeProjectorOptions` collection projector has been moved to `PostConfigure()`.

### Added

- New optional interfaces `IMapperMaps`, `IAsyncMapperMaps` and `IProjectorMaps` which allows discovering types which can be mapped by a given `IMapper`/`IAsyncMapper`/`IProjector`.
- Extension methods `GetNewMaps` and `GetMergeMaps` for any `IMapper`, `GetAsyncNewMaps` and `GetAsyncMergeMaps` for any `IAsyncMapper` and `GetMaps` for any `IProjector` which will fallback to default `Enumerable.Empty<T>` if the corresponding interface (`IMapperMaps`/`IAsyncMapperMaps`/`IProjectorMaps`) is not implemented.
- `IMapper`/`IAsyncMapper` `Map`/`MapAsync` and `MapMergeFactory`/`MapAsyncMergeFactory` extension methods added overloads with `IEqualityComparer` parameter used as matcher when merging collections.
- All the optional interfaces (`IAsyncMapperFactory`, ...) are now also provided as services via DI.
- `EqualityComparerMatcher` which allows using an `IEqualityComparer` to match two elements of the same type.
- `MappingOptions` extension method `AddMergeCollectionMatchers` which allows to Set/Add `IMatcher`s to be used in merge maps.
- `AsyncEnumerableExtensions`.`Project`'s `MappingOptions` `params object[]` parameter overload.
- `IdentityMapper` and `AsyncIdentityMapper` which always return the passed source (for both new and merge maps, provided that the mapped types are the same). Not added to composite mappers by default.
- `IMatchMapFactory` overloads to extension method `Predicate`.
- `MapNewFactory` extension methods for `IMergeMapFactory`/`MergeMapFactory<TSource, TDestination>` and its async counterpart `MapAsyncNewFactory` for `IAsyncMergeMapFactory`/`AsyncMergeMapFactory<TSource, TDestination>`, which allows to create a new factory from a merge factory by creating destination objects.
- `IPredicateFactory` and `PredicateFactory<T>` types which replaces `NewMapFactory<T, bool>`.
- `params object[]` constructor overload for `MappingOptions`.
- `EquatableMatcher` which allows matching types implementing `IEquatable<T>`.
- `ObjectEqualsMatcher` as last matcher (via `PostConfigure()` of `CompositeMatcherOptions`) which allows matching via `object.Equals()`.
- `EqualityOperatorsMatcher` (.NET 7+) which allows matching types implementing `IEqualityOperators<TSelf, TOther, TResult>` (equality operator `==`).
- `NewMapFactory<TSource, TDestination>` implicit conversion to `Converter<TInput,TOutput>` delegate.
- `TypeConverterMapper` which allows mapping types via `TypeConverter`s.
- `ConvertibleMapper` which allows mapping types implementing `IConvertible`.

### Fixed

- Disposable pattern implementation is now idempotent, meaning it can be disposed multiple times safely.
- Some concurrent locks in `CompositeMapper`/`AsyncCompositeMapper` factories.
- `AsyncCompositeMapper` missing `IAsyncMapperFactory` interface.
- `Project` `IQueryable<T>` extension method `params object[]` overload nullability.
- `MergeCollectionMapper` and `AsyncMergeCollectionMapper` now correctly return the destination collection if source collection is null.


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
- `IMatcher` and `MatchMapFactory<TSource, TDestination>` extension method `Predicate`, which allows to compare a single provided value of a given type with other values of a different type repeatedly by returning a predicate (`Func<T, bool>`) which can be used in Linq methods like `Where`, `First`, `Count`, ...
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
