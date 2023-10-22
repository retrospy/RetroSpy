using Avalonia.Controls;
using System;
using System.Text;
using Vortice.XInput;

namespace RetroSpy.Readers
{
    public static class NeoGeoMini_II
    {
        private const int PACKET_SIZE = 22;

        private static readonly string?[] BUTTONS = {
            "A", "B", "G1", "C", "D", "G2", "W1", "W2", null, "options", "select", "start"

        };

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length < PACKET_SIZE)
            {
                return null;
            }

            byte[] binaryPacket = StringToByteArray(Encoding.UTF8.GetString(packet, 0, packet.Length).Trim());

            if ((binaryPacket.Length * 8) < BUTTONS.Length)
                return null;

            ControllerStateBuilder outState = new();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS[i]))
                {
                    continue;
                }

                outState.SetButton(BUTTONS[i], (binaryPacket[i / 8] & (1 << (i % 8))) != 0);
            }

            int position = binaryPacket[2] & 0x0F;
         
            float x = 0;
            float y = 0;

            switch (position)
            {
                case 0:
                    outState.SetButton("up", true);
                    y = -1;
                    outState.SetButton("down", false);
                    outState.SetButton("left", false);
                    outState.SetButton("right", false);
                    break;

                case 1:
                    outState.SetButton("up", true);
                    y = -1;
                    outState.SetButton("right", true);
                    x = 1;
                    outState.SetButton("down", false);
                    outState.SetButton("left", false);
                    break;

                case 2:
                    outState.SetButton("right", true);
                    x = 1;
                    outState.SetButton("down", false);
                    outState.SetButton("left", false);
                    outState.SetButton("up", false);
                    break;

                case 3:
                    outState.SetButton("right", true);
                    x = 1;
                    outState.SetButton("down", true);
                    y = 1;
                    outState.SetButton("up", false);
                    outState.SetButton("left", false);
                    break;

                case 4:
                    outState.SetButton("down", true);
                    y = 1;
                    outState.SetButton("up", false);
                    outState.SetButton("left", false);
                    outState.SetButton("right", false);
                    break;

                case 5:
                    outState.SetButton("left", true);
                    x = -1;
                    outState.SetButton("down", true);
                    y = 1;
                    outState.SetButton("right", false);
                    outState.SetButton("up", false);
                    break;

                case 6:
                    outState.SetButton("right", false);
                    outState.SetButton("down", false);
                    outState.SetButton("up", false);
                    outState.SetButton("left", true);
                    x = -1;
                    break;

                case 7:
                    outState.SetButton("up", true);
                    y = -1;
                    outState.SetButton("left", true);
                    x = -1;
                    outState.SetButton("right", false);
                    outState.SetButton("down", false);
                    break;

                default:
                    outState.SetButton("up", false);
                    outState.SetButton("left", false);
                    outState.SetButton("right", false);
                    outState.SetButton("down", false);
                    break;
            }

            if (y != 0 || x != 0)
            {
                // point on the unit circle at the same angle
                double radian = Math.Atan2(y, x);
                float x1 = (float)Math.Cos(radian);
                float y1 = (float)Math.Sin(radian);

                // Don't let magnitude exceed the unit circle
                if (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) > 1.0)
                {
                    x = x1;
                    y = y1;
                }
            }

            outState.SetAnalog("lstick_x", x, 0);
            outState.SetAnalog("lstick_y", y, 0);

            return outState.Build();
        }
    }
}