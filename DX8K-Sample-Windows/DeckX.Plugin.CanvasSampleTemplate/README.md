# DeckX Canvas Plugin Developer Template

**Version:** 1.0.0  
**Target Framework:** .NET 8.0  
**Target Hardware:** DeckX DX8K 8-Key Control Pad (480x272 4.3" Capacitive Touch Display)  
**Author:** Nactro Development Team  

---

## 1. Overview

The **DeckX Canvas Plugin Template** is a complete, reference-grade starting point for developers wanting to create **full-screen (480x272) interactive dashboards** for DeckX Control Studio.

Unlike traditional 8-button plugins which fit into predefined button grids, a **Canvas Plugin** turns the entire DeckX screen into a customizable display surface. You have pixel-level control using **SkiaSharp** to draw cards, gauges, charts, custom icons, and media controls, and you receive raw capacitive touch coordinates directly from the hardware screen.

---

## 2. Key Capabilities Demonstrated

1. **Full-Screen 480x272 2D SkiaSharp Rendering:**
   - Anti-aliased vector paths, rounded cards, borders, and gradient fills.
   - Text rendering with custom weights, sizes, and alignments.
   - Circular radial arc gauges with progress tracking.
   - Dynamic real-time sparkline telemetry charts.
2. **Capacitive Touch Hit-Testing (`OnCanvasTouchAsync`):**
   - Clean, high-performance hit-testing using `SKRect.Contains(x, y)`.
   - Handling interactive widgets (Toggle Switch, `[-]` / `[+]` Counter, Radial Dial, Color Switcher, Reset Button).
3. **Event-Driven & Continuous Streaming Architecture:**
   - On-demand rendering via `RequestRedraw?.Invoke()` for instant USB transmission.
   - Background timers for live clock tickers without CPU or USB bus overhead.
4. **Automated MSBuild Packaging:**
   - On every build (`dotnet build`), the plugin automatically compiles, validates `plugin.json`, zips into a `.deckxplugin` distribution package in `releases/`, and installs into `%APPDATA%\DeckX\Plugins\`.

---

## 3. Project Structure

```
DeckX.Plugin.CanvasSampleTemplate/
├── DeckX.Plugin.CanvasSampleTemplate.csproj   # MSBuild project with auto-packager
├── plugin.json                               # Plugin manifest with "isCanvasPlugin": true
├── CanvasSamplePlugin.cs                     # Core reference implementation (IDeckXCanvasPlugin)
└── README.md                                 # Developer guide & reference
```

---

## 4. Understanding the `IDeckXCanvasPlugin` Interface

To create a canvas plugin, your class must implement `IDeckXCanvasPlugin`:

```csharp
using DeckX.Core.Models;
using DeckX.Core.Plugins;
using SkiaSharp;

public class MyCustomCanvas : IDeckXCanvasPlugin
{
    public string Id => "com.developer.mycanvas";
    public string Name => "My Custom Dashboard";
    public string Author => "Developer Name";
    public string Version => "1.0.0";
    public string Description => "Description of your dashboard";
    public string Category => "Custom Dashboards";
    public string CategoryIconGeometry => "M12 2L2 7l10 5 10-5-10-5z...";

    // 1. Frame Rate & Streaming Mode
    public int TargetFps => 1; // 0 or 1 for event-driven / clock; 10-20 for high-speed animations
    public bool NeedsContinuousStream => false; // Set true for continuous games/meters

    // 2. Immediate Redraw Notification
    public event Action? RequestRedraw;

    // 3. Render Loop (Width: 480, Height: 272)
    public void RenderCanvas(SKCanvas canvas, int width, int height)
    {
        canvas.Clear(new SKColor(15, 17, 23));
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.White, TextSize = 20 };
        canvas.DrawText("Hello DeckX Canvas!", 50, 50, paint);
    }

    // 4. Hardware Capacitive Touch Input
    public Task OnCanvasTouchAsync(CanvasTouchEventArgs e)
    {
        int x = e.X; // 0 to 479
        int y = e.Y; // 0 to 271
        string action = e.Action; // "click", "down", "up", "move"

        // Hit-test your buttons and trigger redraw if state changes
        RequestRedraw?.Invoke();
        return Task.CompletedTask;
    }

    public void OnInitialize(IDeckXPluginContext context) { }
    public void OnShutdown() { }
    public IEnumerable<ActionItemModel> GetActions() => ...;
    public Task OnActionTriggeredAsync(string actionId, DeckKeyActionContext context) => Task.CompletedTask;
}
```

---

## 5. Manifest Definition (`plugin.json`)

The `plugin.json` manifest must specify `"isCanvasPlugin": true`:

```json
{
  "id": "com.deckx.canvas.sampletemplate",
  "name": "DeckX Canvas Plugin Template",
  "version": "1.0.0",
  "author": "DeckX Developer",
  "description": "Reference developer template for creating full-screen 480x272 interactive canvas dashboards.",
  "category": "Custom Dashboards",
  "categoryIcon": "M3 3h18v18H3V3zm16 16V5H5v14h14zM7 7h10v2H7V7zm0 4h10v2H7v-2zm0 4h7v2H7v-2z",
  "assembly": "DeckX.Plugin.CanvasSampleTemplate.dll",
  "isCanvasPlugin": true,
  "actions": [
    {
      "id": "canvas_page:sample_dashboard",
      "actionType": "Plugin:CanvasPage",
      "title": "Sample Canvas Dashboard",
      "description": "Interactive reference canvas dashboard with widgets, dials, graphs, and buttons.",
      "iconGeometry": "M3 3h18v18H3V3zm16 16V5H5v14h14zM7 7h10v2H7V7zm0 4h10v2H7v-2zm0 4h7v2H7v-2z",
      "category": "Custom Dashboards"
    }
  ]
}
```

---

## 6. How to Build & Test

### Step 1: Build the Plugin
Run in terminal:
```bash
dotnet build plugins/DeckX.Plugin.CanvasSampleTemplate/DeckX.Plugin.CanvasSampleTemplate.csproj
```
The build process will automatically:
1. Compile the .NET 8 class library.
2. Package the DLL, PDB, and `plugin.json` into `releases/DeckX.Plugin.CanvasSampleTemplate.deckxplugin`.
3. Copy the files into `%APPDATA%\DeckX\Plugins\com.deckx.canvas.sampletemplate\` for instant testing.

### Step 2: Test in DeckX Control Studio
1. Open **DeckX Control Studio**.
2. Go to any page (e.g. Page 2).
3. Switch Page Type from **8 Keys Grid** to **Virtual Canvas**.
4. Select **DeckX Canvas Plugin Template**.
5. The full-screen interactive dashboard will instantly appear on both the PC studio preview and the physical DeckX hardware screen!
