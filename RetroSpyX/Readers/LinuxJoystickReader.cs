using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Threading;
using ReactiveUI;

namespace RetroSpy.Readers;

public partial class LinuxJoystickReader : IControllerReader
{
    private const double TIMER_MS = 30;

    private const uint JSIOCSBTNMAP_LARGE = 1140877875;
    private const uint JSIOCSBTNMAP_SMALL = 1107323443;
    private const uint JSIOCGBTNMAP_LARGE = 2214619700;
    private const uint JSIOCGBTNMAP_SMALL = 2181065268;
    private const uint JSIOCGBTNMAP = 2214619700;
    private const uint JSIOCSBTNMAP = 1140877875;
    private const uint JSIOCSAXMAP = 1077963313;
    private const uint JSIOCGAXMAP = 2151705138;

    private const uint JSIOCGVERSION = 2147772929;
    private const uint JSIOCGAXES = 2147576337;
    private const uint JSIOCGBUTTONS = 2147576338;
    private const uint JSIOCGNAME = 2155899411;

    private static bool _shutdown;

    private static uint jsiocgbtnmap;

    private readonly int _id;
    private readonly byte[] axes = { 2 };

    private readonly string[] axis_names =
    {
        "X", "Y", "Z", "Rx", "Ry", "Rz", "s0" /*"Throttle"*/, "s1" /*"Rudder"*/,
        "Wheel", "Gas", "Brake", "?", "?", "?", "?", "?",
        "Hat0X", "Hat0Y", "Hat1X", "Hat1Y", "Hat2X", "Hat2Y", "Hat3X", "Hat3Y",
        "?", "?", "?", "?", "?", "?", "?"
    };

    //private readonly string[] button_names =
    //{
    //    "Btn0", "Btn1", "Btn2", "Btn3", "Btn4", "Btn5", "Btn6", "Btn7", "Btn8", "Btn9",
    //    "?", "?", "?", "?", "?", "?", "LeftBtn", "RightBtn", "MiddleBtn", "SideBtn",
    //    "ExtraBtn", "ForwardBtn", "BackBtn", "TaskBtn", "?", "?", "?", "?", "?", "?",
    //    "?", "?", "Trigger", "ThumbBtn", "ThumbBtn2", "TopBtn", "TopBtn2", "PinkieBtn",
    //    "BaseBtn", "BaseBtn2", "BaseBtn3", "BaseBtn4", "BaseBtn5", "BaseBtn6",
    //    "BtnDead", "BtnA", "BtnB", "BtnC", "BtnX", "BtnY", "BtnZ", "BtnTL", "BtnTR",
    //    "BtnTL2", "BtnTR2", "BtnSelect", "BtnStart", "BtnMode", "BtnThumbL",
    //    "BtnThumbR", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?", "?",
    //    "?", "?", "?", "?", "WheelBtn", "Gear up"
    //};

    private readonly byte[] buttons = { 2 };
    private Thread? _readerThread;
    private DispatcherTimer? _timer;


    private short[]? axis;
    private byte[]? button;

    public LinuxJoystickReader(int id)
    {
        _id = id;
        int i;
        byte[] version = { 0x00, 0x00, 0x08, 0x00 };
        var name = new byte[128];

        var defaultName = "Unknown";
        for (i = 0; i < defaultName.Length; i++) name[i] = (byte)defaultName[i];
        name[defaultName.Length] = 0;

        name[0] = (byte)'U';
        var btnmap = new short[512];
        var axmap = new byte[64];
        var btnmapok = 1;

        using (var fileHandle = File.OpenHandle(string.Format("/dev/input/js{0}", _id), FileMode.Open, FileAccess.Read,
                   FileShare.ReadWrite))
        {
            _ = ioctl(fileHandle.DangerousGetHandle().ToInt32(), JSIOCGVERSION, version);
            _ = ioctl(fileHandle.DangerousGetHandle().ToInt32(), JSIOCGAXES, axes);
            _ = ioctl(fileHandle.DangerousGetHandle().ToInt32(), JSIOCGBUTTONS, buttons);
            _ = ioctl(fileHandle.DangerousGetHandle().ToInt32(), JSIOCGNAME, name);

            Getaxmap(fileHandle.DangerousGetHandle().ToInt32(), axmap);
            Getbtnmap(fileHandle.DangerousGetHandle().ToInt32(), btnmap);

            /* Determine whether the button map is usable. */
            for (i = 0; btnmapok > 0 && i < buttons[0]; i++)
                if (btnmap[i] < 0x100 || btnmap[i] > 0x2ff)
                {
                    btnmapok = 0;
                    break;
                }
        }

        if (btnmapok == 0)
        {
            Finish();
            throw new UnhandledErrorException("Invalid Controller Button Map.");
        }

        _id = id;
        _readerThread = new Thread(ReadController);
        _readerThread.Start();

        _id = id;
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(TIMER_MS)
        };
        _timer.Tick += Tick;
        _timer.Start();
    }

    public event EventHandler<ControllerStateEventArgs>? ControllerStateChanged;
    public event EventHandler? ControllerDisconnected;


    public void Finish()
    {
        _shutdown = true;
        _readerThread?.Join();
        _readerThread = null;

        _timer?.Stop();
        _timer = null;
    }

    public void ReadController()
    {
        axis = new short[axes[0]];
        button = new byte[buttons[0]];
        var k = 0;

        while (!_shutdown)
            using (var fileHandle = File.OpenHandle(string.Format("/dev/input/js{0}", _id), FileMode.Open,
                       FileAccess.Read, FileShare.ReadWrite))
            {
                k = 0;

                while (k < axes[0] + buttons[0])
                {
                    var buf = new byte[8];
                    if (read(fileHandle.DangerousGetHandle().ToInt32(), buf, 8) != 8)
                    {
                        Finish();
                        ControllerDisconnected?.Invoke(this, EventArgs.Empty);
                        return;
                    }

                    switch (buf[6] & ~0x80)
                    {
                        case 0x1:
                            button[buf[7]] = buf[4];
                            break;
                        case 0x2:
                            axis[buf[7]] = (short)((buf[5] << 8) | buf[4]);
                            break;
                    }

                    k++;
                }
            }
    }

    private void Tick(object? sender, EventArgs e)
    {
        ControllerStateBuilder outState = new();

        for (var i = 0; i < button?.Length; ++i)
            outState.SetButton("b" + i.ToString(CultureInfo.CurrentCulture), button[i] != 0x00);

        for (var i = 0; i < axis?.Length; ++i)
        {
            if (i < axis_names.Length)
                outState.SetAnalog(axis_names[i].ToLower(), axis[i] / (float)short.MaxValue, axis[i]);

            outState.SetAnalog("a" + i.ToString(CultureInfo.CurrentCulture), axis[i] / (float)short.MaxValue, axis[i]);
        }

        if (axis?.Length >= 2)
        {
            if (axis[^2] < 0)
            {
                outState.SetButton("left", true);
                outState.SetButton("right", false);
            }
            else if (axis[^2] > 0)
            {
                outState.SetButton("right", true);
                outState.SetButton("left", false);
            }
            else
            {
                outState.SetButton("left", false);
                outState.SetButton("right", false);
            }

            if (axis[^1] < 0)
            {
                outState.SetButton("up", true);
                outState.SetButton("down", false);
            }
            else if (axis[^1] > 0)
            {
                outState.SetButton("down", true);
                outState.SetButton("up", false);
            }
            else
            {
                outState.SetButton("up", false);
                outState.SetButton("down", false);
            }
        }
        else
        {
            outState.SetButton("up", false);
            outState.SetButton("down", false);
            outState.SetButton("left", false);
            outState.SetButton("right", false);
        }

        ControllerStateChanged?.Invoke(this, outState.Build());
    }

    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    private static partial int ioctl(int handle, uint request, byte[] output);

    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    private static partial int ioctl_s(int handle, uint request, short[] output);

    [LibraryImport("libc", EntryPoint = "read", SetLastError = true)]
    private static partial int read(int fd, byte[] buf, int count);

    private static int Determine_ioctl_s(int fd, uint[] ioctls, ref uint ioctl_used, short[] argp)
    {
        int i, retval = 0;

        /* Try each ioctl in turn. */
        for (i = 0; ioctls[i] != 0; i++)
            if ((retval = ioctl_s(fd, ioctls[i], argp)) >= 0)
            {
                /* The ioctl did something. */
                ioctl_used = ioctls[i];
                return retval;
            }
            else if (Marshal.GetLastPInvokeError() != -22)
            {
                /* Some other error occurred. */
                return retval;
            }

        return retval;
    }

    private static int Getbtnmap(int fd, short[] btnmap)
    {
        uint[] ioctls = { JSIOCGBTNMAP, JSIOCGBTNMAP_LARGE, JSIOCGBTNMAP_SMALL, 0 };

        if (jsiocgbtnmap != 0)
            /* We already know which ioctl to use. */
            return ioctl_s(fd, jsiocgbtnmap, btnmap);
        return Determine_ioctl_s(fd, ioctls, ref jsiocgbtnmap, btnmap);
    }

    private static int Getaxmap(int fd, byte[] axmap)
    {
        return ioctl(fd, JSIOCGAXMAP, axmap);
    }

    public static IEnumerable<uint>? GetDevices()
    {
        List<uint> controllers = new();

        var files = Directory.GetFiles("/dev/input/");
        foreach (var file in files)
            if (file.StartsWith("/dev/input/js"))
                controllers.Add(uint.Parse(file[13].ToString()));

        return controllers;
    }
}