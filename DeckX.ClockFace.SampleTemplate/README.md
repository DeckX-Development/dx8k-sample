# DeckX Clock & Weather Face — Single-Project DLL Starter Template

Welcome to the official developer starter kit for creating custom **DeckX Standby Clock & Weather Face Plugins** (`.deckxclock`).

Just like DeckX Plugins (`IDeckXPlugin`), this template is a **single C# project** that implements `IDeckXClockFace` and bundles its own integrated Settings Window.

---

## ⚡ The 1-Step Build Workflow in VS Code

You do **NOT** need batch files or extra build tools.

Simply open this folder in **VS Code** (or any terminal) and run:
```bash
dotnet build -c Release
```
*(or press `Ctrl + Shift + B` in VS Code)*

### What happens automatically in one go:
1. **Compiles:** Your C# code compiles into `DeckX.ClockFace.SampleTemplate.dll`.
2. **Packages:** Bundles `clock.json`, `settings.json`, `preview.png`, and your DLL into `SampleClockFace.deckxclock`.
3. **Deploys:** Automatically installs the package directly into `%APPDATA%\DeckX\DX8K\screensaver\clock_faces\com.deckx.sample.clockface\`!

---

## 📁 File Structure

```text
DeckX.ClockFace.SampleTemplate/
├── DeckX.ClockFace.SampleTemplate.csproj  # Single C# project file with integrated auto-packer
├── SampleClockFace.cs                    # Main class implementing IDeckXClockFace
├── SettingsWindow.cs                     # Integrated Dark Settings Window UI
├── clock.json                            # Manifest (specifies assembly DLL & settings: true)
├── settings.json                         # Default settings data contract
├── preview.png                           # 16:9 thumbnail for the DeckX card
└── libs/
    └── DeckX.Core.dll                    # Standalone reference library for VS Code
```

---

## 🧩 How `OpenSettings()` Works

In `SampleClockFace.cs`:

```csharp
public class SampleClockFace : IDeckXClockFace
{
    private string _packageDirectory = string.Empty;

    public string Id => "com.deckx.sample.clockface";
    public string Name => "Sample Standby Clock & Weather";
    public bool HasSettings => true;

    public void OnInitialize(string packageDirectory)
    {
        _packageDirectory = packageDirectory;
    }

    // Called directly by DeckX when user clicks Settings (⚙) on your card
    public void OpenSettings()
    {
        SettingsWindow.ShowDialog(_packageDirectory);
    }
}
```

When the user clicks the **Settings (⚙)** button in DeckX Studio, DeckX calls `OpenSettings()`, which immediately opens your custom `SettingsWindow`.

---

## 📲 Testing in DeckX Studio

1. Open this folder in **VS Code**.
2. Run `dotnet build -c Release`.
3. Launch **DeckX Control Studio** -> **Device Settings** -> **Screen Saver**.
4. Your clock face will already be visible in the gallery!
5. Click **Apply** to preview it, and click **Settings (⚙)** to open your integrated settings window.
