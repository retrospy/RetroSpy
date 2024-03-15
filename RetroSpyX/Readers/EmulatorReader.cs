using Avalonia.Threading;
using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RetroSpy.Readers
{
    sealed public class EmulatorReader : IControllerReader
    {
        enum State
        {
            INIT,
            RUNNING,
            INVALIDATED,
        };

        public event EventHandler<ControllerStateEventArgs>? ControllerStateChanged;
        public event EventHandler? ControllerDisconnected;

        const double TIMER_MS = 1;
        State state;
#pragma warning disable IDE1006 // Naming Styles
        State _state
#pragma warning restore IDE1006 // Naming Styles
        {
            get { return state; }
            set { if (value != State.RUNNING && state != value) _lastRunningTime = DateTime.Now; state = value; }
        }

        DateTime _lastScanTime;
        DateTime _lastRunningTime = DateTime.Now;
        DispatcherTimer _timer;
#pragma warning disable IDE0044 // Add readonly modifier
        Process _process;
#pragma warning restore IDE0044 // Add readonly modifier
        IntPtr _controllerPadsPtr;
        int _interpretedInstructionsOffset;
        IntPtr _interpretedInstructionsPtr;
        byte[] _interpretedInstructions;
        int _animFrame = 0;
        int _loadingProgress = 0;

        static readonly string[] BUTTONS = {
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            "cright", "cleft", "cdown", "cup", "r", "l", null, null,
#pragma warning restore IDE0079 // Remove unnecessary suppression
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            "right", "left", "down", "up", "start", "z", "b", "a",
#pragma warning restore IDE0079 // Remove unnecessary suppression
        };

        static unsafe bool ByteArrayCompare(byte[] data1, byte[] data2)
        {
            if (data1 == data2)
                return true;
            if (data1.Length != data2.Length)
                return false;

            fixed (byte* bytes1 = data1, bytes2 = data2)
            {
                int len = data1.Length;
                int rem = len % (sizeof(long) * 16);
                long* b1 = (long*)bytes1;
                long* b2 = (long*)bytes2;
                long* e1 = (long*)(bytes1 + len - rem);

                while (b1 < e1)
                {
                    if (*(b1) != *(b2) || *(b1 + 1) != *(b2 + 1) ||
                        *(b1 + 2) != *(b2 + 2) || *(b1 + 3) != *(b2 + 3) ||
                        *(b1 + 4) != *(b2 + 4) || *(b1 + 5) != *(b2 + 5) ||
                        *(b1 + 6) != *(b2 + 6) || *(b1 + 7) != *(b2 + 7) ||
                        *(b1 + 8) != *(b2 + 8) || *(b1 + 9) != *(b2 + 9) ||
                        *(b1 + 10) != *(b2 + 10) || *(b1 + 11) != *(b2 + 11) ||
                        *(b1 + 12) != *(b2 + 12) || *(b1 + 13) != *(b2 + 13) ||
                        *(b1 + 14) != *(b2 + 14) || *(b1 + 15) != *(b2 + 15))
                        return false;
                    b1 += 16;
                    b2 += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data1[len - 1 - i] != data2[len - 1 - i])
                        return false;

                return true;
            }
        }

        void Scan()
        {
            _lastScanTime = DateTime.Now;
            try
            {
                _loadingProgress = 0;
#pragma warning disable IDE0090 // Use 'new(...)'
                List<long> ramPtrBaseSuggestions = new List<long>();
#pragma warning restore IDE0090 // Use 'new(...)'

                var name = _process.ProcessName.ToLower();
                int offset = 0;
                if (name.Contains("project64") || name.Contains("wine-preloader"))
                {
#pragma warning disable IDE0090 // Use 'new(...)'
                    DeepPointer[] ramPtrBaseSuggestionsDPtrs = {
                        new DeepPointer("Project64.exe", 0xD6A1C),     //1.6
                        new DeepPointer("RSP 1.7.dll", 0x4C054),
                        new DeepPointer("RSP 1.7.dll", 0x44B5C),        //2.3.2; 2.4
                    };
#pragma warning restore IDE0090 // Use 'new(...)'

#pragma warning disable IDE0090 // Use 'new(...)'
                    DeepPointer[] romPtrBaseSuggestionsDPtrs = {
                        new DeepPointer("Project64.exe", 0xD6A2C),     //1.6
                        new DeepPointer("RSP 1.7.dll", 0x4C050),
                        new DeepPointer("RSP 1.7.dll", 0x44B58)        //2.3.2; 2.4
                    };
#pragma warning restore IDE0090 // Use 'new(...)'

                    foreach (DeepPointer ramSuggestionPtr in ramPtrBaseSuggestionsDPtrs)
                    {
                        int ptr = -1;
                        try
                        {
                            ptr = ramSuggestionPtr.Deref<int>(_process);
                            ramPtrBaseSuggestions.Add(ptr);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                if (name.Contains("mupen64"))
                {
                    if (name == "mupen64")
                    {
                        // Current mupen releases
                        {
                            ramPtrBaseSuggestions.Add(0x00505CB0); // 1.0.9
                            ramPtrBaseSuggestions.Add(0x00505D80); // 1.0.9.1
                            ramPtrBaseSuggestions.Add(0x0050B110); // 1.0.10
                        }
                    }
                    else
                    {
                        // Legacy mupen versions
#pragma warning disable IDE0090 // Use 'new(...)'
                        Dictionary<string, int> mupenRAMSuggestions = new Dictionary<string, int>
                    {
                        { "mupen64-rerecording", 0x008EBA80 },
                        { "mupen64-pucrash", 0x00912300 },
                        { "mupen64_lua", 0x00888F60 },
                        { "mupen64-wiivc", 0x00901920 },
                        { "mupen64-RTZ", 0x00901920 },
                        { "mupen64-rrv8-avisplit", 0x008ECBB0 },
                        { "mupen64-rerecording-v2-reset", 0x008ECA90 },
                    };
#pragma warning restore IDE0090 // Use 'new(...)'
                        ramPtrBaseSuggestions.Add(mupenRAMSuggestions[name]);
                    }

                    offset = 0x20;
                }

                if (name.Contains("retroarch"))
                {
                    ramPtrBaseSuggestions.Add(0x80000000);
                    offset = 0x40;
                }

                _loadingProgress++;
#pragma warning disable IDE0090 // Use 'new(...)'
                MagicManager mm = new MagicManager(_process, ramPtrBaseSuggestions.ToArray(), offset, ref _loadingProgress);
#pragma warning restore IDE0090 // Use 'new(...)'
                _controllerPadsPtr = new IntPtr((long)mm.ramPtrBase + mm.controllerPadsOffset);
                _interpretedInstructionsOffset = mm.interpretedInstructionsOffset;
                _interpretedInstructionsPtr = new IntPtr((long)mm.ramPtrBase + _interpretedInstructionsOffset);
                _interpretedInstructions = mm.interpretedInstructions;

                _state = State.RUNNING;
            }
            catch (Exception)
            {
                _state = State.INVALIDATED;
            }
        }

        void DrawAnimation()
        {
            _animFrame++;

            var outState = new ControllerStateBuilder();
            for (int i = 0; i < _loadingProgress; i++)
            {
                outState.SetButton(BUTTONS[i], true);
            }

            outState.SetAnalog("stick_x", (float)Math.Sin((double)_animFrame / 10.0), _animFrame);
            outState.SetAnalog("stick_y", (float)Math.Cos((double)_animFrame / 10.0), _animFrame);

            ControllerStateChanged?.Invoke(this, outState.Build());
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EmulatorReader(string spid)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            int pid = int.Parse(spid);
            _process = Process.GetProcessById(pid);
#pragma warning disable IDE0017 // Simplify object initialization
            _timer = new DispatcherTimer();
#pragma warning restore IDE0017 // Simplify object initialization
            _timer.Interval = TimeSpan.FromMilliseconds(TIMER_MS);
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            _timer.Tick += Tick;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            _timer.Start();
        }

        void Tick(object sender, EventArgs e)
        {
            if (_process.HasExited)
            {
                ControllerDisconnected?.Invoke(sender, e);
                return;
            }

            if (_state == State.INIT)
            {
                Scan();
            }

            if (_state == State.INVALIDATED)
            {
                var now = DateTime.Now;
                if (now - _lastScanTime > TimeSpan.FromSeconds(1))
                {
                    Scan();
                }

                if (now - _lastRunningTime > TimeSpan.FromSeconds(5))
                {
                    DrawAnimation();
                }
            }

            if (_state != State.RUNNING)
            {
                return;
            }

            // Running...
            try
            {
                byte[] actualInstructions = _process.ReadBytes(_interpretedInstructionsPtr, _interpretedInstructions.Length);
                bool ok = ByteArrayCompare(actualInstructions, _interpretedInstructions);
                if (!ok)
                {
                    throw new ArgumentException("Validation failed!");
                }
            }
            catch (Exception)
            {
                _state = State.INVALIDATED;
                Scan();
                return;
            }

            ushort flags;
            sbyte x;
            sbyte y;
            try
            {
                var value = _process.ReadValue<uint>(_controllerPadsPtr);
                flags = (ushort)(value >> 16);
                x = (sbyte)(value >> 8);
                y = (sbyte)value;
            }
            catch (Exception)
            {
                // this is kind of a weird situation but let's consider this failure temporary...
                return;
            }

            var outState = new ControllerStateBuilder();

            for (int i = 0; i < BUTTONS.Length; ++i)
            {
#pragma warning disable IDE0083 // Use pattern matching
#pragma warning disable IDE0150 // Prefer 'null' check over type check
                if (!(BUTTONS[i] is object)) continue;
#pragma warning restore IDE0150 // Prefer 'null' check over type check
#pragma warning restore IDE0083 // Use pattern matching
                outState.SetButton(BUTTONS[i], 0 != (flags & (1 << i)));
            }

            outState.SetAnalog("stick_x", (float)x / 127.0f, x);
            outState.SetAnalog("stick_y", (float)y / 127.0f, y);

            ControllerStateChanged?.Invoke(this, outState.Build());
        }

        public void Finish()
        {
            if (_timer != null)
            {
                _timer.Stop();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _timer = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }
    }
}