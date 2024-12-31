using System;

namespace RetroSpy.Readers
{
    public static class Classic
    {
        private const int PACKET_SIZE = 7;

        private static float maxX = float.MinValue, maxY = float.MinValue;

        private static readonly string[] BUTTONS = {
            "up", "down", "left", "right", "1", "2", "3"
        };

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length >= PACKET_SIZE)
            {
                ControllerStateBuilder state = new();

                for (int i = 0; i < BUTTONS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS[i]))
                    {
                        continue;
                    }

                    state.SetButton(BUTTONS[i], packet[i] != 0x00);
                }
                float x = 0;
                float y = 0;

                if (packet[3] != 0x00)
                {
                    x = 1;
                }
                else if (packet[2] != 0x00)
                {
                    x = -1;
                }

                if (packet[0] != 0x00)
                {
                    y = 1;
                }
                else if (packet[1] != 0x00)
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

                state.SetAnalog("x", x, 0);
                state.SetAnalog("y", y, 0);

                return state.Build();
            }
            else if (packet.Length == 2)
            {
                ControllerStateBuilder state = new();

                state.SetAnalog("paddle", packet[0] / 256.0f, packet[0]);
                state.SetButton("1", packet[1] != 0);

                return state.Build();
            }
            else if (packet.Length == 4)
            {
                ControllerStateBuilder state = new();

                sbyte x = (sbyte)packet[0];
                sbyte y = (sbyte)packet[1];

                if (Math.Abs(x) != 0 && Math.Abs(x) > maxX)
                {
                    maxX = Math.Abs(x);
                }

                if (Math.Abs(y) != 0 && Math.Abs(y) > maxY)
                {
                    maxY = Math.Abs(y);
                }

                SignalTool.SetMouseProperties(-1.0f * x / maxX, y / maxY, x, y, state);

                state.SetButton("1", packet[2] != 0);
                state.SetButton("2", packet[3] != 0);

                return state.Build();
            }
            else
            {
                return null;
            }
        }
    }
}