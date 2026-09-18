# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `TranslaasTransportException` for connect / TLS / DNS failures (`InnerException` / unwrap).

### Changed

- Connect / TLS / DNS failures are no longer mapped to `TranslaasApiException` with HTTP **400**. Timeouts remain **408**. Callers that assumed every `GetEntry` failure was `TranslaasApiException` should also handle `TranslaasTransportException` or base `TranslaasException`.
- `Translaas.Caching.File` depends on ICU4N (and its ICU data packages) for offline CLDR plural selection. Mobile / MAUI apps should account for the extra package size.

### Fixed

- Offline / file-cache `GetEntryAsync` plural selection now uses CLDR cardinal rules for the request locale (via ICU4N), matching the live API, instead of treating `1` as `One` and every other value as `Other`.
