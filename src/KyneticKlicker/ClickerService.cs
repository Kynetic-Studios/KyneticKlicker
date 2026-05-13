// Kynetic Klicker
// Copyright (c) 2026 KyneticStudios LLC
// All Rights Reserved.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace KyneticKlicker;

internal sealed class ClickerService : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private Thread? _worker;

    public int Cps { get; set; } = 13;
    public bool Enabled { get; set; }
    public bool MinecraftOnly { get; set; } = true;
    public int ToggleKey { get; set; } = NativeMethods.VK_F6;
    public int ExitKey { get; set; } = NativeMethods.VK_END;

    public event Action<bool>? EnabledChanged;
    public event Action? ExitRequested;

    public void Start()
    {
        if (_worker is not null) return;

        NativeMethods.TimeBeginPeriod(1);

        _worker = new Thread(Loop)
        {
            IsBackground = true,
            Name = "KyneticKlickerWorker",
            Priority = ThreadPriority.AboveNormal
        };

        _worker.Start();
    }

    private void Loop()
    {
        bool wasToggleDown = false;
        bool wasExitDown = false;
        bool wasLeftDown = false;

        var stopwatch = Stopwatch.StartNew();
        double nextClickMs = 0;

        while (!_cts.IsCancellationRequested)
        {
            bool exitDown = NativeMethods.IsKeyDown(ExitKey);
            if (exitDown && !wasExitDown)
            {
                ExitRequested?.Invoke();
                Thread.Sleep(180);
            }
            wasExitDown = exitDown;

            bool toggleDown = NativeMethods.IsKeyDown(ToggleKey);
            if (toggleDown && !wasToggleDown)
            {
                Enabled = !Enabled;
                EnabledChanged?.Invoke(Enabled);
                Thread.Sleep(180);
            }
            wasToggleDown = toggleDown;

            bool leftDown = NativeMethods.IsKeyDown(NativeMethods.VK_LBUTTON);

            if (!leftDown)
            {
                wasLeftDown = false;
                nextClickMs = stopwatch.Elapsed.TotalMilliseconds;
                Thread.Sleep(1);
                continue;
            }

            if (!wasLeftDown)
            {
                nextClickMs = stopwatch.Elapsed.TotalMilliseconds;
                wasLeftDown = true;
            }

            if (!Enabled || (MinecraftOnly && !IsMinecraftFocused()))
            {
                nextClickMs = stopwatch.Elapsed.TotalMilliseconds;
                Thread.Sleep(2);
                continue;
            }

            int safeCps = Math.Clamp(Cps, 1, 13);
            double intervalMs = 1000.0 / safeCps;
            double now = stopwatch.Elapsed.TotalMilliseconds;

            if (now >= nextClickMs)
            {
                NativeMethods.SendLeftClickCycle();
                nextClickMs += intervalMs;

                if (now - nextClickMs > intervalMs * 2)
                    nextClickMs = now + intervalMs;
            }
            else
            {
                double remaining = nextClickMs - now;
                if (remaining > 2)
                    Thread.Sleep(1);
                else
                    Thread.Yield();
            }
        }
    }

    private static bool IsMinecraftFocused()
    {
        try
        {
            IntPtr hwnd = NativeMethods.GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return false;

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint processId);
            if (processId == 0) return false;

            using Process process = Process.GetProcessById((int)processId);
            string name = process.ProcessName.ToLowerInvariant();

            var titleBuilder = new StringBuilder(256);
            NativeMethods.GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
            string title = titleBuilder.ToString().ToLowerInvariant();

            return name.Contains("java")
                   || name.Contains("javaw")
                   || name.Contains("minecraft")
                   || title.Contains("minecraft")
                   || title.Contains("lunar")
                   || title.Contains("badlion")
                   || title.Contains("feather")
                   || title.Contains("prism")
                   || title.Contains("modrinth");
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _worker?.Join(300); } catch { }
        NativeMethods.TimeEndPeriod(1);
        _cts.Dispose();
    }
}
