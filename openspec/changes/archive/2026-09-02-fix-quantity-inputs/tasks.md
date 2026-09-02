## 1. Unit and ViewModel Tests

- [x] 1.1 Add unit tests for `ProductDialogViewModel`, `ReplenishDialogViewModel`, and `WriteOffDialogViewModel` verifying custom quantities (>1) are passed to services and verify `dotnet test` executes them.
- [x] 1.2 Add UI/WPF binding tests verifying `NumberBox.Value` two-way synchronization with integer ViewModel properties.

## 2. Dialog XAML and Code-Behind Fixes

- [x] 2.1 Update `ProductDialog.xaml` with `UpdateSourceTrigger=PropertyChanged` on `NumberBox.Value` and update `ProductDialog.xaml.cs` to force defocus before `SaveAsync`.
- [x] 2.2 Update `ReplenishDialog.xaml` with `UpdateSourceTrigger=PropertyChanged` on `NumberBox.Value` and update `ReplenishDialog.xaml.cs` to force defocus before `ConfirmReplenishCommand`.
- [x] 2.3 Update `WriteOffDialog.xaml` with `UpdateSourceTrigger=PropertyChanged` on `NumberBox.Value` and update `WriteOffDialog.xaml.cs` to force defocus before `ConfirmWriteOffCommand`.

## 3. Verification & Build

- [x] 3.1 Run `dotnet build` and `dotnet test` to verify zero compilation errors and all tests passing.
- [x] 3.2 Update `CHANGELOG.md` with bugfix entries under `## [Unreleased]`.
