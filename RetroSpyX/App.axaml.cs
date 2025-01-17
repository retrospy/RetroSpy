using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace RetroSpy
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop!.Startup += (sender, args) =>
                {
                    string? source = null;
                    string? skin = null;
                    string? delay = null;
                    bool skipSetup = args.Args.Length >= 1 && args.Args[0] == "-skipSetup";

                    foreach (var item in args.Args)
                    {
                        if (item.StartsWith("-source="))
                        {
                            source = item.Substring(8);
                        }
                        else if (item.StartsWith("-skin="))
                        {
                            skin = item.Substring(6);
                        }
                        else if (item.StartsWith("-delay="))
                        {
                            delay = item.Substring(7);
                        }
                    }

                    desktop.MainWindow = new SetupWindow(skipSetup, source, skin, delay);
                };
            }

            base.OnFrameworkInitializationCompleted();
        }


    }
}