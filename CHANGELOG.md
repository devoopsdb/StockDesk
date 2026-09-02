# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Fixed
- Fixed bug where entered initial product quantity in `ProductDialog` reverted to default (1) upon clicking save.
- Fixed bug where entered replenishment quantity in `ReplenishDialog` reverted to default (1) upon confirming inflow.
- Fixed quantity binding and focus handling in `WriteOffDialog` to prevent entered write-off quantities from falling back to default values.
