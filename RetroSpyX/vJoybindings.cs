using RetroSpy.Readers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Desktop.Robot;
using System.Runtime.InteropServices;
using Avalonia.Data;
using Avalonia.Controls;

namespace RetroSpy
{
    public partial class vJoybindings
    {
        private class AnalogAxis
        {
            public readonly string inputAxis;
            public readonly vJoyAxis outputAxis;

            public AnalogAxis(string inputAxis, string outputAxis)
            {
                this.inputAxis = inputAxis;
                this.outputAxis = GetvJoyAxis(outputAxis);
            }

            private static vJoyAxis GetvJoyAxis(string outputAxis)
            {
                switch (outputAxis)
                {
                    case "lstick_x":
                        return vJoyAxis.X;
                    case "lstick_y":
                        return vJoyAxis.Y;
                    case "rstick_x":
                        return vJoyAxis.Z;
                    case "rstick_y":
                        return vJoyAxis.ZR;
                    case "l2_analog":
                        return vJoyAxis.XR;
                    case "r2_analog":
                        return vJoyAxis.YR;
                    default:
                        throw new ConfigParseException(string.Format(CultureInfo.CurrentCulture, "Unknown vjoyoutput attribute in analog element: {0}", outputAxis));
                }

            }
        }

        private class DigitalButton
        {
            public readonly string inputButton;
            public readonly uint outputButton;

            public DigitalButton(string inputButton, string outputButton)
            {
                this.inputButton = inputButton;
                this.outputButton = GetvJoyButton(outputButton);
            }

            private static uint GetvJoyButton(string buttonName)
            {
                switch(buttonName)
                {
                    case "cross":
                        return 2;
                    case "circle":
                        return 4;
                    case "square":
                        return 1;
                    case "triangle":
                        return 3;
                    case "options":
                        return 10;
                    case "share":
                        return 9;
                    case "l1":
                        return 5;
                    case "l2":
                        return 7;
                    case "l3":
                        return 11;
                    case "r1":
                        return 6;
                    case "r2":
                        return 8;
                    case "r3":
                        return 12;
                    case "home":
                        return 13;
                    case "trackpad_click":
                        return 14;
                    default:
                        throw new ConfigParseException(string.Format(CultureInfo.CurrentCulture, "Unknown vjoyoutput attribute in button element: {0}", buttonName));
                }
            }
        }

        private class POV
        {
            public readonly string upName;
            public readonly string downName;
            public readonly string leftName;
            public readonly string rightName;
            
            public POV(string up, string down, string left, string right)
            {
                upName = up;
                downName = down;
                leftName= left;
                rightName = right;
            }

            public static int getPOV(bool up, bool down, bool left, bool right)
            {
                if (up && !right && !down && !left)
                    return 0;
                else if (up && right && !down && !left)
                    return 1;
                else if (!up && right && !down && !left)
                    return 2;
                else if (!up && right && down && !left)
                    return 3;
                else if (!up && !right && down && !left)
                    return 4;
                else if (!up && !right && down && left)
                    return 5;
                else if (!up && !right && !down && left)
                    return 6;
                else if (up && !right && !down && left)
                    return 7;
                else
                    return -1;
            }
        }

        public const string XmlFilePath = "vjoybindings.xml";

        private readonly IControllerReader _reader;
        private readonly List<DigitalButton> buttonBindings = new();
        private readonly List<AnalogAxis> axisBindings = new();
        private readonly List<POV> povBindings = new();

        public vJoybindings(string xmlFilePath, IControllerReader reader)
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
                foreach (XElement binding in doc.Root.Elements("button"))
                {
                    string? outputKey = binding?.Attribute("vjoyoutput")?.Value;
                    string? inputKey = binding?.Attribute("input")?.Value;

                    if (inputKey != null && outputKey != null)
                        buttonBindings.Add(new DigitalButton(inputKey, outputKey));
                    else
                        throw new ConfigParseException(string.Format(CultureInfo.CurrentCulture, "input and vjoyoutput attributes are required for button elements"));
                }

                foreach (XElement binding in doc.Root.Elements("analog"))
                {
                    string? outputKey = binding?.Attribute("vjoyoutput")?.Value;
                    string? inputKey = binding?.Attribute("input")?.Value;

                    if (inputKey != null && outputKey != null)
                        axisBindings.Add(new AnalogAxis(inputKey, outputKey));
                    else
                        throw new ConfigParseException(string.Format(CultureInfo.CurrentCulture, "input and vjoyoutput attributes are required for button elements"));
                }

                foreach (XElement binding in doc.Root.Elements("pov"))
                {
                    string? up = binding?.Attribute("up")?.Value;
                    string? down = binding?.Attribute("down")?.Value; 
                    string? left = binding?.Attribute("left")?.Value;
                    string? right = binding?.Attribute("right")?.Value;

                    if (up != null && down != null && left != null && right != null)
                        povBindings.Add(new POV(up, down, left, right));
                    else
                        throw new ConfigParseException(string.Format(CultureInfo.CurrentCulture, "up, down, left and right attributes are required for POV elements"));
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

        private void Reader_ControllerStateChanged(object? reader, ControllerStateEventArgs e)
        {
            foreach (DigitalButton button in buttonBindings)
            {
                if (e.Buttons.ContainsKey(button.inputButton))
                    vJoyInterface.SetButton(button.outputButton, e.Buttons[button.inputButton]);
            }

            foreach (AnalogAxis axis in axisBindings)
            {
                if (e.Analogs.ContainsKey(axis.inputAxis))
                    vJoyInterface.SetAxis(axis.outputAxis, e.Analogs[axis.inputAxis]);
            }

            foreach (POV pov in povBindings)
            {
                if (e.Buttons.ContainsKey(pov.upName) && e.Buttons.ContainsKey(pov.downName) && e.Buttons.ContainsKey(pov.leftName) && e.Buttons.ContainsKey(pov.rightName))
                    vJoyInterface.SetPOV(POV.getPOV(e.Buttons[pov.upName], e.Buttons[pov.downName], e.Buttons[pov.leftName], e.Buttons[pov.rightName]));
            }
        }
    }
}