using Avalonia;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RetroSpy
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp(args)
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp(string[] args)
        {
            bool disableGPU = false;
            foreach(var arg in args)
            {
                if (arg == "DisableGPU")
                    disableGPU = true;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && disableGPU)
                return AppBuilder.Configure<App>()
                    .UsePlatformDetect().With(new Win32PlatformOptions { RenderingMode = new List<Win32RenderingMode> { Win32RenderingMode.Software } })
                    .LogToTrace();
            else
                return AppBuilder.Configure<App>()
                    .UsePlatformDetect()
                    .LogToTrace();
        }
    }
}