using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using Renci.SshNet;
using RetroSpy.Readers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using ComboBox = Avalonia.Controls.ComboBox;
using File = System.IO.File;
using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using SelectionChangedEventArgs = Avalonia.Controls.SelectionChangedEventArgs;
using Window = Avalonia.Controls.Window;

namespace RetroSpy
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
        private readonly DispatcherTimer _xiAndGamepadListUpdateTimer;
        private Collection<Skin>? _skins;
        private readonly Collection<string> _excludedSources;
        private readonly ResourceManager _resources;
        private bool isClosing;


        private void KeybindingBehavior_Checked(object sender, RoutedEventArgs e)
        {

            if (sender is MenuItem)
                KeybindingCheckbox.IsChecked = !KeybindingCheckbox.IsChecked;

            _vm.LegacyKeybindingBehavior = KeybindingCheckbox.IsChecked ?? false;
            Properties.Settings.Default.LegacyKeybindingBehavior = KeybindingCheckbox.IsChecked ?? false;
        }

        private void FilterCOM_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
                FilterCOMCheckbox.IsChecked = !FilterCOMCheckbox.IsChecked;

            _vm.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;
            Properties.Settings.Default.FilterCOMPorts = FilterCOMCheckbox.IsChecked ?? false;
        }

        private void DoNotSavePassword_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
                DoNotSavePasswordCheckbox.IsChecked = !DoNotSavePasswordCheckbox.IsChecked;

            _vm.DontSavePassword = DoNotSavePasswordCheckbox.IsChecked ?? false;
            Properties.Settings.Default.DontSavePassword = DoNotSavePasswordCheckbox.IsChecked ?? false;
        }

        private void LagFix_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem)
                LagFixCheckbox.IsChecked = !LagFixCheckbox.IsChecked;

            _vm.UseLagFix = LagFixCheckbox.IsChecked ?? false;
            Properties.Settings.Default.UseLagFix = LagFixCheckbox.IsChecked ?? false;
        }

            private void UpdatePortListThread()
            {
                if (letUpdatePortThreadRun)
                {
                    Thread thread = new(UpdatePortList);
                    thread.Start();
                }
            }

        public async Task<string?> GetPath()
        {
            OpenFolderDialog dialog = new();

            string? result = await dialog.ShowAsync(this);

            return result;
        }

        private async void CustomSkinPath_Click(object sender, RoutedEventArgs e)
        {
            string? _path = await GetPath();

            Properties.Settings.Default.CustomSkinPath = _path ?? String.Empty;
            Properties.Settings.Default.Save();

            string? baseDir = Path.GetDirectoryName(Environment.ProcessPath);

            string skinsDirectory = "skins";

            if (baseDir != null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && baseDir.Contains("MacOS") && System.IO.File.Exists(Path.Join(baseDir, "../Info.plist")))
                    skinsDirectory = Path.Join(baseDir, Path.Join("../../../", "skins"));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && baseDir.Contains("bin") && Directory.Exists(Path.Join(baseDir, Path.Join("..", "skins"))))
                    skinsDirectory = Path.Join(baseDir, Path.Join("..", "skins"));
                else
                    skinsDirectory = Path.Join(baseDir, "skins");
            }

            LoadResults results = Skin.LoadAllSkinsFromParentFolder(skinsDirectory, Properties.Settings.Default.CustomSkinPath);
            _skins = results.SkinsLoaded;

            _vm.Skins.UpdateContents(_skins?.Where(x => x.Type == _vm.Sources.SelectedItem));

            PopulateSources();
        }

        private void ReloadSkins_Click(object sender, RoutedEventArgs e)
        {
            string? baseDir = Path.GetDirectoryName(Environment.ProcessPath);

            string skinsDirectory = "skins";

            if (baseDir != null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && baseDir.Contains("MacOS") && System.IO.File.Exists(Path.Join(baseDir, "../Info.plist")))
                    skinsDirectory = Path.Join(baseDir, Path.Join("../../../", "skins"));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && baseDir.Contains("bin") && Directory.Exists(Path.Join(baseDir, Path.Join("..", "skins"))))
                    skinsDirectory = Path.Join(baseDir, Path.Join("..", "skins"));
                else
                    skinsDirectory = Path.Join(baseDir, "skins");
            }

            if (!Directory.Exists(skinsDirectory))
            {
                AvaloniaMessageBoxDialog(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), "Could not find skins folder!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                Environment.Exit(-1);
            }

            LoadResults results = Skin.LoadAllSkinsFromParentFolder(skinsDirectory, Properties.Settings.Default.CustomSkinPath);
            _skins = results.SkinsLoaded;

            _vm.Skins.UpdateContents(_skins?.Where(x => x.Type == _vm.Sources.SelectedItem));

            PopulateSources();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isClosing = true;
            Environment.Exit(0);
        }

        public SetupWindow() : this(false)
        {

        }

        private bool letUpdatePortThreadRun = false;
        public SetupWindow(bool skipSetup = false)
        {
            try
            {
                InitializeComponent();

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
                _vm = new SetupWindowViewModel(this);
                DataContext = _vm;
                _excludedSources = new Collection<string>();
                _resources = Properties.Resources.ResourceManager;

                string? baseDir = Path.GetDirectoryName(Environment.ProcessPath);

                string skinsDirectory = "skins";

                if (baseDir != null)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && baseDir.Contains("MacOS") &&
                        System.IO.File.Exists(Path.Join(baseDir, "../Info.plist")))
                        skinsDirectory = Path.Join(baseDir, Path.Join("../../../", "skins"));
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                             baseDir.Contains("bin") &&
                             Directory.Exists(Path.Join(baseDir, Path.Join("..", "skins"))))
                        skinsDirectory = Path.Join(baseDir, Path.Join("..", "skins"));
                    else
                        skinsDirectory = Path.Join(baseDir, "skins");
                }

                if (!Directory.Exists(skinsDirectory))
                {
                    AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture),
                        "Could not find skins folder!", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                    Environment.Exit(-1);

                }

                LoadResults results =
                    Skin.LoadAllSkinsFromParentFolder(skinsDirectory, Properties.Settings.Default.CustomSkinPath);
                _skins = results.SkinsLoaded;

                _vm.Skins.UpdateContents(_skins?.Where(x => x.Type == InputSource.DEFAULT));

                string[] hiddenConsoles = Properties.Settings.Default.HiddleConsoleList.Split(';');
                foreach (string source in hiddenConsoles)
                {
                    if (source.Length > 0)
                    {
                        _excludedSources.Add(source);
                    }
                }

                PopulateSources();

                _vm.Username = _vm.Sources.SelectedItem == InputSource.MISTER
                    ? Properties.Settings.Default.MisterUsername
                    : Properties.Settings.Default.BeagleboneUsername;

                txtPassword.Text = _vm.Sources.SelectedItem == InputSource.MISTER
                    ? Properties.Settings.Default.MisterPassword
                    : Properties.Settings.Default.BeaglebonePassword;


                _vm.DelayInMilliseconds = Properties.Settings.Default.Delay;
                txtDelay.Text = _vm.DelayInMilliseconds.ToString();

                _vm.StaticViewerWindowName = Properties.Settings.Default.StaticViewerWindowName;

                _vm.LegacyKeybindingBehavior = Properties.Settings.Default.LegacyKeybindingBehavior;
                KeybindingCheckbox.IsChecked = _vm.LegacyKeybindingBehavior;

                _vm.FilterCOMPorts = Properties.Settings.Default.FilterCOMPorts;
                FilterCOMCheckbox.IsChecked = _vm.FilterCOMPorts;

                _vm.DontSavePassword = Properties.Settings.Default.DontSavePassword;
                DoNotSavePasswordCheckbox.IsChecked = _vm.DontSavePassword;

                _vm.UseLagFix = Properties.Settings.Default.UseLagFix;
                LagFixCheckbox.IsChecked = _vm.UseLagFix;

                UpdatePortList();
                _vm.Ports.SelectIdFromText(Properties.Settings.Default.Port);
                _vm.Ports2.SelectIdFromText(Properties.Settings.Default.Port2);
                letUpdatePortThreadRun = true;
                _vm.XIAndGamepad.SelectFirst();
                _vm.Sources.SelectId(Properties.Settings.Default.Source);
                _vm.Skins.SelectId(Properties.Settings.Default.Skin);
                _vm.Hostname = Properties.Settings.Default.Hostname;
                txtHostname.Text = _vm.Hostname;

                _portListUpdateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                _portListUpdateTimer.Tick += (sender, e) => UpdatePortListThread();
                _portListUpdateTimer.Start();

                _xiAndGamepadListUpdateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                _xiAndGamepadListUpdateTimer.Tick += (sender, e) =>
                {
                    if (_vm.Sources.SelectedItem == InputSource.PAD)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            UpdateGamepadList();
                    }
                    else if (_vm.Sources.SelectedItem == InputSource.PC360)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            UpdateXIList();
                    }
                    else if (_vm.Sources.SelectedItem == InputSource.LINUX)
                    {
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            UpdateJoystickList();
                    }
                    else if (_vm.Sources.SelectedItem == InputSource.DOLPHIN)
                    {
                        UpdateMayflashList();
                    }
                    //else if (_vm.Sources.SelectedItem == InputSource.XBOX)
                    //{
                    //    updateBeagleList();
                    //}
                    //else if (_vm.Sources.SelectedItem == InputSource.WII)
                    //{
                    //    updateBeagleI2CList();
                    //}
                };
                _xiAndGamepadListUpdateTimer.Start();

                List<uint> defaultMisterControllers = new();
                for (uint i = 0; i < 10; ++i)
                {
                    defaultMisterControllers.Add(i);
                }

                _vm.MisterGamepad.UpdateContents(defaultMisterControllers);

                if (results.ParseErrors != null && results.ParseErrors.Count > 0)
                {
                    ShowSkinParseErrors(results.ParseErrors);
                }

                if (skipSetup)
                {
                    SourceSelectComboBox_SelectionChanged(null, null);
                    Skin_SelectionChanged(null, null);
                    GoButton_Click(null, null);
                }
                else
                {
                    Show();
                }
            }
            catch (TypeInitializationException ex)
            {
                AvaloniaMessageBox(_resources == null ? "Invalid Resource Handle" : _resources.GetString("RetroSpy", CultureInfo.CurrentUICulture) ?? "Unknown Resource String: RetroSpy", ex?.InnerException?.Message + "\n\n" + ex?.InnerException?.StackTrace, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                AvaloniaMessageBox(_resources == null ? "Invalid Resource Handle" : _resources.GetString("RetroSpy", CultureInfo.CurrentUICulture) ?? "Unknown Resource String: RetroSpy", ex.Message + "\n\n" + ex.StackTrace, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                Environment.Exit(-1);
            }
        }

        private async void GoButton_Click(object? sender, RoutedEventArgs? e)
        {
            ViewWindow? v = null;

            Properties.Settings.Default.Port = _vm.Ports.SelectedItem;
            Properties.Settings.Default.Port2 = _vm.Ports2.SelectedItem;
            Properties.Settings.Default.Source = _vm.Sources.GetSelectedId();
            Properties.Settings.Default.Skin = _vm.Skins.GetSelectedId();
            try
            {
                _vm.DelayInMilliseconds = Int32.Parse(txtDelay.Text);
            }
            catch (Exception)
            {
                _vm.DelayInMilliseconds = 0;
            }
            Properties.Settings.Default.Delay = _vm.DelayInMilliseconds;
            Properties.Settings.Default.Background = _vm.Backgrounds.GetSelectedId();
            Properties.Settings.Default.Hostname = _vm.Hostname;
            Properties.Settings.Default.StaticViewerWindowName = _vm.StaticViewerWindowName;
            Properties.Settings.Default.LegacyKeybindingBehavior = _vm.LegacyKeybindingBehavior;
            Properties.Settings.Default.FilterCOMPorts = _vm.FilterCOMPorts;
            Properties.Settings.Default.DontSavePassword = _vm.DontSavePassword;
            Properties.Settings.Default.UseLagFix = _vm.UseLagFix;

            if (_vm.Sources.SelectedItem == InputSource.MISTER)
            {
                Properties.Settings.Default.MisterUsername = _vm.Username;
                Properties.Settings.Default.MisterPassword = _vm.DontSavePassword ? "" : txtPassword.Text;
            }
            else
            {
                Properties.Settings.Default.BeagleboneUsername = _vm.Username;
                Properties.Settings.Default.BeaglebonePassword = _vm.DontSavePassword ? "" : txtPassword.Text;
            }
            Properties.Settings.Default.Save();

            try
            {
                IControllerReader? reader = null;
                if (_vm.Sources.SelectedItem == InputSource.PAD)
                {
                    if (_vm.Sources.SelectedItem.BuildReader != null)
                        reader = _vm.Sources.SelectedItem.BuildReader(_vm.XIAndGamepad.SelectedItem.ToString(CultureInfo.CurrentCulture), false);
                }
                else if (_vm.Sources.SelectedItem == InputSource.PC360 || _vm.Sources.SelectedItem == InputSource.DOLPHIN)
                {
                    if (_vm.Sources.SelectedItem.BuildReader != null)
                        reader = _vm.Sources.SelectedItem.BuildReader(_vm.XIAndGamepad.SelectedItem.ToString(CultureInfo.CurrentCulture), false);
                }
                else if (_vm.Sources.SelectedItem == InputSource.LINUX)
                {
                    if (_vm.Sources.SelectedItem.BuildReader != null)
                        reader = _vm.Sources.SelectedItem.BuildReader(_vm.XIAndGamepad.SelectedItem.ToString(CultureInfo.CurrentCulture), false);
                }
                else if (_vm.Sources.SelectedItem == InputSource.PCKEYBOARD || _vm.Sources.SelectedItem == InputSource.LINUXKEY)
                {
                    reader = _vm.Sources.SelectedItem.BuildReader3;
                }
                else if (_vm.Sources.SelectedItem != null && _vm.Sources.SelectedItem.BuildReader4 != null && (_vm.Sources.SelectedItem == InputSource.XBOX || _vm.Sources.SelectedItem == InputSource.PSCLASSIC ||
                         _vm.Sources.SelectedItem == InputSource.SWITCH || _vm.Sources.SelectedItem == InputSource.XBOX360 ||
                         _vm.Sources.SelectedItem == InputSource.GENMINI || _vm.Sources.SelectedItem == InputSource.GENMINI2 || _vm.Sources.SelectedItem == InputSource.C64MINI ||
                         _vm.Sources.SelectedItem == InputSource.NEOGEOMINI || _vm.Sources.SelectedItem == InputSource.PS3
                         || _vm.Sources.SelectedItem == InputSource.PS4 || _vm.Sources.SelectedItem == InputSource.TG16MINI
                         || _vm.Sources.SelectedItem == InputSource.ATARIVCS || _vm.Sources.SelectedItem == InputSource.EVERCADE
                         || _vm.Sources.SelectedItem == InputSource.PS4CRONUS || _vm.Sources.SelectedItem == InputSource.A500MINI || _vm.Sources.SelectedItem == InputSource.PS4USB))
                {
                    reader = _vm.Sources.SelectedItem.BuildReader4(txtHostname.Text, txtUsername.Text, txtPassword.Text);
                }
                else if (_vm.Sources.SelectedItem != null && _vm.Sources.SelectedItem.BuildReader5 != null && _vm.Sources.SelectedItem == InputSource.MISTER)
                {
                    reader = _vm.Sources.SelectedItem.BuildReader5(txtHostname.Text, txtUsername.Text, txtPassword.Text, _vm.MisterGamepad.SelectedItem.ToString(CultureInfo.CurrentCulture));
                }
                else if (_vm.Sources.SelectedItem != null && _vm.Sources.SelectedItem.BuildReader2 != null && (_vm.Sources.SelectedItem == InputSource.PADDLES || _vm.Sources.SelectedItem == InputSource.CD32
                            || _vm.Sources.SelectedItem == InputSource.ATARI5200 || _vm.Sources.SelectedItem == InputSource.COLECOVISION
                            || _vm.Sources.SelectedItem == InputSource.GAMECUBE))
                {
                    if (_vm.Ports.SelectedItem == _vm.Ports2.SelectedItem)
                    {
                        throw new ConfigParseException(_resources.GetString("Port1And2CannotBeTheSame", CultureInfo.CurrentUICulture) ?? "Unknown Resource String: Port1And2CannotBeTheSame");
                    }

                    reader = _vm.Sources.SelectedItem.BuildReader2(_vm.Ports.SelectedItem, _vm.Ports2.SelectedItem, _vm.UseLagFix);
                }

                //else if (_vm.Sources.SelectedItem == InputSource.XBOX)
                //{
                //    reader = _vm.Sources.SelectedItem.BuildReader(_vm.XIAndGamepad.SelectedItem.ToString());
                //}
                //else if (_vm.Sources.SelectedItem == InputSource.WII)
                //{
                //    reader = _vm.Sources.SelectedItem.BuildReader(_vm.XIAndGamepad.SelectedItem.ToString());
                //}
                else
                {
                    reader = _vm.Sources.SelectedItem == null || _vm.Sources.SelectedItem.BuildReader == null
                        ? null : _vm.Sources.SelectedItem.BuildReader(_vm.Ports.SelectedItem, _vm.UseLagFix);
                }
                if (_vm.DelayInMilliseconds > 0)
                {
                    reader = new DelayedControllerReader(reader, _vm.DelayInMilliseconds, _vm.LegacyKeybindingBehavior);
                }


                _portListUpdateTimer.Stop();
                v = new ViewWindow(this, _vm.Skins.SelectedItem,
                               _vm.Backgrounds.SelectedItem,
                               reader, _vm.StaticViewerWindowName);
                await v.ShowDialog(this);
                _portListUpdateTimer.Start();


            }
            catch (ConfigParseException ex)
            {
                AvaloniaMessageBoxDialog(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);

            }
            catch (System.Net.Sockets.SocketException)
            {
                AvaloniaMessageBoxDialog(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), string.Format(new CultureInfo("en-US"), "Cannot connect to {0}.", txtHostname.Text), ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                AvaloniaMessageBoxDialog(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
            }
            catch (SSHMonitorDisconnectException)
            {
                v?.Close();
            }
            catch (Exception ex)
            {
                AvaloniaMessageBoxDialog(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), ex.Message + "\n\n" + ex.StackTrace, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                v?.Close();
            }

            Show();
        }

        private void PopulateSources()
        {
            List<InputSource> prunedSources = new();
            foreach (InputSource source in InputSource.GetAllSources())
            {
                if (!_excludedSources.Contains(source.Name))
                {
                    prunedSources.Add(source);
                }
            }
            _vm.Sources.UpdateContents(prunedSources);
        }

        private static void AvaloniaMessageBox(string? title, string message, ButtonEnum buttonType, MessageBox.Avalonia.Enums.Icon iconType)
        {
            using var source = new CancellationTokenSource();
            _ = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow(title ?? "Unknown Title Argument", message, buttonType, iconType)
                        .Show().ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
            Dispatcher.UIThread.MainLoop(source.Token);
        }


        private void AvaloniaMessageBoxDialog(string? title, string message, ButtonEnum buttonType, MessageBox.Avalonia.Enums.Icon iconType)
        { 
            var m = MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow(title ?? "Unknown Title Argument", message, buttonType, iconType);
            m.ShowDialog(this);
        }

        private void ShowSkinParseErrors(Collection<string> errs)
        {
            StringBuilder msg = new();
            _ = msg.AppendLine("Some skins were unable to be parsed:");
            foreach (string err in errs)
            {
                _ = msg.AppendLine(err);
            }

            AvaloniaMessageBox(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), msg.ToString(), ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
        }

        private async void AddRemove_Click(object sender, RoutedEventArgs e)
        {
            Window w = new AddRemoveWindow(InputSource.GetAllSources(), _excludedSources);
            await w.ShowDialog(this);

            PopulateSources();

            string hiddenConsoleList = "";
            foreach (string source in _excludedSources)
            {
                hiddenConsoleList += source + ";";
            }

            Properties.Settings.Default.HiddleConsoleList = hiddenConsoleList;
            Properties.Settings.Default.Save();
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


    private void DelayTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (String.IsNullOrEmpty(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "0";
            }
            else if (Int32.Parse(((TextBox)sender).Text) > 300000)
            {
                ((TextBox)sender).Text = 300000.ToString();
            }
        }

        private void ComPortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //((ComboBox)sender).SelectionChanged -= ComPortCombo_SelectionChanged;

            //UpdatePortList();
            //((ComboBox)sender).SelectionChanged += ComPortCombo_SelectionChanged;
        }

        private void SourceSelectComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (sender != null && sender is ComboBox box)
            {
                _vm.Sources.SelectId(box.SelectedIndex);
            }

            if (_vm.Sources.SelectedItem == null)
            {
                return;
            }

            _vm.Username = _vm.Sources.SelectedItem == InputSource.MISTER ? Properties.Settings.Default.MisterUsername
                                                              : Properties.Settings.Default.BeagleboneUsername;
            txtUsername.Text = _vm.Username;


            txtPassword.Text = _vm.Sources.SelectedItem == InputSource.MISTER ? Properties.Settings.Default.MisterPassword
                                                  : Properties.Settings.Default.BeaglebonePassword;

            _vm.ComPortOptionVisibility = _vm.Sources.SelectedItem.RequiresComPort;
            _vm.ComPort2OptionVisibility = _vm.Sources.SelectedItem.RequiresComPort2;
            _vm.XIAndGamepadOptionVisibility = _vm.Sources.SelectedItem.RequiresId;
            _vm.MiSTerGamepadOptionVisibility = _vm.Sources.SelectedItem.RequiresMisterId;
            _vm.SSHOptionVisibility = _vm.Sources.SelectedItem.RequiresHostname;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UpdateMayflashList();
                UpdateGamepadList();
                UpdateXIList();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                UpdateMayflashList();
                UpdateJoystickList();
            }
            UpdatePortListThread();
            UpdateBeagleList();
            UpdateBeagleI2CList();
            _vm.Skins.UpdateContents(_skins?.Where(x => x.Type == _vm.Sources.SelectedItem));
            _vm.Skins.SelectFirst();
            if (_vm.Sources.GetSelectedId() == Properties.Settings.Default.Source)
            {
                _vm.Skins.SelectId(Properties.Settings.Default.Skin);
            }
        }

        private void Skin_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (sender != null && sender is ListBox box)
            {
                _vm.Skins.SelectId(box.SelectedIndex);
            }

            if (_vm.Skins.SelectedItem == null)
            {
                return;
            }

            _vm.Backgrounds.UpdateContents(_vm.Skins.SelectedItem.Backgrounds);
            _vm.Backgrounds.SelectFirst();
            if (_vm.Skins.GetSelectedId() == Properties.Settings.Default.Skin)
            {
                _vm.Backgrounds.SelectId(Properties.Settings.Default.Background);
            }
        }

        private readonly object updatePortLock = new();

        private void UpdatePortList()
        {

            if (!isClosing && Monitor.TryEnter(updatePortLock))
            {
                try
                {
                    List<string> arduinoPorts = SetupCOMPortInformation();
                    GetTeensyPorts(arduinoPorts);
                    GetRaspberryPiPorts(arduinoPorts);

                    arduinoPorts.Sort();

                    string[] ports = arduinoPorts.ToArray<string>();

                    if (ports.Length == 0)
                    {
                        ports = new string[1];
                        ports[0] = "No Arduino/Teensy Found";
                        _vm.Ports.UpdateContents(ports);
                        _vm.Ports2.UpdateContents(ports);

                    }
                    else
                    {
                        _vm.Ports.UpdateContents(ports);
                        string[] ports2 = new string[ports.Length + 1];
                        ports2[0] = "Not Connected";
                        for (int i = 0; i < ports.Length; ++i)
                        {
                            ports2[i + 1] = ports[i];
                        }
                        _vm.Ports2.UpdateContents(ports2);
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

        private static void GetTeensyPorts(List<string> arduinoPorts)
        {
            const uint vid = 0x16C0;
            const uint serPid = 0x483;
            string vidStr = "'%USB_VID[_]" + vid.ToString("X", CultureInfo.CurrentCulture) + "%'";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using ManagementObjectSearcher searcher = new("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE " + vidStr);
                foreach (ManagementBaseObject mgmtObject in searcher.Get())
                {
                    string[] DeviceIdParts = ((string)mgmtObject["PNPDeviceID"]).Split("\\".ToArray());
                    if (DeviceIdParts[0] != "USB")
                    {
                        break;
                    }

                    int start = DeviceIdParts[1].IndexOf("PID_", StringComparison.Ordinal) + 4;
                    uint pid = Convert.ToUInt32(DeviceIdParts[1].Substring(start, 4), 16);
                    if (pid == serPid)
                    {
                        mgmtObject.ToString();
                        //uint serNum = Convert.ToUInt32(DeviceIdParts[2], CultureInfo.CurrentCulture);
                        string port;
                        if (((string)mgmtObject["Caption"]).Split("()".ToArray()).Length > 2)
                            port = ((string)mgmtObject["Caption"]).Split("()".ToArray())[1];
                        else
                            continue;

                        string hwid = ((string[])mgmtObject["HardwareID"])[0];
                        switch (hwid.Substring(hwid.IndexOf("REV_", StringComparison.Ordinal) + 4, 4))
                        {
                            case "0273":
                                //board = PJRC_Board.Teensy_LC;
                                break;

                            case "0274":
                                //board = PJRC_Board.Teensy_30;
                                break;

                            case "0275":
                                //board = PJRC_Board.Teensy_31_2;
                                break;

                            case "0276":
                                arduinoPorts.Add(port + " (Teensy 3.5)");
                                break;

                            case "0277":
                                arduinoPorts.Add(port + " (Teensy 3.6)");
                                break;

                            case "0279":
                                arduinoPorts.Add(port + " (Teensy 4.0)");
                                break;

                            case "0280":
                                arduinoPorts.Add(port + " (Teensy 4.1)");
                                break;

                            default:
                                //board = PJRC_Board.unknown;
                                break;
                        }
                    }
                }
            }
        }

        private static void GetRaspberryPiPorts(List<string> arduinoPorts)
        {
            const uint vid = 0x2E8A;
            string vidStr = "'%USB_VID[_]" + vid.ToString("X", CultureInfo.CurrentCulture) + "%'";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using ManagementObjectSearcher searcher = new("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE " + vidStr);
                foreach (ManagementBaseObject mgmtObject in searcher.Get())
                {
                    string[] DeviceIdParts = ((string)mgmtObject["PNPDeviceID"]).Split("\\".ToArray());
                    if (DeviceIdParts[0] != "USB")
                    {
                        break;
                    }

                    int start = DeviceIdParts[1].IndexOf("PID_", StringComparison.Ordinal) + 4;
                    uint pid = Convert.ToUInt32(DeviceIdParts[1].Substring(start, 4), 16);

                    string port;
                    if (((string)mgmtObject["Caption"]).Split("()".ToArray()).Length > 2)
                        port = ((string)mgmtObject["Caption"]).Split("()".ToArray())[1];
                    else
                        continue;

                    switch (pid)
                    {
                        case 0x000A:
                            arduinoPorts.Add(port + " (Raspberry Pi Pico)");
                            break;

                        default:
                            break;
                    }
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
                        if (name != null && name.Contains("(COM"))
                        {
                            list.Add(name);
                        }
                    }
                }
                searcher2.Dispose();

            }
            else
            {
                list.InsertRange(0, SerialPort.GetPortNames());
            }

            // remove duplicates, sort alphabetically and convert to array
            string[] usbDevices = list.Distinct().OrderBy(s => s).ToArray();
            return usbDevices;
        }

        private List<string> SetupCOMPortInformation()
        {
            List<COMPortInfo> comPortInformation = new();

            string[] portNames = System.IO.Ports.SerialPort.GetPortNames();
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
                if ((RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && port.PortName?.Contains("tty") == true))
                {
                    ports.Add(string.Format(CultureInfo.CurrentCulture, "{0} ({1})", port.PortName, port.FriendlyName));
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || _vm.FilterCOMPorts || (port.FriendlyName != null && port.FriendlyName.Contains("Arduino")))
                {
                    ports.Add(string.Format(CultureInfo.CurrentCulture, "{0} ({1})", port.PortName, port.FriendlyName));
                }
                else if (port.FriendlyName != null && (port.FriendlyName.Contains("CH340") || port.FriendlyName.Contains("CH341")))
                {
                    ports.Add(string.Format(CultureInfo.CurrentCulture, "{0} (Generic Arduino)", port.PortName));
                }
            }

            return ports;
        }

        private void UpdateMayflashList()
        {
            List<int> controllerList = new() { 1, 2, 3, 4 };
            _vm.XIAndGamepad.UpdateContents(controllerList);
        }

        private void UpdateGamepadList()
        {
            _vm.XIAndGamepad.UpdateContents(GamepadReader.GetDevices());
        }

        private void UpdateJoystickList()
        {
            _vm.XIAndGamepad.UpdateContents(LinuxJoystickReader.GetDevices());
        }

        private void UpdateXIList()
        {
            _vm.XIAndGamepad.UpdateContents(XInputReader.GetDevices());
        }

        private static void UpdateBeagleList()
        {
            //_vm.XIAndGamepad.UpdateContents(XboxReader.GetDevices());
        }

        private static void UpdateBeagleI2CList()
        {
            //_vm.XIAndGamepad.UpdateContents(WiiReaderV1.GetDevices());
        }

        private void MiSTerPopulate_Click(object sender, RoutedEventArgs e)
        {
            SshClient? _client = null;
            try
            {
                List<uint> controllers = new();
                _client = new SshClient(txtHostname.Text, txtUsername.Text, txtPassword.Text);
                _client.Connect();
                ShellStream _data = _client.CreateShellStream("", 0, 0, 0, 0, 1000);

                Thread.Sleep(5000);

                _data.WriteLine("ls -l /dev/input/js*");
                _data.WriteLine("retrospy_end");

                while (true)
                {
                    while (!_data.DataAvailable) { }

                    string line = _data.ReadLine();
                    if (line.Contains("retrospy_end"))
                    {
                        break;
                    }

                    if (line.Contains("/dev/input/js"))
                    {
                        int i = line.LastIndexOf("/dev/input/js", StringComparison.Ordinal);
                        if (line[(i + 13)..] != "*")
                        {
                            controllers.Add(uint.Parse(line.AsSpan(i + 13), CultureInfo.CurrentCulture));
                        }
                    }
                }

                _vm.MisterGamepad.UpdateContents(controllers);

            }
            catch (Exception)
            {

                AvaloniaMessageBoxDialog(_resources.GetString("RetroSpy", CultureInfo.CurrentUICulture), "Couldn't connected to MiSTer to get connected controllers.", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);

            }
            finally
            {
                _client?.Dispose();
            }
        }
    }

    public class SetupWindowViewModel : INotifyPropertyChanged
    {
        public ListView<string> Ports { get; set; }
        public ListView<string> Ports2 { get; set; }
        public ListView<int> XIAndGamepad { get; set; }
        public ListView<uint> MisterGamepad { get; set; }
        public ListView<Skin> Skins { get; set; }
        public ListView<Background> Backgrounds { get; set; }
        public ListView<InputSource> Sources { get; set; }
        public int DelayInMilliseconds { get; set; }
        public bool LegacyKeybindingBehavior { get; set; }
        public string? Hostname { get; set; }
        public bool FilterCOMPorts { get; set; }
        public bool DontSavePassword { get; set; }
        public bool UseLagFix { get; set; }

        public string? Username { get; set; }

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

        private bool _comPort2OptionVisibility;

        public bool ComPort2OptionVisibility
        {
            get => _comPort2OptionVisibility;
            set
            {
                _comPort2OptionVisibility = value;
                NotifyPropertyChanged("ComPort2OptionVisibility");
            }
        }

        private bool _XIAndGamepadOptionVisibility;

        public bool XIAndGamepadOptionVisibility
        {
            get => _XIAndGamepadOptionVisibility;
            set
            {
                _XIAndGamepadOptionVisibility = value;
                NotifyPropertyChanged("XIAndGamepadOptionVisibility");
            }
        }

        private bool _MiSTerGamepadOptionVisibility;

        public bool MiSTerGamepadOptionVisibility
        {
            get => _MiSTerGamepadOptionVisibility;
            set
            {
                _MiSTerGamepadOptionVisibility = value;
                NotifyPropertyChanged("MiSTerGamepadOptionVisibility");
            }
        }

        private bool _SSHOptionVisibility;

        public bool SSHOptionVisibility
        {
            get => _SSHOptionVisibility;
            set
            {
                _SSHOptionVisibility = value;
                NotifyPropertyChanged("SSHOptionVisibility");
            }
        }

        private bool _staticViewerWindowName;
        public bool StaticViewerWindowName
        {
            get => _staticViewerWindowName;
            set
            {
                _staticViewerWindowName = value;
                NotifyPropertyChanged("StaticViewerWindowName");
            }
        }


        public SetupWindowViewModel(SetupWindow setupWindow)
        {
            Ports = new ListView<string>();
            Ports.StoreControl(setupWindow, "ComPortCombo");
            Ports2 = new ListView<string>();
            Ports2.StoreControl(setupWindow, "ComPort2Combo");
            XIAndGamepad = new ListView<int>();
            XIAndGamepad.StoreControl(setupWindow, "ControllerIdCombo");
            MisterGamepad = new ListView<uint>();
            MisterGamepad.StoreControl(setupWindow, "MisterControllerIdCombo");
            Skins = new ListView<Skin>();
            Skins.StoreControl(setupWindow, "SkinListBox");
            Sources = new ListView<InputSource>();
            Sources.StoreControl(setupWindow, "SourcesComboBox");
            Backgrounds = new ListView<Background>();
            Backgrounds.StoreControl(setupWindow, "BackgroundListBox");

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