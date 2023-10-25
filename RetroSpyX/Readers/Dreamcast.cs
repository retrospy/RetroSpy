using System;

namespace RetroSpy.Readers
{
    public static class Dreamcast
    {
        private const int FRAME_HEADER_SIZE = 32;

        private static readonly string?[] MODIFER_KEYS =
        {
            null, null, null, null, null, null, null, null, "RightWindowsKey" /*S2*/, "RightAlt", "RightShift", "RightControl", "LeftWindowsKey" /*S1*/,  "LeftAlt", "LeftShift", "LeftControl"
        };

        private static readonly string?[] MODIFER_KEYS_PI =
{
            "LeftControl", "LeftShift", "LeftAlt", "LeftWindowsKey" /*S1*/, "RightControl", "RigthShift", "RightAlt", "RightWindowsKey" /*S2*/, null, null, null, null, null, null, null, null
        };

        private static readonly string?[] KEYS =
        {
            null, null, null, null, "A", "B", "C", "D",
            "E", "F", "G", "H", "I", "J", "K", "L",
            "M", "N", "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z", "D1", "D2",

            "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0",
            "Return", "Escape", "Back", "Tab", "Space", "Minus", "Equals", "LeftBracket",
            "RightBracket", "Backslash", "LeftOfReturn", "Semicolon", "Apostrophe", "Grave", "Comma", "Period",
            "Slash", "Capital", "F1", "F2", "F3", "F4", "F5", "F6",

            "F7", "F8", "F9", "F10", "F11", "F12", "PrintScreen", "ScrollLock",
            "Pause", "Insert", "Home", "PageUp", "Delete", "End", "PageDown", "Right",
            "Left", "Down", "Up", "NumberLock", "Divide", "Multiply", "Subtract", "Add",
            "NumberPadEnter", "NumberPad1", "NumberPad2", "NumberPad3", "NumberPad4", "NumberPad5", "NumberPad6", "NumberPad7",

            "NumberPad8", "NumberPad9", "NumberPad0", "Decimal", null, "Applications", null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,

            null, null, null, null, null, null, null, "JpSlash",
            "Katakana", "Yen", "Henkan", "Muhenkan", null, null, null, null
        };

        private static readonly string[] BUTTONS = {
            "right2", "left2", "down2", "up2", "d", "x", "y", "z", "right", "left", "down", "up", "start", "a", "b", "c"
        };

        private static readonly string[] BUTTONS_PI = {
            "c", "b", "a", "start", "up", "down", "left", "right", "z", "y", "x", "d", "up2", "down2", "left2", "right2"
        };

        private static readonly string?[] MOUSE_BUTTONS = {
            null, null, null, null, "start", "left", "right", "middle"
        };

        private static readonly string?[] MOUSE_BUTTONS_PI = {
            "middle", "right", "left", "start", null, null, null, null
        };

        private static float ReadTrigger(byte input)
        {
            return (float)input / 256;
        }

        private static float ReadStick(byte input)
        {
            return (float)(input - 128) / 128;
        }

        public static ControllerStateEventArgs? ReadFromPacket(byte[]? packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            if (packet.Length == 24 || packet.Length == 48)
            {
                byte[] reconstructedPacket = new byte[24];
                for (int i = 0; i < packet.Length / 2; ++i)
                {
                    reconstructedPacket[i] = (byte)((packet[i * 2] >> 4) | packet[(i * 2) + 1]);
                }

                ushort controllerType = (ushort)((reconstructedPacket[2] << 8) | reconstructedPacket[3]);

                if (controllerType == 0x01 && packet.Length == 24)
                {
                    ControllerStateBuilder pistate = new();

                    for (int i = 0; i < BUTTONS_PI.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(BUTTONS_PI[i]))
                        {
                            pistate.SetButton(BUTTONS_PI[i], (reconstructedPacket[i / 8 + 4] & (1 << (i % 8))) == 0x0);
                        }
                    }

                    pistate.SetAnalog("trig_l", ReadTrigger(reconstructedPacket[7]), reconstructedPacket[7]);
                    pistate.SetAnalog("trig_r", ReadTrigger(reconstructedPacket[6]), reconstructedPacket[6]);
                    pistate.SetAnalog("stick_x", ReadStick(reconstructedPacket[8]), reconstructedPacket[8]);
                    pistate.SetAnalog("stick_y", ReadStick(reconstructedPacket[9]), reconstructedPacket[9]);


                    return pistate.Build();
                }
                else if (controllerType == 0x200 && packet.Length == 48)
                {
                    ControllerStateBuilder pistate = new();

                    for (int i = 0; i < MOUSE_BUTTONS_PI.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(MOUSE_BUTTONS_PI[i]))
                        {
                            pistate.SetButton(MOUSE_BUTTONS_PI[i], (reconstructedPacket[4] & (1 << i)) == 0x0);
                        }
                    }

                    short axis1 = (short)(reconstructedPacket[11] << 8 | reconstructedPacket[10]);
                    short axis2 = (short)(reconstructedPacket[9] << 8 | reconstructedPacket[8]);
                    short axis3 = (short)(reconstructedPacket[13] << 8 | reconstructedPacket[12]);

                    float x = (axis2 - 512) / 512.0f;
                    float y = -1 * (axis1 - 512) / 512.0f;

                    SignalTool.SetMouseProperties(x, y, axis2, axis1, pistate);

                    pistate.SetButton("scroll_up", axis3 < 512);
                    pistate.SetButton("scroll_down", axis3 > 512);

                    return pistate.Build();
                }   
                else if (controllerType == 0x40 && packet.Length == 24)
                {
                    ControllerStateBuilder pistate = new();

                    for (int i = 0; i < MODIFER_KEYS_PI.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(MODIFER_KEYS_PI[i]))
                        {
                            pistate.SetButton(MODIFER_KEYS_PI[i], (reconstructedPacket[4] & (1 << i)) != 0x0);
                        }
                    }

                    for (int i = 0; i < KEYS.Length; ++i)
                    {
                        if (KEYS[i] != null)
                        {
                            pistate.SetButton(KEYS[i], false);
                        }
                    }

                    for (int i = 0; i < 6; ++i)
                    {
                        if (KEYS[reconstructedPacket[i + 6]] != null)
                        {
                            pistate.SetButton(KEYS[reconstructedPacket[i + 6]], true);
                        }
                    }

                    return pistate.Build();
                }
            }

            if (packet.Length < FRAME_HEADER_SIZE)
            {
                return null;
            }

            ControllerStateBuilder state = new();

            int j = 0;

            byte numWords = 0;
            for (int i = 0; i < 4; ++i)
            {
                numWords |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                numWords |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                j += 2;
            }

            // Skip sender and receiver address
            j += 16;

            try
            {

                byte dcCommand = 0;
                for (int i = 0; i < 4; ++i)
                {
                    dcCommand |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                    dcCommand |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                    j += 2;
                }

                if (dcCommand == 8 && numWords >= 1)
                {
                    uint controllerType = 0;
                    for (int i = 0; i < 2; i++)
                    {
                        for (int k = 0; k < 4; ++k)
                        {
                            controllerType |= (uint)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (k * 2) + (i * 8)));
                            controllerType |= (uint)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8)));
                            j += 2;
                        }
                    }

                    j += 16;

                    if (controllerType == 1 && numWords == 3)
                    {
                        byte ltrigger = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            ltrigger |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            ltrigger |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        byte rtrigger = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            rtrigger |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            rtrigger |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        int k = 0;
                        for (int i = 0; i < BUTTONS.Length / 2; ++i)
                        {
                            if (!string.IsNullOrEmpty(BUTTONS[k]))
                            {
                                state.SetButton(BUTTONS[k], (packet[j] & 0x2) == 0x0);
                            }

                            if (!string.IsNullOrEmpty(BUTTONS[k + 1]))
                            {
                                state.SetButton(BUTTONS[k + 1], (packet[j + 1] & 0x1) == 0x0);
                            }

                            k += 2;
                            j += 2;
                        }

                        byte joyy2 = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            joyy2 |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            joyy2 |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        byte joyx2 = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            joyx2 |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            joyx2 |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        byte joyy = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            joyy |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            joyy |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        byte joyx = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            joyx |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            joyx |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        state.SetAnalog("stick_x", ReadStick(joyx), joyx);
                        state.SetAnalog("stick_y", ReadStick(joyy), joyy);
                        state.SetAnalog("stick_x2", ReadStick(joyx2), joyx2);
                        state.SetAnalog("stick_y2", ReadStick(joyy2), joyy2);
                        state.SetAnalog("trig_r", ReadTrigger(rtrigger), rtrigger);
                        state.SetAnalog("trig_l", ReadTrigger(ltrigger), ltrigger);
                    }
                    else if (controllerType == 0x200 && numWords == 6)
                    {
                        j += 24;

                        int k = 0;
                        for (int i = 0; i < MOUSE_BUTTONS.Length / 2; ++i)
                        {
                            if (!string.IsNullOrEmpty(MOUSE_BUTTONS[k]))
                            {
                                state.SetButton(MOUSE_BUTTONS[k], (packet[j] & 0x2) == 0x0);
                            }

                            if (!string.IsNullOrEmpty(MOUSE_BUTTONS[k + 1]))
                            {
                                state.SetButton(MOUSE_BUTTONS[k + 1], (packet[j + 1] & 0x1) == 0x0);
                            }

                            k += 2;
                            j += 2;
                        }

                        ushort axis1 = 0;
                        for (int i = 1; i >= 0; --i)
                        {
                            for (k = 0; k < 4; ++k)
                            {
                                axis1 |= (ushort)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (k * 2) + (i * 8)));
                                axis1 |= (ushort)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8)));
                                j += 2;
                            }
                        }

                        ushort axis2 = 0;
                        for (int i = 1; i >= 0; --i)
                        {
                            for (k = 0; k < 4; ++k)
                            {
                                axis2 |= (ushort)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (k * 2) + (i * 8)));
                                axis2 |= (ushort)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8)));
                                j += 2;
                            }
                        }

                        j += 16;

                        ushort axis3 = 0;
                        for (int i = 1; i >= 0; --i)
                        {
                            for (k = 0; k < 4; ++k)
                            {
                                axis3 |= (ushort)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (k * 2) + (i * 8)));
                                axis3 |= (ushort)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (k * 2) + (i * 8)));
                                j += 2;
                            }
                        }

                        float x = (axis2 - 512) / 512.0f;
                        float y = -1 * (axis1 - 512) / 512.0f;

                        SignalTool.SetMouseProperties(x, y, axis2, axis1, state);

                        state.SetButton("scroll_up", axis3 < 512);
                        state.SetButton("scroll_down", axis3 > 512);
                    }
                    else if (controllerType == 0x40 && numWords == 3)
                    {
                        for (int i = 0; i < KEYS.Length; ++i)
                        {
                            if (KEYS[i] != null)
                            {
                                state.SetButton(KEYS[i], false);
                            }
                        }

                        byte[] keycode = new byte[6];

                        keycode[1] = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            keycode[1] |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            keycode[1] |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }
                        if (KEYS[keycode[1]] != null)
                        {
                            state.SetButton(KEYS[keycode[1]], true);
                        }

                        keycode[0] = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            keycode[0] |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            keycode[0] |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }
                        if (KEYS[keycode[0]] != null)
                        {
                            state.SetButton(KEYS[keycode[0]], true);
                        }

                        int k = 0;
                        for (int i = 0; i < MODIFER_KEYS.Length / 2; ++i)
                        {
                            if (!string.IsNullOrEmpty(MODIFER_KEYS[k]))
                            {
                                state.SetButton(MODIFER_KEYS[k], (packet[j] & 0x2) != 0x0);
                            }

                            if (!string.IsNullOrEmpty(MODIFER_KEYS[k + 1]))
                            {
                                state.SetButton(MODIFER_KEYS[k + 1], (packet[j + 1] & 0x1) != 0x0);
                            }

                            k += 2;
                            j += 2;
                        }

                        keycode[5] = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            keycode[5] |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            keycode[5] |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        if (KEYS[keycode[5]] != null)
                        {
                            state.SetButton(KEYS[keycode[5]], true);
                        }

                        keycode[4] = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            keycode[4] |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            keycode[4] |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        if (KEYS[keycode[4]] != null)
                        {
                            state.SetButton(KEYS[keycode[4]], true);
                        }

                        keycode[3] = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            keycode[3] |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            keycode[3] |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        if (KEYS[keycode[3]] != null)
                        {
                            state.SetButton(KEYS[keycode[3]], true);
                        }

                        keycode[2] = 0;
                        for (int i = 0; i < 4; ++i)
                        {
                            keycode[2] |= (byte)(((packet[j] & 0x02) != 0 ? 1 : 0) << (7 - (i * 2)));
                            keycode[2] |= (byte)(((packet[j + 1] & 0x01) != 0 ? 1 : 0) << (6 - (i * 2)));
                            j += 2;
                        }

                        if (KEYS[keycode[2]] != null)
                        {
                            state.SetButton(KEYS[keycode[2]], true);
                        }
                    }
                    return state.Build();
                }

            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }

            return null;
        }
    }
}