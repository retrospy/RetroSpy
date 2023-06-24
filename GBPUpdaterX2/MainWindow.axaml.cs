using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MessageBox.Avalonia.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GBPUpdaterX2
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _portListUpdateTimer;
        private bool letUpdatePortThreadRun = false;
        private bool isClosing;
        private readonly object updatePortLock = new();

        public class COMPortInfo
        {
            public string? PortName { get; set; }
            public string? FriendlyName { get; set; }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isClosing = true;
            Environment.Exit(0);
        }

        private void UpdatePortListThread()
        {
            if (letUpdatePortThreadRun)
            {
                Thread thread = new(UpdatePortList);
                thread.Start();
            }
        }

        private void UpdatePortList()
        {

            if (!isClosing && Monitor.TryEnter(updatePortLock))
            {
                try
                {
                    List<string> arduinoPorts = SetupCOMPortInformation();
                    //GetTeensyPorts(arduinoPorts);
                    //GetRaspberryPiPorts(arduinoPorts);

                    arduinoPorts.Sort();

                    string[] ports = arduinoPorts.ToArray<string>();

                    if (ports.Length == 0)
                    {
                        ports = new string[1];
                        ports[0] = "No Arduino/Teensy Found";
                        Dispatcher.UIThread.Post(() =>
                        {
                            COMPortComboBox.Items = ports;
                        });

                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            COMPortComboBox.Items = ports;
                        });
                    }

                    if (COMPortComboBox.SelectedIndex == -1)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            COMPortComboBox.SelectedIndex = 0;
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
                        if (name != null && name.Contains("(COM"))
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


        private static List<string> SetupCOMPortInformation()
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
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || (port.PortName != null && port.FriendlyName != null && port.FriendlyName.Contains("Arduino")))
                {
                    ports.Add(port.PortName ?? "COMX");
                }
                else if (port.PortName != null && port.FriendlyName != null && (port.FriendlyName.Contains("CH340") || port.FriendlyName.Contains("CH341")))
                {
                    ports.Add(port.PortName);
                }
            }

            return ports;
        }

        private async static void DownloadFirmware(string downloadDirectory, string filename = "GBP_Firmware.zip")
        {
            string token = string.Empty;
            if (File.Exists("GITHUB_TOKEN"))
            {
                token = File.ReadAllText("GITHUB_TOKEN").Trim();
            }

            if (token != string.Empty)
            {
                HttpRequestMessage request = new()
                {
                    RequestUri = new Uri("https://api.github.com/repos/retrospy/RetroSpy-private/releases/tags/nightly")
                };

                HttpClient client = new();
                
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.Send(request);

                string strResponse = await response.Content.ReadAsStringAsync();


                if (JsonConvert.DeserializeObject(strResponse) is not JObject json)
                    throw new FileNotFoundException("Cannot find " + filename + "'s Asset ID.");

                string? id = null;
                foreach (var asset in (JArray?)json["assets"] ?? new JArray())
                {
                    if (asset is null)
                        continue;

                    if ((string?)asset["name"] == filename)
                    {
                        id = (string?)asset["id"];
                        break;
                    }
                }

                if (id is null)
                    throw new FileNotFoundException("Cannot find " + filename + "'s Asset ID.");

                request = new()
                {
                    RequestUri = new Uri(string.Format("https://api.github.com/repos/retrospy/RetroSpy-private/releases/assets/{0}", id))
                };

                client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
                client.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("AppName", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                response = client.Send(request);


                using var fs = new FileStream(
                    Path.Combine(downloadDirectory, filename),
                    FileMode.CreateNew);
                response.Content.ReadAsStream().CopyTo(fs);
            }
            else
            {
                HttpRequestMessage request = new()
                {
                    RequestUri = new Uri("https://github.com/retrospy/RetroSpy/releases/latest/download/" + filename)
                };

                HttpClient client = new();
                var response = client.Send(request);
                using var fs = new FileStream(
                    Path.Combine(downloadDirectory, filename),
                    FileMode.CreateNew);
                response.Content.ReadAsStream().CopyTo(fs);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            SerialNumberLabel.IsVisible = true;
            txtboxSerialNumber.IsVisible = true;
            List<string> devices = new List<string>
            {
                "RetroSpy Pixel",
                "RetroSpy Vision",
                "Serial Debugger"
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                devices.Add("Bad CH340 Driver Fix");
            }

            DeviceComboBox.Items = devices;
            DeviceComboBox.SelectedIndex = 0;

            UpdatePortList();
            letUpdatePortThreadRun = true;

            _portListUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _portListUpdateTimer.Tick += (sender, e) => UpdatePortListThread();
            _portListUpdateTimer.Start();
        }

        private void DeviceSelectComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (DeviceComboBox.SelectedIndex == 0)
            {
                SerialNumberLabel.IsVisible = true;
                txtboxSerialNumber.IsVisible = true;
            }
            else
            {
                SerialNumberLabel.IsVisible = false;
                txtboxSerialNumber.IsVisible = false;
            }

            if (DeviceComboBox.SelectedIndex > 0 && DeviceComboBox.SelectedIndex < 3)
            {
                COMPortComboBox.SelectedIndex = 0;
                COMPortLabel.IsVisible = true;
                COMPortComboBox.IsVisible = true;
            }
            else
            {
                COMPortLabel.IsVisible = false;
                COMPortComboBox.IsVisible = false;
            }

            if (DeviceComboBox.SelectedIndex != 2 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                isClosing = true;
                Thread.Sleep(1000);
                isClosing = false;
            }
        }

        private void UpdateThread()
        {
            try
            {
                int serialNumber = 0;

                Dispatcher.UIThread.Post(() =>
                {
                    try
                    {
                        serialNumber = Int32.Parse(txtboxSerialNumber.Text);
                    }
                    catch (Exception)
                    {
                        serialNumber = 0;
                    }

                    this.goButton.IsEnabled = false;
                    txtboxData.Text = string.Empty;
                    txtboxData.CaretIndex = int.MaxValue;
                });

                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _ = Directory.CreateDirectory(tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Downloading latest firmware...";
                    txtboxData.CaretIndex = int.MaxValue;
                });


                DownloadFirmware(tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "GBP_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Searching for GameBoy Printer Emulator...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                SerialPort? _serialPort = null;

                List<string> arduinoPorts = SetupCOMPortInformation();
                string gbpemuPort = "";
                bool foundPort = false;

                foreach (string port in arduinoPorts)
                {
                    try
                    {
                        using (_serialPort = new SerialPort(port, 115200, Parity.None, 8, StopBits.One)
                        {
                            Handshake = Handshake.None,

                            ReadTimeout = 500,
                            WriteTimeout = 500
                        })
                        {

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

                            string? result = null;
                            do
                            {
                                _serialPort.ReadTimeout = 2500;
                                result = _serialPort.ReadLine();
                            } while (result != null && !(result.StartsWith("// GAMEBOY PRINTER Emulator V3 : Copyright (C) 2020 Brian Khuu")
                                || result.StartsWith("// GAMEBOY PRINTER Emulator V3.2.1 (Copyright (C) 2022 Brian Khuu")
                                || result.StartsWith("// GAMEBOY PRINTER Packet Capture V3.2.1 (Copyright (C) 2022 Brian Khuu")
                                || result.StartsWith("d=debug, ?=help")));

                            foundPort = true;
                            gbpemuPort = port;
                            _serialPort.Close();
                        }
                    }
                    catch (Exception) { }
                }

                if (!foundPort)
                {

                    Dispatcher.UIThread.Post(async () =>
                    {
                        txtboxData.Text += "cannot find RetroSpy GameBoy Printer Emulator.\n\n";
                        var m = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("RetroSpy", "Couldn't find RetroSpy Pixel.", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                        await m.ShowDialog(this);
                        txtboxData.CaretIndex = int.MaxValue;
                        this.goButton.IsEnabled = true;
                    });

                }
                else
                {

                    Dispatcher.UIThread.Post(() =>
                    {
                        txtboxData.Text += "found on " + gbpemuPort + ".\n\n";
                        txtboxData.Text += "Updating firmware...\n";
                        txtboxData.CaretIndex = int.MaxValue;
                    });

                    ProcessStartInfo processInfo;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        processInfo = new ProcessStartInfo("cmd.exe",
                            "/c avrdude.exe -Cavrdude.conf -v -patmega328p -carduino -P" + gbpemuPort +
                            string.Format(" -b{0} -D -Uflash:w:firmware{1}.ino.hex:i",
                                serialNumber < 100007 ? "115200" : "57600", serialNumber < 100007 ? "" : "-old"))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            WorkingDirectory = tempDirectory
                        };
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        processInfo = new ProcessStartInfo("chmod",
                            "755 " + Path.Join(tempDirectory, "avrdude"))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        Process? p1 = Process.Start(processInfo);
                        p1?.WaitForExit();

                        processInfo = new ProcessStartInfo(Path.Join(tempDirectory, "avrdude"),
                            "-v -patmega328p -carduino -P" + gbpemuPort +
                            string.Format(" -b{0} -D -Uflash:w:firmware{1}.ino.hex:i",
                                serialNumber < 100007 ? "115200" : "57600", serialNumber < 100007 ? "" : "-old"))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            WorkingDirectory = tempDirectory
                        };
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        processInfo = new ProcessStartInfo("avrdude",
                            "-v -patmega328p -carduino -P" + gbpemuPort +
                            string.Format(" -b{0} -D -Uflash:w:firmware{1}.ino.hex:i",
                                serialNumber < 100007 ? "115200" : "57600", serialNumber < 100007 ? "" : "-old"))
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true,
                            WorkingDirectory = tempDirectory
                        };
                    }
                    else
                    {
                        throw new PlatformNotSupportedException();
                    }

                    StringBuilder sb = new();
                    Process? p = Process.Start(processInfo);
                    if (p != null)
                    {
                        p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data);
                        p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data);
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        p.WaitForExit();
                    }

                    Dispatcher.UIThread.Post(async () =>
                    {
                        txtboxData.Text += sb.ToString() + "\n";
                        txtboxData.Text += "..." + "done.\n\n";
                        txtboxData.CaretIndex = int.MaxValue;

                        try
                        {
                            if (sb.ToString().Contains("attempt 10 of 10"))
                                throw new Exception("Updating Failed.");

                            var m = MessageBox.Avalonia.MessageBoxManager
                                .GetMessageBoxStandardWindow("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info);
                            await m.ShowDialog(this);
                            goButton.IsEnabled = true;

                        }
                        catch (Exception ex)
                        {
                            txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                            var m = MessageBox.Avalonia.MessageBoxManager
                                .GetMessageBoxStandardWindow("RetroSpy", ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                            await m.ShowDialog(this);
                            goButton.IsEnabled = true;
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("RetroSpy", ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                     await m.ShowDialog(this);

                    goButton.IsEnabled = true;
                });

            }

        }

        private void UpdateVisionThread()
        {
            try
            {

                Dispatcher.UIThread.Post(() =>
                {
                    this.goButton.IsEnabled = false;
                    txtboxData.Text = string.Empty;
                    txtboxData.CaretIndex = int.MaxValue;
                });

                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _ = Directory.CreateDirectory(tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Downloading latest firmware...";
                    txtboxData.CaretIndex = int.MaxValue;
                });


                DownloadFirmware(tempDirectory, "Vision_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "Vision_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                string? port = (string?)COMPortComboBox.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Updating firmware...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ProcessStartInfo processInfo;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c avrdude.exe -Cavrdude.conf -v -patmega328p -carduino -P" + port +
                        string.Format(" -b{0} -D -Uflash:w:firmware.ino.hex:i", "57600"))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = tempDirectory
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    processInfo = new ProcessStartInfo("chmod",
                        "755 " + Path.Join(tempDirectory, "avrdude"))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process? p1 = Process.Start(processInfo);
                    p1?.WaitForExit();

                    processInfo = new ProcessStartInfo(Path.Join(tempDirectory, "avrdude"),
                        "-v -patmega328p -carduino -P" + port +
                        string.Format(" -b{0} -D -Uflash:w:firmware.ino.hex:i", "57600"))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = tempDirectory
                    };
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    processInfo = new ProcessStartInfo("avrdude",
                        "-v -patmega328p -carduino -P" + port +
                        string.Format(" -b{0} -D -Uflash:w:firmware.ino.hex:i", "57600"))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        WorkingDirectory = tempDirectory
                    };
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }

                StringBuilder sb = new();
                Process? p = Process.Start(processInfo);
                if (p != null)
                {
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data);
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }

                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += sb.ToString() + "\n";
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;

                    try
                    {
                        var m = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info);
                        await m.ShowDialog(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("RetroSpy", ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                        await m.ShowDialog(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("RetroSpy", ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                    await m.ShowDialog(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void SerialDebuggerThread()
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.goButton.IsEnabled = false;
                txtboxData.Text = string.Empty;
                txtboxData.CaretIndex = int.MaxValue;
            });

            try
            {
                SerialPort? _serialPort = null;

                string? port = (string?)COMPortComboBox.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                using (_serialPort = new SerialPort(port, 115200, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None,

                    DtrEnable = true,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                })
                {
                    _serialPort.Open();

                    while (!isClosing)
                    {
                        int readCount = _serialPort.BytesToRead;
                        if (readCount > 0)
                        {
                            byte[] readBuffer = new byte[readCount];
                            _ = _serialPort.Read(readBuffer, 0, readCount);

                            Dispatcher.UIThread.Post(() =>
                            {
                                txtboxData.Text += System.Text.Encoding.Default.GetString(readBuffer);
                                txtboxData.CaretIndex = int.MaxValue;
                            });
                            Thread.Sleep(1);
                        }
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        goButton.IsEnabled = true;
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("RetroSpy", ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                    await m.ShowDialog(this);

                    goButton.IsEnabled = true;
                });

            }

        }

#if OS_WINDOWS
        private void DriverFixThread()
        {
            try
            {
                Dispatcher.UIThread.Post(() =>
                {
                    this.goButton.IsEnabled = false;
                    txtboxData.Text = string.Empty;
                    txtboxData.CaretIndex = int.MaxValue;
                });

                var deviceId = "USB\\VID_1A86&PID_7523";

                // Guilty driver version to be uninstalled
                var driverVersion = "3.8.2023.02";

                if (AnalyzePorts())
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtboxData.Text += "Installing default driver ...\n";
                        txtboxData.CaretIndex = int.MaxValue;
                    });
                    InstallDefaultDriver();

                    if (!OEMDriversHelper.UninstallDriverVersion(deviceId, driverVersion, txtboxData))
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            txtboxData.Text += "Driver non compatible with fake CH340 not found\n";
                            txtboxData.CaretIndex = int.MaxValue;
                        });
                    }

                    if (!WindowsUpdateHelper.BlockUpdatesFor(deviceId, txtboxData))
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            txtboxData.Text += "Could not find pending driver updates\n";
                            txtboxData.CaretIndex = int.MaxValue;
                        });
                    }
                }

                Dispatcher.UIThread.Post(async () =>
                {
                    var m = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info);
                    await m.ShowDialog(this);
                    goButton.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("RetroSpy", ex.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Error);
                    await m.ShowDialog(this);

                    goButton.IsEnabled = true;
                });
            }
        }

        public bool AnalyzePorts()
        {
            Dispatcher.UIThread.Post(() =>
            {
                txtboxData.Text += "Checking ports :\n";
                txtboxData.CaretIndex = int.MaxValue;
            });

            var CH340GPorts = SerialPortDescriptionHelper.GetSerialPorts()?.Where(i => i.IsCH340).ToList();
            if (CH340GPorts != null)
            {
                foreach (var port in CH340GPorts)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtboxData.Text += $"\tFound CH340G on port {port.PortName}, driver : {port.DriverVersion} ({port.DriverInf}) : likely to be {(port.IsFakeCH340 ? "Fake" : "Legit")}\n";
                        txtboxData.CaretIndex = int.MaxValue;
                    });
                }
            }

            if (!(CH340GPorts?.Any() ?? true))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "\tNo CH340G found, please make sure to plug it first.\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });
                return false;
            }
            else
            {
                return true;
            }
        }

        public void InstallDefaultDriver()
        {
            var temp2 = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(temp2);
            ZipFile.ExtractToDirectory(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? string.Empty, "CH340G_2019.zip"), temp2);
            string inffile = Path.Combine(temp2, "CH341ser.Inf");
            PNPUtilHelper.InstallDriver(inffile, txtboxData);
        }
#endif

        private void GoButton_Click(object? sender, RoutedEventArgs? e)
        {
            if (DeviceComboBox.SelectedIndex == 0)
            {
                Thread thread = new(UpdateThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == 1)
            {
                Thread thread = new(UpdateVisionThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == 2)
            {
                Thread thread = new(SerialDebuggerThread);
                thread.Start();
            }
#if OS_WINDOWS
            if (DeviceComboBox.SelectedIndex == 3 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Thread thread = new(DriverFixThread);
                thread.Start();
            }
#endif
        }
    }
}