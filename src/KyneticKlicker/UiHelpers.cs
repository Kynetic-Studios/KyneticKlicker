// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KyneticKlicker;

internal static class UiHelpers
{
    public static void TryLoadLogo(PictureBox box)
    {
        try
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", "kynetic-logo.png");
            if (!File.Exists(path))
                path = Path.Combine(AppContext.BaseDirectory, "kynetic-logo.png");

            if (File.Exists(path))
                box.Image = Image.FromFile(path);
        }
        catch
        {
            // Cosmetic only.
        }
    }

    public static void StyleButton(Button button, Color color)
    {
        button.BackColor = color;
        button.ForeColor = Theme.Text;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Theme.Red;
        button.FlatAppearance.BorderSize = 1;
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
    }

    public static Label Label(string text, int x, int y, int width, int height, float size, Color color, FontStyle style = FontStyle.Regular)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            ForeColor = color,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", size, style)
        };
    }
}
