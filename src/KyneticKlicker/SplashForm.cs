// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace KyneticKlicker;

public sealed class SplashForm : Form
{
    private readonly System.Windows.Forms.Timer _timer = new();
    private int _ticks;

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(500, 300);
        BackColor = Theme.Background;
        Icon = IconHelper.LoadAppIcon();
        ShowInTaskbar = false;

        var logo = new PictureBox
        {
            Location = new Point(130, 32),
            Size = new Size(240, 145),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };
        UiHelpers.TryLoadLogo(logo);
        Controls.Add(logo);

        var title = UiHelpers.Label("kynetic Klicker", 0, 176, 500, 50, 25F, Theme.Red, FontStyle.Bold);
        title.TextAlign = ContentAlignment.MiddleCenter;
        Controls.Add(title);

        var footer = UiHelpers.Label("External 13 CPS Clicker", 0, 228, 500, 28, 10F, Theme.Muted);
        footer.TextAlign = ContentAlignment.MiddleCenter;
        Controls.Add(footer);

        _timer.Interval = 280;
        _timer.Tick += (_, _) =>
        {
            _ticks++;
            if (_ticks >= 4)
            {
                _timer.Stop();
                Close();
            }
        };

        Shown += (_, _) => _timer.Start();
    }
}
