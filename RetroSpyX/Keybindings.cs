using RetroSpy.Readers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Desktop.Robot;
using System.Runtime.InteropServices;
using System.Linq;

namespace RetroSpy
{
    public partial class Keybindings
    {
        private class Binding
        {
            public readonly Key OutputKey;
            public readonly IReadOnlyList<string> RequiredButtons;

            public readonly IReadOnlyList<string>? Platforms;

            public bool CurrentlyDepressed;

            public Binding(Key outputKey, IReadOnlyList<string> requiredButtons, IReadOnlyList<string>? platforms)
            {
                OutputKey = outputKey;
                RequiredButtons = requiredButtons;
                Platforms = platforms;
            }
        }

        public const string XmlFilePath = "keybindings.xml";

        private readonly IControllerReader _reader;
        private readonly List<Binding> _bindings = new();

        public Keybindings(string xmlFilePath, IControllerReader reader)
        {
            string? baseDir = Path.GetDirectoryName(Environment.ProcessPath);

            string keybindings_location = xmlFilePath;

            if (baseDir != null)
            {


                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && baseDir.Contains("MacOS") && File.Exists(Path.Join(baseDir, "../Info.plist")))
                    keybindings_location = Path.Join(baseDir, Path.Join("../../../", xmlFilePath));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && baseDir.Contains("bin") && File.Exists(Path.Join(baseDir, Path.Join("..", xmlFilePath))))
                    keybindings_location = Path.Join(baseDir, Path.Join("..", xmlFilePath));
                else
                    keybindings_location = Path.Join(baseDir, xmlFilePath);
            }

            if (!File.Exists(keybindings_location))
            {
                throw new ConfigParseException(string.Format(CultureInfo.CurrentCulture, "Could not find {0}", XmlFilePath));
            }

            XDocument doc = XDocument.Load(keybindings_location);

            if (doc.Root != null)
            {
                foreach (XElement binding in doc.Root.Elements("binding"))
                {
                    string type = binding.Attribute("type")?.Value ?? string.Empty;
                    string[] typesVec = type != null ? type.Split(';') : Array.Empty<string>();

                    Key outputKey = ReadKeybinding(binding.Attribute("output-key")?.Value ?? string.Empty);
                    if (outputKey == Key.Shift)
                    {
                        continue;
                    }

                    List<string> requiredButtons = new();
                    foreach (XElement input in binding.Elements("input"))
                    {
                        string? buttonName = input?.Attribute("button")?.Value;
                        if (buttonName != null)
                            requiredButtons.Add(buttonName);
                    }

                    if (requiredButtons.Count < 1)
                    {
                        continue;
                    }

                    _bindings.Add(new Binding(outputKey, requiredButtons, typesVec.Length == 1 && typesVec[0] == string.Empty ? null : typesVec));
                }
            }

            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            if (_reader.GetType() == typeof(DelayedControllerReader))
            {
                ((DelayedControllerReader)_reader).ControllerStateChangedNoDelay += Reader_ControllerStateChanged;
            }
            else
            {
                _reader.ControllerStateChanged += Reader_ControllerStateChanged;
            }
        }

        public void Finish()
        {
            _reader.ControllerStateChanged -= Reader_ControllerStateChanged;
        }

        public string CurrentlyActiveType { get; set; }

        private void Reader_ControllerStateChanged(object? reader, ControllerStateEventArgs e)
        {
            foreach (Binding binding in _bindings)
            {
                if (binding.Platforms == null || binding.Platforms.Contains(CurrentlyActiveType))
                {
                    bool allRequiredButtonsDown = true;

                    foreach (string requiredButton in binding.RequiredButtons)
                    {
                        allRequiredButtonsDown &= e.Buttons[requiredButton];
                    }

                    if (allRequiredButtonsDown && !binding.CurrentlyDepressed)
                    {
                        SendKeys.PressKey(binding.OutputKey);
                        binding.CurrentlyDepressed = true;
                    }
                    else if (!allRequiredButtonsDown && binding.CurrentlyDepressed)
                    {
                        SendKeys.ReleaseKey(binding.OutputKey);
                        binding.CurrentlyDepressed = false;
                    }
                }
            }
        }

        

        private static Key ReadKeybinding(string name)
        {
            string upperName = name.ToUpperInvariant();

            return StringToKey.ContainsKey(upperName) ? StringToKey[upperName] : Key.Shift;
        }

        private static readonly IReadOnlyDictionary<string, Key> StringToKey = new Dictionary<string, Key> {
            { "A", Key.A},
            { "B", Key.B},
            { "C", Key.C},
            { "D", Key.D},
            { "E", Key.E},
            { "F", Key.F},
            { "G", Key.G},
            { "H", Key.H},
            { "I", Key.I},
            { "J", Key.J},
            { "K", Key.K},
            { "L", Key.L},
            { "M", Key.M},
            { "N", Key.N},
            { "O", Key.O},
            { "P", Key.P},
            { "Q", Key.Q},
            { "R", Key.R},
            { "S", Key.S},
            { "T", Key.T},
            { "U", Key.U},
            { "V", Key.V},
            { "W", Key.W},
            { "X", Key.X},
            { "Y", Key.Y},
            { "Z", Key.Z},
            { "1", Key.One},
            { "2", Key.Two},
            { "3", Key.Three},
            { "4", Key.Four},
            { "5", Key.Five},
            { "6", Key.Six},
            { "7", Key.Seven},
            { "8", Key.Eight},
            { "9", Key.Nine},
            { "0", Key.Zero},
            { "ENTER", Key.Enter},
            { "TAB", Key.Tab},
            { "ESC", Key.Esc},
            { "ESCAPE", Key.Esc},
            { "HOME", Key.Home},
            { "END", Key.End},
            { "LEFT", Key.Left},
            { "RIGHT", Key.Right},
            { "UP", Key.Up},
            { "DOWN", Key.Down},
            { "PGUP", Key.PageUp},
            { "PGDN", Key.PageDown},
            { "NUMLOCK", Key.NumLock},
            { "SCROLLLOCK", Key.ScrollLock},
            { "PRTSC", Key.PrintScreen},
            { "BREAK", Key.Pause},
            { "BACKSPACE", Key.Backspace},
            { "BKSP", Key.Backspace},
            { "BS", Key.Backspace},
            //{ "CLEAR", Key.Clear},
            { "CAPSLOCK", Key.CapsLock},
            { "INS", Key.Insert},
            { "INSERT", Key.Insert},
            { "DEL", Key.Delete},
            { "DELETE", Key.Delete},
            //{ "HELP", Key.Help},
            { "F1", Key.F1},
            { "F2", Key.F2},
            { "F3", Key.F3},
            { "F4", Key.F4},
            { "F5", Key.F5},
            { "F6", Key.F6},
            { "F7", Key.F7},
            { "F8", Key.F8},
            { "F9", Key.F9},
            { "F10", Key.F10},
            { "F11", Key.F11},
            { "F12", Key.F12},
            //{ "F13", Key.F13},
            //{ "F14", Key.F14},
            //{ "F15", Key.F15},
            //{ "F16", Key.F16},
            //{ "NUMPAD0", Key.NumPad0},
            //{ "NUMPAD1", Key.NumPad1},
            //{ "NUMPAD2", Key.NumPad2},
            //{ "NUMPAD3", Key.NumPad3},
            //{ "NUMPAD4", Key.NumPad4},
            //{ "NUMPAD5", Key.NumPad5},
            //{ "NUMPAD6", Key.NumPad6},
            //{ "NUMPAD7", Key.NumPad7},
            //{ "NUMPAD8", Key.NumPad8},
            //{ "NUMPAD9", Key.NumPad9},
            //{ "MULTIPLY", Key.Multiply},
            //{ "*", Key.Multiply},
            { "ADD", Key.Plus},
            { "+", Key.Plus},
            { "SUBTRACT", Key.Minus},
            { "-", Key.Minus},
            { "DIVIDE", Key.Slash},
            { "/", Key.Slash }
        };
    }
}