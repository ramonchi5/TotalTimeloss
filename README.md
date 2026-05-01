# LiveSplit.TotalTimeloss

TotalTimeloss is a LiveSplit component that displays Sum of Best, Total Timeloss, and Best Possible Time in a compact three-column layout.

## Features

- Editable labels for SoB, delta, and BPT.
- Optional label row with configurable label/time distance.
- Per-label and per-time colors, with optional layout-color overrides.
- Per-label and per-time horizontal movement controls.
- Optional underlined labels, including optional underlines for spaces.
- Optional visibility for each time value, so the component can show any combination of SoB, delta, and BPT.
- Configurable accuracy, background, and instance name for layouts with multiple TotalTimeloss components.

## Build

Build the solution in Release mode:

```powershell
dotnet build .\TotalTimeloss.sln -c Release
```

The component DLL is produced at:

```text
src\TotalTimeloss\bin\Release\net481\LiveSplit.TotalTimeloss.dll
```

When this repository is cloned next to a `LiveSplit` repository, the build also copies the DLL and PDB to:

```text
..\LiveSplit\artifacts\bin\TotalTimeloss\release\
```

Set `CopyToLiveSplitArtifacts=false` to disable that copy.

## Install

Copy `LiveSplit.TotalTimeloss.dll` into LiveSplit's `Components` folder, then add `Total Timeloss` from LiveSplit's layout editor.

## License

MIT. See [LICENSE](LICENSE).
