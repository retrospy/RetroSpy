using Avalonia.Controls.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if OS_WINDOWS
using vJoyInterfaceWrap;
#endif

namespace RetroSpy
{
    public enum vJoyAxis
    {
        X = 48,
        Y = 49,
        Z = 50,
        XR = 51,
        YR = 52,
        ZR = 53,
        SL0 = 54,
        SL1 = 55,
        WHL = 56,
        POV = 57
    }

    public class vJoyInterface
    {
        static bool inited = false;
#if OS_WINDOWS
        static public vJoy joystick;
        static public vJoy.JoystickState iReport;

        static public uint id = 1;

        static long maxval = 0;
#endif
        public static void InitVJoy(uint id = 1)
        {
            if (!inited)
            {
#if OS_WINDOWS
                // Create one joystick object and a position structure.
                joystick = new vJoy();
                iReport = new vJoy.JoystickState();

                // Device ID can only be in the range 1-16

                vJoyInterface.id = id;
                if (id <= 0 || id > 16)
                {
                    throw new Exception(string.Format("Illegal device ID {0}", id));
                }

                // Get the driver attributes (Vendor ID, Product ID, Version Number)
                if (!joystick.vJoyEnabled())
                {
                    throw new Exception("vJoy driver not enabled: Failed Getting vJoy attributes.");
                }

                VjdStat status = joystick.GetVJDStatus(id);
                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        throw new Exception(string.Format("vJoy Device {0} is already owned by this feeder", id));
                    case VjdStat.VJD_STAT_FREE:
                        Console.WriteLine("vJoy Device {0} is free\n", id);
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        throw new Exception(string.Format("vJoy Device {0} is already owned by another feeder", id));
                    case VjdStat.VJD_STAT_MISS:
                        throw new Exception(string.Format("vJoy Device {0} is not installed or disabled", id));
                    default:
                        throw new Exception(string.Format("vJoy Device {0} general error", id));
                };

                if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
                {
                    throw new Exception(string.Format("Failed to acquire vJoy device number {0}.", id));
                }

                joystick.GetVJDAxisMax(id, HID_USAGES.HID_USAGE_X, ref maxval);
                joystick.ResetVJD(id);

                inited = true;
#endif
            }
        }

        public static void SetAxis(vJoyAxis axis, float value)
        {

            if (inited)
            {
#if OS_WINDOWS
                joystick.SetAxis((int)(((value + 1.0) / 2.0) * maxval), id, (HID_USAGES)axis);
#endif
            }
        }

        public static void SetButton(uint buttonNumber, bool value)
        {
            if (inited)
            {
#if OS_WINDOWS
                joystick.SetBtn(value, id, buttonNumber);
#endif
            }
        }

        public static void SetPOV(int value)
        {
            if (inited)
            {
#if OS_WINDOWS
                switch (value)
                {
                    case 0:
                        joystick.SetContPov(0, id, 1);
                        break;
                    case 1:
                        joystick.SetContPov(4500, id, 1);
                        break;
                    case 2:
                        joystick.SetContPov(9000, id, 1);
                        break;
                    case 3:
                        joystick.SetContPov(13500, id, 1);
                        break;
                    case 4:
                        joystick.SetContPov(18000, id, 1);
                        break;
                    case 5:
                        joystick.SetContPov(22500, id, 1);
                        break;
                    case 6:
                        joystick.SetContPov(27000, id, 1);
                        break;
                    case 7:
                        joystick.SetContPov(31500, id, 1);
                        break;
                    default:
                        joystick.SetContPov(-1, id, 1);
                        break;
                }
#endif
            }
        }
    }
}
