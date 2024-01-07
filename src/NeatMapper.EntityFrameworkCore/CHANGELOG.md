# Changelog

## [2.2.0] - Unreleased

### Changed

- Updated NeatMapper dependency version

## [2.1.0] - 2023-12-05

### Changed

- `EntityFrameworkCoreMapper` and `AsyncEntityFrameworkCoreMapper` will now map only keys to entities
- Dependency Injection (DI) extension methods with lifetime parameters are now marked as obsolete, 
and will be removed in the next major version. The lifetime of the EF Core mappers/matcher/projector
is now the same as the one of the corresponding core service (eg: `EntityFrameworkCoreMapper` will have
the same lifetime as `IMapper` and all the other core mappers)
- Updated NeatMapper dependency version

### Added

- `EntityFrameworkCoreProjector` which projects entities into their keys (even shadow ones),
it is also used in `ProjectionMapper` and enables also mapping entities into their keys

### Fixed

- `EntityFrameworkCoreMatcher` now correctly throws `MapNotFoundException` instead of `MatcherNotFound`

## [2.0.0] - 2023-11-12

- Initial version (version matching NeatMapper package)