# Changelog

## [6.1.0] - Unreleased

### Changed

- Updated NeatMapper dependency version.


## [6.0.0] - 2025-08-05

### Removed

- .NET Framework 4.7 support.

### Changed

- Updated NeatMapper dependency version.

### Added

- .NET 9.0 support.


## [5.4.0] - 2025-05-31

### Changed

- Updated NeatMapper dependency version.


## [5.3.0] - 2025-05-24

### Changed

- Updated NeatMapper dependency version.


## [5.2.0] - 2025-05-19

### Changed

- Updated NeatMapper dependency version.

### Fixed

- Dependency Injection (DI) now uses `IOptionsMonitor<T>` instead of `IOptionsSnapshot<T>` to allow instantiating services for all the lifetime options.


## [5.1.0] - 2025-05-18

### Changed

- Updated NeatMapper dependency version.

### Fixed

- Added internal null checks.
- `params object[]` nullability in extensions overloads.


## [5.0.0] - 2024-11-03

- Initial version (version matching NeatMapper package)