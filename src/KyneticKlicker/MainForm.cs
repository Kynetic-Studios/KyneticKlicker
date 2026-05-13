// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KyneticKlicker;

public sealed class MainForm : Form
{
    private readonly ClickerService _clicker = new();
    private readonly AppSettings _settings = AppSettings.Load();

    private readonly Panel _contentPanel = new();
    private readonly Label _statusValue = new();
    private readonly Label _statusFooter = new();
    private readonly Label _cpsBig = new();
    private readonly TrackBar _cpsSlider = new();
    private readonly CheckBox _minecraftOnlyCheck = new();
    private readonly Button _toggleKeyButton = new();
    private readonly Button _exitKeyButton = new();
    private readonly NotifyIcon _trayIcon = new();
    private readonly ContextMenuStrip _trayMenu = new();

    private Button? _mainNav;
    private Button? _keybindsNav;
    private Button? _aboutNav;

    private string _waitingFor = string.Empty;
    private string _currentPage = "main";

    private const int TopBarHeight = 46;
    private const int FooterHeight = 92;
    private const int SidebarWidth = 260;
    private const int ContentLeft = 290;

    public MainForm()
    {
        Text = "kynetic Klicker";
        Icon = IconHelper.LoadAppIcon();
        ClientSize = new Size(1280, 820);
        MinimumSize = new Size(1180, 820);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        ForeColor = Theme.Text;
        Font = new Font("Segoe UI", 10F);

        _clicker.ToggleKey = _settings.ToggleKey;
        _clicker.ExitKey = _settings.ExitKey;
        _clicker.Cps = Math.Clamp(_settings.Cps, 1, 13);
        _clicker.MinecraftOnly = _settings.MinecraftOnly;

        BuildUi();
        BuildTray();

        _clicker.EnabledChanged += _ =>
        {
            if (IsHandleCreated)
                BeginInvoke(UpdateStatus);
        };
        _clicker.ExitRequested += () =>
        {
            if (IsHandleCreated)
                BeginInvoke(Close);
        };

        _clicker.Start();

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;

        Resize += (_, _) =>
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                _trayIcon.Visible = true;
            }
        };

        FormClosing += (_, _) =>
        {
            SaveSettings();
            _trayIcon.Visible = false;
            _clicker.Dispose();
        };
    }

    private void BuildUi()
    {
        BuildTopBar();
        BuildSidebar();

        _contentPanel.Location = new Point(SidebarWidth, TopBarHeight);
        _contentPanel.Size = new Size(ClientSize.Width - SidebarWidth, ClientSize.Height - TopBarHeight - FooterHeight);
        _contentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        _contentPanel.BackColor = Theme.Background;
        Controls.Add(_contentPanel);

        BuildFooter();
        ShowMainPage();
    }

    private void BuildTopBar()
    {
        var top = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(ClientSize.Width, TopBarHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Color.Black
        };
        Controls.Add(top);

        var logo = new PictureBox
        {
            Location = new Point(22, 8),
            Size = new Size(34, 30),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        UiHelpers.TryLoadLogo(logo);
        top.Controls.Add(logo);

        top.Controls.Add(UiHelpers.Label("kynetic Klicker", 62, 10, 250, 28, 12F, Theme.Text, FontStyle.Bold));

        var minimize = new Button
        {
            Text = "—",
            Location = new Point(ClientSize.Width - 128, 6),
            Size = new Size(34, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        UiHelpers.StyleButton(minimize, Color.Black);
        minimize.FlatAppearance.BorderSize = 0;
        minimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
        top.Controls.Add(minimize);

        var close = new Button
        {
            Text = "×",
            Location = new Point(ClientSize.Width - 48, 6),
            Size = new Size(34, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            ForeColor = Theme.Red
        };
        UiHelpers.StyleButton(close, Color.Black);
        close.FlatAppearance.BorderSize = 0;
        close.Click += (_, _) => Close();
        top.Controls.Add(close);
    }

    private void BuildSidebar()
    {
        var side = new Panel
        {
            Location = new Point(0, TopBarHeight),
            Size = new Size(SidebarWidth, ClientSize.Height - TopBarHeight - FooterHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
            BackColor = Theme.Surface
        };
        Controls.Add(side);

        var logo = new PictureBox
        {
            Location = new Point(44, 36),
            Size = new Size(172, 112),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        UiHelpers.TryLoadLogo(logo);
        side.Controls.Add(logo);

        side.Controls.Add(UiHelpers.Label("K Y N E T I C", 48, 154, 170, 26, 15F, Theme.Text, FontStyle.Bold));
        side.Controls.Add(UiHelpers.Label("K L I C K E R", 56, 183, 160, 24, 14F, Theme.Red, FontStyle.Bold));

        _mainNav = AddNavButton(side, "⌂   Main", 250, true);
        _mainNav.Click += (_, _) => ShowMainPage();

        _keybindsNav = AddNavButton(side, "⌨   Keybinds", 320, false);
        _keybindsNav.Click += (_, _) => ShowKeybindsPage();

        _aboutNav = AddNavButton(side, "ⓘ   About", 390, false);
        _aboutNav.Click += (_, _) => ShowAboutPage();
    }

    private static Button AddNavButton(Control parent, string text, int y, bool active)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(18, y),
            Size = new Size(224, 58),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(18, 0, 0, 0),
            BackColor = active ? Theme.RedSoft : Theme.Surface,
            ForeColor = Theme.Text,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12F, FontStyle.Regular)
        };
        button.FlatAppearance.BorderColor = active ? Theme.Red : Theme.Surface;
        button.FlatAppearance.BorderSize = 1;
        parent.Controls.Add(button);
        return button;
    }

    private void SetActiveNav(string page)
    {
        _currentPage = page;

        void Style(Button? button, bool active)
        {
            if (button is null) return;
            button.BackColor = active ? Theme.RedSoft : Theme.Surface;
            button.FlatAppearance.BorderColor = active ? Theme.Red : Theme.Surface;
        }

        Style(_mainNav, page == "main");
        Style(_keybindsNav, page == "keybinds");
        Style(_aboutNav, page == "about");
    }

    private void ClearContent()
    {
        _contentPanel.Controls.Clear();
    }

    private void ShowMainPage()
    {
        SetActiveNav("main");
        ClearContent();

        int left = 30;
        int top = 46;

        _contentPanel.Controls.Add(UiHelpers.Label("kynetic ", left + 10, top, 120, 42, 22F, Theme.Text, FontStyle.Bold));
        _contentPanel.Controls.Add(UiHelpers.Label("Klicker", left + 124, top, 180, 42, 22F, Theme.Red, FontStyle.Bold));
        _contentPanel.Controls.Add(UiHelpers.Label("Hold LMB to activate • Release LMB to stop", left + 10, top + 52, 460, 28, 12F, Theme.Muted));

        var statusCard = Card(_contentPanel.Width - 330, 30, 270, 88);
        statusCard.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        _contentPanel.Controls.Add(statusCard);

        statusCard.Controls.Add(UiHelpers.Label("●", 20, 24, 22, 28, 14F, Theme.Red));

        var statusLabel = UiHelpers.Label("Status:", 52, 22, 75, 30, 12F, Theme.Text, FontStyle.Bold);
        statusCard.Controls.Add(statusLabel);

        _statusValue.Text = _clicker.Enabled ? "Enabled" : "Disabled";
        _statusValue.Location = new Point(135, 22);
        _statusValue.Size = new Size(120, 30);
        _statusValue.ForeColor = _clicker.Enabled ? Theme.Red : Theme.Muted;
        _statusValue.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
        _statusValue.BackColor = Color.Transparent;
        statusCard.Controls.Add(_statusValue);

        var toggleHint = UiHelpers.Label($"{KeyName(_clicker.ToggleKey)} to toggle", 52, 56, 180, 22, 10F, Theme.Muted);
        statusCard.Controls.Add(toggleHint);

        var cpsCard = Card(left, 144, 455, 250);
        _contentPanel.Controls.Add(cpsCard);
        cpsCard.Controls.Add(UiHelpers.Label("CPS", 25, 25, 100, 34, 13F, Theme.Red, FontStyle.Bold));

        _cpsBig.Text = _clicker.Cps.ToString();
        _cpsBig.Location = new Point(25, 76);
        _cpsBig.Size = new Size(80, 62);
        _cpsBig.ForeColor = Theme.Text;
        _cpsBig.Font = new Font("Segoe UI", 32F, FontStyle.Bold);
        _cpsBig.BackColor = Color.Transparent;
        cpsCard.Controls.Add(_cpsBig);

        cpsCard.Controls.Add(UiHelpers.Label("CPS", 100, 105, 70, 30, 16F, Theme.Red, FontStyle.Bold));

        _cpsSlider.Minimum = 1;
        _cpsSlider.Maximum = 13;
        _cpsSlider.Value = Math.Clamp(_clicker.Cps, 1, 13);
        _cpsSlider.TickFrequency = 1;
        _cpsSlider.SmallChange = 1;
        _cpsSlider.LargeChange = 1;
        _cpsSlider.AutoSize = false;
        _cpsSlider.Location = new Point(25, 145);
        _cpsSlider.Size = new Size(378, 30);
        _cpsSlider.BackColor = Theme.SurfaceAlt;
        _cpsSlider.Scroll += (_, _) =>
        {
            _clicker.Cps = Math.Clamp(_cpsSlider.Value, 1, 13);
            _cpsBig.Text = _clicker.Cps.ToString();
            SaveSettings();
            UpdateStatus();
        };
        cpsCard.Controls.Add(_cpsSlider);


        cpsCard.Controls.Add(UiHelpers.Label("Max 13 CPS", 25, 215, 180, 25, 11F, Theme.Muted));

        var keyCard = Card(left + 475, 144, _contentPanel.Width - (left + 475) - 50, 250);
        keyCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _contentPanel.Controls.Add(keyCard);
        keyCard.Controls.Add(UiHelpers.Label("KEYBINDS", 25, 25, 200, 32, 13F, Theme.Red, FontStyle.Bold));
        AddKeybindRow(keyCard, "Toggle Clicker", KeyName(_clicker.ToggleKey), 76, _toggleKeyButton, "toggle");
        AddKeybindRow(keyCard, "Exit Program", KeyName(_clicker.ExitKey), 146, _exitKeyButton, "exit");

        var howCard = Card(left, 414, 370, 205);
        _contentPanel.Controls.Add(howCard);
        howCard.Controls.Add(UiHelpers.Label("HOW IT WORKS", 25, 24, 230, 32, 13F, Theme.Red, FontStyle.Bold));
        AddCheckLine(howCard, "Hold Left Mouse Button to start clicking", 70);
        AddCheckLine(howCard, "Release Left Mouse Button to stop clicking", 104);
        AddCheckLine(howCard, "Respects the CPS you set above", 138);
        AddCheckLine(howCard, $"Toggle with {KeyName(_clicker.ToggleKey)} anytime", 172);

        var logoCard = Card(left + 390, 414, _contentPanel.Width - (left + 390) - 50, 205);
        logoCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _contentPanel.Controls.Add(logoCard);

        var bigLogo = new PictureBox
        {
            Location = new Point((logoCard.Width - 300) / 2, 20),
            Size = new Size(300, 165),
            Anchor = AnchorStyles.None,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        UiHelpers.TryLoadLogo(bigLogo);
        logoCard.Controls.Add(bigLogo);
    }

    private void ShowKeybindsPage()
    {
        SetActiveNav("keybinds");
        ClearContent();

        int left = 55;
        int top = 55;

        _contentPanel.Controls.Add(UiHelpers.Label("Keybinds", left, top, 260, 44, 24F, Theme.Text, FontStyle.Bold));
        _contentPanel.Controls.Add(UiHelpers.Label("Change and save your clicker controls. These are remembered after closing the app.", left, top + 50, 720, 30, 12F, Theme.Muted));

        var card = Card(left, top + 105, _contentPanel.Width - 110, 260);
        card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _contentPanel.Controls.Add(card);

        card.Controls.Add(UiHelpers.Label("CURRENT KEYBINDS", 25, 25, 280, 32, 13F, Theme.Red, FontStyle.Bold));
        AddKeybindRow(card, "Toggle Clicker", KeyName(_clicker.ToggleKey), 78, _toggleKeyButton, "toggle");
        AddKeybindRow(card, "Exit Program", KeyName(_clicker.ExitKey), 148, _exitKeyButton, "exit");

        var info = Card(left, top + 390, _contentPanel.Width - 110, 160);
        info.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _contentPanel.Controls.Add(info);

        info.Controls.Add(UiHelpers.Label("MEMORY SYSTEM", 25, 24, 260, 32, 13F, Theme.Red, FontStyle.Bold));
        AddCheckLine(info, "Keybinds save automatically when changed", 70);
        AddCheckLine(info, "CPS and Minecraft-only mode are also remembered", 104);
    }

    private void ShowAboutPage()
    {
        SetActiveNav("about");
        ClearContent();

        int left = 55;
        int top = 45;

        var logo = new PictureBox
        {
            Location = new Point(left, top),
            Size = new Size(130, 95),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        UiHelpers.TryLoadLogo(logo);
        _contentPanel.Controls.Add(logo);

        _contentPanel.Controls.Add(UiHelpers.Label("kynetic Klicker", left + 155, top + 12, 420, 45, 24F, Theme.Text, FontStyle.Bold));
        _contentPanel.Controls.Add(UiHelpers.Label("External 13 CPS clicker utility", left + 155, top + 62, 420, 30, 12F, Theme.Muted));

        var aboutCard = Card(left, top + 130, _contentPanel.Width - 110, 210);
        aboutCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _contentPanel.Controls.Add(aboutCard);

        aboutCard.Controls.Add(UiHelpers.Label("HOW TO USE THE CLICKER", 25, 24, 320, 32, 13F, Theme.Red, FontStyle.Bold));
        AddCheckLine(aboutCard, $"Press {KeyName(_clicker.ToggleKey)} to enable or disable the clicker", 70);
        AddCheckLine(aboutCard, "Hold Left Mouse Button to start clicking", 104);
        AddCheckLine(aboutCard, "Release Left Mouse Button to stop clicking instantly", 138);
        AddCheckLine(aboutCard, "Use the CPS slider on Main to choose 1-13 CPS", 172);

        var linksCard = Card(left, top + 365, _contentPanel.Width - 110, 165);
        linksCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        _contentPanel.Controls.Add(linksCard);

        linksCard.Controls.Add(UiHelpers.Label("LINKS", 25, 24, 180, 32, 13F, Theme.Red, FontStyle.Bold));
        linksCard.Controls.Add(UiHelpers.Label("guns.lol/tk746", 25, 70, 260, 30, 12F, Theme.Text));

        var openButton = new Button
        {
            Text = "Open Link",
            Location = new Point(25, 108),
            Size = new Size(150, 40)
        };
        UiHelpers.StyleButton(openButton, Theme.RedDark);
        openButton.Click += (_, _) => OpenUrl("https://guns.lol/tk746");
        linksCard.Controls.Add(openButton);
    }

    private RoundedPanel Card(int x, int y, int w, int h)
    {
        return new RoundedPanel
        {
            Location = new Point(x, y),
            Size = new Size(w, h),
            BackColor = Theme.SurfaceAlt,
            BorderColor = Theme.Border,
            Radius = 8
        };
    }

    private void AddKeybindRow(Control parent, string label, string value, int y, Button target, string mode)
    {
        var row = new RoundedPanel
        {
            Location = new Point(18, y),
            Size = new Size(parent.Width - 36, 56),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = Theme.Surface,
            BorderColor = Theme.Border,
            Radius = 6
        };
        parent.Controls.Add(row);

        row.Controls.Add(UiHelpers.Label(label, 16, 14, 170, 28, 11F, Theme.Text));

        target.Text = value;
        target.Location = new Point(row.Width - 266, 11);
        target.Size = new Size(128, 34);
        target.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        UiHelpers.StyleButton(target, Color.Black);
        target.FlatAppearance.BorderColor = Theme.Border;
        row.Controls.Add(target);

        var change = new Button
        {
            Text = "Change",
            Location = new Point(row.Width - 122, 11),
            Size = new Size(90, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        UiHelpers.StyleButton(change, Color.Black);
        change.Click += (_, _) =>
        {
            _waitingFor = mode;
            change.Text = "Press...";
            Focus();
        };
        row.Controls.Add(change);
    }

    private static void AddCheckLine(Control parent, string text, int y)
    {
        parent.Controls.Add(UiHelpers.Label("●", 25, y, 22, 24, 12F, Theme.Red, FontStyle.Bold));
        parent.Controls.Add(UiHelpers.Label(text, 55, y, 640, 24, 10F, Theme.Muted));
    }

    private void BuildFooter()
    {
        var footer = new Panel
        {
            Location = new Point(0, ClientSize.Height - FooterHeight),
            Size = new Size(ClientSize.Width, FooterHeight),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            BackColor = Theme.Surface
        };
        Controls.Add(footer);
        footer.BringToFront();

        footer.Controls.Add(UiHelpers.Label("●", 32, 24, 24, 24, 12F, Theme.Success));
        footer.Controls.Add(UiHelpers.Label("Running", 56, 22, 160, 28, 12F, Theme.Text));
        _statusFooter.Text = "Hold LMB to click  •  Release LMB to stop";
        _statusFooter.Location = new Point(56, 55);
        _statusFooter.Size = new Size(430, 25);
        _statusFooter.ForeColor = Theme.Muted;
        _statusFooter.BackColor = Color.Transparent;
        _statusFooter.Font = new Font("Segoe UI", 10F);
        footer.Controls.Add(_statusFooter);

        var startup = new Button
        {
            Text = "⊞   Start with Windows",
            Location = new Point(ClientSize.Width - 560, 20),
            Size = new Size(210, 52),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        UiHelpers.StyleButton(startup, Color.Black);
        startup.Click += (_, _) => MessageBox.Show("Startup toggle placeholder for open-source build.", "Kynetic Klicker");
        footer.Controls.Add(startup);

        var tray = new Button
        {
            Text = "⌂   Minimize to Tray",
            Location = new Point(ClientSize.Width - 335, 20),
            Size = new Size(190, 52),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        UiHelpers.StyleButton(tray, Color.Black);
        tray.Click += (_, _) =>
        {
            Hide();
            _trayIcon.Visible = true;
        };
        footer.Controls.Add(tray);

        var exit = new Button
        {
            Text = "×   Exit",
            Location = new Point(ClientSize.Width - 125, 20),
            Size = new Size(100, 52),
            Anchor = AnchorStyles.Right | AnchorStyles.Bottom
        };
        UiHelpers.StyleButton(exit, Theme.RedDark);
        exit.Click += (_, _) => Close();
        footer.Controls.Add(exit);
    }

    private void BuildTray()
    {
        _trayMenu.BackColor = Theme.SurfaceAlt;
        _trayMenu.ForeColor = Theme.Text;

        var showItem = new ToolStripMenuItem("Show Kynetic Klicker");
        showItem.Click += (_, _) =>
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        };

        var toggleItem = new ToolStripMenuItem("Toggle Klicker");
        toggleItem.Click += (_, _) =>
        {
            _clicker.Enabled = !_clicker.Enabled;
            UpdateStatus();
        };

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Close();

        _trayMenu.Items.Add(showItem);
        _trayMenu.Items.Add(toggleItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(exitItem);

        _trayIcon.Text = "Kynetic Klicker";
        _trayIcon.Icon = IconHelper.LoadAppIcon();
        _trayIcon.ContextMenuStrip = _trayMenu;
        _trayIcon.Visible = true;
        _trayIcon.DoubleClick += (_, _) =>
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
        };
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_waitingFor)) return;

        if (_waitingFor == "toggle")
        {
            _clicker.ToggleKey = e.KeyValue;
        }
        else if (_waitingFor == "exit")
        {
            _clicker.ExitKey = e.KeyValue;
        }

        _waitingFor = string.Empty;
        SaveSettings();
        RefreshCurrentPage();
        e.Handled = true;
    }

    private void RefreshCurrentPage()
    {
        if (_currentPage == "keybinds")
            ShowKeybindsPage();
        else if (_currentPage == "about")
            ShowAboutPage();
        else
            ShowMainPage();

        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (_statusValue.IsHandleCreated || _statusValue.Parent is not null)
        {
            _statusValue.Text = _clicker.Enabled ? "Enabled" : "Disabled";
            _statusValue.ForeColor = _clicker.Enabled ? Theme.Red : Theme.Muted;
        }

        _statusFooter.Text = _clicker.Enabled
            ? $"Enabled • Hold LMB to click at {_clicker.Cps} CPS"
            : "Hold LMB to click  •  Release LMB to stop";
    }

    private void SaveSettings()
    {
        _settings.ToggleKey = _clicker.ToggleKey;
        _settings.ExitKey = _clicker.ExitKey;
        _settings.Cps = Math.Clamp(_clicker.Cps, 1, 13);
        _settings.MinecraftOnly = _clicker.MinecraftOnly;
        _settings.Save();
    }

    private static string KeyName(int keyValue)
    {
        try
        {
            return ((Keys)keyValue).ToString().ToUpperInvariant();
        }
        catch
        {
            return keyValue.ToString();
        }
    }

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            MessageBox.Show(url, "Open this link");
        }
    }
}
