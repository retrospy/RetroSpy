#if OS_WINDOWS

using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Runtime.InteropServices;
using WUApiLib;

namespace GBPUpdaterX2
{
    internal static class WindowsUpdateHelper
    {
        public static bool BlockUpdatesFor(string deviceId, TextBox txtBox)
        {
            return EnumerateUpdatesFor(deviceId, txtBox, driverUpdate =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtBox.Text += $"Found pending update {driverUpdate.Title} {driverUpdate.DriverVerDate} : \n";
                    txtBox.CaretIndex = int.MaxValue;
                });

                if (driverUpdate.IsHidden)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Update was already hidden, skipping\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Update was not hidden, hiding it\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                    driverUpdate.IsHidden = true;
                }
            });
        }

        public static bool UnblockUpdatesFor(string deviceId, TextBox txtBox)
        {
            return EnumerateUpdatesFor(deviceId, txtBox, driverUpdate=>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    txtBox.Text += $"Found pending update {driverUpdate.Title} {driverUpdate.DriverVerDate} : t\n";
                    txtBox.CaretIndex = int.MaxValue;
                });
 
                if (!driverUpdate.IsHidden)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Update was not hidden, skipping\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                }
                else
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        txtBox.Text += $"Update was hidden, unhiding it\n";
                        txtBox.CaretIndex = int.MaxValue;
                    });
                    driverUpdate.IsHidden = false;
                }
            });
        }

        private static bool EnumerateUpdatesFor(string deviceId, TextBox txtBox, Action<IWindowsDriverUpdate> action)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Dispatcher.UIThread.Post(() =>
            {
                txtBox.Text += $"Looking for pending updates ...\n";
                txtBox.CaretIndex = int.MaxValue;
            });
#pragma warning disable CS8604 // Possible null reference argument.
                IUpdateSession? _updateSession = (IUpdateSession?)Activator.CreateInstance(Type.GetTypeFromProgID("Microsoft.Update.Session"));
#pragma warning restore CS8604 // Possible null reference argument.
                IUpdateSearcher? searcher = _updateSession?.CreateUpdateSearcher();
                bool updateFound = false;
                var updates = searcher?.Search("IsInstalled = 0");
                if (updates != null)
                {
                    foreach (IUpdate update in updates.Updates)
                    {
                        if (update is IWindowsDriverUpdate driverUpdate)
                        {
                            if (driverUpdate.DriverHardwareID.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
                            {
                                action(driverUpdate);

                                updateFound = true;
                            }
                        }
                    }
                }

                return updateFound;
            }

            return false;
        }
    }
}
#endif