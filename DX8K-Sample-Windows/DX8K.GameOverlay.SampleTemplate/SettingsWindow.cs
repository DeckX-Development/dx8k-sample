using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace DX8K.GameOverlay.SampleTemplate;

/// <summary>
/// Dark-Themed Settings Window for the Game Overlay HUD plugin.
/// Includes Theme Presets, Custom Color Pickers, Hardware Specification Overrides, and Sensor Toggles.
/// </summary>
public static class SettingsWindow
{
    private record ThemePreset(string Id, string Name, string Bg, string CardBg, string CardBorder, string Accent, string Secondary);

    private static readonly ThemePreset[] Presets = new[]
    {
        new ThemePreset("cyberpunk_neon", "Cyber Neon", "#0A0B10", "#10131D", "#23293B", "#00F0FF", "#3B82F6"),
        new ThemePreset("stealth_black", "Stealth Dark", "#0A0A0C", "#121216", "#24242C", "#FFFFFF", "#3B82F6"),
        new ThemePreset("arctic_glacier", "Arctic Glacier", "#070E1A", "#0C182E", "#1A2F54", "#38BDF8", "#818CF8"),
        new ThemePreset("ember_flame", "Ember Flame", "#140906", "#22120C", "#3D1F16", "#F97316", "#EF4444"),
        new ThemePreset("neon_pink", "Neon Synth", "#140810", "#240E1D", "#421B35", "#EC4899", "#A855F7"),
        new ThemePreset("acid_green", "Matrix Green", "#061208", "#0D2111", "#183D20", "#10B981", "#84CC16"),
        new ThemePreset("violet_night", "Violet Night", "#0C0817", "#161029", "#2C2052", "#8B5CF6", "#3B82F6")
    };

    public static DialogResult ShowDialog(string packageDirectory)
    {
        string settingsPath = Path.Combine(packageDirectory, "settings.json");

        using var form = new Form
        {
            Text = "DeckX — Game Overlay HUD Settings",
            Size = new Size(580, 680),
            MinimumSize = new Size(520, 500),
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.Sizable,
            MaximizeBox = true,
            MinimizeBox = false,
            BackColor = Color.FromArgb(16, 17, 24),
            ForeColor = Color.FromArgb(235, 238, 245),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
        };

        // Header Panel
        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 74,
            BackColor = Color.FromArgb(22, 24, 34),
            Padding = new Padding(24, 14, 24, 14)
        };

        var titleLabel = new Label
        {
            Text = "Game Overlay HUD Settings",
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        var subTitleLabel = new Label
        {
            Text = "Customize theme presets, colors, hardware name overrides, and sensor metrics",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(24, 42)
        };

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(subTitleLabel);

        // Content Panel (Scrollable)
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 16, 24, 16),
            AutoScroll = true
        };

        int currentY = 10;

        // Section 1: HUD Branding & Title
        contentPanel.Controls.Add(CreateSectionHeader("HUD TITLE & DISPLAY HEADER", ref currentY));
        contentPanel.Controls.Add(CreateFieldLabel("Header Title (Shown on top-left of HUD):", ref currentY));
        var titleBox = CreateStyledTextBox(ref currentY);
        titleBox.Text = "CYBERPUNK TELEMETRY HUD";
        contentPanel.Controls.Add(titleBox);

        // Section 2: Theme Presets
        contentPanel.Controls.Add(CreateSectionHeader("OVERLAY THEME PRESETS", ref currentY));
        var presetPanel = new FlowLayoutPanel
        {
            Location = new Point(0, currentY),
            Size = new Size(510, 72),
            FlowDirection = FlowDirection.LeftToRight,
            AutoScroll = false
        };

        // Section 3: Color Customization
        var accentBox = new TextBox { Size = new Size(130, 28), BackColor = Color.FromArgb(28, 30, 42), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "#00F0FF" };
        var secBox = new TextBox { Size = new Size(130, 28), BackColor = Color.FromArgb(28, 30, 42), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "#3B82F6" };
        var bgBox = new TextBox { Size = new Size(130, 28), BackColor = Color.FromArgb(28, 30, 42), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "#0A0B10" };
        var cardBgBox = new TextBox { Size = new Size(130, 28), BackColor = Color.FromArgb(28, 30, 42), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "#10131D" };
        var cardBorderBox = new TextBox { Size = new Size(130, 28), BackColor = Color.FromArgb(28, 30, 42), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Text = "#23293B" };

        string currentPresetId = "cyberpunk_neon";

        foreach (var p in Presets)
        {
            var btn = new Button
            {
                Text = p.Name,
                Size = new Size(116, 30),
                BackColor = Color.FromArgb(28, 32, 48),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 8, 6),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderColor = ColorTranslator.FromHtml(p.Accent);
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += (s, e) =>
            {
                currentPresetId = p.Id;
                accentBox.Text = p.Accent;
                secBox.Text = p.Secondary;
                bgBox.Text = p.Bg;
                cardBgBox.Text = p.CardBg;
                cardBorderBox.Text = p.CardBorder;
            };
            presetPanel.Controls.Add(btn);
        }
        contentPanel.Controls.Add(presetPanel);
        currentY += 80;

        contentPanel.Controls.Add(CreateSectionHeader("COLOR PALETTE & NEON ACCENTS", ref currentY));

        contentPanel.Controls.Add(CreateFieldLabel("Primary Neon Accent Color (HEX):", ref currentY));
        var accentRow = CreateColorPickerRow(accentBox, new[] { "#00F0FF", "#38BDF8", "#10B981", "#EC4899", "#8B5CF6", "#F97316", "#FFFFFF" }, ref currentY);
        contentPanel.Controls.Add(accentRow);

        contentPanel.Controls.Add(CreateFieldLabel("Secondary Accent Color (HEX):", ref currentY));
        var secRow = CreateColorPickerRow(secBox, new[] { "#3B82F6", "#818CF8", "#84CC16", "#A855F7", "#EF4444", "#14B8A6" }, ref currentY);
        contentPanel.Controls.Add(secRow);

        contentPanel.Controls.Add(CreateFieldLabel("Card Border Color (HEX):", ref currentY));
        var borderRow = CreateColorPickerRow(cardBorderBox, new[] { "#23293B", "#24242C", "#1A2F54", "#3D1F16", "#421B35", "#183D20" }, ref currentY);
        contentPanel.Controls.Add(borderRow);

        // Section 4: Hardware Sensors & Custom Specification Overrides
        contentPanel.Controls.Add(CreateSectionHeader("HARDWARE SENSORS & SPECIFICATION OVERRIDES", ref currentY));

        contentPanel.Controls.Add(CreateFieldLabel("Custom CPU Model Name (Leave blank for auto-detected):", ref currentY));
        var cpuNameBox = CreateStyledTextBox(ref currentY);
        cpuNameBox.PlaceholderText = "e.g. AMD Ryzen 9 7950X3D";
        contentPanel.Controls.Add(cpuNameBox);

        contentPanel.Controls.Add(CreateFieldLabel("Custom GPU Model Name (Leave blank for auto-detected):", ref currentY));
        var gpuNameBox = CreateStyledTextBox(ref currentY);
        gpuNameBox.PlaceholderText = "e.g. NVIDIA GeForce RTX 4090";
        contentPanel.Controls.Add(gpuNameBox);

        contentPanel.Controls.Add(CreateFieldLabel("Temperature Unit:", ref currentY));
        var tempPanel = new FlowLayoutPanel { Location = new Point(0, currentY), Size = new Size(510, 32), FlowDirection = FlowDirection.LeftToRight };
        var celsiusRadio = new RadioButton { Text = "Celsius (°C)", Checked = true, AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
        var fahrenheitRadio = new RadioButton { Text = "Fahrenheit (°F)", AutoSize = true, ForeColor = Color.White };
        tempPanel.Controls.Add(celsiusRadio);
        tempPanel.Controls.Add(fahrenheitRadio);
        contentPanel.Controls.Add(tempPanel);
        currentY += 36;

        // Section 5: Telemetry Modules Toggle
        contentPanel.Controls.Add(CreateSectionHeader("TELEMETRY MODULES VISIBILITY", ref currentY));

        var fpsCheck = CreateStyledCheckBox("Display In-Game FPS Counter (Top Bar)", true, ref currentY);
        contentPanel.Controls.Add(fpsCheck);

        var cpuCheck = CreateStyledCheckBox("Display CPU Telemetry Card (Usage, Clock, Fan, Temp)", true, ref currentY);
        contentPanel.Controls.Add(cpuCheck);

        var gpuCheck = CreateStyledCheckBox("Display GPU Telemetry Card (Usage, Clock, Fan, Temp)", true, ref currentY);
        contentPanel.Controls.Add(gpuCheck);

        var ramCheck = CreateStyledCheckBox("Display System Memory (RAM) Card", true, ref currentY);
        contentPanel.Controls.Add(ramCheck);

        var vramCheck = CreateStyledCheckBox("Display GPU Video Memory (VRAM) Card", true, ref currentY);
        contentPanel.Controls.Add(vramCheck);

        // Load existing settings if file exists
        if (File.Exists(settingsPath))
        {
            try
            {
                string json = File.ReadAllText(settingsPath);
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("hudTitle", out var ht) && !string.IsNullOrWhiteSpace(ht.GetString()))
                    titleBox.Text = ht.GetString()!;

                if (root.TryGetProperty("themePreset", out var tp) && !string.IsNullOrWhiteSpace(tp.GetString()))
                    currentPresetId = tp.GetString()!;

                if (root.TryGetProperty("accentColor", out var ac) && !string.IsNullOrWhiteSpace(ac.GetString()))
                    accentBox.Text = ac.GetString()!;

                if (root.TryGetProperty("secondaryColor", out var sc) && !string.IsNullOrWhiteSpace(sc.GetString()))
                    secBox.Text = sc.GetString()!;

                if (root.TryGetProperty("backgroundColor", out var bg) && !string.IsNullOrWhiteSpace(bg.GetString()))
                    bgBox.Text = bg.GetString()!;

                if (root.TryGetProperty("cardBackground", out var cbg) && !string.IsNullOrWhiteSpace(cbg.GetString()))
                    cardBgBox.Text = cbg.GetString()!;

                if (root.TryGetProperty("cardBorder", out var cb) && !string.IsNullOrWhiteSpace(cb.GetString()))
                    cardBorderBox.Text = cb.GetString()!;

                if (root.TryGetProperty("customCpuName", out var ccn))
                    cpuNameBox.Text = ccn.GetString() ?? string.Empty;

                if (root.TryGetProperty("customGpuName", out var cgn))
                    gpuNameBox.Text = cgn.GetString() ?? string.Empty;

                if (root.TryGetProperty("tempUnit", out var tu))
                {
                    bool isF = string.Equals(tu.GetString(), "F", StringComparison.OrdinalIgnoreCase);
                    celsiusRadio.Checked = !isF;
                    fahrenheitRadio.Checked = isF;
                }

                if (root.TryGetProperty("showFps", out var sf)) fpsCheck.Checked = sf.GetBoolean();
                if (root.TryGetProperty("showCpu", out var scp)) cpuCheck.Checked = scp.GetBoolean();
                if (root.TryGetProperty("showGpu", out var sgp)) gpuCheck.Checked = sgp.GetBoolean();
                if (root.TryGetProperty("showRam", out var srm)) ramCheck.Checked = srm.GetBoolean();
                if (root.TryGetProperty("showVram", out var svrm)) vramCheck.Checked = svrm.GetBoolean();
            }
            catch { }
        }

        // Bottom Action Bar Panel
        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            BackColor = Color.FromArgb(22, 24, 34),
            Padding = new Padding(24, 11, 24, 11)
        };

        var actionsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            WrapContents = false
        };

        var saveButton = new Button
        {
            Text = "Save & Apply",
            Size = new Size(130, 38),
            BackColor = Color.FromArgb(0, 240, 255),
            ForeColor = Color.FromArgb(10, 11, 16),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 0)
        };
        saveButton.FlatAppearance.BorderSize = 0;

        var cancelButton = new Button
        {
            Text = "Cancel",
            Size = new Size(90, 38),
            BackColor = Color.FromArgb(36, 40, 56),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 10, 0)
        };
        cancelButton.FlatAppearance.BorderSize = 0;

        saveButton.Click += (s, e) =>
        {
            try
            {
                var node = new JsonObject
                {
                    ["themePreset"] = currentPresetId,
                    ["hudTitle"] = titleBox.Text.Trim(),
                    ["backgroundColor"] = bgBox.Text.Trim(),
                    ["cardBackground"] = cardBgBox.Text.Trim(),
                    ["cardBorder"] = cardBorderBox.Text.Trim(),
                    ["accentColor"] = accentBox.Text.Trim(),
                    ["secondaryColor"] = secBox.Text.Trim(),
                    ["textPrimary"] = "#FFFFFF",
                    ["textMuted"] = "#8E9BAE",
                    ["customCpuName"] = cpuNameBox.Text.Trim(),
                    ["customGpuName"] = gpuNameBox.Text.Trim(),
                    ["tempUnit"] = fahrenheitRadio.Checked ? "F" : "C",
                    ["showFps"] = fpsCheck.Checked,
                    ["showCpu"] = cpuCheck.Checked,
                    ["showGpu"] = gpuCheck.Checked,
                    ["showRam"] = ramCheck.Checked,
                    ["showVram"] = vramCheck.Checked
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonOutput = node.ToJsonString(options);
                File.WriteAllText(settingsPath, jsonOutput);

                // Mirror save to alternate directory if present to prevent dual-folder divergence
                try
                {
                    string altDir = packageDirectory.Contains("devices\\control_pad")
                        ? packageDirectory.Replace("devices\\control_pad\\dx8k", "DX8K")
                        : packageDirectory.Replace("DeckX\\DX8K", "DeckX\\devices\\control_pad\\dx8k");
                    if (Directory.Exists(altDir))
                    {
                        File.WriteAllText(Path.Combine(altDir, "settings.json"), jsonOutput);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "DeckX Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            form.DialogResult = DialogResult.Cancel;
            form.Close();
        };

        actionsPanel.Controls.Add(saveButton);
        actionsPanel.Controls.Add(cancelButton);
        bottomPanel.Controls.Add(actionsPanel);

        // WinForms Docking Order: Top & Bottom docked first, Fill control brought to front
        form.Controls.Add(contentPanel);
        form.Controls.Add(bottomPanel);
        form.Controls.Add(headerPanel);
        headerPanel.SendToBack();
        bottomPanel.SendToBack();
        contentPanel.BringToFront();

        return form.ShowDialog();
    }

    private static Label CreateSectionHeader(string text, ref int currentY)
    {
        var lbl = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 240, 255),
            Location = new Point(0, currentY),
            Size = new Size(510, 20)
        };
        currentY += 24;
        return lbl;
    }

    private static Label CreateFieldLabel(string text, ref int currentY)
    {
        var lbl = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9.0f),
            ForeColor = Color.FromArgb(203, 213, 225),
            Location = new Point(0, currentY),
            Size = new Size(510, 18)
        };
        currentY += 20;
        return lbl;
    }

    private static TextBox CreateStyledTextBox(ref int currentY)
    {
        var tb = new TextBox
        {
            Location = new Point(0, currentY),
            Size = new Size(510, 28),
            BackColor = Color.FromArgb(28, 30, 42),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        currentY += 34;
        return tb;
    }

    private static CheckBox CreateStyledCheckBox(string text, bool isChecked, ref int currentY)
    {
        var cb = new CheckBox
        {
            Text = text,
            Checked = isChecked,
            Location = new Point(0, currentY),
            Size = new Size(510, 24),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
        currentY += 28;
        return cb;
    }

    private static FlowLayoutPanel CreateColorPickerRow(TextBox box, string[] swatches, ref int currentY)
    {
        var panel = new FlowLayoutPanel
        {
            Location = new Point(0, currentY),
            Size = new Size(510, 34),
            FlowDirection = FlowDirection.LeftToRight
        };
        panel.Controls.Add(box);

        foreach (var hex in swatches)
        {
            var pBtn = new Button
            {
                Size = new Size(24, 24),
                BackColor = ColorTranslator.FromHtml(hex),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4, 2, 0, 0),
                Cursor = Cursors.Hand
            };
            pBtn.FlatAppearance.BorderSize = 0;
            pBtn.Click += (s, e) => box.Text = hex;
            panel.Controls.Add(pBtn);
        }

        currentY += 38;
        return panel;
    }
}
