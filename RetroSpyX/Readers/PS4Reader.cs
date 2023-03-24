using System;

namespace RetroSpy.Readers
{
    public static class PS4Reader
    {
        private const int PACKET_SIZE = 154;
        private const int PACKET_HEADER = 21;
        private const int POLISHED_PACKET_SIZE = 34;

        private static readonly string[] BUTTONS = {
            "up", "down", "left", "right", "x", "circle", "square", "triangle", "l1", "l2", "lstick", "r1", "r2", "rstick", "share", "options", "ps", "trackpad", "trackpad0_touch", "trackpad1_touch"
        };

        private static float ReadStick(byte input)
        {
            return (float)(input - 128) / 128;
        }
        private static float ReadTouchPad(int input, int maxValue)
        {
            return input > maxValue ? 1.0f : input < 0 ? 0.0f : input / (float)maxValue;
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

            byte[] polishedPacket = new byte[POLISHED_PACKET_SIZE];

            for (int i = 0; i < 20; ++i)
            {
                polishedPacket[i] = (byte)((packet[PACKET_HEADER + i] == 0x31) ? 1 : 0);
            }

            for (int i = 0; i < 14; ++i)
            {
                polishedPacket[20 + i] = 0;
                for (byte j = 0; j < 8; ++j)
                {
                    polishedPacket[20 + i] |= (byte)((packet[PACKET_HEADER + 20 + (i * 8) + j] == 0x30 ? 0 : 1) << j);
                }
            }

            ControllerStateBuilder outState = new();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS[i]))
                {
                    continue;
                }

                outState.SetButton(BUTTONS[i], polishedPacket[i] != 0x00);
            }

            outState.SetAnalog("rstick_x", ReadStick(polishedPacket[22]), polishedPacket[22]);
            outState.SetAnalog("rstick_y", ReadStick(polishedPacket[23]), polishedPacket[23]);
            outState.SetAnalog("lstick_x", ReadStick(polishedPacket[20]), polishedPacket[20]);
            outState.SetAnalog("lstick_y", ReadStick(polishedPacket[21]), polishedPacket[21]);

            outState.SetAnalog("l_trig", ReadStick(polishedPacket[24]), polishedPacket[24]);
            outState.SetAnalog("r_trig", ReadStick(polishedPacket[25]), polishedPacket[25]);

            int touchpad_x1 = (polishedPacket[27] << 8) | polishedPacket[26];
            int touchpad_y1 = (polishedPacket[29] << 8) | polishedPacket[28];
            int touchpad_x2 = (polishedPacket[31] << 8) | polishedPacket[30];
            int touchpad_y2 = (polishedPacket[33] << 8) | polishedPacket[32];

            if (polishedPacket[18] == 1) // touch
            {
                if (polishedPacket[17] == 1) // click
                {
                    outState.SetAnalog("touchpad_x3", ReadTouchPad(touchpad_x1, 1920), touchpad_x1);
                    outState.SetAnalog("touchpad_y3", ReadTouchPad(touchpad_y1, 943), touchpad_y1);
                }
                else
                {
                    outState.SetAnalog("touchpad_x1", ReadTouchPad(touchpad_x1, 1920), touchpad_x1);
                    outState.SetAnalog("touchpad_y1", ReadTouchPad(touchpad_y1, 943), touchpad_y1);
                }
            }

            if (polishedPacket[19] == 1) // touch
            {
                if (polishedPacket[17] == 1) // click
                {
                    outState.SetAnalog("touchpad_x4", ReadTouchPad(touchpad_x2, 1920), touchpad_x2);
                    outState.SetAnalog("touchpad_y4", ReadTouchPad(touchpad_y2, 943), touchpad_y2);
                }
                else
                {
                    outState.SetAnalog("touchpad_x2", ReadTouchPad(touchpad_x2, 1920), touchpad_x2);
                    outState.SetAnalog("touchpad_y2", ReadTouchPad(touchpad_y2, 943), touchpad_y2);
                }
            }

            return outState.Build();
        }
    }
}