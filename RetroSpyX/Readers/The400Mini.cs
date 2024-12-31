using System;
using Vortice.XInput;

namespace RetroSpy.Readers
{
    public static class The400Mini
    {
        private const int CONTROLLER_PACKET_SIZE = 17;

        private static readonly string[] BUTTONS = {
            "right2", "up2", "1", "left2", "down2"
        };

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length != CONTROLLER_PACKET_SIZE)
            {
                return null;
            }

            string str = System.Text.Encoding.Default.GetString(packet, 0, packet.Length - 1);

            byte[] binaryPacket = new byte[(packet.Length - 1) / 2];

            try
            {
                for (int i = 0; i < str.Length; i += 2)
                {
                    binaryPacket[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
                }
            }
            catch (ArgumentException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }

            ControllerStateBuilder outState = new();

                for (int i = 0; i < BUTTONS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS[i]))
                    {
                        continue;
                    }
                    outState.SetButton(BUTTONS[i], (binaryPacket[6] & (1 << i)) != 0);
                }

            float x = 0;
            float y = 0;

            if (binaryPacket[0] == 255)
            {
                x = 1;
            }
            else if (binaryPacket[0] == 0)
            {
                x = -1;
            }

            if (binaryPacket[1] == 0)
            {
                y = 1;
            }
            else if (binaryPacket[1] == 255)
            {
                y = -1;
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

            outState.SetAnalog("x", x, 0);
            outState.SetAnalog("y", y, 0);

            outState.SetButton("left", binaryPacket[0] == 0);
            outState.SetButton("right", binaryPacket[0] == 255);
            outState.SetButton("up", binaryPacket[1] == 0);
            outState.SetButton("down", binaryPacket[1] == 255);

            return outState.Build();

        }
    }
}