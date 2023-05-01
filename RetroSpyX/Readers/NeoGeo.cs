using System;

namespace RetroSpy.Readers
{
    public static class NeoGeo
    {
        private const int PACKET_SIZE = 10;

        private static readonly string[] BUTTONS = {
            "select", "B", "D", "right", "down", "start", "C", "A", "left", "up"
        };

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

            ControllerStateBuilder state = new();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS[i]))
                {
                    continue;
                }

                state.SetButton(BUTTONS[i], packet[i] != 0x00);
            }

            float lstick_x = 0;
            float lstick_y = 0;

            if (packet[3] != 0x00)
            {
                lstick_x = 1;
            }
            else if (packet[8] != 0x00)
            {
                lstick_x = -1;
            }

            if (packet[4] != 0x00)
            {
                lstick_y = 1;
            }
            else if (packet[9] != 0x00)
            {
                lstick_y = -1;
            }

            if (lstick_y != 0 || lstick_x != 0)
            {
                // point on the unit circle at the same angle
                double radian = Math.Atan2(lstick_y, lstick_x);
                float x1 = (float)Math.Cos(radian);
                float y1 = (float)Math.Sin(radian);

                // Don't let magnitude exceed the unit circle
                if (Math.Sqrt(Math.Pow(lstick_x, 2) + Math.Pow(lstick_y, 2)) > 1.0)
                {
                    lstick_x = x1;
                    lstick_y = y1;
                }
            }

            state.SetAnalog("lstick_x", lstick_x, 0);
            state.SetAnalog("lstick_y", lstick_y, 0);

            return state.Build();
        }
    }
}