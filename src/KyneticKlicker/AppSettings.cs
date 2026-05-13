// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System;
using System.IO;
using System.Text.Json;

namespace KyneticKlicker;

internal sealed class AppSettings
{
    public int ToggleKey { get; set; } = NativeMethods.VK_F6;
    public int ExitKey { get; set; } = NativeMethods.VK_END;
    public int Cps { get; set; } = 13;
    public bool MinecraftOnly { get; set; } = true;

    private static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KyneticKlicker");

    private static string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new AppSettings();

            string json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();

            settings.Cps = Math.Clamp(settings.Cps, 1, 13);

            if (settings.ToggleKey <= 0)
                settings.ToggleKey = NativeMethods.VK_F6;

            if (settings.ExitKey <= 0)
                settings.ExitKey = NativeMethods.VK_END;

            return settings;
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Settings persistence should never crash the app.
        }
    }
}
