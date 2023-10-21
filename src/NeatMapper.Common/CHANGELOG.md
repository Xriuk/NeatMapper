# Changelog

## [2.0.0] - Unreleased

### Changed

- Every namespace was renamed to `NeatMapper`
- `IMatcher` added optional `mappingOptions` parameter to override settings for specific matches,
support and options depends on the mappers and/or the maps, added also to extension methods
- `MergeMapsCollectionsOptions` renamed in `MergeCollectionsMappingOptions`

### Added

- `IHierarchyMatchMap<TSource, TDestination>` (and its .NET 7+ static counterpart `IHierarchyMatchMapStatic<TSource, TDestination>`) which allows matching two types as well as derived types, will be automatically used when merging collections.
- `Matcher`, public implementation of `IMatcher`
- `CompositeMatcher`: combines one or more `IMatcher`s of the above in a defined order and tries them all, the first one to succeeds matches the objects
- `CustomMatchAdditionalMapsOptions` which can be used to specify additional matching methods like delegates/lambdas or compiled ones like expressions

## [1.1.0] - 2023-10-02

### Added

- Added support for:
  - .NET Framework 4.7, 4.8
  - .NET Standard 2.1 (not tested)
  - .NET Core 3.1
  - .NET 5.0

## [1.0.1] - 2023-10-02

### Changed

- Previous versions deprecated

### Fixed

- Configuration fixes

## [1.0.0] - 2023-10-01

- Initial version