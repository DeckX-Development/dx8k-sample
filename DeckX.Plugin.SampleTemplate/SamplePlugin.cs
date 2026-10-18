using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DeckX.Core.Models;
using DeckX.Core.Plugins;

namespace DeckX.Plugin.SampleTemplate;

/// <summary>
/// Reference-grade sample button plugin for DeckX Control Studio.
/// Designed for third-party developers to quickly build and customize custom button plugins.
/// Demonstrates:
///  1. Two-way dynamic toggle buttons (ON/OFF with live color & title sync)
///  2. Interactive tap counters with live hardware LCD updates
///  3. Color cycler button
///  4. 1-second live telemetry ticker (live clock)
///  5. Asynchronous URL launcher (non-blocking)
///  6. Custom border styling (hasCustomBorder, customBorderColorHex, customBorderWidth)
///  7. Smart rendering cache to prevent USB serial port flooding
/// </summary>
public class SamplePlugin : IDeckXPlugin
{
    // =========================================================================
    // 1. Vector SVG Icons (Geometry Path Data)
    // Developers can obtain SVG path strings from any standard SVG icon library
    // =========================================================================
    private const string PluginCategoryIcon = "M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5";
    private const string PowerIcon = "M13 3h-2v10h2V3zm4.83 2.17l-1.42 1.42A6.92 6.92 0 0 1 19 12c0 3.87-3.13 7-7 7s-7-3.13-7-7c0-2.12.95-4.02 2.46-5.3L6.04 5.17A8.932 8.932 0 0 0 3 12c0 4.97 4.03 9 9 9s9-4.03 9-9c0-2.61-1.12-4.96-2.92-6.68z";
    private const string CounterIcon = "M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-2 10h-4v4h-2v-4H7v-2h4V7h2v4h4v2z";
    private const string PaletteIcon = "M12 3c-4.97 0-9 4.03-9 9 0 2.12.74 4.07 1.97 5.61L4.35 19.4c-.39.39-.39 1.02 0 1.41.39.39 1.02.39 1.41 0l1.9-1.9C9.18 19.64 10.55 20 12 20c4.97 0 9-4.03 9-9s-4.03-9-9-9zm-5 8c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm3-3c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm4 0c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm3 3c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1z";
    private const string ClockIcon = "M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z";
    private const string LinkIcon = "M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z";
    private const string MacroIcon = "M20 5H4c-1.1 0-1.99.9-1.99 2L2 17c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm-9 3h2v2h-2V8zm0 3h2v2h-2v-2zM8 8h2v2H8V8zm0 3h2v2H8v-2zm-1 2H5v-2h2v2zm0-3H5V8h2v2zm9 7H8v-2h8v2zm0-4h-2v-2h2v2zm0-3h-2V8h2v2zm3 3h-2v-2h2v2zm0-3h-2V8h2v2z";
    private const string ResetIcon = "M17.65 6.35C16.2 4.9 14.21 4 12 4c-4.42 0-7.99 3.58-7.99 8s3.57 8 7.99 8c3.73 0 6.84-2.55 7.73-6h-2.08c-.82 2.33-3.04 4-5.65 4-3.31 0-6-2.69-6-6s2.69-6 6-6c1.66 0 3.14.69 4.22 1.78L13 11h7V4l-2.35 2.35z";
    private const string StatusIcon = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z";

    // =========================================================================
    // 2. Plugin Identification & Metadata
    // Must match the ID and Category in plugin.json
    // =========================================================================
    public string Id => "com.deckx.sampletemplate";
    public string Name => "DeckX Button Plugin Template";
    public string Description => "Sample reference template for creating customizable button plugins for DeckX Control Studio.";
    public string Version => "1.0.0";
    public string Author => "DeckX Developer";
    public string Category => "Sample Template (Plugin)";
    public string CategoryIconGeometry => PluginCategoryIcon;
    public string? IconGeometry => PluginCategoryIcon;

    // =========================================================================
    // 3. Internal Plugin State & Visual Configurations
    // =========================================================================
    private IDeckXPluginContext? _context;
    private bool _isPowerActive = true;
    private int _counterValue = 0;
    private int _colorIndex = 0;

    // Preset brand colors for Color Cycler
    private static readonly string[] VibrantColors =
    {
        "#8B5CF6", // Purple
        "#3B82F6", // Blue
        "#10B981", // Emerald
        "#F59E0B", // Amber
        "#EF4444", // Red
        "#06B6D4"  // Cyan
    };

    // Visual Standards
    private readonly bool _bordersEnabled = true;
    private readonly string _defaultBorderColor = "#FFFFFF";
    private readonly double _borderThickness = 1.5;
    private readonly double _iconSize = 50.0;

    // Smart Caching to prevent redundant redraws and avoid serial flooding
    private readonly ConcurrentDictionary<int, string> _renderCache = new();

    // =========================================================================
    // 4. Lifecycle: Initialize
    // Called when DeckX loads or imports the plugin
    // =========================================================================
    public void OnInitialize(IDeckXPluginContext context)
    {
        _context = context;

        // Register action handlers for live tickers and runtime updates
        RegisterKeyHandler("sample.toggle_power", "Plugin: Power Toggle", UpdatePowerKey);
        RegisterKeyHandler("sample.live_counter", "Plugin: Tap Counter", UpdateCounterKey);
        RegisterKeyHandler("sample.color_cycle", "Plugin: Color Cycler", UpdateColorKey);
        RegisterKeyHandler("sample.live_clock", "Plugin: Live Clock", UpdateClockKey);
        RegisterKeyHandler("sample.open_url", "Plugin: Open Web Link", UpdateUrlKey);
        RegisterKeyHandler("sample.hotkey_macro", "Plugin: Hotkey Macro", UpdateMacroKey);
        RegisterKeyHandler("sample.reset_state", "Plugin: Reset All", UpdateResetKey);
        RegisterKeyHandler("sample.status_badge", "Plugin: Status Badge", UpdateStatusKey);
    }

    private void RegisterKeyHandler(string actionId, string actionType, Action<DeckKeyActionContext> handler)
    {
        _context?.RegisterLiveKey(actionId, handler);
        _context?.RegisterLiveKey(actionType, handler);
    }

    // =========================================================================
    // 5. Actions Registry
    // Supplies the left sidebar action items in DeckX Control Studio
    // =========================================================================
    public IEnumerable<ActionItemModel> GetActions()
    {
        return new List<ActionItemModel>
        {
            new()
            {
                Id = "sample.toggle_power",
                Title = "Sample: Power Toggle",
                Category = Category,
                ActionType = "Plugin: Power Toggle",
                IconGeometry = PowerIcon,
                Description = "Toggles an ON/OFF state with dynamic color, title, and border synchronization.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.live_counter",
                Title = "Sample: Tap Counter",
                Category = Category,
                ActionType = "Plugin: Tap Counter",
                IconGeometry = CounterIcon,
                Description = "Counts taps and updates the button label in real-time.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.color_cycle",
                Title = "Sample: Color Cycler",
                Category = Category,
                ActionType = "Plugin: Color Cycler",
                IconGeometry = PaletteIcon,
                Description = "Cycles through vibrant button colors on every press.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.live_clock",
                Title = "Sample: Live Clock Ticker",
                Category = Category,
                ActionType = "Plugin: Live Clock",
                IconGeometry = ClockIcon,
                Description = "Demonstrates 1-second live telemetry updates directly on the hardware button.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.open_url",
                Title = "Sample: Open Web Link",
                Category = Category,
                ActionType = "Plugin: Open Web Link",
                IconGeometry = LinkIcon,
                Description = "Opens a web link in the default browser asynchronously without freezing the UI.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.hotkey_macro",
                Title = "Sample: Hotkey Macro",
                Category = Category,
                ActionType = "Plugin: Hotkey Macro",
                IconGeometry = MacroIcon,
                Description = "Simulates a keyboard shortcut sequence safely in the background.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.reset_state",
                Title = "Sample: Reset All",
                Category = Category,
                ActionType = "Plugin: Reset All",
                IconGeometry = ResetIcon,
                Description = "Resets all sample button states back to default.",
                PluginId = Id
            },
            new()
            {
                Id = "sample.status_badge",
                Title = "Sample: Status Badge",
                Category = Category,
                ActionType = "Plugin: Status Badge",
                IconGeometry = StatusIcon,
                Description = "Displays live system/plugin status on the physical deck.",
                PluginId = Id
            }
        };
    }

    // =========================================================================
    // 6. Action Execution Handler
    // Triggered when a physical key on the ESP32 or software button is pressed
    // CRITICAL: Heavy tasks (processes, web calls, hotkeys) must run via Task.Run
    // so the Avalonia UI Thread is NEVER blocked!
    // =========================================================================
    public async Task OnActionTriggeredAsync(string actionId, DeckKeyActionContext context)
    {
        switch (actionId)
        {
            case "sample.toggle_power":
            case "Plugin: Power Toggle":
                _isPowerActive = !_isPowerActive;
                UpdatePowerKey(context);
                _context?.TriggerKeyUpdate("sample.toggle_power", UpdatePowerKey);
                _context?.TriggerKeyUpdate("sample.status_badge", UpdateStatusKey);
                break;

            case "sample.live_counter":
            case "Plugin: Tap Counter":
                _counterValue++;
                UpdateCounterKey(context);
                _context?.TriggerKeyUpdate("sample.live_counter", UpdateCounterKey);
                break;

            case "sample.color_cycle":
            case "Plugin: Color Cycler":
                _colorIndex = (_colorIndex + 1) % VibrantColors.Length;
                UpdateColorKey(context);
                _context?.TriggerKeyUpdate("sample.color_cycle", UpdateColorKey);
                break;

            case "sample.live_clock":
            case "Plugin: Live Clock":
                // Tapping clock can show an instant alert toast
                context.ShowAlert($"Current Time: {DateTime.Now:HH:mm:ss}");
                break;

            case "sample.open_url":
            case "Plugin: Open Web Link":
                // Run process launch asynchronously so UI thread never pauses
                _ = Task.Run(() =>
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://github.com",
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[SamplePlugin] Error opening URL: {ex.Message}");
                    }
                });
                context.ShowAlert("Opening Web Page...");
                break;

            case "sample.hotkey_macro":
            case "Plugin: Hotkey Macro":
                context.ShowAlert("Macro Triggered!");
                break;

            case "sample.reset_state":
            case "Plugin: Reset All":
                _isPowerActive = true;
                _counterValue = 0;
                _colorIndex = 0;
                _renderCache.Clear();
                context.ShowAlert("Reset All States!");
                // Trigger targeted updates for all keys
                _context?.TriggerKeyUpdate("sample.toggle_power", UpdatePowerKey);
                _context?.TriggerKeyUpdate("sample.live_counter", UpdateCounterKey);
                _context?.TriggerKeyUpdate("sample.color_cycle", UpdateColorKey);
                _context?.TriggerKeyUpdate("sample.status_badge", UpdateStatusKey);
                break;

            case "sample.status_badge":
            case "Plugin: Status Badge":
                context.ShowAlert("Status: All systems operational.");
                break;
        }

        await Task.CompletedTask;
    }

    // =========================================================================
    // 7. Key Updaters & Visual Render Methods
    // Demonstrates setting Title, TitleColor, BackgroundColor, Border, and Icon
    // =========================================================================

    private void UpdatePowerKey(DeckKeyActionContext ctx)
    {
        string expectedTitle = _isPowerActive ? "STATE: ON" : "STATE: OFF";
        string fp = $"power_{_isPowerActive}";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp && ctx.CurrentTitle == expectedTitle)
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle(expectedTitle);
        ctx.SetTitleColor("#FFFFFF");
        ctx.SetBackgroundColor(_isPowerActive ? "#10B981" : "#EF4444"); // Emerald Green / Crimson Red
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(PowerIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateCounterKey(DeckKeyActionContext ctx)
    {
        string expectedTitle = $"CLICKS: {_counterValue}";
        string fp = $"counter_{_counterValue}";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp && ctx.CurrentTitle == expectedTitle)
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle(expectedTitle);
        ctx.SetTitleColor("#FFFFFF");
        ctx.SetBackgroundColor("#3B82F6"); // Electric Blue
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(CounterIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateColorKey(DeckKeyActionContext ctx)
    {
        string activeColor = VibrantColors[_colorIndex];
        string fp = $"color_{_colorIndex}";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp)
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle("COLOR CYCLE");
        ctx.SetTitleColor("#FFFFFF");
        ctx.SetBackgroundColor(activeColor);
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(PaletteIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateClockKey(DeckKeyActionContext ctx)
    {
        // Updates once per second from 1-second live telemetry ticker
        string nowTime = DateTime.Now.ToString("HH:mm:ss");
        if (ctx.CurrentTitle == nowTime) return;

        ctx.SetTitle(nowTime);
        ctx.SetTitleColor("#00FFAA");
        ctx.SetBackgroundColor("#06B6D4"); // Teal Cyan
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(ClockIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateUrlKey(DeckKeyActionContext ctx)
    {
        string fp = "url_static";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp && ctx.CurrentTitle == "WEBSITE")
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle("WEBSITE");
        ctx.SetTitleColor("#FFFFFF");
        ctx.SetBackgroundColor("#F59E0B"); // Cyber Amber
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(LinkIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateMacroKey(DeckKeyActionContext ctx)
    {
        string fp = "macro_static";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp && ctx.CurrentTitle == "HOTKEY")
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle("HOTKEY");
        ctx.SetTitleColor("#FFFFFF");
        ctx.SetBackgroundColor("#EC4899"); // Neon Pink
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(MacroIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateResetKey(DeckKeyActionContext ctx)
    {
        string fp = "reset_static";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp && ctx.CurrentTitle == "RESET ALL")
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle("RESET ALL");
        ctx.SetTitleColor("#FFFFFF");
        ctx.SetBackgroundColor("#EF4444"); // Red
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(ResetIcon);
        ctx.SetIconSize(_iconSize);
    }

    private void UpdateStatusKey(DeckKeyActionContext ctx)
    {
        string stateText = _isPowerActive ? "STATUS: ON" : "STATUS: OFF";
        string stateColor = _isPowerActive ? "#00FF66" : "#FFAA00";
        string fp = $"status_{_isPowerActive}";
        if (_renderCache.TryGetValue(ctx.KeyIndex, out var last) && last == fp && ctx.CurrentTitle == stateText)
            return;
        _renderCache[ctx.KeyIndex] = fp;

        ctx.SetTitle(stateText);
        ctx.SetTitleColor(stateColor);
        ctx.SetBackgroundColor("#1F2937"); // Dark Slate
        ctx.SetBorder(_bordersEnabled, _defaultBorderColor, _borderThickness);
        ctx.SetIcon(StatusIcon);
        ctx.SetIconSize(_iconSize);
    }

    // =========================================================================
    // 8. Lifecycle: Shutdown
    // Unregisters handlers and cleans up memory when DeckX unloads the plugin
    // =========================================================================
    public void OnShutdown()
    {
        _context?.UnregisterLiveKey("sample.toggle_power");
        _context?.UnregisterLiveKey("Plugin: Power Toggle");
        _context?.UnregisterLiveKey("sample.live_counter");
        _context?.UnregisterLiveKey("Plugin: Tap Counter");
        _context?.UnregisterLiveKey("sample.color_cycle");
        _context?.UnregisterLiveKey("Plugin: Color Cycler");
        _context?.UnregisterLiveKey("sample.live_clock");
        _context?.UnregisterLiveKey("Plugin: Live Clock");
        _context?.UnregisterLiveKey("sample.open_url");
        _context?.UnregisterLiveKey("Plugin: Open Web Link");
        _context?.UnregisterLiveKey("sample.hotkey_macro");
        _context?.UnregisterLiveKey("Plugin: Hotkey Macro");
        _context?.UnregisterLiveKey("sample.reset_state");
        _context?.UnregisterLiveKey("Plugin: Reset All");
        _context?.UnregisterLiveKey("sample.status_badge");
        _context?.UnregisterLiveKey("Plugin: Status Badge");

        _renderCache.Clear();
    }
}
