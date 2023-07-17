using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Reflection.Metadata;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


namespace GBPemu
{

    public class COMPortInfo
    {
        public string? PortName { get; set; }
        public string? FriendlyName { get; set; }
    }

    public partial class SetupWindow : Window
    {
        private readonly SetupWindowViewModel _vm;
        private readonly DispatcherTimer _portListUpdateTimer;
        private readonly ResourceManager _resources;
        private bool isClosing;

        private List<string>? arduinoPorts;
        private void UpdatePortListThread()
        {
            Thread thread = new(UpdatePortList);
            thread.Start();
        }

        private static void AvaloniaMessageBox(string? title, string message, ButtonEnum buttonType, MsBox.Avalonia.Enums.Icon iconType)
        {
            using var source = new CancellationTokenSource();
            _ = MsBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandard(title ?? "Unknown Title Argument", message, buttonType, iconType)
                        .ShowAsync().ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
            Dispatcher.UIThread.MainLoop(source.Token);
        }

        public SetupWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                isClosing = true;
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var localDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".local");

                if (!Directory.Exists(localDir))
                {
                    Directory.CreateDirectory(localDir);
                    var shareDir = Path.Join(localDir, "share");
                    if (!Directory.Exists(shareDir))
                    {
                        Directory.CreateDirectory(shareDir);
                    }   
                }
            }

            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }


            isClosing = false;
            _vm = new SetupWindowViewModel();
            DataContext = _vm;
            _resources = Properties.Resources.ResourceManager;

            _vm.FilterCOMPorts = Properties.Settings.Default.FilterCOMPorts || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            FilterCOMCheckbox.IsChecked = _vm.FilterCOMPorts;
            
            MenuItem menuItem = new()
            {
                Header = "COM Ports"
            };

            COMMenu = menuItem;

            if (_vm.FilterCOMPorts == true)
            {
                COMMenuActive = true;
                ((ItemCollection)OptionsMenu.Items).Add(COMMenu);
            }

            _portListUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _portListUpdateTimer.Tick += (sender, e) => UpdatePortListThread();
            _portListUpdateTimer.Start();

            UpdatePortList();
        }


        MenuItem COMMenu;
        private bool COMMenuActive = false;
        private void FilterCOM_Checked(object sender, RoutedEventArgs e)
        {
            FilterCOMCheckbox.IsChecked = !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || FilterCOMCheckbox.IsChecked == true;

            if (sender is MenuItem)
                FilterCOMCheckbox.IsChecked = !FilterCOMCheckbox.IsChecked == true || !RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (!COMMenuActive && FilterCOMCheckbox.IsChecked == true)
            {
                COMMenuActive = true;
                _vm.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;
                Properties.Settings.Default.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;

                MenuItem menuItem = new()
                {
                    Header = "COM Ports"
                };

                COMMenu = menuItem;

                ((ItemCollection)OptionsMenu.Items).Add(COMMenu);
            }
            else if(FilterCOMCheckbox.IsChecked == false)
            {
                COMMenuActive = false;
                _vm.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;
                Properties.Settings.Default.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;

                ((ItemCollection)OptionsMenu.Items).Remove(COMMenu);
            }
        }

        private async void COMPortClicked(object? sender, RoutedEventArgs e)
        {

            string? port = ((MenuItem?)sender)?.Header?.ToString();

            Properties.Settings.Default.Port = _vm.Ports.SelectedItem;
            Properties.Settings.Default.FilterCOMPorts = _vm.FilterCOMPorts;
            Properties.Settings.Default.Save();

            try
            {
                if (Dispatcher.UIThread.CheckAccess())
                {
                    IControllerReader? reader = InputSource.PRINTER.BuildReader(port);
                    var g = new GameBoyPrinterEmulatorWindow(reader, this);
                    await g.ShowDialog(this);
                }
                else
                {
                    Dispatcher.UIThread.Post(async () =>
                    {
                        IControllerReader? reader = InputSource.PRINTER.BuildReader(port);
                        var g = new GameBoyPrinterEmulatorWindow(reader, this);
                        await g.ShowDialog(this);
                    });
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                Show();
            }
            catch (Exception ex)
            {
                AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                Show();
            }
        }

        private readonly object updatePortLock = new();

        private async void UpdatePortList()
        {
            if (!isClosing && Monitor.TryEnter(updatePortLock))
            {
                try
                {
                    arduinoPorts = SetupCOMPortInformation();

                    if (_vm.FilterCOMPorts == false)
                    {
                        foreach (string port in arduinoPorts)
                        {
                            using SerialPort _serialPort = new(port, 115200, Parity.None, 8, StopBits.One)
                            {
                                Handshake = Handshake.None,
                                ReadTimeout = 500,
                                WriteTimeout = 500
                            };

                            try
                            {
                                _serialPort.Open();
                            }
                            catch (Exception)
                            {
                                continue;
                            }

                            try
                            {
                                _serialPort.Write("\x88\x33\x0F\x00\x00\x00\x0F\x00\x00");
                            }
                            catch (Exception)
                            {
                                _serialPort.Close();
                                continue;
                            }

                            try
                            {
                                string? result = null;
                                do
                                {
                                    result = _serialPort.ReadLine();
                                } while (result != null && (result.StartsWith("!", StringComparison.Ordinal) || result.StartsWith("#", StringComparison.Ordinal)));

                                if (result == "parse_state:0\r" || result?.Contains("d=debug") == true)
                                {
                                    _serialPort.Close();
                                    Thread.Sleep(1000);


                                    Properties.Settings.Default.Port = _vm.Ports.SelectedItem;
                                    Properties.Settings.Default.FilterCOMPorts = _vm.FilterCOMPorts;
                                    Properties.Settings.Default.Save();

                                    try
                                    {
                                        if (Dispatcher.UIThread.CheckAccess())
                                        {
                                            if (this.IsVisible)
                                            {
                                                IControllerReader reader = InputSource.PRINTER.BuildReader(port);
                                                var g = new GameBoyPrinterEmulatorWindow(reader, this);
                                                await g.ShowDialog(this);
                                            }
                                        }
                                        else
                                        {
                                            Dispatcher.UIThread.Post(async () =>
                                            {
                                                if (this.IsVisible)
                                                {
                                                    IControllerReader reader = InputSource.PRINTER.BuildReader(port);
                                                    var g = new GameBoyPrinterEmulatorWindow(reader, this);
                                                    await g.ShowDialog(this);
                                                }
                                            });
                                        }


                                    }
                                    catch (UnauthorizedAccessException ex)
                                    {
                                        AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                                        Show();
                                    }

                                }
                                else
                                {
                                    _serialPort.Close();
                                    continue;
                                }
                            }
                            catch (Exception)
                            {
                                _serialPort.Close();
                                continue;
                            }
                        }
                    }
                    else
                    {
                        Dispatcher.UIThread.Post((Action)delegate
                        {
                            if (((ItemCollection)COMMenu.Items).Count != arduinoPorts.Count)
                            {
                                ((ItemCollection)COMMenu.Items).Clear();
                                foreach (var port in arduinoPorts)
                                {
                                    var newMenuItem = new MenuItem
                                    {
                                        Header = port
                                    };
                                    newMenuItem.Click += COMPortClicked;
                                    ((ItemCollection)COMMenu.Items).Add(newMenuItem);
                                }
                            }
                        });
                    }

                }
                catch (TaskCanceledException)
                {
                    // Closing the window can cause this due to a race condition
                }
                finally
                {
                    Monitor.Exit(updatePortLock);
                }
            }
        }

        private static string[] GetUSBCOMDevices()
        {
            List<string> list = new();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {

                ManagementObjectSearcher searcher2 = new("SELECT * FROM Win32_PnPEntity");
                foreach (ManagementObject mo2 in searcher2.Get().Cast<ManagementObject>())
                {
                    if (mo2["Name"] != null)
                    {
                        string? name = mo2["Name"].ToString();
                        // Name will have a substring like "(COM12)" in it.
                        if (name?.Contains("(COM") == true)
                        {
                            list.Add(name);
                        }
                    }
                }
                searcher2.Dispose();
            }

            // remove duplicates, sort alphabetically and convert to array
            string[] usbDevices = list.Distinct().OrderBy(s => s).ToArray();
            return usbDevices;
        }

        private List<string> SetupCOMPortInformation()
        {
            List<COMPortInfo> comPortInformation = new();

            string[] portNames = SerialPort.GetPortNames();
            foreach (string s in portNames)
            {
                // s is like "COM14"
                COMPortInfo ci = new()
                {
                    PortName = s,
                    FriendlyName = s
                };
                comPortInformation.Add(ci);
            }

            string[] usbDevs = GetUSBCOMDevices();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (string s in usbDevs)
                {
                    // Name will be like "USB Bridge (COM14)"
                    int start = s.IndexOf("(COM", StringComparison.Ordinal) + 1;
                    if (start >= 0)
                    {
                        int end = s.IndexOf(")", start + 3, StringComparison.Ordinal);
                        if (end >= 0)
                        {
                            // cname is like "COM14"
                            string cname = s[start..end];
                            for (int i = 0; i < comPortInformation.Count; i++)
                            {
                                if (comPortInformation[i].PortName == cname)
                                {
                                    comPortInformation[i].FriendlyName = s.Remove(start - 1).TrimEnd();
                                }
                            }
                        }
                    }
                }
            }

            List<string> ports = new();
            foreach (COMPortInfo port in comPortInformation)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || (_vm.FilterCOMPorts == true || port.FriendlyName?.Contains("Arduino") == true))
                {
                    ports.Add(port.PortName ?? "COMX");
                }
                else if (port.FriendlyName?.Contains("CH340") == true || port.FriendlyName?.Contains("CH341") == true)
                {
                    ports.Add(port.PortName ?? "COMX");
                }
            }

            return ports;
        }

        private void ComPortCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdatePortList();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isClosing = true;
        }

        private void Window_Open(object sender, EventArgs e)
        {
            UpdatePortList();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string url = String.Format("https://retro-spy.com/about-retrospy/?version={0}&buildtime={1}",
                System.Web.HttpUtility.UrlEncode(Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString()), System.Web.HttpUtility.UrlEncode(Properties.Resources.BuildDate));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }

    public class SetupWindowViewModel : INotifyPropertyChanged
    {
        public ListView<string> Ports { get; set; }
        public bool FilterCOMPorts { get; set; }

        private bool _comPortOptionVisibility;

        public bool ComPortOptionVisibility
        {
            get => _comPortOptionVisibility;
            set
            {
                _comPortOptionVisibility = value;
                NotifyPropertyChanged("ComPortOptionVisibility");
            }
        }


        public SetupWindowViewModel()
        {
            Ports = new ListView<string>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged(string prop)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

     

    }
}