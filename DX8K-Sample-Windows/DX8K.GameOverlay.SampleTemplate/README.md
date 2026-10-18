# DeckX Game Monitoring HUD Developer Template

Welcome to the **DeckX Game Overlay & Telemetry HUD Plugin Template**. This single-project C# template allows developers to create custom hardware monitoring HUDs, cyberpunk gaming overlays, and thermal telemetry dashboards for the **DeckX Control Pad (DX8K)** and desktop overlay.

> 📖 **Looking for an in-depth architecture breakdown, telemetry pipelines, SkiaSharp HUD drawing tips, and STA thread settings guide?**  
> Check out the [**Comprehensive Developer Guide (DEVELOPER_GUIDE.md)**](./DEVELOPER_GUIDE.md).

---

## 🚀 Key Features

- **Single C# Project**: No scripts or complex batch files required. Build directly with `dotnet build` or inside Visual Studio / VS Code.
- **Auto-Packaging**: Every build automatically compiles your DLL, bundles assets and `overlay.json`, zips into `releases/DX8K.GameOverlay.SampleTemplate.deckxoverlay`, and deploys directly to DeckX Studio for instant live testing.
- **Integrated Settings Window (`HasSettings`)**: Implements `OpenSettings()` directly in the DLL using a dark WinForms dialog to adjust titles, colors, sensor units, and visible telemetry.
- **SkiaSharp 480x272 Canvas**: High-performance 2D hardware-accelerated rendering matching the exact IPS aspect ratio of DeckX Control Pad.
- **Hardware Telemetry Context**: `RenderOverlay` receives real-time FPS, CPU load/temp/clock/fan, GPU load/temp/clock/fan, and RAM/VRAM usage.

---

## 📁 Project Structure

```
DX8K.GameOverlay.SampleTemplate/
│
├── overlay.json               # Package manifest (ID, name, author, version, entry assembly)
├── settings.json              # Default user configuration values
├── preview.png                # Thumbnail image shown in DeckX Studio Gallery
├── SampleGameOverlay.cs       # Core plugin implementing IDeckXGameOverlay & SkiaSharp HUD
├── SettingsWindow.cs          # Dark-themed GUI settings dialog
├── libs/
│   └── DeckX.Core.dll         # Standalone reference for development outside the solution
└── assets/                    # Optional fonts, icons, or background PNGs
```

---

## 🛠️ Building & Testing

### Option 1: Command Line (CLI)
```bash
dotnet build -c Release
```

### Option 2: Visual Studio / VS Code
Press `Ctrl + Shift + B` or click **Build Solution**.

Upon build completion:
1. `releases/DX8K.GameOverlay.SampleTemplate.deckxoverlay` is generated.
2. The package is automatically deployed to:
   - `%APPDATA%\DeckX\devices\control_pad\dx8k\game_overlays\`
   - `%APPDATA%\DeckX\DX8K\game_overlays\`
3. Open **DeckX Control Studio**, navigate to **Game Overlays & System HUD**, and click **Apply** or **Settings (⚙)**!

---

## 🎨 Implementing `IDeckXGameOverlay`

```csharp
public class MyGameOverlay : IDeckXGameOverlay
{
    public string Id => "com.mycompany.myoverlay";
    public string Name => "Apex Telemetry HUD";
    public string Author => "My Studio";
    public string Version => "1.0.0";
    public string Description => "Futuristic HUD with dual thermals and FPS meter.";
    public bool HasSettings => true;

    public void OnInitialize(string packageDirectory)
    {
        // Load initial settings or custom TTF fonts
    }

    public void OpenSettings()
    {
        // Launch custom configuration dialog
    }

    public void RenderOverlay(SKCanvas canvas, int width, int height, GameOverlayTelemetryData telemetry)
    {
        // Draw 480x272 SkiaSharp graphics using hardware metrics
    }
}
```

---

## 📦 Sharing & Publishing

Distribute the generated `SampleGameOverlay.deckxoverlay` file. Users can drag and drop it into DeckX Studio or click **Import Overlay (.deckxoverlay)** to install it in one click!
