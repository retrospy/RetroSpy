using System;
using System.Text;

namespace RetroSpy.Readers
{
    public static class PS4Reader_II
    {
        private const int PACKET_SIZE = 129;

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

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private static int[,] window = new int[4, 3];
        private static int windowPosition = 0;

        private static int middleOfThree(int a, int b, int c)
        {
            // Compare each three number to find middle  
            // number. Enter only if a > b 
            if (a > b)
            {
                if (b > c)
                    return b;
                else if (a > c)
                    return c;
                else
                    return a;
            }
            else
            {
                // Decided a is not greater than b. 
                if (a > c)
                    return a;
                else if (b > c)
                    return c;
                else
                    return b;
            }
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

            ControllerStateBuilder outState = new();

            outState.SetButton(BUTTONS[0], (binaryPacket[5] & 0x0F) == 0x07 
                                            || (binaryPacket[5] & 0x0F) == 0x01 || (binaryPacket[5] & 0x0F) == 0x00);
            outState.SetButton(BUTTONS[1], (binaryPacket[5] & 0x0F) == 0x05 
                                            || (binaryPacket[5] & 0x0F) == 0x04 || (binaryPacket[5] & 0x0F) == 0x03);
            outState.SetButton(BUTTONS[2], (binaryPacket[5] & 0x0F) == 0x07 
                                            || (binaryPacket[5] & 0x0F) == 0x06 || (binaryPacket[5] & 0x0F) == 0x05);
            outState.SetButton(BUTTONS[3], (binaryPacket[5] & 0x0F) == 0x03 
                                            || (binaryPacket[5] & 0x0F) == 0x02 || (binaryPacket[5] & 0x0F) == 0x01);
            
            outState.SetButton(BUTTONS[4], (binaryPacket[5] & 0b00100000) != 0);
            outState.SetButton(BUTTONS[5], (binaryPacket[5] & 0b01000000) != 0);
            outState.SetButton(BUTTONS[6], (binaryPacket[5] & 0b00010000) != 0);
            outState.SetButton(BUTTONS[7], (binaryPacket[5] & 0b10000000) != 0);

            outState.SetButton(BUTTONS[8], (binaryPacket[6] & 0b00000001) != 0);
            outState.SetButton(BUTTONS[9], (binaryPacket[6] & 0b00000100) != 0);
            outState.SetButton(BUTTONS[10], (binaryPacket[6] & 0b01000000) != 0);
            outState.SetButton(BUTTONS[11], (binaryPacket[6] & 0b00000010) != 0);
            outState.SetButton(BUTTONS[12], (binaryPacket[6] & 0b00001000) != 0);
            outState.SetButton(BUTTONS[13], (binaryPacket[6] & 0b10000000) != 0);

            outState.SetButton(BUTTONS[14], (binaryPacket[6] & 0b00010000) != 0);
            outState.SetButton(BUTTONS[15], (binaryPacket[6] & 0b00100000) != 0);
            outState.SetButton(BUTTONS[16], (binaryPacket[7] & 0b00000001) != 0);
            outState.SetButton(BUTTONS[17], (binaryPacket[7] & 0b00000010) != 0);

            outState.SetButton(BUTTONS[18], (binaryPacket[35] & 0b10000000) == 0);
            outState.SetButton(BUTTONS[19], (binaryPacket[39] & 0b10000000) == 0);

            outState.SetAnalog("lstick_x", ReadStick(binaryPacket[1]), binaryPacket[1]);
            outState.SetAnalog("lstick_y", ReadStick(binaryPacket[2]), binaryPacket[2]);
            outState.SetAnalog("rstick_x", ReadStick(binaryPacket[3]), binaryPacket[3]);
            outState.SetAnalog("rstick_y", ReadStick(binaryPacket[4]), binaryPacket[4]);

            outState.SetAnalog("l_trig", ReadStick(binaryPacket[8]), binaryPacket[3]);
            outState.SetAnalog("r_trig", ReadStick(binaryPacket[9]), binaryPacket[4]);

            window[0,windowPosition] = ((binaryPacket[37] & 0x0F) << 8) | binaryPacket[36];
            window[1,windowPosition] = (binaryPacket[38] << 4) | ((binaryPacket[37] & 0xF0) >> 4);
            window[2,windowPosition] = ((binaryPacket[41] & 0x0F) << 8) | binaryPacket[40];
            window[3,windowPosition] = (binaryPacket[42] << 4) | ((binaryPacket[41] & 0xF0) >> 4);
            windowPosition += 1;
            windowPosition %= 3;

            int touchpad_x1 = middleOfThree(window[0,0], window[0,1], window[0,2]);
            int touchpad_y1 = middleOfThree(window[1,0], window[1,1], window[1,2]);
            int touchpad_x2 = middleOfThree(window[2,0], window[2,1], window[2,2]);
            int touchpad_y2 = middleOfThree(window[3,0], window[3,1], window[3,2]);

            if ((binaryPacket[35] & 0b10000000) == 0) // touch
            {
                if ((binaryPacket[7] & 0b00000010) != 0) // click
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

            if ((binaryPacket[39] & 0b10000000) == 0) // touch
            {
                if ((binaryPacket[7] & 0b00000010) != 0) // click
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

            //byte[] polishedPacket = new byte[POLISHED_PACKET_SIZE];

            //for (int i = 0; i < 20; ++i)
            //{
            //    polishedPacket[i] = (byte)((packet[PACKET_HEADER + i] == 0x31) ? 1 : 0);
            //}

            //for (int i = 0; i < 14; ++i)
            //{
            //    polishedPacket[20 + i] = 0;
            //    for (byte j = 0; j < 8; ++j)
            //    {
            //        polishedPacket[20 + i] |= (byte)((packet[PACKET_HEADER + 20 + (i * 8) + j] == 0x30 ? 0 : 1) << j);
            //    }
            //}

            //ControllerStateBuilder outState = new();

            //for (int i = 0; i < BUTTONS.Length; ++i)
            //{
            //    if (string.IsNullOrEmpty(BUTTONS[i]))
            //    {
            //        continue;
            //    }

            //    outState.SetButton(BUTTONS[i], polishedPacket[i] != 0x00);
            //}

            //outState.SetAnalog("rstick_x", ReadStick(polishedPacket[22]), polishedPacket[22]);
            //outState.SetAnalog("rstick_y", ReadStick(polishedPacket[23]), polishedPacket[23]);
            //outState.SetAnalog("lstick_x", ReadStick(polishedPacket[20]), polishedPacket[20]);
            //outState.SetAnalog("lstick_y", ReadStick(polishedPacket[21]), polishedPacket[21]);

            //outState.SetAnalog("l_trig", ReadStick(polishedPacket[24]), polishedPacket[24]);
            //outState.SetAnalog("r_trig", ReadStick(polishedPacket[25]), polishedPacket[25]);

            //int touchpad_x1 = (polishedPacket[27] << 8) | polishedPacket[26];
            //int touchpad_y1 = (polishedPacket[29] << 8) | polishedPacket[28];
            //int touchpad_x2 = (polishedPacket[31] << 8) | polishedPacket[30];
            //int touchpad_y2 = (polishedPacket[33] << 8) | polishedPacket[32];

            //if (polishedPacket[18] == 1) // touch
            //{
            //    if (polishedPacket[17] == 1) // click
            //    {
            //        outState.SetAnalog("touchpad_x3", ReadTouchPad(touchpad_x1, 1920), touchpad_x1);
            //        outState.SetAnalog("touchpad_y3", ReadTouchPad(touchpad_y1, 943), touchpad_y1);
            //    }
            //    else
            //    {
            //        outState.SetAnalog("touchpad_x1", ReadTouchPad(touchpad_x1, 1920), touchpad_x1);
            //        outState.SetAnalog("touchpad_y1", ReadTouchPad(touchpad_y1, 943), touchpad_y1);
            //    }
            //}

            //if (polishedPacket[19] == 1) // touch
            //{
            //    if (polishedPacket[17] == 1) // click
            //    {
            //        outState.SetAnalog("touchpad_x4", ReadTouchPad(touchpad_x2, 1920), touchpad_x2);
            //        outState.SetAnalog("touchpad_y4", ReadTouchPad(touchpad_y2, 943), touchpad_y2);
            //    }
            //    else
            //    {
            //        outState.SetAnalog("touchpad_x2", ReadTouchPad(touchpad_x2, 1920), touchpad_x2);
            //        outState.SetAnalog("touchpad_y2", ReadTouchPad(touchpad_y2, 943), touchpad_y2);
            //    }
            //}

            //return outState.Build();
        }
    }
}