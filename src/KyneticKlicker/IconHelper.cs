// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KyneticKlicker;

internal static class IconHelper
{
    public static Icon LoadAppIcon()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico"),
            Path.Combine(AppContext.BaseDirectory, "app.ico"),
            Path.Combine(Application.StartupPath, "Assets", "app.ico"),
            Path.Combine(Application.StartupPath, "app.ico")
        ];

        foreach (string candidate in candidates)
        {
            try
            {
                if (File.Exists(candidate))
                    return new Icon(candidate);
            }
            catch
            {
                // Try the next path.
            }
        }

        return SystemIcons.Application;
    }
}
