# NVLite

A lightweight, portable, zero-telemetry alternative to the official NVIDIA App.

## Features

- **GPU + CPU Dashboard** — Real-time temps, clocks, usage, power, VRAM, and fan speed. CPU temperature included (the #1 missing feature from NVIDIA's overlay).
- **Driver Checker & Updater** — One-click check for latest drivers with optional clean install and shader cache clearing.
- **DRS Profile Viewer/Editor** — Browse, edit, export, and import NVIDIA driver profiles. Profiles survive driver updates via JSON backup.
- **Zero Telemetry** — No analytics, no account login, no background services.
- **Portable** — Single-file EXE. Unzip and run.

## Download

See [Releases](https://github.com/baldknobber/nv-lite/releases) for the latest build.

## Build from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 version 1809 (build 17763) or later

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project src/NVLite.App
```

### Publish portable EXE

```bash
dotnet publish src/NVLite.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

## Tech Stack

- C# / .NET 10 / WinUI 3 (Windows App SDK)
- [LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) for hardware monitoring
- NVAPI DRS for driver profile management (referencing [NVIDIA Profile Inspector](https://github.com/Orbmu2k/nvidiaProfileInspector) patterns)

## License

[MIT](LICENSE)
