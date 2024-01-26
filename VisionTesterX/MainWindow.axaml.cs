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

namespace VisionTester
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
            uint[] vid = new uint[] { 0x2E8A, 0x6666 };

            for (int i = 0; i < vid.Length; ++i)
            {
                string vidStr = "'%USB_VID[_]" + vid[i].ToString("X", CultureInfo.CurrentCulture) + "%'";

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

                            case 0x6610:
                                arduinoPorts.Add(port + " (RetroSpy USB Lite)");
                                break;

                            default:
                                break;
                        }
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
            VISION_FLEX = 0,
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
                "RetroSpy Vision Flex",
            };

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
            SerialNumberLabel.IsVisible = false;
            txtboxSerialNumber.IsVisible = false;

            if (DeviceComboBox.SelectedIndex == ((int)Devices.VISION_FLEX))
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

            COMPortComboBox2.IsVisible = false;
            txtboxData.Margin = new Thickness(10, 55, 5, 0);
            COMPortLabel2.IsVisible = false;
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

                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                _ = Directory.CreateDirectory(tempDirectory);

                DownloadFirmware(tempDirectory, "Flex_Firmware.zip");

                ZipFile.ExtractToDirectory(Path.Combine(tempDirectory, "Flex_Firmware.zip"), tempDirectory);

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
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        P1Red.IsVisible = true;
                        P1Green.IsVisible = false;
                    });
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        P1Red.IsVisible = false;
                        P1Green.IsVisible = true;
                    });
                }
            }
            catch (Exception)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    P1Red.IsVisible = true;
                    P1Green.IsVisible = false;
                });
            }
        }


        private static bool testRunning = false;

        private void TestProgramThread()
        {
            try
            {
                Dispatcher.UIThread.Post(() =>
                {
                    T1Red.IsVisible = true;
                    T1Green.IsVisible = false;
                    testButton.Content = "Stop Test";

                    S1Red.IsVisible = true;
                    S1Green.IsVisible = false;
                    S2Red.IsVisible = true;
                    S2Green.IsVisible = false;
                    S3Red.IsVisible = true;
                    S3Green.IsVisible = false;
                    S4Red.IsVisible = true;
                    S4Green.IsVisible = false;
                    S5Red.IsVisible = true;
                    S5Green.IsVisible = false;
                    S6Red.IsVisible = true;
                    S6Green.IsVisible = false;
                    S7Red.IsVisible = true;
                    S7Green.IsVisible = false;
                    S8Red.IsVisible = true;
                    S8Green.IsVisible = false;
                });

                SerialPort? _serialPort = null;

                string? port = (string?)COMPortComboBox.SelectedItem;
                port ??= "No Arduino/Teensy Found";

                using (_serialPort = new SerialPort(port != null ? port.Split(' ')[0] : "", 115200, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None,

                    DtrEnable = true,
                    ReadTimeout = 5000,
                    WriteTimeout = 5000
                })
                {
                    _serialPort.Open();

                    while (testRunning && !isClosing)
                    {
                        string message = _serialPort.ReadLine();

                        var splits = message.Split(',');
                        if (splits.Length != 2)
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                T1Red.IsVisible = true;
                                T1Green.IsVisible = false;

                                S1Red.IsVisible = true;
                                S1Green.IsVisible = false;
                                S2Red.IsVisible = true;
                                S2Green.IsVisible = false;
                                S3Red.IsVisible = true;
                                S3Green.IsVisible = false;
                                S4Red.IsVisible = true;
                                S4Green.IsVisible = false;
                                S5Red.IsVisible = true;
                                S5Green.IsVisible = false;
                                S6Red.IsVisible = true;
                                S6Green.IsVisible = false;
                                S7Red.IsVisible = true;
                                S7Green.IsVisible = false;
                                S8Red.IsVisible = true;
                                S8Green.IsVisible = false;
                            });
                            continue;
                        }

                        if (splits[0] == "1")
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                T1Red.IsVisible = false;
                                T1Green.IsVisible = true;
                            });
                        }
                        else
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                T1Red.IsVisible = true;
                                T1Green.IsVisible = false;
                            });
                        }
                        Dispatcher.UIThread.Post(() =>
                        {

                            if (int.TryParse(splits[1], CultureInfo.InvariantCulture, out int switches))
                            {
                                if ((switches & 0x01) == 0)
                                {
                                    S1Red.IsVisible = true;
                                    S1Green.IsVisible = false;
                                }
                                else
                                {
                                    S1Red.IsVisible = false;
                                    S1Green.IsVisible = true;
                                }

                                if ((switches & 0x02) == 0)
                                {
                                    S2Red.IsVisible = true;
                                    S2Green.IsVisible = false;
                                }
                                else
                                {
                                    S2Red.IsVisible = false;
                                    S2Green.IsVisible = true;
                                }

                                if ((switches & 0x04) == 0)
                                {
                                    S3Red.IsVisible = true;
                                    S3Green.IsVisible = false;
                                }
                                else
                                {
                                    S3Red.IsVisible = false;
                                    S3Green.IsVisible = true;
                                }

                                if ((switches & 0x08) == 0)
                                {
                                    S4Red.IsVisible = true;
                                    S4Green.IsVisible = false;
                                }
                                else
                                {
                                    S4Red.IsVisible = false;
                                    S4Green.IsVisible = true;
                                }

                                if ((switches & 0x10) == 0)
                                {
                                    S5Red.IsVisible = true;
                                    S5Green.IsVisible = false;
                                }
                                else
                                {
                                    S5Red.IsVisible = false;
                                    S5Green.IsVisible = true;
                                }

                                if ((switches & 0x20) == 0)
                                {
                                    S6Red.IsVisible = true;
                                    S6Green.IsVisible = false;
                                }
                                else
                                {
                                    S6Red.IsVisible = false;
                                    S6Green.IsVisible = true;
                                }

                                if ((switches & 0x40) == 0)
                                {
                                    S7Red.IsVisible = true;
                                    S7Green.IsVisible = false;
                                }
                                else
                                {
                                    S7Red.IsVisible = false;
                                    S7Green.IsVisible = true;
                                }

                                if ((switches & 0x80) == 0)
                                {
                                    S8Red.IsVisible = true;
                                    S8Green.IsVisible = false;
                                }
                                else
                                {
                                    S8Red.IsVisible = false;
                                    S8Green.IsVisible = true;
                                }
                            }
                            else
                            {
                                S1Red.IsVisible = true;
                                S1Green.IsVisible = false;
                                S2Red.IsVisible = true;
                                S2Green.IsVisible = false;
                                S3Red.IsVisible = true;
                                S3Green.IsVisible = false;
                                S4Red.IsVisible = true;
                                S4Green.IsVisible = false;
                                S5Red.IsVisible = true;
                                S5Green.IsVisible = false;
                                S6Red.IsVisible = true;
                                S6Green.IsVisible = false;
                                S7Red.IsVisible = true;
                                S7Green.IsVisible = false;
                                S8Red.IsVisible = true;
                                S8Green.IsVisible = false;
                            }


                        });
                    }
                    Dispatcher.UIThread.Post(() =>
                    {
                        testButton.Content = "Start Test";
                        S1Red.IsVisible = false;
                        S1Green.IsVisible = false;
                        S2Red.IsVisible = false;
                        S2Green.IsVisible = false;
                        S3Red.IsVisible = false;
                        S3Green.IsVisible = false;
                        S4Red.IsVisible = false;
                        S4Green.IsVisible = false;
                        S5Red.IsVisible = false;
                        S5Green.IsVisible = false;
                        S6Red.IsVisible = false;
                        S6Green.IsVisible = false;
                        S7Red.IsVisible = false;
                        S7Green.IsVisible = false;
                        S8Red.IsVisible = false;
                        S8Green.IsVisible = false;
                    });
                }
            }
            catch(Exception)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    testButton.Content = "Start Test";
                    S1Red.IsVisible = false;
                    S1Green.IsVisible = false;
                    S2Red.IsVisible = false;
                    S2Green.IsVisible = false;
                    S3Red.IsVisible = false;
                    S3Green.IsVisible = false;
                    S4Red.IsVisible = false;
                    S4Green.IsVisible = false;
                    S5Red.IsVisible = false;
                    S5Green.IsVisible = false;
                    S6Red.IsVisible = false;
                    S6Green.IsVisible = false;
                    S7Red.IsVisible = false;
                    S7Green.IsVisible = false;
                    S8Red.IsVisible = false;
                    S8Green.IsVisible = false;
                });
            }
        }

        private void TestButtonButton_Click(object? sender, RoutedEventArgs? e)
        {
            testRunning = !testRunning;
            if (testRunning && DeviceComboBox.SelectedIndex == 0)
            {
                Thread thread = new(TestProgramThread);
                thread.Start();
            }
        }

        private void LoadTestProgramThread()
        {
            Dispatcher.UIThread.Post(() =>
            {
                L1Red.IsVisible = false;
                L1Green.IsVisible = false;
            });

            DriveInfo[] drives = DriveInfo.GetDrives();
            bool found = false;
            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.VolumeLabel == "RPI-RP2")
                {
                    File.Copy(Path.Combine(".", "vision_flex_test_client.ino.uf2"), Path.Combine(drive.Name, "vision_flex_test_client.ino.uf2"), true);
                    found = true;
                }
            }

            if (!found)
            {
                ForcePiPicoIntoBootSel();

                drives = DriveInfo.GetDrives();
                found = false;
                foreach (var drive in drives)
                {
                    if (drive.IsReady && drive.VolumeLabel == "RPI-RP2")
                    {
                        File.Copy(Path.Combine(".", "vision_flex_test_client.ino.uf2"), Path.Combine(drive.Name, "vision_flex_test_client.ino.uf2"), true);
                        found = true;
                    }
                }
            }

            if (found)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    L1Red.IsVisible = false;
                    L1Green.IsVisible = true;
                });
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    L1Red.IsVisible = true;
                    L1Green.IsVisible = false;
                });
            }
        }

        private void LoadTestProgramButtonButton_Click(object? sender, RoutedEventArgs? e)
        {
            if (DeviceComboBox.SelectedIndex == 0)
            {
                Thread thread = new(LoadTestProgramThread);
                thread.Start();
            }
        }

        private void GoButton_Click(object? sender, RoutedEventArgs? e)
        {
            if (DeviceComboBox.SelectedIndex == 0)
            {
                Thread thread = new(UpdateFlexThread);
                thread.Start();
            }
        }
    }
}