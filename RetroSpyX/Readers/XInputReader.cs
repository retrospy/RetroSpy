using Avalonia.Threading;
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
            vJoyInterface.SetButton(2, ((int)state.Gamepad.Buttons & 0x1000) != 0);
            outState.SetButton("b", ((int)state.Gamepad.Buttons & 0x2000) != 0);
            vJoyInterface.SetButton(4, ((int)state.Gamepad.Buttons & 0x2000) != 0);
            outState.SetButton("x", ((int)state.Gamepad.Buttons & 0x4000) != 0);
            vJoyInterface.SetButton(1, ((int)state.Gamepad.Buttons & 0x4000) != 0);
            outState.SetButton("y", ((int)state.Gamepad.Buttons & 0x8000) != 0);
            vJoyInterface.SetButton(3, ((int)state.Gamepad.Buttons & 0x8000) != 0);

            bool up = ((int)state.Gamepad.Buttons & 0x0001) != 0;
            bool right = ((int)state.Gamepad.Buttons & 0x0008) != 0;
            bool down = ((int)state.Gamepad.Buttons & 0x0002) != 0;
            bool left = ((int)state.Gamepad.Buttons & 0x0004) != 0;

            outState.SetButton("up", up);
            outState.SetButton("down", down);
            outState.SetButton("left", left);
            outState.SetButton("right", right);

            if (up && !right && !down && !left)
                vJoyInterface.SetPOV(0);
            else if (up && right && !down && !left)
                vJoyInterface.SetPOV(1);
            else if (!up && right && !down && !left)
                vJoyInterface.SetPOV(2);
            else if (!up && right && down && !left)
                vJoyInterface.SetPOV(3);
            else if (!up && !right && down && !left)
                vJoyInterface.SetPOV(4);
            else if (!up && !right && down && left)
                vJoyInterface.SetPOV(5);
            else if (!up && !right && !down && left)
                vJoyInterface.SetPOV(6);
            else if (up && !right && !down && left)
                vJoyInterface.SetPOV(7);
            else
                vJoyInterface.SetPOV(-1);

            outState.SetButton("start", ((int)state.Gamepad.Buttons & 0x0010) != 0);
            vJoyInterface.SetButton(10, ((int)state.Gamepad.Buttons & 0x0010) != 0);
            outState.SetButton("back", ((int)state.Gamepad.Buttons & 0x0020) != 0);
            vJoyInterface.SetButton(9, ((int)state.Gamepad.Buttons & 0x0020) != 0);
            outState.SetButton("l3", ((int)state.Gamepad.Buttons & 0x0040) != 0);
            vJoyInterface.SetButton(11, ((int)state.Gamepad.Buttons & 0x0040) != 0);
            outState.SetButton("r3", ((int)state.Gamepad.Buttons & 0x0080) != 0);
            vJoyInterface.SetButton(12, ((int)state.Gamepad.Buttons & 0x0080) != 0);
            outState.SetButton("l", ((int)state.Gamepad.Buttons & 0x0100) != 0);
            vJoyInterface.SetButton(5, ((int)state.Gamepad.Buttons & 0x0100) != 0);
            outState.SetButton("r", ((int)state.Gamepad.Buttons & 0x0200) != 0);
            vJoyInterface.SetButton(6, ((int)state.Gamepad.Buttons & 0x0200) != 0);

            //vJoyInterface.SetButton(13, ??);  // Home Button
            //vJoyInterface.SetButton(14, ??);  // TouchPad Click

            outState.SetAnalog("lstick_x", (float)state.Gamepad.LeftThumbX / 32768, state.Gamepad.LeftThumbX);
            vJoyInterface.SetAxis(vJoyAxis.X, (float)state.Gamepad.LeftThumbX / 32768);
            outState.SetAnalog("lstick_y", (float)state.Gamepad.LeftThumbY / 32768, state.Gamepad.LeftThumbY);
            vJoyInterface.SetAxis(vJoyAxis.Y, (float)state.Gamepad.LeftThumbY / 32768);
            outState.SetAnalog("rstick_x", (float)state.Gamepad.RightThumbX / 32768, state.Gamepad.RightThumbX);
            vJoyInterface.SetAxis(vJoyAxis.Z, (float)state.Gamepad.RightThumbX / 32768);
            outState.SetAnalog("rstick_y", (float)state.Gamepad.RightThumbY / 32768, state.Gamepad.RightThumbY);
            vJoyInterface.SetAxis(vJoyAxis.ZR, (float)state.Gamepad.RightThumbY / 32768);
            outState.SetAnalog("trig_l", (float)state.Gamepad.LeftTrigger / 255, state.Gamepad.LeftTrigger);
            vJoyInterface.SetAxis(vJoyAxis.XR, (float)state.Gamepad.LeftTrigger / 255);
            outState.SetButton("trig_l_d", ((float)state.Gamepad.LeftTrigger / 255) > 0);
            vJoyInterface.SetButton(7, ((float)state.Gamepad.LeftTrigger / 255) > 0);
            outState.SetAnalog("trig_r", (float)state.Gamepad.RightTrigger / 255, state.Gamepad.RightTrigger);
            vJoyInterface.SetAxis(vJoyAxis.YR, (float)state.Gamepad.RightTrigger / 255);
            outState.SetButton("trig_r_d", ((float)state.Gamepad.RightTrigger / 255) > 0);
            vJoyInterface.SetButton(8, ((float)state.Gamepad.RightTrigger / 255) > 0);

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