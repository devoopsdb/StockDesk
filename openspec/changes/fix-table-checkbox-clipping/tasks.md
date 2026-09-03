## 1. Table UI Fix

- [ ] 1.1 Update the selection `CheckBox` in `MainWindow.xaml` with `Padding="0"`, `MinWidth="0"`, and `MinHeight="0"`, and verify that the solution builds cleanly without errors (`dotnet build`)
- [ ] 1.2 Verify visually and programmatically via existing/new tests that all catalog tests pass (`dotnet test`) and checkbox renders fully without horizontal clipping in unselected and selected states
