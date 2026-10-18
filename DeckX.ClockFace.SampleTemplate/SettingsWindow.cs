using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace DeckX.ClockFace.SampleTemplate;

/// <summary>
/// Dark-Themed Settings Window for the clock face plugin.
/// Runs inside the same DLL, reading and writing settings.json.
/// </summary>
public static class SettingsWindow
{
    public static void ShowDialog(string packageDirectory)
    {
        string settingsPath = Path.Combine(packageDirectory, "settings.json");

        using var form = new Form
        {
            Text = "DeckX — Standby Face Settings",
            Size = new Size(540, 660),
            StartPosition = FormStartPosition.CenterScreen,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
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
            Text = "Standby Face Settings",
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(24, 14)
        };

        var subTitleLabel = new Label
        {
            Text = "Configure live weather parameters, typography, and accent colors",
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = true,
            Location = new Point(24, 42)
        };

        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(subTitleLabel);

        // Content Panel
        var contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 16, 24, 16),
            AutoScroll = true
        };

        int currentY = 10;

        // Section 1: Weather
        contentPanel.Controls.Add(CreateSectionHeader("WEATHER PROVIDER & LOCATION", ref currentY));

        contentPanel.Controls.Add(CreateFieldLabel("City Name or Query (e.g. Colombo, London):", ref currentY));
        var cityBox = CreateStyledTextBox(ref currentY);
        contentPanel.Controls.Add(cityBox);

        contentPanel.Controls.Add(CreateFieldLabel("OpenWeatherMap API Key (Optional):", ref currentY));
        var apiKeyBox = CreateStyledTextBox(ref currentY);
        contentPanel.Controls.Add(apiKeyBox);

        contentPanel.Controls.Add(CreateFieldLabel("Temperature Unit:", ref currentY));
        var tempPanel = new FlowLayoutPanel { Location = new Point(0, currentY), Size = new Size(470, 32), FlowDirection = FlowDirection.LeftToRight };
        var celsiusRadio = new RadioButton { Text = "Celsius (°C)", Checked = true, AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
        var fahrenheitRadio = new RadioButton { Text = "Fahrenheit (°F)", AutoSize = true, ForeColor = Color.White };
        tempPanel.Controls.Add(celsiusRadio);
        tempPanel.Controls.Add(fahrenheitRadio);
        contentPanel.Controls.Add(tempPanel);
        currentY += 36;

        contentPanel.Controls.Add(CreateFieldLabel("Update Interval (Minutes):", ref currentY));
        var refreshNum = new NumericUpDown
        {
            Location = new Point(0, currentY),
            Size = new Size(120, 28),
            BackColor = Color.FromArgb(28, 30, 42),
            ForeColor = Color.White,
            Minimum = 5,
            Maximum = 180,
            Value = 15
        };
        contentPanel.Controls.Add(refreshNum);
        currentY += 40;

        // Section 2: Clock & Styling
        contentPanel.Controls.Add(CreateSectionHeader("CLOCK TYPOGRAPHY & ACCENTS", ref currentY));

        contentPanel.Controls.Add(CreateFieldLabel("Time Format:", ref currentY));
        var formatPanel = new FlowLayoutPanel { Location = new Point(0, currentY), Size = new Size(470, 32), FlowDirection = FlowDirection.LeftToRight };
        var format24Radio = new RadioButton { Text = "24-Hour (14:30)", Checked = true, AutoSize = true, ForeColor = Color.White, Margin = new Padding(0, 0, 20, 0) };
        var format12Radio = new RadioButton { Text = "12-Hour (02:30 PM)", AutoSize = true, ForeColor = Color.White };
        formatPanel.Controls.Add(format24Radio);
        formatPanel.Controls.Add(format12Radio);
        contentPanel.Controls.Add(formatPanel);
        currentY += 36;

        var showSecondsCheck = new CheckBox
        {
            Text = "Show Live Seconds Counter (:59)",
            Location = new Point(0, currentY),
            Size = new Size(300, 26),
            Checked = true,
            ForeColor = Color.White
        };
        contentPanel.Controls.Add(showSecondsCheck);
        currentY += 34;

        contentPanel.Controls.Add(CreateFieldLabel("Accent Color Theme:", ref currentY));
        var accentCombo = new ComboBox
        {
            Location = new Point(0, currentY),
            Size = new Size(260, 28),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = Color.FromArgb(28, 30, 42),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        accentCombo.Items.AddRange(new object[]
        {
            "Cyan Neon (#00FFCC)",
            "Emerald Green (#10B981)",
            "Cyber Gold (#F59E0B)",
            "Neon Violet (#A855F7)",
            "Ice Blue (#38BDF8)",
            "Monochrome White (#FFFFFF)"
        });
        accentCombo.SelectedIndex = 0;
        contentPanel.Controls.Add(accentCombo);
        currentY += 44;

        var statusLabel = new Label
        {
            Location = new Point(0, currentY),
            Size = new Size(470, 24),
            ForeColor = Color.FromArgb(16, 185, 129),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        contentPanel.Controls.Add(statusLabel);

        // Load existing settings
        if (File.Exists(settingsPath))
        {
            try
            {
                var node = JsonNode.Parse(File.ReadAllText(settingsPath));
                if (node != null)
                {
                    cityBox.Text = node["city"]?.ToString() ?? "Colombo";
                    apiKeyBox.Text = node["apiKey"]?.ToString() ?? string.Empty;
                    if (node["temperatureUnit"]?.ToString() == "F") fahrenheitRadio.Checked = true;
                    if (node["clockFormat"]?.ToString() == "12h") format12Radio.Checked = true;
                    showSecondsCheck.Checked = node["showSeconds"]?.GetValue<bool>() ?? true;
                    if (node["refreshIntervalMinutes"] != null && int.TryParse(node["refreshIntervalMinutes"]!.ToString(), out int mins))
                    {
                        if (mins >= refreshNum.Minimum && mins <= refreshNum.Maximum) refreshNum.Value = mins;
                    }
                    string accent = node["accentColor"]?.ToString() ?? "#00FFCC";
                    for (int i = 0; i < accentCombo.Items.Count; i++)
                    {
                        if (accentCombo.Items[i]?.ToString()?.Contains(accent, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            accentCombo.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        // Footer Actions Panel
        var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.FromArgb(22, 24, 34), Padding = new Padding(24, 14, 24, 14) };

        var saveButton = new Button
        {
            Text = "Save & Apply",
            Size = new Size(130, 36),
            Location = new Point(366, 14),
            BackColor = Color.FromArgb(16, 185, 129),
            ForeColor = Color.Black,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (s, e) =>
        {
            try
            {
                string accent = accentCombo.SelectedItem?.ToString() switch
                {
                    var txt when txt != null && txt.Contains("#10B981") => "#10B981",
                    var txt when txt != null && txt.Contains("#F59E0B") => "#F59E0B",
                    var txt when txt != null && txt.Contains("#A855F7") => "#A855F7",
                    var txt when txt != null && txt.Contains("#38BDF8") => "#38BDF8",
                    var txt when txt != null && txt.Contains("#FFFFFF") => "#FFFFFF",
                    _ => "#00FFCC"
                };

                var obj = new JsonObject
                {
                    ["city"] = cityBox.Text.Trim(),
                    ["apiKey"] = apiKeyBox.Text.Trim(),
                    ["temperatureUnit"] = fahrenheitRadio.Checked ? "F" : "C",
                    ["clockFormat"] = format12Radio.Checked ? "12h" : "24h",
                    ["showSeconds"] = showSecondsCheck.Checked,
                    ["accentColor"] = accent,
                    ["refreshIntervalMinutes"] = (int)refreshNum.Value
                };

                Directory.CreateDirectory(packageDirectory);
                File.WriteAllText(settingsPath, JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true }));
                statusLabel.Text = "Settings saved successfully! Updating standby face...";
                statusLabel.ForeColor = Color.FromArgb(16, 185, 129);

                var timer = new System.Windows.Forms.Timer { Interval = 600 };
                timer.Tick += (_, _) => { timer.Stop(); form.Close(); };
                timer.Start();
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Error saving: {ex.Message}";
                statusLabel.ForeColor = Color.FromArgb(239, 68, 68);
            }
        };

        var cancelButton = new Button
        {
            Text = "Close",
            Size = new Size(90, 36),
            Location = new Point(266, 14),
            BackColor = Color.FromArgb(42, 45, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (s, e) => form.Close();

        footerPanel.Controls.Add(cancelButton);
        footerPanel.Controls.Add(saveButton);

        form.Controls.Add(contentPanel);
        form.Controls.Add(footerPanel);
        form.Controls.Add(headerPanel);

        form.ShowDialog();
    }

    private static Label CreateSectionHeader(string text, ref int y)
    {
        var lbl = new Label
        {
            Text = text,
            Location = new Point(0, y),
            Size = new Size(470, 22),
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = Color.FromArgb(16, 185, 129)
        };
        y += 26;
        return lbl;
    }

    private static Label CreateFieldLabel(string text, ref int y)
    {
        var lbl = new Label
        {
            Text = text,
            Location = new Point(0, y),
            Size = new Size(470, 20),
            Font = new Font("Segoe UI", 9f),
            ForeColor = Color.FromArgb(203, 213, 225)
        };
        y += 22;
        return lbl;
    }

    private static TextBox CreateStyledTextBox(ref int y)
    {
        var box = new TextBox
        {
            Location = new Point(0, y),
            Size = new Size(470, 28),
            BackColor = Color.FromArgb(28, 30, 42),
            ForeColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        y += 38;
        return box;
    }
}
