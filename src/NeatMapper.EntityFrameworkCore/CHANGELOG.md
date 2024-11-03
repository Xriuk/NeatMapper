# Changelog

## [5.0.0] - 2024-11-03

### Removed

- `EntitiesRetrievalMode.Remote` value removed.

### Changed

- `EntitiesRetrievalMode.Local` value renamed to `EntitiesRetrievalMode.LocalOnly`.
- `EntityFrameworkCoreMatcher` now matches owned types which have a primary key (those used inside collections). The key configuration must be the same for all owned type instances (excluding foreign primary keys to parent entity).
- Updated NeatMapper dependency version and corrected breaking changes (see NeatMapper changelog).
- Adjusted Dependency Injection configuration since now collection mappers of NeatMapper core package are added in PostConfiguration there's no need to check them before adding EF Core mappers.

### Added

- `AsyncEntityFrameworkCoreMapper` now supports `IAsyncEnumerable<T>`, as source or destination for new maps and as source for merge maps.

### Fixed

- Fields are now correctly supported as primary keys.
- `EntityFrameworkCoreMapper` and `AsyncEntityFrameworkCoreMapper` when mapping collections now correctly return the destination collection if source collection is null.


## [4.0.0] - 2024-07-16

### Changed

- Updated NeatMapper dependency version.


## [3.1.0] - 2024-04-26

### Changed

- Updated NeatMapper dependency version.
- `EntityFrameworkCoreMatcher` now handles shadow keys too, and now it requires a `DbContext` type and optionally a `ServiceProvider` in its constructor.
- Instead of `TaskCanceledException` which where caught and re-thrown directly by maps and mappers (instead of being wrapped in `MappingException` like the others) now `OperationCanceledException`s are caught and re-thrown, this is backwards compatible, since `TaskCanceledException` is derived from it, but now other exceptions can be caught too.
- `EntityFrameworkCoreProjector` no longer throws `MapNotFoundException` in case of a disposed `DbContext`, instead the exception is now wrapped in a `ProjectionException`.

### Added

- .NET 8.0 support.

### Fixed

- `AsyncEntityFrameworkCoreMapper` now correctly resolves the `DbContext` from an overridden `IServiceProvider` from `AsyncMapperOverrideMappingOptions` instead of `MapperOverrideMappingOptions`
- Added optional `IServiceProvider` parameter to `EntityFrameworkCoreProjector` to provide `DbContext` instances to project shadow keys during compilation
- Fixed some conditional null checks, which apparently worked even if broken somehow, and managed to pass the tests...
- `EntityFrameworkCoreMatcher` now correctly handles default values for keys (eg: allows to match an entity with a key with value 0)
- Fixed `EntityFrameworkCoreProjector`'s `DbContext` retrieval


## [2.2.0] - 2024-02-03

### Changed

- Updated NeatMapper dependency version.

### Added

- `EntityFrameworkCoreMapper` and `AsyncEntityFrameworkCoreMapper` now implement respectively `IMapperFactory` and `IAsyncMapperFactory`.
- `EntityFrameworkCoreMatcher` now implements `IMatcherFactory`.


## [2.1.0] - 2023-12-05

### Changed

- `EntityFrameworkCoreMapper` and `AsyncEntityFrameworkCoreMapper` will now map only keys to entities.
- Dependency Injection (DI) extension methods with lifetime parameters are now marked as obsolete, and will be removed in the next major version. The lifetime of the EF Core mappers/matcher/projector is now the same as the one of the corresponding core service (eg: `EntityFrameworkCoreMapper` will have the same lifetime as `IMapper` and all the other core mappers).
- Updated NeatMapper dependency version.

### Added

- `EntityFrameworkCoreProjector` which projects entities into their keys (even shadow ones), it is also used in `ProjectionMapper` and enables also mapping entities into their keys.

### Fixed

- `EntityFrameworkCoreMatcher` now correctly throws `MapNotFoundException` instead of `MatcherNotFound`.


## [2.0.0] - 2023-11-12

- Initial version (version matching NeatMapper package).