# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- `TranslaasTransportException` for connect / TLS / DNS failures (`InnerException` / unwrap).

### Changed

- Connect / TLS / DNS failures are no longer mapped to `TranslaasApiException` with HTTP **400**. Timeouts remain **408**. Callers that assumed every `GetEntry` failure was `TranslaasApiException` should also handle `TranslaasTransportException` or base `TranslaasException`.
