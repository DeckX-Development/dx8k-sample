using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using DeckX.Core.Models;
using DeckX.Core.Plugins;
using SkiaSharp;

namespace DeckX.GameOverlay.SampleTemplate;

/// <summary>
/// Sample Game Monitoring & Telemetry HUD DLL Plugin.
/// Implements IDeckXGameOverlay with pixel-perfect DeckX Control Pad hardware HUD rendering,
/// full color customization, hardware sensor overrides, and integrated settings dialog.
/// </summary>
public class SampleGameOverlay : IDeckXGameOverlay
{
    private string _packageDirectory = string.Empty;
    private string _cachedHudTitle = "CYBERPUNK TELEMETRY HUD";
    private string _cachedBgHex = "#0A0B10";
    private string _cachedCardBgHex = "#10131D";
    private string _cachedCardBorderHex = "#23293B";
    private string _cachedAccentHex = "#00F0FF";
    private string _cachedSecondaryHex = "#3B82F6";
    private string _cachedTextPriHex = "#FFFFFF";
    private string _cachedTextMutHex = "#8E9BAE";
    private string _cachedCustomCpu = string.Empty;
    private string _cachedCustomGpu = string.Empty;
    private string _cachedTempUnit = "C";
    private bool _cachedShowFps = true;
    private bool _cachedShowCpu = true;
    private bool _cachedShowGpu = true;
    private bool _cachedShowRam = true;
    private bool _cachedShowVram = true;
    private DateTime _lastSettingsCheck = DateTime.MinValue;

    public string Id => "com.deckx.sample.gameoverlay";
    public string Name => "Cyber Neon Telemetry HUD";
    public string Author => "DeckX Developer Community";
    public string Version => "1.0.0";
    public string Description => "High-performance gaming & hardware telemetry HUD plugin with real-time FPS, CPU/GPU thermal arc gauges, and integrated settings dialog.";

    public bool HasSettings => true;
    public event Action? SettingsSaved;

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
            string settingsPath = Path.Combine(_packageDirectory, "settings.json");
            if (File.Exists(settingsPath))
            {
                string json = File.ReadAllText(settingsPath);
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("hudTitle", out var ht) && !string.IsNullOrWhiteSpace(ht.GetString())) _cachedHudTitle = ht.GetString()!;
                if (root.TryGetProperty("backgroundColor", out var bg) && !string.IsNullOrWhiteSpace(bg.GetString())) _cachedBgHex = bg.GetString()!;
                if (root.TryGetProperty("cardBackground", out var cbg) && !string.IsNullOrWhiteSpace(cbg.GetString())) _cachedCardBgHex = cbg.GetString()!;
                if (root.TryGetProperty("cardBorder", out var cb) && !string.IsNullOrWhiteSpace(cb.GetString())) _cachedCardBorderHex = cb.GetString()!;
                if (root.TryGetProperty("accentColor", out var ac) && !string.IsNullOrWhiteSpace(ac.GetString())) _cachedAccentHex = ac.GetString()!;
                if (root.TryGetProperty("secondaryColor", out var sc) && !string.IsNullOrWhiteSpace(sc.GetString())) _cachedSecondaryHex = sc.GetString()!;
                if (root.TryGetProperty("textPrimary", out var tp) && !string.IsNullOrWhiteSpace(tp.GetString())) _cachedTextPriHex = tp.GetString()!;
                if (root.TryGetProperty("textMuted", out var tm) && !string.IsNullOrWhiteSpace(tm.GetString())) _cachedTextMutHex = tm.GetString()!;
                if (root.TryGetProperty("customCpuName", out var ccn)) _cachedCustomCpu = ccn.GetString() ?? string.Empty;
                if (root.TryGetProperty("customGpuName", out var cgn)) _cachedCustomGpu = cgn.GetString() ?? string.Empty;
                if (root.TryGetProperty("tempUnit", out var tu) && !string.IsNullOrWhiteSpace(tu.GetString())) _cachedTempUnit = tu.GetString()!;
                if (root.TryGetProperty("showFps", out var sf)) _cachedShowFps = sf.GetBoolean();
                if (root.TryGetProperty("showCpu", out var scp)) _cachedShowCpu = scp.GetBoolean();
                if (root.TryGetProperty("showGpu", out var sgp)) _cachedShowGpu = sgp.GetBoolean();
                if (root.TryGetProperty("showRam", out var srm)) _cachedShowRam = srm.GetBoolean();
                if (root.TryGetProperty("showVram", out var svrm)) _cachedShowVram = svrm.GetBoolean();
            }
        }
        catch { }
    }

    public void OpenSettings()
    {
        if (string.IsNullOrEmpty(_packageDirectory)) return;

        var thread = new Thread(() =>
        {
            try
            {
                var result = SettingsWindow.ShowDialog(_packageDirectory);
                _lastSettingsCheck = DateTime.MinValue;
                RefreshSettingsCache();
                if (result == DialogResult.OK)
                {
                    SettingsSaved?.Invoke();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SampleGameOverlay] Error showing settings: {ex.Message}");
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
    }

    /// <summary>
    /// Renders the pixel-perfect 480x272 DeckX Control Pad hardware HUD.
    /// </summary>
    public void RenderOverlay(SKCanvas canvas, int width, int height, GameOverlayTelemetryData telemetry)
    {
        RefreshSettingsCache();

        SKColor bgColor = ParseColor(_cachedBgHex, new SKColor(0x0A, 0x0B, 0x10));
        SKColor cardBgColor = ParseColor(_cachedCardBgHex, new SKColor(0x10, 0x13, 0x1D));
        SKColor cardBorderColor = ParseColor(_cachedCardBorderHex, new SKColor(0x23, 0x29, 0x3B));
        SKColor accentColor = ParseColor(_cachedAccentHex, new SKColor(0x00, 0xF0, 0xFF));
        SKColor trackColor = new SKColor(0x1C, 0x22, 0x33);
        SKColor textPrimary = ParseColor(_cachedTextPriHex, new SKColor(0xFF, 0xFF, 0xFF));
        SKColor textMuted = ParseColor(_cachedTextMutHex, new SKColor(0x8E, 0x9B, 0xAE));

        // 1. Clear Screen Background
        canvas.Clear(bgColor);

        // 2. TOP STATUS BAR (y: 0..27)
        DrawTopBar(canvas, telemetry, accentColor, textPrimary, textMuted);

        // 3. CPU CARD (Top-Left: x: 6, y: 28, w: 231, h: 144)
        if (_cachedShowCpu)
        {
            DrawCpuCard(canvas, telemetry, cardBgColor, cardBorderColor, accentColor, trackColor, textPrimary, textMuted);
        }

        // 4. GPU CARD (Top-Right: x: 243, y: 28, w: 231, h: 144)
        if (_cachedShowGpu)
        {
            DrawGpuCard(canvas, telemetry, cardBgColor, cardBorderColor, accentColor, trackColor, textPrimary, textMuted);
        }

        // 5. RAM CARD (Bottom-Left: x: 6, y: 176, w: 231, h: 90)
        if (_cachedShowRam)
        {
            DrawRamCard(canvas, telemetry, cardBgColor, cardBorderColor, accentColor, trackColor, textPrimary, textMuted);
        }

        // 6. GPU VRAM CARD (Bottom-Right: x: 243, y: 176, w: 231, h: 90)
        if (_cachedShowVram)
        {
            DrawVramCard(canvas, telemetry, cardBgColor, cardBorderColor, accentColor, trackColor, textPrimary, textMuted);
        }
    }

    private void DrawTopBar(SKCanvas canvas, GameOverlayTelemetryData telemetry, SKColor accent, SKColor textPri, SKColor textMut)
    {
        // Left Title / Active Game
        string title = !string.IsNullOrWhiteSpace(telemetry.ActiveGameName)
            ? telemetry.ActiveGameName.ToUpperInvariant()
            : _cachedHudTitle;

        using var titlePaint = new SKPaint
        {
            Color = accent,
            TextSize = 12f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText(TruncateText(title, 140, titlePaint), 10, 18, titlePaint);

        // Center FPS
        if (_cachedShowFps)
        {
            string fpsStr = $"{telemetry.Fps} FPS";
            using var fpsPaint = new SKPaint
            {
                Color = textPri,
                TextSize = 14.5f,
                Typeface = GetTypeface(bold: true),
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText(fpsStr, 240, 18, fpsPaint);
        }

        // Right Time
        string timeStr = !string.IsNullOrWhiteSpace(telemetry.TimeText)
            ? telemetry.TimeText
            : DateTime.Now.ToString("h:mm tt");

        using var timePaint = new SKPaint
        {
            Color = textMut,
            TextSize = 12f,
            Typeface = GetTypeface(bold: false),
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };
        canvas.DrawText(timeStr, 468, 18, timePaint);
    }

    private void DrawCpuCard(SKCanvas canvas, GameOverlayTelemetryData telemetry, SKColor cardBg, SKColor cardBorder, SKColor accent, SKColor track, SKColor textPri, SKColor textMut)
    {
        float cardX = 6, cardY = 28, cardW = 231, cardH = 144;
        DrawCardFrame(canvas, cardX, cardY, cardW, cardH, cardBg, cardBorder);

        string cpuName = !string.IsNullOrWhiteSpace(_cachedCustomCpu)
            ? _cachedCustomCpu
            : (!string.IsNullOrWhiteSpace(telemetry.CpuName) ? telemetry.CpuName : "AMD Ryzen / Intel Core");

        string title = $"CPU  {cpuName}";
        using var titlePaint = new SKPaint
        {
            Color = accent,
            TextSize = 11.5f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText(TruncateText(title, 215, titlePaint), 16, 47, titlePaint);

        // Arc Temperature Gauge
        int tempVal = string.Equals(_cachedTempUnit, "F", StringComparison.OrdinalIgnoreCase)
            ? (int)(telemetry.CpuTempC * 1.8 + 32)
            : telemetry.CpuTempC;
        DrawArcGauge(canvas, 16, 60, tempVal, _cachedTempUnit, accent, track, textPri, textMut);

        // 3 Stat Rows (Usage, Clock, Fan)
        float barX = 86;
        float barW = 142;

        DrawStatRow(canvas, "USAGE", $"{telemetry.CpuUsagePercent}%", telemetry.CpuUsagePercent, barX, 70, 74, barW, 5, accent, track, textPri, textMut);

        float clockPct = (float)Math.Clamp(telemetry.CpuClockGhz * 16.6f, 0, 100);
        DrawStatRow(canvas, "CLOCK", $"{telemetry.CpuClockGhz:0.00} GHz", clockPct, barX, 104, 108, barW, 5, accent, track, textPri, textMut);

        float fanPct = Math.Clamp(telemetry.CpuFanRpm / 30f, 0, 100);
        DrawStatRow(canvas, "FAN", $"{telemetry.CpuFanRpm} RPM", fanPct, barX, 138, 142, barW, 5, accent, track, textPri, textMut);
    }

    private void DrawGpuCard(SKCanvas canvas, GameOverlayTelemetryData telemetry, SKColor cardBg, SKColor cardBorder, SKColor accent, SKColor track, SKColor textPri, SKColor textMut)
    {
        float cardX = 243, cardY = 28, cardW = 231, cardH = 144;
        DrawCardFrame(canvas, cardX, cardY, cardW, cardH, cardBg, cardBorder);

        string gpuName = !string.IsNullOrWhiteSpace(_cachedCustomGpu)
            ? _cachedCustomGpu
            : (!string.IsNullOrWhiteSpace(telemetry.GpuName) ? telemetry.GpuName : "NVIDIA GeForce / AMD Radeon");

        string title = $"GPU  {gpuName}";
        using var titlePaint = new SKPaint
        {
            Color = accent,
            TextSize = 11.5f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText(TruncateText(title, 215, titlePaint), 253, 47, titlePaint);

        // Arc Temperature Gauge
        int tempVal = string.Equals(_cachedTempUnit, "F", StringComparison.OrdinalIgnoreCase)
            ? (int)(telemetry.GpuTempC * 1.8 + 32)
            : telemetry.GpuTempC;
        DrawArcGauge(canvas, 253, 60, tempVal, _cachedTempUnit, accent, track, textPri, textMut);

        // 3 Stat Rows (Usage, Clock, Fan)
        float barX = 323;
        float barW = 142;

        DrawStatRow(canvas, "USAGE", $"{telemetry.GpuUsagePercent}%", telemetry.GpuUsagePercent, barX, 70, 74, barW, 5, accent, track, textPri, textMut);

        float clockPct = Math.Clamp(telemetry.GpuClockMhz / 30f, 0, 100);
        DrawStatRow(canvas, "CLOCK", $"{telemetry.GpuClockMhz} MHz", clockPct, barX, 104, 108, barW, 5, accent, track, textPri, textMut);

        float fanPct = Math.Clamp(telemetry.GpuFanRpm / 30f, 0, 100);
        DrawStatRow(canvas, "FAN", $"{telemetry.GpuFanRpm} RPM", fanPct, barX, 138, 142, barW, 5, accent, track, textPri, textMut);
    }

    private static void DrawRamCard(SKCanvas canvas, GameOverlayTelemetryData telemetry, SKColor cardBg, SKColor cardBorder, SKColor accent, SKColor track, SKColor textPri, SKColor textMut)
    {
        float cardX = 6, cardY = 176, cardW = 231, cardH = 90;
        DrawCardFrame(canvas, cardX, cardY, cardW, cardH, cardBg, cardBorder);

        string ramName = !string.IsNullOrWhiteSpace(telemetry.RamName) ? telemetry.RamName : "System Memory";
        string title = $"RAM  {ramName}";
        using var titlePaint = new SKPaint
        {
            Color = accent,
            TextSize = 11.5f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText(TruncateText(title, 150, titlePaint), 16, 195, titlePaint);

        using var pctPaint = new SKPaint
        {
            Color = textPri,
            TextSize = 14.5f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };
        canvas.DrawText($"{telemetry.RamUsagePercent}%", 225, 196, pctPaint);

        DrawProgressBar(canvas, 16, 212, 215, 7, telemetry.RamUsagePercent, accent, track);

        string subText = $"U: {telemetry.RamUsedGb:0.0} GB  F: {telemetry.RamFreeGb:0.0} GB";
        using var subPaint = new SKPaint
        {
            Color = textMut,
            TextSize = 11f,
            Typeface = GetTypeface(bold: false),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText(subText, 16, 239, subPaint);
    }

    private static void DrawVramCard(SKCanvas canvas, GameOverlayTelemetryData telemetry, SKColor cardBg, SKColor cardBorder, SKColor accent, SKColor track, SKColor textPri, SKColor textMut)
    {
        float cardX = 243, cardY = 176, cardW = 231, cardH = 90;
        DrawCardFrame(canvas, cardX, cardY, cardW, cardH, cardBg, cardBorder);

        using var titlePaint = new SKPaint
        {
            Color = accent,
            TextSize = 11.5f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText("GPU VRAM", 253, 195, titlePaint);

        using var pctPaint = new SKPaint
        {
            Color = textPri,
            TextSize = 14.5f,
            Typeface = GetTypeface(bold: true),
            IsAntialias = true,
            TextAlign = SKTextAlign.Right
        };
        canvas.DrawText($"{telemetry.VramUsagePercent}%", 462, 196, pctPaint);

        DrawProgressBar(canvas, 253, 212, 215, 7, telemetry.VramUsagePercent, accent, track);

        string subText = $"U: {telemetry.VramUsedGb:0.0} GB  F: {telemetry.VramFreeGb:0.0} GB";
        using var subPaint = new SKPaint
        {
            Color = textMut,
            TextSize = 11f,
            Typeface = GetTypeface(bold: false),
            IsAntialias = true,
            TextAlign = SKTextAlign.Left
        };
        canvas.DrawText(subText, 253, 239, subPaint);
    }

    private static void DrawCardFrame(SKCanvas canvas, float x, float y, float w, float h, SKColor bg, SKColor border)
    {
        var fillRect = new SKRoundRect(new SKRect(x, y, x + w, y + h), 10, 10);
        using var fillPaint = new SKPaint { Color = bg, Style = SKPaintStyle.Fill, IsAntialias = true };
        canvas.DrawRoundRect(fillRect, fillPaint);

        var strokeRect = new SKRoundRect(new SKRect(x + 0.5f, y + 0.5f, x + w - 0.5f, y + h - 0.5f), 9.5f, 9.5f);
        using var strokePaint = new SKPaint { Color = border, Style = SKPaintStyle.Stroke, StrokeWidth = 1.2f, IsAntialias = true };
        canvas.DrawRoundRect(strokeRect, strokePaint);
    }

    private static void DrawArcGauge(SKCanvas canvas, float arcX, float arcY, int tempVal, string unit, SKColor accent, SKColor track, SKColor textPri, SKColor textMut)
    {
        float size = 62;
        float strokeW = 5f;
        float halfStroke = strokeW / 2f;
        var oval = new SKRect(arcX + halfStroke, arcY + halfStroke, arcX + size - halfStroke, arcY + size - halfStroke);
        float cx = arcX + (size / 2f);
        float cy = arcY + (size / 2f);

        using var trackPaint = new SKPaint
        {
            Color = track,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = strokeW,
            StrokeCap = SKStrokeCap.Round,
            IsAntialias = true
        };
        canvas.DrawArc(oval, 135f, 270f, false, trackPaint);

        float maxTemp = string.Equals(unit, "F", StringComparison.OrdinalIgnoreCase) ? 212f : 100f;
        float sweepAngle = (Math.Clamp(tempVal, 0, maxTemp) / maxTemp) * 270f;
        if (sweepAngle > 0.5f)
        {
            using var valPaint = new SKPaint
            {
                Color = accent,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeW,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = true
            };
            canvas.DrawArc(oval, 135f, sweepAngle, false, valPaint);
        }

        string unitSymbol = string.Equals(unit, "F", StringComparison.OrdinalIgnoreCase) ? "°F" : "°C";
        using var tempPaint = new SKPaint
        {
            Color = textPri,
            TextSize = 13f,
            Typeface = GetTypeface(bold: true),
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText($"{tempVal}{unitSymbol}", cx, cy - 3f, tempPaint);

        using var subPaint = new SKPaint
        {
            Color = textMut,
            TextSize = 9f,
            Typeface = GetTypeface(bold: false),
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("TEMP", cx, cy + 12f, subPaint);
    }

    private static void DrawStatRow(SKCanvas canvas, string label, string valText, float pctVal, float barX, float labelBaselineY, float barY, float barW, float barH, SKColor accent, SKColor track, SKColor textPri, SKColor textMut)
    {
        using var labelPaint = new SKPaint
        {
            Color = textMut,
            TextSize = 10f,
            Typeface = GetTypeface(bold: false),
            TextAlign = SKTextAlign.Left,
            IsAntialias = true
        };
        canvas.DrawText(label, barX, labelBaselineY, labelPaint);

        using var valPaint = new SKPaint
        {
            Color = textPri,
            TextSize = 10.5f,
            Typeface = GetTypeface(bold: true),
            TextAlign = SKTextAlign.Right,
            IsAntialias = true
        };
        canvas.DrawText(valText, barX + barW, labelBaselineY, valPaint);

        DrawProgressBar(canvas, barX, barY, barW, barH, pctVal, accent, track);
    }

    private static void DrawProgressBar(SKCanvas canvas, float x, float y, float w, float h, float pctVal, SKColor accent, SKColor track)
    {
        float radius = h / 2f;
        var trackRect = new SKRoundRect(new SKRect(x, y, x + w, y + h), radius, radius);
        using var trackPaint = new SKPaint { Color = track, Style = SKPaintStyle.Fill, IsAntialias = true };
        canvas.DrawRoundRect(trackRect, trackPaint);

        float fillW = w * Math.Clamp(pctVal / 100f, 0f, 1f);
        if (fillW >= 1.5f)
        {
            var fillRect = new SKRoundRect(new SKRect(x, y, x + fillW, y + h), radius, radius);
            using var fillPaint = new SKPaint { Color = accent, Style = SKPaintStyle.Fill, IsAntialias = true };
            canvas.DrawRoundRect(fillRect, fillPaint);
        }
    }

    private static string TruncateText(string text, float maxWidth, SKPaint paint)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (paint.MeasureText(text) <= maxWidth) return text;

        string ellipsis = "...";
        float ellipsisW = paint.MeasureText(ellipsis);
        int len = text.Length;
        while (len > 1)
        {
            len--;
            if (paint.MeasureText(text.AsSpan(0, len)) + ellipsisW <= maxWidth)
            {
                return text.Substring(0, len) + ellipsis;
            }
        }
        return text;
    }

    private static SKTypeface GetTypeface(bool bold)
    {
        var weight = bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        var tf = SKTypeface.FromFamilyName("Montserrat", weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        if (tf != null && tf.FamilyName.Equals("Montserrat", StringComparison.OrdinalIgnoreCase))
            return tf;

        tf = SKTypeface.FromFamilyName("Segoe UI", weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        if (tf != null && tf.FamilyName.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
            return tf;

        return SKTypeface.FromFamilyName("Arial", weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright) ?? SKTypeface.Default;
    }

    private static SKColor ParseColor(string? hex, SKColor fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;
        if (SKColor.TryParse(hex, out var col)) return col;
        return fallback;
    }
}
