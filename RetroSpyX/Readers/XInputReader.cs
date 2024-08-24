﻿using Avalonia.Threading;
using System;
using System.Collections.ObjectModel;
using Vortice.XInput;


namespace RetroSpy.Readers
{
    public sealed class XInputReader : IControllerReader
    {

        public event EventHandler<ControllerStateEventArgs>? ControllerStateChanged;

        public event EventHandler? ControllerDisconnected;

        public static Collection<int> GetDevices()
        {
            Collection<int> result = new();
            for (int i = 0; i < 4; i++) //Poll all 4 possible controllers to see which are connected, thats how it works :/
            {
                if (XInput.GetState(i, out _))
                {
                    result.Add(i);
                }
            }
            return result;
        }

        private const double TIMER_MS = 30;
        private DispatcherTimer? _timer;
        private readonly int _id;


        public XInputReader(int id = 0)
        {
            _id = id;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TIMER_MS)
            };
            _timer.Tick += Tick;
            _timer.Start();
        }

        private void Tick(object? sender, EventArgs e)
        {
            if (!XInput.GetState(_id, out State state))
            {
                ControllerDisconnected?.Invoke(this, EventArgs.Empty);
                Finish();
                return;
            }

            ControllerStateBuilder outState = new();

            outState.SetButton("a", ((int)state.Gamepad.Buttons & 0x1000) != 0);
            outState.SetButton("b", ((int)state.Gamepad.Buttons & 0x2000) != 0);
            outState.SetButton("x", ((int)state.Gamepad.Buttons & 0x4000) != 0);
            outState.SetButton("y", ((int)state.Gamepad.Buttons & 0x8000) != 0);

            bool up = ((int)state.Gamepad.Buttons & 0x0001) != 0;
            bool right = ((int)state.Gamepad.Buttons & 0x0008) != 0;
            bool down = ((int)state.Gamepad.Buttons & 0x0002) != 0;
            bool left = ((int)state.Gamepad.Buttons & 0x0004) != 0;

            outState.SetButton("up", up);
            outState.SetButton("down", down);
            outState.SetButton("left", left);
            outState.SetButton("right", right);

            outState.SetButton("start", ((int)state.Gamepad.Buttons & 0x0010) != 0);
            outState.SetButton("back", ((int)state.Gamepad.Buttons & 0x0020) != 0);
            outState.SetButton("l3", ((int)state.Gamepad.Buttons & 0x0040) != 0);
            outState.SetButton("r3", ((int)state.Gamepad.Buttons & 0x0080) != 0);
            outState.SetButton("l", ((int)state.Gamepad.Buttons & 0x0100) != 0);
            outState.SetButton("r", ((int)state.Gamepad.Buttons & 0x0200) != 0);

            outState.SetAnalog("lstick_x", (float)state.Gamepad.LeftThumbX / 32768, state.Gamepad.LeftThumbX);
            outState.SetAnalog("lstick_y", (float)state.Gamepad.LeftThumbY / 32768, state.Gamepad.LeftThumbY);
            outState.SetAnalog("rstick_x", (float)state.Gamepad.RightThumbX / 32768, state.Gamepad.RightThumbX);
            outState.SetAnalog("rstick_y", (float)state.Gamepad.RightThumbY / 32768, state.Gamepad.RightThumbY);
            outState.SetAnalog("trig_l", (float)state.Gamepad.LeftTrigger / 255, state.Gamepad.LeftTrigger);
            outState.SetButton("trig_l_d", ((float)state.Gamepad.LeftTrigger / 255) > 0);
            outState.SetAnalog("trig_r", (float)state.Gamepad.RightTrigger / 255, state.Gamepad.RightTrigger);
            outState.SetButton("trig_r_d", ((float)state.Gamepad.RightTrigger / 255) > 0);

            ControllerStateChanged?.Invoke(this, outState.Build());
        }

        public void Finish()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer = null;
            }
        }

    }
}