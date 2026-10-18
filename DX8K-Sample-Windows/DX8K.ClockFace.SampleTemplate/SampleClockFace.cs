using System;
using System.IO;
using System.Threading;
using DeckX.Core.Plugins;
using SkiaSharp;

namespace DX8K.ClockFace.SampleTemplate;

/// <summary>
/// Sample Standby Clock & Weather Face DLL Plugin.
/// Implements IDeckXClockFace to provide custom standby face metadata, SkiaSharp rendering, and direct Settings window integration.
/// </summary>
public class SampleClockFace : IDeckXClockFace
{
    private string _packageDirectory = string.Empty;
    private string _cachedCity = "Colombo";
    private string _cachedTempUnit = "C";
    private string _cachedClockFormat = "24h";
    private bool _cachedShowSeconds = true;
    private string _cachedAccentColor = "#00FFCC";
    private DateTime _lastSettingsCheck = DateTime.MinValue;

    public string Id => "com.dx8k.sample.clockface";
    public string Name => "Sample Standby Clock & Weather";
    public string Author => "DeckX Developer Community";
    public string Version => "1.0.0";
    public string Description => "Standalone clock & weather standby face implemented as a single C# DLL plugin with integrated Settings UI window.";

    /// <summary>
    /// Controls whether the Settings (⚙) button appears on this clock face card in DeckX Studio.
    /// </summary>
    public bool HasSettings => true;

    /// <summary>
    /// Called when DeckX Studio initializes and loads this clock face plugin.
    /// </summary>
    public void OnInitialize(string packageDirectory)
    {
        _packageDirectory = packageDirectory;
        RefreshSettingsCache();
    }

    private void RefreshSettingsCache()
    {
        if (string.IsNullOrEmpty(_packageDirectory)) return;
        if ((DateTime.UtcNow - _lastSettingsCheck).TotalSeconds < 2) return;
        _lastSettingsCheck = DateTime.UtcNow;

        try
        {
            string path = Path.Combine(_packageDirectory, "settings.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("city", out var c) && !string.IsNullOrWhiteSpace(c.GetString())) _cachedCity = c.GetString()!;
                if (root.TryGetProperty("temperatureUnit", out var u) && !string.IsNullOrWhiteSpace(u.GetString())) _cachedTempUnit = u.GetString()!;
                if (root.TryGetProperty("clockFormat", out var f) && !string.IsNullOrWhiteSpace(f.GetString())) _cachedClockFormat = f.GetString()!;
                if (root.TryGetProperty("showSeconds", out var s)) _cachedShowSeconds = s.GetBoolean();
                if (root.TryGetProperty("accentColor", out var a) && !string.IsNullOrWhiteSpace(a.GetString())) _cachedAccentColor = a.GetString()!;
            }
        }
        catch { }
    }

    /// <summary>
    /// Live Standby rendering callback. Draws a sleek, high-contrast clock face with weather telemetry.
    /// </summary>
    public void RenderPreview(SKCanvas canvas, int width, int height, DateTime currentTime)
    {
        RefreshSettingsCache();

        SKColor accentColor = SKColor.TryParse(_cachedAccentColor, out var parsed) ? parsed : new SKColor(0, 255, 204);

        // Deep OLED background
        canvas.Clear(new SKColor(8, 10, 16));

        // Corner decorative accent ticks
        using var tickPaint = new SKPaint
        {
            Color = accentColor.WithAlpha(140),
            StrokeWidth = 2.0f,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };
        const float tickLen = 14f;
        // Top-Left
        canvas.DrawLine(16, 16, 16 + tickLen, 16, tickPaint);
        canvas.DrawLine(16, 16, 16, 16 + tickLen, tickPaint);
        // Top-Right
        canvas.DrawLine(width - 16, 16, width - 16 - tickLen, 16, tickPaint);
        canvas.DrawLine(width - 16, 16, width - 16, 16 + tickLen, tickPaint);
        // Bottom-Left
        canvas.DrawLine(16, height - 16, 16 + tickLen, height - 16, tickPaint);
        canvas.DrawLine(16, height - 16, 16, height - 16 - tickLen, tickPaint);
        // Bottom-Right
        canvas.DrawLine(width - 16, height - 16, width - 16 - tickLen, height - 16, tickPaint);
        canvas.DrawLine(width - 16, height - 16, width - 16, height - 16 - tickLen, tickPaint);

        // Top Weather Badge Capsule
        float badgeW = 260, badgeH = 28;
        var badgeRect = new SKRoundRect(new SKRect(width / 2.0f - badgeW / 2.0f, 22, width / 2.0f + badgeW / 2.0f, 22 + badgeH), 14, 14);
        using var badgeBg = new SKPaint
        {
            Color = new SKColor(16, 20, 30),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        using var badgeBorder = new SKPaint
        {
            Color = accentColor.WithAlpha(100),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.2f,
            IsAntialias = true
        };
        canvas.DrawRoundRect(badgeRect, badgeBg);
        canvas.DrawRoundRect(badgeRect, badgeBorder);

        using var badgeTextPaint = new SKPaint
        {
            Color = accentColor,
            TextSize = 11.5f,
            IsAntialias = true,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center
        };
        string unitSymbol = _cachedTempUnit.Equals("F", StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";
        string weatherStr = $"28{unitSymbol} Clear  •  {_cachedCity.ToUpperInvariant()}";
        canvas.DrawText(weatherStr, width / 2.0f, 40.5f, badgeTextPaint);

        // Central Time Render
        bool is12H = _cachedClockFormat.Equals("12h", StringComparison.OrdinalIgnoreCase);
        string timeFormat = is12H ? "hh:mm" : "HH:mm";
        string timeStr = currentTime.ToString(timeFormat);

        using var timePaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 92f,
            IsAntialias = true,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center
        };

        float centerY = height / 2.0f + 20f;
        canvas.DrawText(timeStr, width / 2.0f, centerY, timePaint);

        // Seconds indicator or AM/PM
        if (_cachedShowSeconds)
        {
            using var secPaint = new SKPaint
            {
                Color = accentColor,
                TextSize = 22f,
                IsAntialias = true,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Left
            };
            float textHalfWidth = timePaint.MeasureText(timeStr) / 2.0f;
            canvas.DrawText(currentTime.ToString(":ss"), width / 2.0f + textHalfWidth + 6, centerY - 28, secPaint);
        }

        if (is12H)
        {
            using var ampmPaint = new SKPaint
            {
                Color = new SKColor(160, 175, 195),
                TextSize = 15f,
                IsAntialias = true,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Left
            };
            float textHalfWidth = timePaint.MeasureText(timeStr) / 2.0f;
            canvas.DrawText(currentTime.ToString("tt"), width / 2.0f + textHalfWidth + 6, centerY, ampmPaint);
        }

        // Date Display
        using var datePaint = new SKPaint
        {
            Color = new SKColor(165, 180, 205),
            TextSize = 15f,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText(currentTime.ToString("dddd, MMMM d, yyyy"), width / 2.0f, centerY + 50f, datePaint);

        // Subtle accent line under date
        using var linePaint = new SKPaint
        {
            Color = accentColor.WithAlpha(70),
            StrokeWidth = 1.0f,
            IsAntialias = true
        };
        canvas.DrawLine(width / 2.0f - 80, centerY + 62f, width / 2.0f + 80, centerY + 62f, linePaint);
    }

    /// <summary>
    /// Invoked directly by DeckX Control Studio when the user clicks the Settings (⚙) button on this card.
    /// Opens the developer's custom settings dialog window.
    /// </summary>
    public void OpenSettings()
    {
        // Run on STA thread for native Windows Forms dialog
        var thread = new Thread(() =>
        {
            try
            {
                SettingsWindow.ShowDialog(_packageDirectory);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ClockFace] Error showing settings dialog: {ex.Message}");
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
    }

    public void OnShutdown()
    {
    }
}
