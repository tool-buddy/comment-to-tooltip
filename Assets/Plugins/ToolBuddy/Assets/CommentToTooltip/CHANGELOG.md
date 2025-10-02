# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
This project does NOT adhere to Semantic Versioning.

When updating, always remove the old version before installing the new one.

## [1.1.1] - 2025-10-02

### Changed

- Improved code structure. No visible change for the end user.

## [1.1.0] - 2025-09-30

### Added

- Assembly definitions.
- Changelog file.
- Support for serialized non-public fields.

### Fixed

- Tooltips incorrectly added to 0-arity interface methods. (#1)
- Expression-bodied properties falsely detected as fields.
- Invalid tooltips generated when comments contained characters such as `"` or `\`.
- Attribute order not preserved when replacing an existing tooltip.
- File modification does not preserve character encoding.

### Changed

- Open `ReadMe.txt` when the user clicks the Help menu item.
- Increased minimum Unity version to 2021 LTS.
- Updated folder structure to match other ToolBuddy assets.
- Changed menu item paths to harmonize with other ToolBuddy assets.
- Significantly refactored the code.

## [1.0.0] - 2015-04-29

- Initial release.