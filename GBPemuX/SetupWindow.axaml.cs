using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
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

        private static void AvaloniaMessageBox(string? title, string message, ButtonEnum buttonType, MessageBox.Avalonia.Enums.Icon iconType)
        {
            using var source = new CancellationTokenSource();
            _ = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow(title ?? "Unknown Title Argument", message, buttonType, iconType)
                        .Show().ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
            Dispatcher.UIThread.MainLoop(source.Token);
        }

        public SetupWindow()
        {
            InitializeComponent();

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
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? strWorkPath = Path.GetDirectoryName(strExeFilePath);

            _vm.FilterCOMPorts = Properties.Settings.Default.FilterCOMPorts;

            MenuItem menuItem = new()
            {
                Header = "COM Ports"
            };

            COMMenu = menuItem;

            if (_vm.FilterCOMPorts == true)
            {
                ((AvaloniaList<object>)OptionsMenu.Items).Add(COMMenu);
            }
            else
            {
                firstTime = true;
            }

            _portListUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _portListUpdateTimer.Tick += (sender, e) => UpdatePortListThread();
            _portListUpdateTimer.Start();

            UpdatePortList();
            _vm.Ports.SelectIdFromText(Properties.Settings.Default.Port);
        }

        bool firstTime = false;
        MenuItem COMMenu;

        private void FilterCOM_Checked(object sender, RoutedEventArgs e)
        {
            if (!firstTime)
            {
                firstTime = true;
                return;
            }

            if (sender is MenuItem)
                FilterCOMCheckbox.IsChecked = !FilterCOMCheckbox.IsChecked;

            if (FilterCOMCheckbox.IsChecked == true)
            {

                _vm.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;
                Properties.Settings.Default.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;

                MenuItem menuItem = new()
                {
                    Header = "COM Ports"
                };

                COMMenu = menuItem;

                ((AvaloniaList<object>)OptionsMenu.Items).Add(COMMenu);
            }
            else
            {
                _vm.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;
                Properties.Settings.Default.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;

                ((AvaloniaList<object>)OptionsMenu.Items).Remove(COMMenu);
            }
        }

        private async void COMPortClicked(object? sender, RoutedEventArgs e)
        {
            try
            {
                string? port = ((MenuItem?)sender)?.Header.ToString();
                using SerialPort _serialPort = new(port ?? "COMX", 115200, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                _serialPort.Open();
                _serialPort.Write("\x88\x33\x0F\x00\x00\x00\x0F\x00\x00");
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
                        AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                        Show();
                    }
                }
            }
            catch (Exception ex)
            {
                AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
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
                                        AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
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
                            if (((AvaloniaList<object>)COMMenu.Items).Count != arduinoPorts.Count)
                            {
                                ((AvaloniaList<object>)COMMenu.Items).Clear();
                                foreach (var port in arduinoPorts)
                                {
                                    var newMenuItem = new MenuItem
                                    {
                                        Header = port
                                    };
                                    newMenuItem.Click += COMPortClicked;
                                    ((AvaloniaList<object>)COMMenu.Items).Add(newMenuItem);
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

            List<string> ports = new();
            foreach (COMPortInfo port in comPortInformation)
            {
                if ((_vm.FilterCOMPorts == true || port.FriendlyName?.Contains("Arduino") == true))
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