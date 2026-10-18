# DeckX Control Studio — Button Plugin Developer Template
**Version:** 1.0.0  
**Target Platform:** DeckX Control Studio (.NET 8 / DX8K Hardware)  
**Author:** Nactro Development Team  

Welcome to the **DeckX Button Plugin Template**! This template provides a complete, production-ready starting point for developers who want to build their own custom plugins for DeckX Control Studio.

> 📖 **Looking for a deep-dive architecture and API breakdown?**  
> Check out the [**Comprehensive Developer Guide (DEVELOPER_GUIDE.md)**](./DEVELOPER_GUIDE.md).

---

## 🚀 What This Sample Demonstrates

| Button Action | Feature Demonstrated |
| :--- | :--- |
| **Power Toggle** | Dynamic ON/OFF state with live background color (`#10B981` / `#EF4444`) and white border synchronization. |
| **Tap Counter** | Interactive click counting with instantaneous hardware LCD title updates (`CLICKS: N`). |
| **Color Cycler** | Cycling through vibrant button background colors on every hardware tap. |
| **Live Clock** | 1-second live telemetry ticker updating `HH:mm:ss` continuously without lagging the deck. |
| **Open Web Link** | Asynchronous, non-blocking process execution (opening URLs/apps without UI freeze). |
| **Hotkey Macro** | Executing automation macros safely on the host system. |
| **Reset All** | Clearing and resetting multi-button states atomically. |
| **Status Badge** | Dynamic two-way status indicators. |

---

## 📁 Project Structure

```
DX8K.Plugin.SampleTemplate/
├── libs/
│   └── DeckX.Core.dll                   # DeckX plugin interfaces & data contracts
├── DX8K.Plugin.SampleTemplate.csproj    # MSBuild project file with Auto-Packer
├── plugin.json                          # Plugin manifest & default 8-key layout
├── SamplePlugin.cs                      # Main C# plugin implementation
└── README.md                            # Developer documentation
```

---

## 🛠️ Step-by-Step: How to Create Your Own Plugin

### Step 1: Copy or Rename the Project
1. Copy the `DX8K.Plugin.SampleTemplate` folder and give it your desired plugin name (e.g., `DX8K.Plugin.Twitch` or `DX8K.Plugin.SmartHome`).
2. Rename the `.csproj` file to match your plugin name.

### Step 2: Edit `plugin.json`
Open `plugin.json` and change the following:
- **`id`**: Unique identifier for your plugin in reverse-DNS format (e.g. `com.yourname.smartlights`).
- **`name`**: Friendly name shown in DeckX Control Studio.
- **`assembly`**: Name of the compiled DLL output (e.g. `DX8K.Plugin.SmartHome.dll`).
- **`actions`**: Define the list of action buttons your plugin exposes to the DeckX left sidebar.
- **`defaultPage`**: Configure the initial 8-button layout, background colors, and white custom borders (`hasCustomBorder: true`, `customBorderColorHex: "#FFFFFF"`, `customBorderWidth: 1.5`).

### Step 3: Customize `SamplePlugin.cs`
Implement your desired business logic inside `SamplePlugin.cs`:
1. **Change Class & Namespace**:
   ```csharp
   namespace DX8K.Plugin.MyService;
   public class MyPlugin : IDeckXPlugin
   ```
2. **Handle Button Presses**:
   Inside `OnActionTriggeredAsync(string actionId, DeckKeyActionContext context)`:
   ```csharp
   switch (actionId)
   {
       case "my.action_id":
           // Execute your command!
           // NOTE: Always run heavy operations (HTTP, Process, Thread sleep) inside Task.Run()!
           _ = Task.Run(async () =>
           {
               await CallMyApiAsync();
           });
           break;
   }
   ```
3. **Change Button Appearance Dynamically**:
   Use the `DeckKeyActionContext` methods:
   - `context.SetTitle("NEW TITLE")` — Changes the LCD button title
   - `context.SetTitleColor("#00FF66")` — Sets title text color
   - `context.SetBackgroundColor("#5865F2")` — Sets button background color
   - `context.SetBorder(true, "#FFFFFF", 1.5)` — Controls custom button border
   - `context.SetIcon(svgPathData)` — Updates button vector icon
   - `context.SetIconSize(50.0)` — Adjusts icon scale
   - `context.ShowAlert("Message")` — Shows a toast alert on the PC screen

---

## ⚡ Performance Best Practices

1. **Never Block the UI Thread**:
   When a physical button is pressed on the DX8K hardware deck, DeckX calls `OnActionTriggeredAsync`. Never call synchronous `Thread.Sleep()`, synchronous HTTP requests, or `Process.WaitForExit()` directly on the calling thread. Always offload them:
   ```csharp
   _ = Task.Run(async () =>
   {
       // Background work here
   });
   ```
2. **Use Render State Caching**:
   To avoid flooding the USB serial port with redundant full-screen frame updates, check if your state actually changed before invoking `SetBackgroundColor` or `SetBorder`. See `_renderCache` in `SamplePlugin.cs` for the reference implementation.

---

## 📦 How to Build and Package

Simply run:
```bash
dotnet build -c Release
```

The built-in MSBuild target will automatically:
1. Compile the C# `.dll`.
2. Stage the binaries and `plugin.json`.
3. Package everything into a `.deckxplugin` zip file located in `releases/`.
4. Deploy the plugin directly into `%APPDATA%\DeckX\Plugins\` for instant testing!

---

## 🧪 Testing in DeckX Control Studio

1. Open **DeckX Control Studio**.
2. Click **Import Plugin (.deckxplugin)** in the sidebar or Actions page.
3. Select your generated `.deckxplugin` file.
4. A new Deck tab page will appear with all 8 customized buttons, colors, icons, and white borders ready to use!
