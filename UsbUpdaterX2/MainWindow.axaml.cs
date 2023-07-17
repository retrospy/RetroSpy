using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using Renci.SshNet;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace UsbUpdaterX2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateThread()
        {
            try
            {
                string hostname = txtboxHostname.Text;
                string username = txtboxUserName.Text;
                string password = txtboxPassword.Text;


                Dispatcher.UIThread.Post(() =>
                {
                    this.goButton.IsEnabled = false;
                    txtboxData.Text = string.Empty;
                    txtboxData.Text += "\n";
                    txtboxData.Text += "\nLogging into " + (string.IsNullOrEmpty(hostname) ? "retrospy.local" : hostname) + "...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });


                string strIP = string.IsNullOrEmpty(hostname) ? "retrospy.local" : hostname;

                var ips = Dns.GetHostEntry(hostname);
                foreach (var ip in ips.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        strIP = ip.ToString();
                        break;
                    }
                }

                using (SshClient _client = new(strIP,
                                    string.IsNullOrEmpty(username) ? "retrospy" : username,
                                    string.IsNullOrEmpty(password) ? "retrospy" : password))
                {
                    _client.Connect();
                    ShellStream _data = _client.CreateShellStream("", 0, 0, 0, 0, 1000);

                    string token = string.Empty;
                    if (File.Exists("GITHUB_TOKEN"))
                    {
                        token = File.ReadAllText("GITHUB_TOKEN").Trim();
                    }

                    if (token != string.Empty)
                    {
                        _data.WriteLine(string.Format("sudo /usr/local/bin/update-usb-retrospy-nightly.sh {0}", token));
                    }
                    else
                    {
                        _data.WriteLine("sudo /usr/local/bin/update-usb-retrospy.sh");
                    }

                    while (true)
                    {
                        while (!_data.DataAvailable) { };
                        string line = _data.Read();

                        Dispatcher.UIThread.Post(() =>
                        {
                            txtboxData.Text += line;
                            txtboxData.CaretIndex = int.MaxValue;
                        });

                        if (line.Contains("Device needs full image installed!"))
                        {
                            Dispatcher.UIThread.Post(async () =>
                            {
                                var m = MsBox.Avalonia.MessageBoxManager
                                    .GetMessageBoxStandard("RetroSpy", "Unfortunately, this device requires a full reimage.\nIf you are unsure how to do this please reach out to support@retro-spy.com.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                await m.ShowWindowDialogAsync(this);
                                goButton.IsEnabled = true;
                            });

                            break;
                        }
                        else if (line.Contains("Installation complete!"))
                        {

                            Dispatcher.UIThread.Post(async () =>
                            {
                                var m = MsBox.Avalonia.MessageBoxManager
                                    .GetMessageBoxStandard("RetroSpy", "Installation complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                                await m.ShowWindowDialogAsync(this);
                                goButton.IsEnabled = true;
                            });

                            break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                Dispatcher.UIThread.Post(async() =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);
                    goButton.IsEnabled = true;
                });


            }
        }

        private void GoButton_Click(object? sender, RoutedEventArgs? e)
        {
            Thread thread = new(UpdateThread);
            thread.Start();
        }
    }
}