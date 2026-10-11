# ScreenQuarters

[English](README.md) | [Türkçe](README.tr.md)

A lightweight Windows system tray tool for arranging windows into monitor-specific zones and keeping supported browser fullscreen content inside a selected zone.

Built by **GallabiGaminG**, with AI-assisted development, to make everyday desktop work more comfortable: write code in one zone, watch a video in another, and switch back to full-monitor use when needed.

## Features

- Choose a separate layout for each connected monitor.
- Five presets: 2×2, 3×3, two columns, three columns, and a wide left zone with two stacked right zones.
- Numbered layout previews in Settings.
- Move the active window with Ctrl+Alt+1–9.
- Optional automatic placement when dragging and dropping windows.
- Experimental browser F11 / video fullscreen containment.
- Release one window or switch off zone management for movies and games.
- Local settings, process exclusions, and a tray menu with Settings, About, and Exit.

The application UI is currently Turkish. An English UI is not implemented yet.

## Quick start

Requires Windows 10/11, Visual Studio 2022 17.8 or later with the **.NET desktop development** workload, and the .NET 8 SDK.

1. Clone or download this repository and open `ScreenQuarters.sln`.
2. Press **F5**. Find ScreenQuarters in the system tray, including the hidden icons area.
3. Right-click the icon → **Settings**. Select the monitor, choose a layout, and click **Kaydet** (Save).
4. Focus an application window on that monitor and press **Ctrl+Alt+1–9** to place it in a zone.
5. For a browser, assign the zone first, then try F11 or the video's fullscreen button.

Selecting a layout does not draw permanent lines on the desktop or fill empty zones. Unused space continues to show the desktop wallpaper.

## Shortcuts

| Shortcut | Action |
|---|---|
| Ctrl+Alt+1–9 | Place the active window in the corresponding zone on its current monitor |
| Ctrl+Alt+0 | Release the active window from zone management |
| Ctrl+Alt+F12 | Toggle workspace / full-monitor mode |

Use the number row, not the numpad. Only numbers present in the chosen layout are valid; unavailable zones show a notification. Shortcut conflicts are reported at startup. Custom shortcut editing is not implemented.

Grid zones are numbered left to right, top to bottom. The wide-left preset uses **1 = left**, **2 = upper right**, **3 = lower right**. The Settings preview shows the exact mapping.

Full-monitor mode releases managed windows. Enter fullscreen using the application's own controls. Switching back enables zone management; assign windows again.

## Settings and exclusions

Enable **Sürükleyip bırakınca otomatik bölgeye yerleştir** for automatic drag placement. Assigned windows can also be dragged to another zone.

The exclusion field accepts comma-separated **process names without `.exe`**. For example, `explorer` excludes File Explorer; remove it if you want to arrange Explorer windows. Exclusions apply to the process owning the window, not every component of an application. A helper process may therefore need its own entry.

Settings are saved as UTF-8 JSON at:

```text
%LOCALAPPDATA%\ScreenQuarters\settings.json
```

Changing source-code defaults does not overwrite an existing saved configuration.

## Fullscreen support and limitations

This is an early prototype using .NET 8, Windows Forms, and Win32 window positioning.

- Browser fullscreen containment is experimental. Recognized processes include Chrome, Edge, Firefox, Brave, Opera, and Vivaldi. Support depends on browser and player behavior; a separate fullscreen window is not automatically managed.
- A brief resize may be visible when entering fullscreen. If a window repeatedly rejects its zone size, the tool releases it and displays a notification.
- The tool does not create virtual monitors, change display drivers, detect DirectX/Vulkan, or contain exclusive fullscreen games. Switch off zone management before gaming.
- NVIDIA Surround is treated as the single display surface exposed by Windows. Physical panel boundaries are not used. Surround-specific behavior has not been separately verified.
- Zones use each monitor's working area, excluding the taskbar. Application minimum sizes may prevent an exact fit.
- Elevated applications may reject positioning from the normally running tool. The application does not request administrator privileges.
- Windows Snap can interact with drag placement. The tool does not change Windows Snap settings.
- Screen identifiers can change after display reconfiguration; check saved layouts after reconnecting monitors.
- There is no background service, telemetry, or automatic startup registration. To start with Windows, optionally place a shortcut to the published executable in `shell:startup`.

## Build a standalone executable

From the repository root, with the .NET 8 SDK installed:

```powershell
.\Publish.ps1
```

Output: `publish\ScreenQuarters.exe` — a self-contained Windows x64 build. Publishing may download required SDK packages.

## Validation

Manually built and tested by the project owner on Windows: zone shortcuts, quarter-screen browser/video fullscreen, per-monitor setting persistence, and 3×3 placement on two monitors. A numbered layout preview was also demonstrated.

Compatibility across all browsers, applications, DPI combinations, monitor reconnection scenarios, and exclusive fullscreen games is not established.

## Planned improvements

- Resize zone boundaries with the mouse and save custom layouts.
- Friendly monitor model names where available.
- Select a window to create an exclusion rule.
- Clear feedback for excluded applications and refreshed About/help text.

These are planned features, not current capabilities.

## License

MIT. See [LICENSE](LICENSE).
