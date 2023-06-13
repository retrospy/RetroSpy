using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GBPUpdaterX2
{
    internal static class PNPUtilHelper
    {
        public static void InstallDriver(string inffile, TextBox txtBox)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                Arguments = $"/add-driver {inffile} /install",
                FileName = GetArchitectureExePath("pnputil.exe"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (var process = Process.Start(processStartInfo))
            {
                process?.WaitForExit();
                if (process?.ExitCode == 0 || process?.ExitCode == 259)
                {
                    string strExitCode = process.ExitCode.ToString();
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Driver install successful (exit code {strExitCode})\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                }
                else
                {
                    string strExitCode = process?.ExitCode.ToString() ?? "-1";
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Driver install failed (exit code {strExitCode})\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                }
            }
        }

        public static void UninstallDriver(OEMDriverInf guiltyDriver, TextBox txtBox)
        {
            Dispatcher.UIThread.Post(() =>
            {
                txtBox.Text += $"Uninstalling driver\n";
                txtBox.CaretIndex = int.MaxValue;
            });

            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                Arguments = $"/delete-driver {guiltyDriver.FileName} /uninstall /force",
                FileName = GetArchitectureExePath("pnputil.exe"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (var process = Process.Start(processStartInfo))
            {
                process?.WaitForExit();
                if (process?.ExitCode == 0)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += "Driver uninstall successful\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                }
                else
                {
                    string strExitCode = process?.ExitCode.ToString() ?? "-1";
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Driver uninstall failed (exit code {strExitCode})\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                }
            }
        }

        private static string GetArchitectureExePath(string executable)
        {
            var result = string.Empty;
            var sys32 = Environment.ExpandEnvironmentVariables($"%SystemRoot%\\System32\\{executable}");
            var sysna = Environment.ExpandEnvironmentVariables($"%SystemRoot%\\Sysnative\\{executable}");

            if (File.Exists(sys32))
                result = sys32;
            else if (File.Exists(sysna))
                result = sysna;

            return result;
        }
    }
}