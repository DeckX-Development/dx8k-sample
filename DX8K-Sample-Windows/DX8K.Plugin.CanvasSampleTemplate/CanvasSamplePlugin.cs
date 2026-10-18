using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DeckX.Core.Models;
using DeckX.Core.Plugins;
using SkiaSharp;

namespace DX8K.Plugin.CanvasSampleTemplate;

/// <summary>
/// Reference-grade Canvas Plugin Template for DeckX Control Studio.
/// Demonstrates how to build full-screen (480x272) interactive dashboards with SkiaSharp
/// and handle capacitive touch input from the physical hardware screen.
/// 
/// Core Concepts Demonstrated:
/// 1. Full-screen SkiaSharp 2D graphics rendering (cards, borders, text, gauges, graphs)
/// 2. Capacitive touch hit-testing using SKRect bounds
/// 3. Interactive state management (Toggle switch, +/- Counter, Radial Dial, Theme Switcher)
/// 4. Event-driven immediate redraws via RequestRedraw event
/// 5. Live ticking clock / telemetry updates via background timer
/// </summary>
public class CanvasSamplePlugin : IDeckXCanvasPlugin, IDisposable
{
    // =========================================================================
    // 1. Plugin Identification & Metadata
    // =========================================================================
    public string Id => "com.dx8k.canvas.sampletemplate";
    public string Name => "DX8K Canvas Plugin Template";
    public string Author => "DeckX Developer";
    public string Version => "1.0.0";
    public string Description => "Reference developer template for creating full-screen 480x272 interactive canvas dashboards.";
    public string Category => "Custom Dashboards";
    public string CategoryIconGeometry => "M3 3h18v18H3V3zm16 16V5H5v14h14zM7 7h10v2H7V7zm0 4h10v2H7v-2zm0 4h7v2H7v-2z";

    /// <summary>
    /// Frame rate for continuous rendering stream.
    /// Set to 0 or 1-2 for event-driven / clock dashboards, or 10-20 FPS for high-speed live meters.
    /// </summary>
    public int TargetFps => 1;

    /// <summary>
    /// When true, DeckX continuously redraws the canvas at TargetFps.
    /// When false, DeckX redraws on-demand only when RequestRedraw is triggered or on touch.
    /// </summary>
    public bool NeedsContinuousStream => false;

    /// <summary>
    /// Trigger this event whenever internal plugin state changes to immediately re-render on PC and hardware.
    /// </summary>
    public event Action? RequestRedraw;

    // =========================================================================
    // 2. Interactive Plugin State (Widgets)
    // =========================================================================
    private bool _isToggleOn = true;
    private int _counterValue = 24;
    private int _dialPercentage = 68; // 0 to 100
    private int _themeIndex = 0;
    private readonly float[] _waveformData = new float[] { 30, 45, 25, 60, 50, 75, 40, 85, 65, 90, 55, 70, 80, 95 };

    // Dashboard Theme Color Palettes (Accent, Glow, Background)
    private static readonly SKColor[] ThemeColors = new[]
    {
        new SKColor(16, 185, 129), // Emerald Green (#10B981)
        new SKColor(59, 130, 246), // Electric Blue  (#3B82F6)
        new SKColor(168, 85, 247), // Vivid Purple   (#A855F7)
        new SKColor(245, 158, 11), // Warm Amber     (#F59E0B)
        new SKColor(239, 68, 68),  // Crimson Red    (#EF4444)
        new SKColor(6, 182, 212)   // Neon Cyan      (#06B6D4)
    };

    private SKColor ActiveAccentColor => ThemeColors[_themeIndex % ThemeColors.Length];

    // Touch Target Hit-Test Regions (Pre-calculated for 480x272 screen)
    // Developers: Keep hit areas slightly larger than visual bounds for optimal finger tap detection!
    private static readonly SKRect HitRectToggle     = new(20, 56, 150, 150);
    private static readonly SKRect HitRectMinus      = new(20, 164, 76, 224);
    private static readonly SKRect HitRectPlus       = new(94, 164, 150, 224);
    private static readonly SKRect HitRectDial       = new(166, 56, 314, 224);
    private static readonly SKRect HitRectCycleTheme = new(330, 164, 460, 192);
    private static readonly SKRect HitRectReset      = new(330, 196, 460, 224);

    private Timer? _clockTimer;
    private bool _isDisposed;

    // =========================================================================
    // 3. Lifecycle Methods
    // =========================================================================
    public void OnInitialize(IDeckXPluginContext context)
    {
        // Start 1-second timer to keep the dashboard clock ticking live
        _clockTimer = new Timer(_ =>
        {
            if (!_isDisposed)
            {
                // Advance waveform animation slightly
                ShiftWaveform();
                RequestRedraw?.Invoke();
            }
        }, null, 1000, 1000);
    }

    public void OnShutdown()
    {
        Dispose();
    }

    public IEnumerable<ActionItemModel> GetActions()
    {
        return new[]
        {
            new ActionItemModel
            {
                Id = "canvas_page:sample_dashboard",
                ActionType = "Plugin:CanvasPage",
                Title = "Sample Canvas Dashboard",
                Description = "Interactive reference canvas dashboard with widgets, dials, graphs, and buttons.",
                IconGeometry = CategoryIconGeometry,
                Category = Category,
                PluginId = Id
            }
        };
    }

    public Task OnActionTriggeredAsync(string actionId, DeckKeyActionContext context)
    {
        return Task.CompletedTask;
    }

    private void ShiftWaveform()
    {
        for (int i = 0; i < _waveformData.Length - 1; i++)
        {
            _waveformData[i] = _waveformData[i + 1];
        }
        // Generate pseudo-random organic value between 20 and 95
        _waveformData[^1] = (float)(20 + (DateTime.Now.Millisecond % 75));
    }

    // =========================================================================
    // 4. SkiaSharp 480x272 Canvas Rendering
    // =========================================================================
    public void RenderCanvas(SKCanvas canvas, int width, int height)
    {
        // A. Deep Sleek Dark Background
        canvas.Clear(new SKColor(15, 17, 23)); // #0F1117

        using var paint = new SKPaint { IsAntialias = true };

        // B. Top Header Bar (Y: 0 to 48)
        DrawHeader(canvas, paint, width);

        // C. Left Card 1: Interactive Toggle Switch (X: 20, Y: 56, W: 130, H: 94)
        DrawToggleCard(canvas, paint);

        // D. Left Card 2: Interactive +/- Counter (X: 20, Y: 164, W: 130, H: 60)
        DrawCounterCard(canvas, paint);

        // E. Center Card: Radial Gauge Meter (X: 166, Y: 56, W: 148, H: 168)
        DrawRadialGaugeCard(canvas, paint);

        // F. Right Card 1: Live Waveform / Mini-Telemetry (X: 330, Y: 56, W: 130, H: 94)
        DrawWaveformCard(canvas, paint);

        // G. Right Card 2: Interactive Action Buttons (Theme Cycle & Reset)
        DrawActionButtons(canvas, paint);

        // H. Bottom Status / Touch Hint Footer (Y: 236 to 272)
        DrawFooter(canvas, paint, width, height);
    }

    private void DrawHeader(SKCanvas canvas, SKPaint paint, int width)
    {
        // Subtle divider line
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.0f;
        paint.Color = new SKColor(35, 40, 52);
        canvas.DrawLine(20, 46, width - 20, 46, paint);
        paint.Style = SKPaintStyle.Fill;

        // Title
        paint.Color = SKColors.White;
        paint.TextSize = 16;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        paint.TextAlign = SKTextAlign.Left;
        canvas.DrawText("DECKX CANVAS STUDIO", 20, 32, paint);

        // Category Tag Badge
        paint.Color = ActiveAccentColor.WithAlpha(40);
        var badgeRect = new SKRoundRect(new SKRect(215, 16, 295, 36), 6, 6);
        canvas.DrawRoundRect(badgeRect, paint);

        paint.Color = ActiveAccentColor;
        paint.TextSize = 10;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        paint.TextAlign = SKTextAlign.Center;
        canvas.DrawText("DEV TEMPLATE", 255, 30, paint);

        // Live Clock (Top Right)
        paint.Color = new SKColor(220, 225, 235);
        paint.TextSize = 14;
        paint.TextAlign = SKTextAlign.Right;
        string timeStr = DateTime.Now.ToString("HH:mm:ss");
        canvas.DrawText(timeStr, width - 20, 32, paint);

        // Status Green Dot
        paint.Color = ActiveAccentColor;
        canvas.DrawCircle(width - 92, 27, 4.0f, paint);
    }

    private void DrawToggleCard(SKCanvas canvas, SKPaint paint)
    {
        var cardRect = new SKRoundRect(new SKRect(20, 56, 150, 150), 12, 12);
        
        // Background card fill
        paint.Color = new SKColor(24, 27, 36);
        canvas.DrawRoundRect(cardRect, paint);

        // Border
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.2f;
        paint.Color = _isToggleOn ? ActiveAccentColor : new SKColor(45, 52, 68);
        canvas.DrawRoundRect(cardRect, paint);
        paint.Style = SKPaintStyle.Fill;

        // Title
        paint.Color = new SKColor(160, 170, 185);
        paint.TextSize = 11;
        paint.TextAlign = SKTextAlign.Left;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("DEVICE POWER", 32, 78, paint);

        // State Text (ON / OFF)
        paint.Color = _isToggleOn ? SKColors.White : new SKColor(140, 145, 155);
        paint.TextSize = 20;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText(_isToggleOn ? "ACTIVE" : "STANDBY", 32, 106, paint);

        // Pill Switch graphic
        float switchX = 32;
        float switchY = 120;
        var switchPill = new SKRoundRect(new SKRect(switchX, switchY, switchX + 46, switchY + 18), 9, 9);
        
        paint.Color = _isToggleOn ? ActiveAccentColor : new SKColor(55, 62, 78);
        canvas.DrawRoundRect(switchPill, paint);

        // Switch thumb dot
        float thumbX = _isToggleOn ? (switchX + 37) : (switchX + 9);
        paint.Color = SKColors.White;
        canvas.DrawCircle(thumbX, switchY + 9, 7, paint);

        // Subtext hint
        paint.Color = new SKColor(110, 120, 135);
        paint.TextSize = 9;
        canvas.DrawText("Tap to toggle", 86, switchY + 13, paint);
    }

    private void DrawCounterCard(SKCanvas canvas, SKPaint paint)
    {
        var cardRect = new SKRoundRect(new SKRect(20, 164, 150, 224), 10, 10);
        paint.Color = new SKColor(24, 27, 36);
        canvas.DrawRoundRect(cardRect, paint);

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.0f;
        paint.Color = new SKColor(45, 52, 68);
        canvas.DrawRoundRect(cardRect, paint);
        paint.Style = SKPaintStyle.Fill;

        // Minus Button [-] (X: 20 to 56)
        var minusRect = new SKRoundRect(new SKRect(26, 172, 56, 216), 6, 6);
        paint.Color = new SKColor(35, 40, 52);
        canvas.DrawRoundRect(minusRect, paint);

        paint.Color = SKColors.White;
        paint.TextSize = 18;
        paint.TextAlign = SKTextAlign.Center;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("-", 41, 200, paint);

        // Counter Number (Center)
        paint.Color = ActiveAccentColor;
        paint.TextSize = 20;
        canvas.DrawText(_counterValue.ToString(), 85, 201, paint);

        // Plus Button [+] (X: 114 to 144)
        var plusRect = new SKRoundRect(new SKRect(114, 172, 144, 216), 6, 6);
        paint.Color = new SKColor(35, 40, 52);
        canvas.DrawRoundRect(plusRect, paint);

        paint.Color = SKColors.White;
        paint.TextSize = 18;
        canvas.DrawText("+", 129, 200, paint);
    }

    private void DrawRadialGaugeCard(SKCanvas canvas, SKPaint paint)
    {
        var cardRect = new SKRoundRect(new SKRect(166, 56, 314, 224), 12, 12);
        paint.Color = new SKColor(24, 27, 36);
        canvas.DrawRoundRect(cardRect, paint);

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.2f;
        paint.Color = new SKColor(45, 52, 68);
        canvas.DrawRoundRect(cardRect, paint);
        paint.Style = SKPaintStyle.Fill;

        // Title
        paint.Color = new SKColor(160, 170, 185);
        paint.TextSize = 11;
        paint.TextAlign = SKTextAlign.Center;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("RADIAL GAUGE", 240, 78, paint);

        // Radial Gauge Arc
        float cx = 240;
        float cy = 142;
        float radius = 48;
        var arcBounds = new SKRect(cx - radius, cy - radius, cx + radius, cy + radius);

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 8.0f;
        paint.StrokeCap = SKStrokeCap.Round;

        // Background Ring (270 degrees sweep starting from 135 deg)
        paint.Color = new SKColor(40, 46, 60);
        canvas.DrawArc(arcBounds, 135, 270, false, paint);

        // Active Value Arc
        float pct = Math.Clamp(_dialPercentage / 100.0f, 0.0f, 1.0f);
        paint.Color = ActiveAccentColor;
        canvas.DrawArc(arcBounds, 135, 270 * pct, false, paint);

        paint.Style = SKPaintStyle.Fill;

        // Percentage Value Text (Centered in circle)
        paint.Color = SKColors.White;
        paint.TextSize = 24;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText($"{_dialPercentage}%", cx, cy + 8, paint);

        // Touch Hint Label
        paint.Color = new SKColor(130, 140, 155);
        paint.TextSize = 9;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("Tap left/right to adjust", cx, 208, paint);
    }

    private void DrawWaveformCard(SKCanvas canvas, SKPaint paint)
    {
        var cardRect = new SKRoundRect(new SKRect(330, 56, 460, 150), 12, 12);
        paint.Color = new SKColor(24, 27, 36);
        canvas.DrawRoundRect(cardRect, paint);

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.0f;
        paint.Color = new SKColor(45, 52, 68);
        canvas.DrawRoundRect(cardRect, paint);
        paint.Style = SKPaintStyle.Fill;

        // Title
        paint.Color = new SKColor(160, 170, 185);
        paint.TextSize = 11;
        paint.TextAlign = SKTextAlign.Left;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.SemiBold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("LIVE TELEMETRY", 342, 78, paint);

        // Draw Mini Sparkline Waveform (X: 342 to 448, Y: 90 to 138)
        float startX = 342;
        float endX = 448;
        float baseY = 138;
        float maxHeight = 44;
        float stepX = (endX - startX) / (_waveformData.Length - 1);

        using var path = new SKPath();
        for (int i = 0; i < _waveformData.Length; i++)
        {
            float x = startX + (i * stepX);
            float y = baseY - (_waveformData[i] / 100.0f * maxHeight);
            if (i == 0) path.MoveTo(x, y);
            else path.LineTo(x, y);
        }

        // Draw line
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 2.0f;
        paint.StrokeCap = SKStrokeCap.Round;
        paint.Color = ActiveAccentColor;
        canvas.DrawPath(path, paint);

        // Draw subtle end dot
        float lastX = endX;
        float lastY = baseY - (_waveformData[^1] / 100.0f * maxHeight);
        paint.Style = SKPaintStyle.Fill;
        canvas.DrawCircle(lastX, lastY, 3.5f, paint);
    }

    private void DrawActionButtons(SKCanvas canvas, SKPaint paint)
    {
        // 1. Theme Cycler Button (X: 330 to 460, Y: 164 to 190)
        var themeRect = new SKRoundRect(new SKRect(330, 164, 460, 190), 8, 8);
        paint.Color = ActiveAccentColor.WithAlpha(40);
        canvas.DrawRoundRect(themeRect, paint);

        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = 1.0f;
        paint.Color = ActiveAccentColor;
        canvas.DrawRoundRect(themeRect, paint);
        paint.Style = SKPaintStyle.Fill;

        paint.Color = ActiveAccentColor;
        paint.TextSize = 10;
        paint.TextAlign = SKTextAlign.Center;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("CYCLE THEME COLOR", 395, 181, paint);

        // 2. Reset Button (X: 330 to 460, Y: 198 to 224)
        var resetRect = new SKRoundRect(new SKRect(330, 198, 460, 224), 8, 8);
        paint.Color = new SKColor(35, 40, 52);
        canvas.DrawRoundRect(resetRect, paint);

        paint.Color = new SKColor(200, 205, 215);
        paint.TextSize = 10;
        canvas.DrawText("RESET TO DEFAULTS", 395, 215, paint);
    }

    private void DrawFooter(SKCanvas canvas, SKPaint paint, int width, int height)
    {
        paint.Color = new SKColor(110, 120, 135);
        paint.TextSize = 10;
        paint.TextAlign = SKTextAlign.Center;
        paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        canvas.DrawText("Full-Screen 480x272 Touch Surface — Tap any widget or button to interact", width / 2f, 252, paint);
    }

    // =========================================================================
    // 5. Capacitive Touch Input Handling
    // =========================================================================
    public Task OnCanvasTouchAsync(CanvasTouchEventArgs e)
    {
        int x = e.X;
        int y = e.Y;

        // 1. Hit-Test: Toggle Switch Card
        if (HitRectToggle.Contains(x, y))
        {
            _isToggleOn = !_isToggleOn;
            Debug.WriteLine($"[CanvasTemplate] Power Toggle Tapped: {_isToggleOn}");
            RequestRedraw?.Invoke();
            return Task.CompletedTask;
        }

        // 2. Hit-Test: Counter Minus Button [-]
        if (HitRectMinus.Contains(x, y))
        {
            _counterValue = Math.Max(0, _counterValue - 1);
            Debug.WriteLine($"[CanvasTemplate] Counter Minus: {_counterValue}");
            RequestRedraw?.Invoke();
            return Task.CompletedTask;
        }

        // 3. Hit-Test: Counter Plus Button [+]
        if (HitRectPlus.Contains(x, y))
        {
            _counterValue++;
            Debug.WriteLine($"[CanvasTemplate] Counter Plus: {_counterValue}");
            RequestRedraw?.Invoke();
            return Task.CompletedTask;
        }

        // 4. Hit-Test: Radial Gauge Dial (Tap left half to decrease, right half to increase)
        if (HitRectDial.Contains(x, y))
        {
            if (x < 240)
            {
                _dialPercentage = Math.Max(0, _dialPercentage - 5);
            }
            else
            {
                _dialPercentage = Math.Min(100, _dialPercentage + 5);
            }
            Debug.WriteLine($"[CanvasTemplate] Radial Dial Tapped: {_dialPercentage}%");
            RequestRedraw?.Invoke();
            return Task.CompletedTask;
        }

        // 5. Hit-Test: Cycle Theme Button
        if (HitRectCycleTheme.Contains(x, y))
        {
            _themeIndex = (_themeIndex + 1) % ThemeColors.Length;
            Debug.WriteLine($"[CanvasTemplate] Theme Index Cycled: {_themeIndex}");
            RequestRedraw?.Invoke();
            return Task.CompletedTask;
        }

        // 6. Hit-Test: Reset State Button
        if (HitRectReset.Contains(x, y))
        {
            _isToggleOn = true;
            _counterValue = 24;
            _dialPercentage = 68;
            _themeIndex = 0;
            Debug.WriteLine("[CanvasTemplate] State Reset to Defaults.");
            RequestRedraw?.Invoke();
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _isDisposed = true;
        _clockTimer?.Dispose();
        _clockTimer = null;
    }
}
