# Changelog

## [2.0.0] - Unreleased

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

- `BaseMapper` from NeatMapper.Common was split into different mappers which can be used separately or combined together (check the [README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md) for instructions). Also each mapper has a corresponding `Async` version:
   - `NewMapper` (`AsyncNewMapper`): maps only `INewMap` (`IAsyncNewMap`)
   - `MergeMapper` (`AsyncMergeMapper`): maps only `IMergeMap` (`IAsyncMergeMap`)
   - `NewCollectionMapper` (`AsyncNewCollectionMapper`): creates new collections by using another `IMapper` (`IAsyncMapper`) to map elements
   - `MergeCollectionMapper` (`AsyncMergeCollectionMapper`): merges collections by using another `IMapper` (`IAsyncMapper`) to map elements
   - `CompositeMapper` (`AsyncCompositeMapper`): combines one or more `IMapper`s of the above in a defined order and tries them all, the first one to succeeds maps the objects
- `Custom{New|Merge|Match|AsyncNew|AsyncMerge}AdditionalMapsOptions` which can be used to specify additional mapping methods like delegates/lambdas or compiled ones like expressions
- `MapperOverrideMappingOptions`, used to override `IServiceProvider` and/or `IMapper` in the `MappingContext` created by the mapper, implementation depends on the mapper itself. Also the async version is available: `AsyncMapperOverriddeMappingOptions`.
- NeatMapper.DependencyInjection package embedded, now the core package allows injecting `IMapper` (and also `IAsyncMapper`) and `IMatcher` and configuring them via `IOptions` (check the [README](https://github.com/Xriuk/NeatMapper/blob/main/src/NeatMapper/README.md) for instructions)
- `IHierarchyMatchMap<TSource, TDestination>` (and its .NET 7+ static counterpart `IHierarchyMatchMapStatic<TSource, TDestination>`) which allows matching two types as well as derived types, will be automatically used when merging collections.
- Various `IMatcher` default implementations:
   - `CustomMatcher`: matches using `IMatchMap`
   - `HierachyCustomMatcher`: matches using `IHierarchyMatchMap`
   - `EmptyMatcher`: passthrough matcher which returns false for every match
   - `SafeMatcher`: wraps another `IMatcher` and returns false in case it cannot match the given types
   - `DelegateMatcher`: matches using a custom delegate
   - `CompositeMatcher`: combines one or more `IMatcher`s of the above in a defined order and tries them all, the first one to succeeds matches the objects
- `MatcherOverrideMappingOptions`, used to override `IServiceProvider` and/or `IMatcher` in the `MatchingContext` created by the mapper, implementation depends on the matcher itself
- `AsyncCollectionMappersOptions` (and override `AsyncCollectionMappersMappingOptions`) which allows to specify settings for parallel async maps in collections

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