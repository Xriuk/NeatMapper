# Changelog

## [2.1.0] - Unreleased

### Changed

- `MapNotFoundException` now inherits from `Exception` instead of `ArgumentException`
- `NewCollectionMapper` and `AsyncNewCollectionMapper` constructors with `IServiceProvider`
are now marked as obsolete, since the parameter was not used, and will be removed in the next major version
- `MatcherNotFound` is now marked as obsolete since it was not used (correctly), and will be removed in the next major version
- Fixed DI creation for collection mappers and marked some public constants as obsolete, now `(Async)CompositeMapperOptions`
can be configures in any way (and not just with ConfigureAll or PostConfigureAll)

### Added

- `IProjectionMap` (and `IProjectionMapStatic` in .NET 7+), `IProjector`, `IProjectorCanProject`
interfaces to create projections between types
- Various `IProjector` default implementations:
   - `CustomProjector`: projects using `IProjectionMap`
   - `CollectionProjector`: projects collections by using another `IProjector`
   - `CompositeProjector`: combines one or more `IProjector` of the above in a defined order and tries them all,
the first one to succeeds projects the objects
- Projection interfaces and options added to Dependency Injection (DI)
- `ProjectionMapper`, an `IMapper` which uses an `IProjector` to map types by compiling and caching expressions into delegates
- `From` and `To` properties on `MapNotFoundException`
- `CompositeMapper` and `AsyncCompositeMapper` now forward new maps to merge maps if not found, by creating a destination object

### Fixed

- All the mappers now should not wrap `TaskCanceledException` but throw it directly instead
- Maps are now able to reject themselves (maybe based on their Mapping/Matching/ProjectionContext?)
by throwing `MapNotFoundException` (`MatcherNotFound` for matchers), which won't be wrapped by
the mapper/matcher/projector (it could be replaced by any parent mapper/matcher/projector with
another exception of the same type)
- Arrays, as generic maps type parameters, are now correctly recognized
- Collection mappers now throw correctly `MapNotFoundException` for their types and not element ones

## [2.0.0] - 2023-11-12

### Removed

- `Mapper` which was replaced by the separate mappers below in the **Added** section
- `CollectionMappingException` which was replaced by `MappingException` (nested)

### Changed

- Every namespace was renamed to `NeatMapper`
- `IMapper` added optional `mappingOptions` parameter to override settings for specific mappings,
support and options depends on the mappers and/or the maps, added also to extension methods
- `MergeMapsCollectionsOptions` renamed in `MergeCollectionsMappingOptions`
- NeatMapper.Common package embedded

### Added

- `BaseMapper` from NeatMapper.Common was split into different mappers which can be used separately 
or combined together (check the [README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md)
for instructions). Also each mapper has a corresponding `Async` version:
   - `NewMapper` (`AsyncNewMapper`): maps only `INewMap` (`IAsyncNewMap`)
   - `MergeMapper` (`AsyncMergeMapper`): maps only `IMergeMap` (`IAsyncMergeMap`)
   - `NewCollectionMapper` (`AsyncNewCollectionMapper`): creates new collections
by using another `IMapper` (`IAsyncMapper`) to map elements
   - `MergeCollectionMapper` (`AsyncMergeCollectionMapper`): merges collections
by using another `IMapper` (`IAsyncMapper`) to map elements
   - `CompositeMapper` (`AsyncCompositeMapper`): combines one or more `IMapper`s of the above
in a defined order and tries them all, the first one to succeeds maps the objects
- `Custom{New|Merge|Match|AsyncNew|AsyncMerge}AdditionalMapsOptions` which can be used
to specify additional mapping methods like delegates/lambdas or compiled ones like expressions
- `MapperOverrideMappingOptions`, used to override `IServiceProvider` and/or `IMapper`
in the `MappingContext` created by the mapper, implementation depends on the mapper itself.
Also the async version is available: `AsyncMapperOverriddeMappingOptions`.
- NeatMapper.DependencyInjection package embedded, now the core package allows injecting
`IMapper` (and also `IAsyncMapper`) and `IMatcher` and configuring them via `IOptions`
(check the [README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md)
for instructions)
- `IHierarchyMatchMap<TSource, TDestination>` (and its .NET 7+ static counterpart
`IHierarchyMatchMapStatic<TSource, TDestination>`) which allows matching two types
as well as derived types, will be automatically used when merging collections.
- Various `IMatcher` default implementations:
   - `CustomMatcher`: matches using `IMatchMap`
   - `HierachyCustomMatcher`: matches using `IHierarchyMatchMap`
   - `EmptyMatcher`: passthrough matcher which returns false for every match
   - `SafeMatcher`: wraps another `IMatcher` and returns false in case it cannot match the given types
   - `DelegateMatcher`: matches using a custom delegate
   - `CompositeMatcher`: combines one or more `IMatcher`s of the above in a defined order
and tries them all, the first one to succeeds matches the objects
- `MatcherOverrideMappingOptions`, used to override `IServiceProvider` and/or `IMatcher`
in the `MatchingContext` created by the mapper, implementation depends on the matcher itself
- `AsyncCollectionMappersOptions` (and override `AsyncCollectionMappersMappingOptions`)
which allows to specify settings for parallel async maps in collections

## [1.1.0] - 2023-10-02

### Deprecated

- Mapper empty constructor

### Added

- Added support for:
  - .NET Framework 4.7, 4.8
  - .NET Standard 2.1
  - .NET Core 3.1
  - .NET 5.0

## [1.0.2] - 2023-10-02

### Changed

- Previous versions deprecated

### Fixed

- Dependencies versions

## [1.0.1] - 2023-10-02

### Fixed

- Dependencies versions

## [1.0.0] - 2023-10-01

- Initial version