using Playnite.SDK;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MultiMcLibrary;

public class MultiMcLibraryClient : LibraryClient
{
    [DllImport("user32")]
    private static extern bool SetForegroundWindow(IntPtr hwnd);

    private readonly MultiMcLibrary _multiMcLibrary;

    public override bool IsInstalled
        => _multiMcLibrary.Launcher is { ExecutablePath: { } path } && File.Exists(path);

    public override string Icon => Path.Combine(MultiMcLibrary.AssemblyPath, _multiMcLibrary.Launcher?.IconName ?? "icon-multimc.png");

    public MultiMcLibraryClient(MultiMcLibrary multiMcLibrary)
    {
        _multiMcLibrary = multiMcLibrary;
    }

    public override void Open()
    {
        if (_multiMcLibrary.Launcher == null)
        {
            _multiMcLibrary.DisplayLauncherError();
            return;
        }
        
        var mainProc = Process.GetProcessesByName(_multiMcLibrary.Launcher.ProcessName).FirstOrDefault();
        if (mainProc is { HasExited: false })
        {
            SetForegroundWindow(mainProc.MainWindowHandle);
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _multiMcLibrary.Launcher.ExecutablePath,
                WorkingDirectory = _multiMcLibrary.Launcher.InstallDirectory
            });
        }
    }

    public override void Shutdown()
    {
        if (_multiMcLibrary.Launcher == null)
        {
            _multiMcLibrary.DisplayLauncherError();
            return;
        }

        var mainProc = Process.GetProcessesByName(_multiMcLibrary.Launcher.ProcessName).FirstOrDefault();
        if (mainProc is { HasExited: false })
        {
            mainProc.CloseMainWindow();
        }

        _multiMcLibrary.OnClientClosed();

        base.Shutdown();
    }
}