﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintendoSpy.Readers
{
    static public class Playstation2
    {
        const int PACKET_SIZE = 152;
        const int POLISHED_PACKET_SIZE = 33;

        static readonly string[] BUTTONS = {
            null, "select", "lstick", "rstick", "start", "up", "right", "down", "left", "l2", "r2", "l1", "r1", "triangle", "circle", "x", "square"
        };

        static float readAnalogButton(byte input)
        {
            return (float) (input) / 256;
        }
        static float readStick(byte input)
        {
            return (float)(input - 128) / 128;
        }

        static public ControllerState ReadFromPacket(byte[] packet)
        {
            if (packet.Length < PACKET_SIZE) return null;

            byte[] polishedPacket = new byte[POLISHED_PACKET_SIZE];

            polishedPacket[0] = 0;
            for (byte i = 0; i < 8; ++i)
            {
                polishedPacket[0] |= (byte)((packet[i] == 0 ? 0 : 1) << i);
            }

            Console.WriteLine(polishedPacket[0]);

            for (byte i = 0; i < 16; ++i)
            {
                polishedPacket[i + 1] = packet[i + 8];
            }

            if (polishedPacket[0] == 0x73 || polishedPacket[0] == 0x79)
            {
                for (int i = 0; i < 4; ++i)
                {
                    polishedPacket[17 + i] = 0;
                    for (byte j = 0; j < 8; ++j)
                    {
                        polishedPacket[17 + i] |= (byte)((packet[24 + (i * 8 + j)] == 0 ? 0 : 1) << j);
                    }
                }
            }

            if (polishedPacket[0] == 0x79)
            {
                for (int i = 0; i < 12; ++i)
                {
                    polishedPacket[21 + i] = 0;
                    for (byte j = 0; j < 8; ++j)
                    {
                        polishedPacket[21 + i] |= (byte)((packet[56 + (i * 8 + j)] == 0 ? 0 : 1) << j);
                    }
                }
            }

            if (polishedPacket.Length < POLISHED_PACKET_SIZE) return null;
            // Currently only support digital and analog in red mode
            if (polishedPacket[0] != 0x41 && polishedPacket[0] != 0x73 && polishedPacket[0] != 0x79) return null;

            var state = new ControllerStateBuilder();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
                if (string.IsNullOrEmpty(BUTTONS[i])) continue;
                state.SetButton(BUTTONS[i], polishedPacket[i] != 0x00);
            }

            if (polishedPacket[0] == 0x73 || polishedPacket[0] == 0x79)
            {
                state.SetAnalog("rstick_x", readStick(polishedPacket[17]));
                state.SetAnalog("rstick_y", readStick(polishedPacket[18]));
                state.SetAnalog("lstick_x", readStick(polishedPacket[19]));
                state.SetAnalog("lstick_y", readStick(polishedPacket[20]));
            }

            if (polishedPacket[0] == 0x79)
            {
                state.SetAnalog("analog_right", readAnalogButton(polishedPacket[21]));
                state.SetAnalog("analog_left", readAnalogButton(polishedPacket[22]));
                state.SetAnalog("analog_up", readAnalogButton(polishedPacket[23]));
                state.SetAnalog("analog_down", readAnalogButton(polishedPacket[24]));

                state.SetAnalog("analog_triangle", readAnalogButton(polishedPacket[25]));
                state.SetAnalog("analog_circle", readAnalogButton(polishedPacket[26]));
                state.SetAnalog("analog_x", readAnalogButton(polishedPacket[27]));
                state.SetAnalog("analog_square", readAnalogButton(polishedPacket[28]));

                state.SetAnalog("analog_l1", readAnalogButton(polishedPacket[29]));
                state.SetAnalog("analog_r1", readAnalogButton(polishedPacket[30]));
                state.SetAnalog("analog_l2", readAnalogButton(polishedPacket[31]));
                state.SetAnalog("analog_r2", readAnalogButton(polishedPacket[32]));
            }

            return state.Build();
        }
    }
}