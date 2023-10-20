# Changelog

## [2.0.0] - Unreleased

### Removed

- `IMapperConfiguration`

### Changed

- 

### Added

- `IHierarchyMatchMap<TSource, TDestination>` (and its .NET 7+ static counterpart `IHierarchyMatchMapStatic<TSource, TDestination>`) which allows matching two types as well as derived types, will be automatically used when merging collections.
- `Matcher`, implementation of `IMatcher`

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