# LiveSplit.TotalTimeloss

TotalTimeloss is a LiveSplit component that displays Sum of Best, Total Timeloss, and Best Possible Time in a compact three-column layout.

## Features

- Editable labels for SoB, delta, and BPT.
- Optional label row with configurable label/time distance.
- Per-label and per-time colors, with optional layout-color overrides.
- Per-label and per-time horizontal movement controls.
- Optional underlined labels, including optional underlines for spaces.
- Optional visibility for each time value, so the component can show any combination of SoB, delta, and BPT.
- Configurable accuracy, optional disabled-by-default 2- or 3-color backgrounds, rounded all/top/bottom corners, and instance name for layouts with multiple TotalTimeloss components.

## How It Works

The component reads the active run from LiveSplit on each update and calculates:

- **Sum of Best** from LiveSplit's best segment data.
- **Total Timeloss** as the difference between the runner's personal best and Sum of Best when both values are available.
- **Best Possible Time** from LiveSplit's current best-possible-time calculation.

The settings UI controls labels, colors, time visibility, text spacing, background styling, and accuracy. Layout instances can be named so multiple TotalTimeloss components are easier to identify in the LiveSplit layout editor.

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

## Release Package

For a normal GitHub release, upload `LiveSplit.TotalTimeloss.dll`. The `packages` folder is only for compile-time references and should not be included in the release package. The PDB is only useful for debugging.

## Development Notes

Important files:

| File | Purpose |
|---|---|
| `src/TotalTimeloss/UI/Components/TotalTimeloss.cs` | Component entry point, value calculation, layout, and drawing. |
| `src/TotalTimeloss/UI/Components/TotalTimelossSettings.cs` | Settings UI, XML persistence, and color/font/background options. |
| `src/TotalTimeloss/UI/Components/TotalTimelossFactory.cs` | LiveSplit component registration. |
| `packages/` | Compile-time references only. |

Before publishing a build, test a run with an available PB, a run with missing/empty comparisons, each time-value visibility toggle, label row on/off, background modes, and all accuracy options.

## License

TotalTimeloss is licensed under the MIT License. See [LICENSE](LICENSE).

LiveSplit and related LiveSplit assemblies are licensed separately. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md).
