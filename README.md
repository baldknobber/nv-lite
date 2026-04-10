# NVLite

A lightweight, portable, zero-telemetry alternative to the official NVIDIA App. Monitor your GPU and CPU in real time, manage drivers, and tweak key NVIDIA driver settings — without the bloat.

> **Status:** Beta (v0.8.x) — core features working, actively developed.

## Features

### Dashboard
- Real-time GPU monitoring: temperature, clock speed, usage, power draw, VRAM, fan speed
- Expandable details: hot spot temp, memory junction temp, voltage, power limit, memory controller load, video engine load
- CPU monitoring: package temp, per-core temps/clocks/loads, voltage, power draw, core/thread count
- Display info: connected monitors with resolution, refresh rate, and color depth
- GPU Settings: view and change key global driver settings (Power Management, VSync, Frame Rate Limiter, Low Latency, Threaded Optimization) with one-click restore to defaults
- Quick Actions: launch NVIDIA Control Panel, clear shader cache
- Fahrenheit / Celsius toggle
- Temperature alerts with configurable threshold
- CSV sensor logging for benchmarking and diagnostics
- WMI fallback for CPU temperature when kernel driver access is restricted

### Driver Management
- One-click check for latest Game Ready drivers via NVIDIA's API
- Download with progress tracking and cancel support
- **Minimal Install** — silent, driver-only install tracked in-app (no NVIDIA UI)
- **Express Install** — launches NVIDIA's full installer for component selection
- Optional clean install and shader cache clearing
- Driver version history with release notes links
- One-click rollback to previous driver version
- Automatic driver check on startup (configurable)

### System
- System tray with minimize-to-tray support
- Start with Windows option
- Light / Dark / System theme
- Configurable polling interval
- Maximized window state persisted across sessions
- Requires administrator privileges for full sensor access

## Download

See [Releases](https://github.com/baldknobber/nv-lite/releases) for the latest build.

> **Note:** NVLite requires Windows 10 1809+ and an NVIDIA GPU. Run as administrator for full functionality.

## Build from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 version 1809 (build 17763) or later
- Windows App SDK 1.8+

### Build & Run

```bash
dotnet build
# Run as administrator for hardware sensor access:
# Start-Process -Verb RunAs "src\NVLite.App\bin\Debug\net10.0-windows10.0.26100.0\win-x64\NVLite.App.exe"
```

### Run Tests

```bash
dotnet test
```

### Publish Portable EXE

```bash
dotnet publish src/NVLite.App -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish
```

## How It Compares

| Feature | NVLite | NVIDIA App | NVCleanstall |
| :------ | :----: | :--------: | :----------: |
| GPU/CPU monitoring | ✅ | ✅ (overlay) | ❌ |
| Driver updates | ✅ | ✅ | ✅ (download only) |
| Global driver settings | ✅ | ❌ | ❌ |
| Game optimization | ❌ | ✅ (cloud-based) | ❌ |
| ShadowPlay / recording | ❌ | ✅ | ❌ |
| Telemetry | None | Yes | None |
| Account required | No | No | No |
| Open source | ✅ MIT | ❌ | ❌ |
| Portable | ✅ | ❌ | ✅ |

## Tech Stack

- **C# / .NET 10** — Modern, high-performance runtime
- **WinUI 3** (Windows App SDK 1.8) — Native Windows 11 UI framework
- **[LibreHardwareMonitorLib](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)** — Hardware sensor monitoring
- **NVAPI DRS** — NVIDIA driver settings via P/Invoke
- **[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)** — MVVM architecture
- **[H.NotifyIcon.WinUI](https://github.com/HavenDV/H.NotifyIcon)** — System tray integration

## License

[MIT](LICENSE)

## Credits & Acknowledgments

NVLite is built on the shoulders of these excellent open-source projects:

- **[LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor)** — The core hardware monitoring engine. Licensed under MPL-2.0.
- **[NVIDIA Profile Inspector](https://github.com/Orbmu2k/nvidiaProfileInspector)** — Inspiration and reference for NVAPI DRS P/Invoke patterns.
- **[NVCleanstall](https://www.techpowerup.com/nvcleanstall/)** by TechPowerUp — Inspiration for the lightweight NVIDIA tool concept.
- **[CommunityToolkit](https://github.com/CommunityToolkit/dotnet)** — MVVM framework. Licensed under MIT.
- **[H.NotifyIcon](https://github.com/HavenDV/H.NotifyIcon)** — System tray support for WinUI 3. Licensed under MIT.
- **[Microsoft Windows App SDK](https://github.com/microsoft/WindowsAppSDK)** — The WinUI 3 framework.

Special thanks to the NVIDIA developer community and the contributors to the open-source hardware monitoring ecosystem.

## Disclaimer

- **Not affiliated with NVIDIA.** NVLite is an independent, open-source project. NVIDIA, GeForce, and related trademarks are property of NVIDIA Corporation.
- **Use at your own risk.** Driver installation modifies system-level components. While NVLite uses NVIDIA's own installer under the hood, incorrect driver versions or interrupted installs can cause display issues. The clean install option will reset your NVIDIA settings.
- **Administrator privileges required.** NVLite needs admin access to read hardware sensors (via LibreHardwareMonitor's kernel driver) and to install drivers. The app does not collect, transmit, or store any personal data.
- **Beta software.** This is pre-release software under active development. Expect bugs. Please [report issues](https://github.com/baldknobber/nv-lite/issues) to help improve the project.
- **Hardware monitoring accuracy.** Sensor readings depend on LibreHardwareMonitor's kernel driver, which may not load on all systems (e.g., Secure Boot configurations). A WMI fallback is used for CPU temperature when the kernel driver is unavailable.

## Contributing

Contributions are welcome! Please open an issue or pull request. For community profiles, see the [community-profiles/](community-profiles/) directory and the profile schema.

## License

[MIT](LICENSE)
