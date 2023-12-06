using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering;
using Avalonia.Threading;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
        private readonly bool letUpdatePortThreadRun = false;
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
                        Dispatcher.UIThread.Post(() =>
                        {
                            COMPortComboBox.ItemsSource = ports;
                            COMPortComboBox2.ItemsSource = ports;
                        });

                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            COMPortComboBox.ItemsSource = ports;
                            COMPortComboBox2.ItemsSource = ports;
                        });
                    }

                    if (COMPortComboBox.SelectedIndex == -1)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            COMPortComboBox.SelectedIndex = 0;
                            COMPortComboBox2.SelectedIndex = 0;
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
                    ports.Add(port.PortName ?? String.Empty);
                }
            }

            return ports;
        }

        private static void DownloadFirmware(string downloadDirectory, string filename = "GBP_Firmware.zip")
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

                var responseTask = response.Content.ReadAsStringAsync();
                responseTask.Wait();
                string strResponse = responseTask.Result.ToString();


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

        public enum Devices
        {
            PIXEL = 0,
            VISION,
            VISION_DREAM,
            VISION_CDI,
            VISION_COLECO,
            VISION_PIPPIN,
            VISION_ANALOG,
            VISION_FLEX,
            VISION_USBLITE,
            SERIAL_DEBUG
        }


        public MainWindow()
        {
            InitializeComponent();

            Closing += (s, e) =>
            {
                isClosing = true;
                Environment.Exit(0);
            };

            SerialNumberLabel.IsVisible = true;
            txtboxSerialNumber.IsVisible = true;
            List<string> devices = new()
            {
                "RetroSpy Pixel",
                "RetroSpy Vision",
                "RetroSpy Vision Dream",
                "RetroSpy Vision CDi",
                "RetroSpy Vision CV",
                "RetroSpy Vision ADB",
                "RetroSpy Vision Analog",
                "RetroSpy Vision Flex",
                "RetroSpy Vision USB Lite",
                "Serial Debugger"
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                devices.Add("Install Pi Pico Driver");
                devices.Add("List Drives");
                devices.Add("Bad CH340 Driver Fix");
            }

            DeviceComboBox.ItemsSource = devices;
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
            if (DeviceComboBox.SelectedIndex == ((int)Devices.PIXEL))
            {
                SerialNumberLabel.IsVisible = true;
                txtboxSerialNumber.IsVisible = true;
            }
            else
            {
                SerialNumberLabel.IsVisible = false;
                txtboxSerialNumber.IsVisible = false;
            }

            if (DeviceComboBox.SelectedIndex == ((int)Devices.VISION) || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_DREAM)
                || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_COLECO) || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_PIPPIN)
                || DeviceComboBox.SelectedIndex == ((int)Devices.SERIAL_DEBUG) || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_ANALOG)
                 || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_FLEX) || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_CDI)
                 || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_USBLITE))
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

            if (DeviceComboBox.SelectedIndex == ((int)Devices.VISION_COLECO) || DeviceComboBox.SelectedIndex == ((int)Devices.VISION_ANALOG))
            {
                COMPortComboBox2.SelectedIndex = 0;
                COMPortComboBox2.IsVisible = true;
                txtboxData.Margin = new Thickness(10, 23, 5, 0);
                COMPortLabel.IsVisible = false;
                COMPortLabel2.IsVisible = true;
            }
            else
            {
                COMPortComboBox2.IsVisible = false;
                txtboxData.Margin = new Thickness(10, 55, 5, 0);
                COMPortLabel2.IsVisible = false;
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
                        serialNumber = Int32.Parse(txtboxSerialNumber.Text ?? "0");
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
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Couldn't find RetroSpy Pixel.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
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
                        p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                        p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
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

                            var m = MsBox.Avalonia.MessageBoxManager
                                .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                            await m.ShowWindowDialogAsync(this);
                            goButton.IsEnabled = true;

                        }
                        catch (Exception ex)
                        {
                            txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                            var m = MsBox.Avalonia.MessageBoxManager
                                .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                            await m.ShowWindowDialogAsync(this);
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
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

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
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
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
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }


        private void ForcePiPicoIntoBootSel()
        {
            string? port = (string?)COMPortComboBox.SelectedItem;
            port ??= "No Arduino/Teensy Found";

            SerialPort? _serialPort = null;
            using (_serialPort = new SerialPort(port != null ? port.Split(' ')[0] : "", 1200, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,

                DtrEnable = true,
                ReadTimeout = 500,
                WriteTimeout = 500
            })
            {
                try
                {
                    _serialPort.Open();
                    _serialPort.Close();
                }
                catch (Exception)
                {

                }
            }

            Thread.Sleep(1000);
        }

        private void UpdateFlexThread()
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


                DownloadFirmware(tempDirectory, "Flex_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "Flex_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ForcePiPicoIntoBootSel();

                DriveInfo[] drives = DriveInfo.GetDrives();
                bool found = false;
                foreach(var drive in drives)
                {
                    if (drive.IsReady && drive.VolumeLabel == "RPI-RP2")
                    {
                        File.Copy(Path.Combine(tempDirectory, "firmware.ino.uf2"), Path.Combine(drive.Name, "firmware.ino.uf2"), true);
                        found = true;
                    }
                }

                if (!found)
                    throw new Exception("Could not find Raspberry Pi Pico device.");

                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;

                    try
                    {
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void UpdateUSBLiteThread()
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


                DownloadFirmware(tempDirectory, "USB_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "USB_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Updating firmware...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ForcePiPicoIntoBootSel();

                DriveInfo[] drives = DriveInfo.GetDrives();
                bool found = false;
                foreach (var drive in drives)
                {
                    if (drive.IsReady && drive.VolumeLabel == "RPI-RP2")
                    {
                        File.Copy(Path.Combine(tempDirectory, "firmware.ino.uf2"), Path.Combine(drive.Name, "firmware.ino.uf2"), true);
                        found = true;
                    }
                }

                if (!found)
                    throw new Exception("Could not find Raspberry Pi Pico device.");

                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;

                    try
                    {
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void UpdateDreamThread()
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


                DownloadFirmware(tempDirectory, "Dream_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "Dream_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Updating firmware...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ProcessStartInfo processInfo;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c teensy_loader_cli.exe --mcu=TEENSY40 -v -w firmware.ino.hex")
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
                        "755 " + Path.Join(tempDirectory, "teensy_loader_cli.mac"))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process? p1 = Process.Start(processInfo);
                    p1?.WaitForExit();

                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c teensy_loader_cli.mac --mcu=TEENSY40 -v -w firmware.ino.hex")
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
                    processInfo = new ProcessStartInfo("chmod",
                        "755 " + Path.Join(tempDirectory, "teensy_loader_cli.linux"))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process? p1 = Process.Start(processInfo);
                    p1?.WaitForExit();

                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c teensy_loader_cli.linux --mcu=TEENSY40 -v -w firmware.ino.hex")
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
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
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
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void UpdateCDiThread()
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


                DownloadFirmware(tempDirectory, "CDi_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "CDi_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Updating firmware...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ForcePiPicoIntoBootSel();

                DriveInfo[] drives = DriveInfo.GetDrives();
                bool found = false;
                foreach (var drive in drives)
                {
                    if (drive.IsReady && drive.VolumeLabel == "RPI-RP2")
                    {
                        File.Copy(Path.Combine(tempDirectory, "firmware.ino.uf2"), Path.Combine(drive.Name, "firmware.ino.uf2"), true);
                        found = true;
                    }
                }

                if (!found)
                    throw new Exception("Could not find Raspberry Pi Pico device.");

                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;

                    try
                    {
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }
    

        private void UpdateColecoVisionThread()
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


                DownloadFirmware(tempDirectory, "CV_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "CV_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                string? port = (string?)COMPortComboBox.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                string? port2 = (string?)COMPortComboBox2.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                if (port == port2)
                    throw new Exception("Port 1 and 2 cannot be the same port.");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Updating firmware on port 1...\n";
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
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += sb.ToString() + "\n";
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;

                    txtboxData.Text += "Updating firmware on port 2...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c avrdude.exe -Cavrdude.conf -v -patmega328p -carduino -P" + port2 +
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
                        "-v -patmega328p -carduino -P" + port2 +
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
                        "-v -patmega328p -carduino -P" + port2 +
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

                sb = new();
                p = Process.Start(processInfo);
                if (p != null)
                {
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
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
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void UpdatePippinThread()
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


                DownloadFirmware(tempDirectory, "ADB_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "ADB_Firmware.zip"), tempDirectory);

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
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
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
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void UpdateAnalogThread()
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


                DownloadFirmware(tempDirectory, "Analog_Firmware.zip");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.Text += "Decompressing firmware package...";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "Analog_Firmware.zip"), tempDirectory);

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                string? port = (string?)COMPortComboBox.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                string? port2 = (string?)COMPortComboBox2.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                if (port == port2)
                    throw new Exception("Port 1 and 2 cannot be the same port.");

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Updating firmware on port 1...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                ProcessStartInfo processInfo;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c avrdude.exe -Cavrdude.conf -v -patmega328p -carduino -P" + port +
                        string.Format(" -b{0} -D -Uflash:w:firmware_1.ino.hex:i", "115200"))
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
                        string.Format(" -b{0} -D -Uflash:w:firmware_1.ino.hex:i", "115200"))
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
                        string.Format(" -b{0} -D -Uflash:w:firmware_1.ino.hex:i", "115200"))
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
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += sb.ToString() + "\n";
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;

                    txtboxData.Text += "Updating firmware on port 2...\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    processInfo = new ProcessStartInfo("cmd.exe",
                        "/c avrdude.exe -Cavrdude.conf -v -patmega328p -carduino -P" + port2 +
                        string.Format(" -b{0} -D -Uflash:w:firmware_2.ino.hex:i", "115200"))
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
                        "-v -patmega328p -carduino -P" + port2 +
                        string.Format(" -b{0} -D -Uflash:w:firmware_2.ino.hex:i", "115200"))
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
                        "-v -patmega328p -carduino -P" + port2 +
                        string.Format(" -b{0} -D -Uflash:w:firmware_2.ino.hex:i", "115200"))
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

                sb = new();
                p = Process.Start(processInfo);
                if (p != null)
                {
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
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
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;

                    }
                    catch (Exception ex)
                    {
                        txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                        var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                        await m.ShowWindowDialogAsync(this);
                        goButton.IsEnabled = true;
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

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

                using (_serialPort = new SerialPort(port != null ? port.Split(' ')[0] : "", 115200, Parity.None, 8, StopBits.One)
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
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }


#if OS_WINDOWS


        private void InstallRpiDriverThread()
        {
            Dispatcher.UIThread.Post(() =>
            {
                this.goButton.IsEnabled = false;
                txtboxData.Text = string.Empty;
                txtboxData.CaretIndex = int.MaxValue;
            });

            try
            {
                ProcessStartInfo processInfo;

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += "Starting installation.  This can take a few minutes...\n";
                });

                processInfo = new ProcessStartInfo("cmd.exe",
                    "/c wdi-simple.exe")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = "."
                };

                StringBuilder sb = new();
                Process? p = Process.Start(processInfo);
                if (p != null)
                {
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += sb.ToString() + "\n";
                    txtboxData.Text += "..." + "done.\n\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }

        private void ListDeviceThread()
        {
            Dispatcher.UIThread.Post(() =>
            {
                txtboxData.Text = string.Empty;
                txtboxData.CaretIndex = int.MaxValue;
            });

            try
            {
                ProcessStartInfo processInfo;
                
                processInfo = new ProcessStartInfo("cmd.exe",
                    "/c echo list volume | diskpart")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = "."
                };

                StringBuilder sb = new();
                Process? p = Process.Start(processInfo);
                if (p != null)
                {
                    p.OutputDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.ErrorDataReceived += (sender, args1) => sb.AppendLine(args1.Data ?? String.Empty);
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                }

                Dispatcher.UIThread.Post(() =>
                {
                    txtboxData.Text += sb.ToString() + "\n";
                    txtboxData.CaretIndex = int.MaxValue;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message ?? String.Empty, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

                    goButton.IsEnabled = true;
                });

            }
        }


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
                    var m = MsBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandard("RetroSpy", "Update complete! Please reboot your device.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                    await m.ShowWindowDialogAsync(this);
                    goButton.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(async () =>
                {
                    txtboxData.Text += "\nUpdater encountered an error.  Message: " + ex.Message + "\n";
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", ex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    await m.ShowWindowDialogAsync(this);

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
                Thread thread = new(UpdateDreamThread);
                thread.Start();
            }


            if (DeviceComboBox.SelectedIndex == 3)
            {
                Thread thread = new(UpdateCDiThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == 4)
            {
                Thread thread = new(UpdateColecoVisionThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == 5)
            {
                Thread thread = new(UpdatePippinThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == 6)
            {
                Thread thread = new(UpdateAnalogThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == 7)
            {
                Thread thread = new(UpdateFlexThread);
                thread.Start();
            }


            if (DeviceComboBox.SelectedIndex == 8)
            {
                Thread thread = new(UpdateUSBLiteThread);
                thread.Start();
            }
            int serialDebuggerOffset = 1;
#if OS_WINDOWS
            serialDebuggerOffset = 4;
#endif

            if (DeviceComboBox.SelectedIndex == DeviceComboBox.ItemCount - serialDebuggerOffset)
            {
                Thread thread = new(SerialDebuggerThread);
                thread.Start();
            }
#if OS_WINDOWS

            if (DeviceComboBox.SelectedIndex == DeviceComboBox.ItemCount - 3)
            {
                Thread thread = new(InstallRpiDriverThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == DeviceComboBox.ItemCount - 2)
            {
                Thread thread = new(ListDeviceThread);
                thread.Start();
            }

            if (DeviceComboBox.SelectedIndex == DeviceComboBox.ItemCount - 1 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Thread thread = new(DriverFixThread);
                thread.Start();
            }
#endif
        }
    }
}