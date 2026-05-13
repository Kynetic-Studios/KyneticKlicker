// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System;
using System.Windows.Forms;

namespace KyneticKlicker;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var splash = new SplashForm();
        splash.ShowDialog();

        Application.Run(new MainForm());
    }
}
