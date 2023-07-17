using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData.Experimental;
using LibUsbDotNet;
using MsBox.Avalonia.Enums;
using NodaTime;
using Renci.SshNet.Messages;
using RetroSpy.Readers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Image = Avalonia.Controls.Image;

namespace RetroSpy
{
    public partial class ViewWindow : Avalonia.Controls.Window
    {
        private readonly double _originalHeight;
        private readonly double _originalWidth;
        private readonly Skin _skin;
        private readonly IControllerReader _reader;
        private readonly Keybindings? _keybindings;
        private readonly BlinkReductionFilter _blinkFilter = new();
        private readonly List<Tuple<Detail, Image>> _detailsWithImages = new();
        private readonly List<Tuple<Button, Image>> _buttonsWithImages = new();
        private readonly List<Tuple<TouchPad, Image>> _touchPadWithImages = new();
        private readonly List<Tuple<RangeButton, Image>> _rangeButtonsWithImages = new();
        private readonly List<Tuple<AnalogStick, Image>> _sticksWithImages = new();
        private readonly List<Tuple<AnalogStickTracker, Image>> _trackersWithImages = new();
        private readonly List<Tuple<AnalogText, Label>> _analogTextBoxes = new();

        private readonly Dictionary<string, List<Tuple<Button, Image>>> _dictOfButtonsWithImages = new();

        // The triggers images are embedded inside of a Grid element so that we can properly mask leftwards and upwards
        // without the image aligning to the top left of its element.
        private readonly List<Tuple<AnalogTrigger, Grid>> _triggersWithGridImages = new();

        public ViewWindow()
        {
            throw new NotImplementedException();
        }

        private readonly SetupWindow _sw;

        public ViewWindow(SetupWindow sw, Skin? skin, Background? skinBackground, IControllerReader? reader, bool staticViewerWindowName)
        {
            Closing += (s, e) =>
            {
                Properties.Settings.Default.Save();
                _keybindings?.Finish();
                _reader?.Finish();
            };

            if (skin == null)
                throw new ArgumentNullException(nameof(skin));
            if (skinBackground == null)
                throw new ArgumentNullException(nameof(skinBackground));
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));



            _sw = sw;

            InitializeComponent();
            DataContext = this;

            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            else if (skin == null)
            {
                throw new ArgumentNullException(nameof(skin));
            }
            else if (skinBackground == null)
            {
                throw new ArgumentNullException(nameof(skinBackground));
            }




            _skin = skin;
            _reader = reader;

            Title = staticViewerWindowName ? "RetroSpy Viewer" : skin.Name;

            ControllerGrid.Width = Width = _originalWidth = skinBackground.Width;
            ControllerGrid.Height = Height = _originalHeight = skinBackground.Height;

            SolidColorBrush brush = new(skinBackground.Color);
            ControllerGrid.Background = brush;

            if (skinBackground.Image != null)
            {
                Image img = new()
                {
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    Source = skinBackground.Image,
                    Stretch = Stretch.Fill,
                    Margin = new Avalonia.Thickness(0, 0, 0, 0),
                    Width = skinBackground.Image.Size.Width,
                    Height = skinBackground.Image.Size.Height
                };

                ControllerGrid.Children.Add(img);
            }

            foreach (Detail detail in _skin.Details)
            {
                if (BgIsActive(skinBackground.Name, detail.Config?.TargetBackgrounds, detail.Config?.IgnoreBackgrounds))
                {
                    if (detail.Config != null)
                    {
                        detail.Config.X = detail.Config.OriginalX;
                        detail.Config.Y = detail.Config.OriginalY;
                        Image image = GetImageForElement(detail.Config);
                        _detailsWithImages.Add(new Tuple<Detail, Image>(detail, image));
                        ControllerGrid.Children.Add(image);
                    }
                }
            }

            foreach (AnalogTrigger trigger in _skin.AnalogTriggers)
            {
                if (BgIsActive(skinBackground.Name, trigger.Config?.TargetBackgrounds, trigger.Config?.IgnoreBackgrounds))
                {
                    if (trigger.Config != null)
                    {

                        trigger.Config.X = trigger.Config.OriginalX;
                        trigger.Config.Y = trigger.Config.OriginalY;
                        Grid grid = GetGridForAnalogTrigger(trigger);
                        _triggersWithGridImages.Add(new Tuple<AnalogTrigger, Grid>(trigger, grid));
                        ControllerGrid.Children.Add(grid);
                    }
                }
            }

            foreach (Button button in _skin.Buttons)
            {
                if (BgIsActive(skinBackground.Name, button.Config?.TargetBackgrounds, button.Config?.IgnoreBackgrounds))
                {
                    if (button.Config != null)
                    {
                        button.Config.X = button.Config.OriginalX;
                        button.Config.Y = button.Config.OriginalY;
                        Image image = GetImageForElement(button.Config);
                        Tuple<Button, Image> tuple = new(button, image);
                        _buttonsWithImages.Add(tuple);
                        if (_dictOfButtonsWithImages.ContainsKey(button.Name ?? string.Empty))
                        {
                            _dictOfButtonsWithImages[button.Name ?? string.Empty].Add(tuple);
                        }
                        else
                        {
                            List<Tuple<Button, Image>> list = new()
                        {
                            tuple
                        };
                            _dictOfButtonsWithImages.Add(button.Name ?? string.Empty, list);
                        }
                        image.IsVisible = false;
                        ControllerGrid.Children.Add(image);
                    }
                }
            }

            foreach (RangeButton button in _skin.RangeButtons)
            {
                if (BgIsActive(skinBackground.Name, button?.Config?.TargetBackgrounds, button?.Config?.IgnoreBackgrounds))
                {
                    if (button?.Config != null)
                    {
                        button.Config.X = button.Config.OriginalX;
                        button.Config.Y = button.Config.OriginalY;
                        Image image = GetImageForElement(button.Config);
                        _rangeButtonsWithImages.Add(new Tuple<RangeButton, Image>(button, image));
                        image.IsVisible = false;
                        ControllerGrid.Children.Add(image);
                    }
                }
            }

            foreach (AnalogStickTracker tracker in _skin.AnalogStickTrackers)
            {
                if (BgIsActive(skinBackground.Name, tracker.Config?.TargetBackgrounds, tracker.Config?.IgnoreBackgrounds))
                {
                    if (tracker.Config != null)
                    {
                        tracker.Config.X = tracker.Config.OriginalX;
                        tracker.Config.Y = tracker.Config.OriginalY;
                        tracker.XRange = tracker.OriginalXRange;
                        tracker.YRange = tracker.OriginalYRange;
                        Image image = GetImageForElement(tracker.Config);
                        _trackersWithImages.Add(new Tuple<AnalogStickTracker, Image>(tracker, image));
                        if (tracker?.VisibilityName?.Length > 0)
                        {
                            image.IsVisible = false;
                        }

                        ControllerGrid.Children.Add(image);
                    }
                }
            }

            foreach (AnalogStick stick in _skin.AnalogSticks)
            {
                if (BgIsActive(skinBackground.Name, stick.Config?.TargetBackgrounds, stick.Config?.IgnoreBackgrounds))
                {
                    if (stick.Config != null)
                    {
                        stick.Config.X = stick.Config.OriginalX;
                        stick.Config.Y = stick.Config.OriginalY;
                        stick.XRange = stick.OriginalXRange;
                        stick.YRange = stick.OriginalYRange;
                        Image image = GetImageForElement(stick.Config);
                        _sticksWithImages.Add(new Tuple<AnalogStick, Image>(stick, image));
                        if (stick?.VisibilityName?.Length > 0)
                        {
                            image.IsVisible = false;
                        }

                        ControllerGrid.Children.Add(image);
                    }
                }
            }

            foreach (TouchPad touchpad in _skin.TouchPads)
            {
                if (BgIsActive(skinBackground.Name, touchpad.Config?.TargetBackgrounds, touchpad.Config?.IgnoreBackgrounds))
                {
                    if (touchpad.Config != null)
                    {
                        touchpad.Config.X = touchpad.Config.OriginalX;
                        touchpad.Config.Y = touchpad.Config.OriginalY;
                        touchpad.XRange = touchpad.OriginalXRange;
                        touchpad.YRange = touchpad.OriginalYRange;
                        Image image = GetImageForElement(touchpad.Config);
                        _touchPadWithImages.Add(new Tuple<TouchPad, Image>(touchpad, image));
                        image.IsVisible = false;
                        ControllerGrid.Children.Add(image);
                    }
                }
            }

            foreach (AnalogText analogtext in _skin.AnalogTexts)
            {
                if (BgIsActive(skinBackground.Name, analogtext.TargetBackgrounds, analogtext.IgnoreBackgrounds))
                {
                    LayoutTransformControl l = new();
                    Label label = GetLabelForElement(analogtext);
                    l.Child = label;
                    _analogTextBoxes.Add(new Tuple<AnalogText, Label>(analogtext, label));
                    ControllerGrid.Children.Add(l);
                }
            }

            _reader.ControllerStateChanged += Reader_ControllerStateChanged;
            _reader.ControllerDisconnected += Reader_ControllerDisconnected;

            // This is terrible, but there is no way to show a dialog before the
            // window is visible.
            try
            {
                _keybindings = new Keybindings(Keybindings.XmlFilePath, _reader);
            }
            catch (ConfigParseException)
            {
                noKeyBindings = true;
                StartNoKeybindingDialogThread();
            }

            MassBlinkReductionEnabled = Properties.Settings.Default.MassFilter;
            AnalogBlinkReductionEnabled = Properties.Settings.Default.AnalogFilter;
            ButtonBlinkReductionEnabled = Properties.Settings.Default.ButtonFilter;
            Topmost = Properties.Settings.Default.TopMost;

            WindowAspectRatio.Register(this);
        }

        private void NoKeybindingDialogThread()
        {
            bool loop = true;
            while (loop)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    loop = !IsVisible;
                });
            }

            if (noKeyBindings)
            {

                Dispatcher.UIThread.Post(async() =>
                {
                    var m = MsBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandard("RetroSpy", "Error parsing keybindings.xml. Not binding any keys to gamepad inputs", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    this.Topmost = false;
                    await m.ShowWindowDialogAsync(this);
                });
            }
        }

        private void StartNoKeybindingDialogThread()
        {
            Thread thread = new(NoKeybindingDialogThread);
            thread.Start();
        }

        readonly bool noKeyBindings = false;

        private void Reader_ControllerDisconnected(object? sender, EventArgs e)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                Close();
            }
            else
            {
                Dispatcher.UIThread.Post(Close);
            }
        }

        private static Label GetLabelForElement(AnalogText config)
        {
            Label label = new()
            {
                FontFamily = config.Font ?? new("Segoe UI"),
                FontSize = config.Size,
                Margin = new Thickness(config.X, config.Y, 0, 0),
                Foreground = config.Color,
            };
            return label;
        }

        private void Reader_ControllerStateChanged(object? reader, ControllerStateEventArgs e)
        {
            e = _blinkFilter.Process(e);

            // This assumes you can't press left/right and up/down at the same time.  The code gets more complicated otherwise.
            Dictionary<string, bool> compassDirectionStates = new();
            if (e.Buttons.ContainsKey("up") && e.Buttons.ContainsKey("left") && e.Buttons.ContainsKey("right") && e.Buttons.ContainsKey("down"))
            {
                string[] compassDirections = { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest" };

                bool[] compassDirectionStatesTemp = new bool[8];
                compassDirectionStatesTemp[0] = e.Buttons["up"];
                compassDirectionStatesTemp[2] = e.Buttons["right"];
                compassDirectionStatesTemp[4] = e.Buttons["down"];
                compassDirectionStatesTemp[6] = e.Buttons["left"];

                if (compassDirectionStatesTemp[0] && compassDirectionStatesTemp[2])
                {
                    compassDirectionStatesTemp[1] = true;
                    compassDirectionStatesTemp[0] = compassDirectionStatesTemp[2] = false;
                }
                else if (compassDirectionStatesTemp[2] && compassDirectionStatesTemp[4])
                {
                    compassDirectionStatesTemp[3] = true;
                    compassDirectionStatesTemp[2] = compassDirectionStatesTemp[4] = false;
                }
                else if (compassDirectionStatesTemp[4] && compassDirectionStatesTemp[6])
                {
                    compassDirectionStatesTemp[5] = true;
                    compassDirectionStatesTemp[4] = compassDirectionStatesTemp[6] = false;
                }
                else if (compassDirectionStatesTemp[6] && compassDirectionStatesTemp[0])
                {
                    compassDirectionStatesTemp[7] = true;
                    compassDirectionStatesTemp[6] = compassDirectionStatesTemp[0] = false;
                }

                for (int i = 0; i < compassDirections.Length; ++i)
                {
                    compassDirectionStates[compassDirections[i]] = compassDirectionStatesTemp[i];
                }
            }

            foreach (Tuple<Button, Image> button in _buttonsWithImages)
            {
                if (e.Buttons.ContainsKey(button.Item1.Name ?? string.Empty))
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        button.Item2.IsVisible = e.Buttons[button.Item1.Name ?? string.Empty];
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            button.Item2.IsVisible = e.Buttons[button.Item1.Name ?? string.Empty];
                        });
                    }
                }
                else if (compassDirectionStates.ContainsKey(button.Item1.Name ?? string.Empty))
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        button.Item2.IsVisible = compassDirectionStates[button.Item1.Name ?? string.Empty];
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            button.Item2.IsVisible = compassDirectionStates[button.Item1.Name ?? string.Empty];
                        });
                    }
                }
            }

            foreach (Tuple<AnalogText, Label> text in _analogTextBoxes)
            {
                float value = 0;
                if (e.Analogs.ContainsKey(text.Item1.Name ?? string.Empty))
                {
                    value = e.Analogs[text.Item1.Name ?? string.Empty] * text.Item1.Range;
                }
                else if (e.RawAnalogs.ContainsKey(text.Item1.Name ?? string.Empty))
                {
                    value = e.RawAnalogs[text.Item1.Name ?? string.Empty] * text.Item1.Range;
                }

                text.Item2.Content = (int)value;
            }

            foreach (Tuple<RangeButton, Image> button in _rangeButtonsWithImages)
            {
                if (!e.Analogs.ContainsKey(button.Item1.Name ?? string.Empty))
                {
                    continue;
                }

                float value = e.Analogs[button.Item1.Name ?? string.Empty];
                bool visible = button.Item1.From <= value && value <= button.Item1.To;
                button.Item2.IsVisible = visible;
            }

            foreach (Tuple<AnalogStick, Image> stick in _sticksWithImages)
            {
                AnalogStick skin = stick.Item1;
                Image image = stick.Item2;

                float xrange = (skin.XReverse ? -1 : 1) * skin.XRange;
                float yrange = (skin.YReverse ? 1 : -1) * skin.YRange;

                if (skin.Config != null)
                {
                    float x = e.Analogs.ContainsKey(skin.XName ?? string.Empty)
                      ? skin.Config.X + (xrange * e.Analogs[skin.XName ?? string.Empty])
                      : skin.Config.X;

                    if (e.Analogs.ContainsKey(skin.XName ?? string.Empty) && Math.Abs(e.Analogs[skin.XName ?? string.Empty]) < skin.XPrecision)
                        x = skin.Config.X;

                    float y = e.Analogs.ContainsKey(skin.YName ?? string.Empty)
                          ? skin.Config.Y + (yrange * e.Analogs[skin.YName ?? string.Empty])
                          : skin.Config.Y;

                    if (e.Analogs.ContainsKey(skin.YName ?? string.Empty) && Math.Abs(e.Analogs[skin.YName ?? string.Empty]) < skin.YPrecision)
                        y = skin.Config.Y;

                    bool visibility = skin.VisibilityName?.Length == 0 || (e.Buttons.ContainsKey(skin.VisibilityName ?? string.Empty) && e.Buttons[skin.VisibilityName ?? string.Empty]);
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        image.Margin = new Thickness(x, y, 0, 0);
                        image.IsVisible = visibility;
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            image.Margin = new Thickness(x, y, 0, 0);
                            image.IsVisible = visibility;
                        });
                    }
                }
            }

            foreach (Tuple<AnalogStickTracker, Image> tracker in _trackersWithImages)
            {
                AnalogStickTracker skin = tracker.Item1;
                Image image = tracker.Item2;

                float xrange = (skin.XReverse ? -1 : 1) * skin.XRange;
                float yrange = (skin.YReverse ? 1 : -1) * skin.YRange;

                if (skin.Config != null)
                {
                    float x = e.Analogs.ContainsKey(skin.XName ?? string.Empty)
                      ? skin.Config.X + (xrange * e.Analogs[skin.XName ?? string.Empty])
                      : skin.Config.X;

                    if (e.Analogs.ContainsKey(skin.XName ?? string.Empty) && Math.Abs(e.Analogs[skin.XName ?? string.Empty]) < skin.XPrecision)
                        x = skin.Config.X;

                    float y = e.Analogs.ContainsKey(skin.YName ?? string.Empty)
                          ? skin.Config.Y + (yrange * e.Analogs[skin.YName ?? string.Empty])
                          : skin.Config.Y;

                    if (e.Analogs.ContainsKey(skin.YName ?? string.Empty) && Math.Abs(e.Analogs[skin.YName ?? string.Empty]) < skin.YPrecision)
                        y = skin.Config.Y;

                    double angle = Math.Atan2(y - skin.Config.Y, x - skin.Config.X) * 180 / Math.PI;

                    bool visibility = skin.VisibilityName?.Length == 0 || (e.Buttons.ContainsKey(skin.VisibilityName ?? string.Empty) && e.Buttons[skin.VisibilityName ?? string.Empty]);
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        image.Margin = new Thickness(skin.Config.X, skin.Config.Y, 0, 0);
                        image.IsVisible = visibility;
                        image.Width = skin.TrackingWidth;
                        image.Height = Math.Abs(Math.Sqrt(Math.Pow(x - skin.Config.X, 2) + Math.Pow(y - skin.Config.Y, 2)));
                        image.RenderTransform = new RotateTransform(angle - 90 % 360, 0, -image.Height/2);
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            image.Margin = new Thickness(skin.Config.X, skin.Config.Y, 0, 0);
                            image.IsVisible = visibility;
                            image.Width = skin.TrackingWidth;
                            image.Height = Math.Abs(Math.Sqrt(Math.Pow(x - skin.Config.X, 2) + Math.Pow(y - skin.Config.Y, 2)));
                            image.RenderTransform = new RotateTransform(angle - 90 % 360, 0, -image.Height / 2);

                        });
                    }
                }
            }

            foreach (Tuple<TouchPad, Image> touchpad in _touchPadWithImages)
            {
                TouchPad skin = touchpad.Item1;

                if (e.Analogs.ContainsKey(skin.XName ?? string.Empty) && e.Analogs.ContainsKey(skin.YName ?? string.Empty))
                {
                    if (skin.Config != null)
                    {
                        // Show
                        double x = (e.Analogs[skin.XName ?? string.Empty] * skin.XRange) + skin.Config.X - (touchpad.Item2.Width / 2);
                        double y = (e.Analogs[skin.YName ?? string.Empty] * skin.YRange) + skin.Config.Y - (touchpad.Item2.Height / 2);

                        if (Dispatcher.UIThread.CheckAccess())
                        {
                            touchpad.Item2.Margin = new Thickness(x, y, 0, 0);
                            touchpad.Item2.IsVisible = true;
                        }
                        else
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                touchpad.Item2.Margin = new Thickness(x, y, 0, 0);
                                touchpad.Item2.IsVisible = true;
                            });
                        }
                    }
                }
                else
                {
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        touchpad.Item2.IsVisible = false;
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            touchpad.Item2.IsVisible = false;
                        });
                    }
                }
            }

            foreach (Tuple<AnalogTrigger, Grid> trigger in _triggersWithGridImages)
            {
                AnalogTrigger skin = trigger.Item1;
                Grid grid = trigger.Item2;

                if (!e.Analogs.ContainsKey(skin.Name ?? string.Empty))
                {
                    continue;
                }

                float val = e.Analogs[skin.Name ?? string.Empty];
                if (skin.UseNegative)
                {
                    val *= -1;
                }

                if (skin.IsReversed)
                {
                    val = 1 - val;
                }

                if (val < 0)
                {
                    val = 0;
                }

                if (skin.Config != null)
                {
                    switch (skin.Direction)
                    {
                        case AnalogTrigger.DirectionValue.Right:
                            if (Dispatcher.UIThread.CheckAccess())
                            {
                                grid.Width = skin.Config.Width * val;
                            }
                            else
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    grid.Width = skin.Config.Width * val;
                                });
                            }
                            break;

                        case AnalogTrigger.DirectionValue.Left:
                            float width = skin.Config.Width * val;
                            float offx = skin.Config.Width - width;
                            if (Dispatcher.UIThread.CheckAccess())
                            {
                                grid.Margin = new Thickness(skin.Config.X + offx, skin.Config.Y, 0, 0);
                                grid.Width = width;
                            }
                            else
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    grid.Margin = new Thickness(skin.Config.X + offx, skin.Config.Y, 0, 0);
                                    grid.Width = width;
                                });
                            }
                            break;

                        case AnalogTrigger.DirectionValue.Down:
                            if (Dispatcher.UIThread.CheckAccess())
                            {
                                grid.Height = skin.Config.Height * val;
                            }
                            else
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    grid.Height = skin.Config.Height * val;
                                });
                            }
                            break;

                        case AnalogTrigger.DirectionValue.Up:
                            float height = skin.Config.Height * val;
                            float offy = skin.Config.Height - height;
                            if (Dispatcher.UIThread.CheckAccess())
                            {
                                grid.Margin = new Thickness(skin.Config.X, skin.Config.Y + offy, 0, 0);
                                grid.Height = height;
                            }
                            else
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    grid.Margin = new Thickness(skin.Config.X, skin.Config.Y + offy, 0, 0);
                                    grid.Height = height;
                                });
                            }
                            break;

                        case AnalogTrigger.DirectionValue.Fade:
                            if (Dispatcher.UIThread.CheckAccess())
                            {
                                grid.Height = skin.Config.Height;
                                grid.Width = skin.Config.Width;
                                grid.Opacity = val;
                            }
                            else
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    grid.Height = skin.Config.Height;
                                    grid.Width = skin.Config.Width;
                                    grid.Opacity = val;
                                });
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            foreach (KeyValuePair<string, float> analog in e.Analogs)
            {
                string[] buttonNames = { analog.Key + '-', analog.Key + '+' };
                bool[] buttonStates = { false, false };

                float posPrecision = _dictOfButtonsWithImages.ContainsKey(buttonNames[1]) ? _dictOfButtonsWithImages[buttonNames[1]].ToArray()[0].Item1.Precision : 0.0f;
                float negPrecision = -1.0f * (_dictOfButtonsWithImages.ContainsKey(buttonNames[0]) ? _dictOfButtonsWithImages[buttonNames[0]].ToArray()[0].Item1.Precision : 0.0f);

                if (analog.Value < negPrecision)
                {
                    buttonStates[0] = true;
                }
                else if (analog.Value > posPrecision)
                {
                    buttonStates[1] = true;
                }

                for (int i = 0; i < buttonNames.Length; ++i)
                {
                    if (_dictOfButtonsWithImages.ContainsKey(buttonNames[i]))
                    {
                        foreach (Tuple<Button, Image> button in _dictOfButtonsWithImages[buttonNames[i]])
                        {
                            if (Dispatcher.UIThread.CheckAccess())
                            {
                                button.Item2.IsVisible = buttonStates[i];
                            }
                            else
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    button.Item2.IsVisible = buttonStates[i];
                                });
                            }
                        }
                    }
                }
            }


        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            _keybindings?.Finish();
            _reader.Finish();

        }

        bool _firstOpenHasHappened = false;

        private void Window_Open(object sender, EventArgs e)
        {
            // This is a terrible hack to get around 
            // not being able to have the parent hidden 
            // from a show dialog.
            if (!_firstOpenHasHappened)
            {
                _sw.Hide();
                Show();
                _firstOpenHasHappened = true;
            }
        }

        private static Grid GetGridForAnalogTrigger(AnalogTrigger trigger)
        {
            Image img = new()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,

                HorizontalAlignment =
                  trigger.Direction == AnalogTrigger.DirectionValue.Left
                ? Avalonia.Layout.HorizontalAlignment.Right
                : Avalonia.Layout.HorizontalAlignment.Left
            };

            img.VerticalAlignment =
                  trigger.Direction == AnalogTrigger.DirectionValue.Up
                ? Avalonia.Layout.VerticalAlignment.Bottom
                : Avalonia.Layout.VerticalAlignment.Top;

            img.Source = trigger.Config?.Image;
            img.Stretch = Stretch.Fill;
            img.Margin = new Thickness(0, 0, 0, 0);
            img.Width = trigger.Config?.Width ?? 0;
            img.Height = trigger.Config?.Height ?? 0;

            Grid grid = new()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = new Thickness(trigger.Config?.X ?? 0, trigger.Config?.Y ?? 0, 0, 0),
                Width = trigger.Config?.Width ?? 0,
                Height = trigger.Config?.Height ?? 0,
                ClipToBounds = true
            };

            grid.Children.Add(img);

            return grid;
        }
        private static Image GetImageForElement(ElementConfig config)
        {
            Image img = new()
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                Source = config.Image,
                Stretch = Stretch.Fill,
                Margin = new Thickness(config.X, config.Y, 0, 0),
                Width = config.Width,
                Height = config.Height
            };
            return img;
        }

        private static bool BgIsActive(string? bgName, Collection<string>? eligableBgs, Collection<string>? ignoreBgs)
        {
            return ignoreBgs?.Contains(bgName ?? string.Empty) == false && (eligableBgs?.Count == 0 || eligableBgs?.Contains(bgName ?? string.Empty) == true);
        }

        /// Expose the enabled status of the low-pass filter for data binding.
        public bool ButtonBlinkReductionEnabled
        {
            get => _blinkFilter.ButtonEnabled;
            set
            {
                _blinkFilter.ButtonEnabled = value;
                OnPropertyChanged(nameof(ButtonBlinkReductionEnabled));
            }
        }

        public bool MassBlinkReductionEnabled
        {
            get => _blinkFilter.MassEnabled;
            set
            {
                _blinkFilter.MassEnabled = value;
                OnPropertyChanged(nameof(MassBlinkReductionEnabled));
            }
        }

        public bool AnalogBlinkReductionEnabled
        {
            get => _blinkFilter.AnalogEnabled;
            set
            {
                _blinkFilter.AnalogEnabled = value;
                OnPropertyChanged(nameof(AnalogBlinkReductionEnabled));
            }
        }

        public bool AllBlinkReductionEnabled
        {
            get => ButtonBlinkReductionEnabled && AnalogBlinkReductionEnabled && MassBlinkReductionEnabled;
            set
            {
                ButtonBlinkReductionEnabled = AnalogBlinkReductionEnabled = MassBlinkReductionEnabled = value;
                OnPropertyChanged(nameof(AllBlinkReductionEnabled));
            }
        }

        private void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
                Topmost = !Topmost;
            Properties.Settings.Default.TopMost = Topmost;
            OnTopCheckbox.IsChecked = Topmost;
        }

        public event PropertyChangedEventHandler? PropertyChangedEvent;
        private void OnPropertyChanged(string prop)
        {
            if (PropertyChangedEvent == null)
            {
                return;
            }

            PropertyChangedEvent(this, new PropertyChangedEventArgs(prop));
        }

        private void AllBlinkReductionEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
                AllBlinkReductionEnabled = !ButtonBlinkReductionEnabled || !AnalogBlinkReductionEnabled || !MassBlinkReductionEnabled;

            AllBlinkCheckbox.IsChecked = AllBlinkReductionEnabled;
            Properties.Settings.Default.ButtonFilter = ButtonBlinkReductionEnabled;
            ButtonBlinkCheckbox.IsChecked = ButtonBlinkReductionEnabled;
            Properties.Settings.Default.AnalogFilter = AnalogBlinkReductionEnabled;
            AnalogBlinkCheckbox.IsChecked = AnalogBlinkReductionEnabled;
            Properties.Settings.Default.MassFilter = MassBlinkReductionEnabled;
            MassBlinkCheckbox.IsChecked = MassBlinkReductionEnabled;

        }

        private void ButtonBlinkReductionEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
                ButtonBlinkReductionEnabled = !ButtonBlinkReductionEnabled;
            Properties.Settings.Default.ButtonFilter = ButtonBlinkReductionEnabled;
            ButtonBlinkCheckbox.IsChecked = ButtonBlinkReductionEnabled;
        }

        private void AnalogBlinkReductionEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
                AnalogBlinkReductionEnabled = !AnalogBlinkReductionEnabled;
            Properties.Settings.Default.AnalogFilter = AnalogBlinkReductionEnabled;
            AnalogBlinkCheckbox.IsChecked = AnalogBlinkReductionEnabled;
        }

        private void MassBlinkReductionEnabled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox)
                MassBlinkReductionEnabled = !MassBlinkReductionEnabled;
            Properties.Settings.Default.MassFilter = MassBlinkReductionEnabled;
            MassBlinkCheckbox.IsChecked = MassBlinkReductionEnabled;
        }

        public void CalculateMagic()
        {
            MagicHeight = Height - _originalHeight;
            MagicWidth = Width - _originalWidth;
        }

        public double MagicHeight { get; private set; }

        public double MagicWidth { get; private set; }

        public double AdjustedHeight => Height - MagicHeight;

        public double AdjustedWidth => Width - MagicWidth;

        public double Ratio => _originalWidth / _originalHeight;

        private static void AdjustImage(ElementConfig config, Image image, double xRatio, double yRatio)
        {
            uint newX = config.X = (uint)Math.Round(config.OriginalX * xRatio);
            uint newY = config.Y = (uint)Math.Round(config.OriginalY * yRatio);

            int newWidth = (int)Math.Round(config.Width * xRatio);
            int newHeight = (int)Math.Round(config.Height * yRatio);

            image.Margin = new Thickness(newX, newY, 0, 0);
            image.Width = newWidth;
            image.Height = newHeight;
        }

        private static void AdjustGrid(ElementConfig config, Grid grid, double xRatio, double yRatio)
        {
            uint newX = config.X = (uint)Math.Round(config.OriginalX * xRatio);
            uint newY = config.Y = (uint)Math.Round(config.OriginalY * yRatio);

            uint newWidth = config.Width = (uint)Math.Round(config.OriginalWidth * xRatio);
            uint newHeight = config.Height = (uint)Math.Round(config.OriginalHeight * yRatio);

            ((Image)grid.Children[0]).Width = newWidth;
            ((Image)grid.Children[0]).Height = newHeight;

            grid.Margin = new Thickness(newX, newY, 0, 0);
            grid.Width = newWidth;
            grid.Height = newHeight;
        }

        public void AdjustControllerElements()
        {
            ControllerGrid.Width = ((Image)ControllerGrid.Children[0]).Width = AdjustedWidth;
            ControllerGrid.Height = ((Image)ControllerGrid.Children[0]).Height = AdjustedHeight;
            double xRatio = AdjustedWidth / _originalWidth;
            double yRatio = AdjustedHeight / _originalHeight;

            foreach (Tuple<Detail, Image> detail in _detailsWithImages)
            {
                if (detail.Item1.Config != null)
                    AdjustImage(detail.Item1.Config, detail.Item2, xRatio, yRatio);
            }

            foreach (Tuple<AnalogTrigger, Grid> trigger in _triggersWithGridImages)
            {
                if (trigger.Item1.Config != null)
                    AdjustGrid(trigger.Item1.Config, trigger.Item2, xRatio, yRatio);
            }

            foreach (Tuple<Button, Image> button in _buttonsWithImages)
            {
                if (button.Item1.Config != null)
                    AdjustImage(button.Item1.Config, button.Item2, xRatio, yRatio);
            }

            foreach (Tuple<RangeButton, Image> button in _rangeButtonsWithImages)
            {
                if (button.Item1.Config != null)
                    AdjustImage(button.Item1.Config, button.Item2, xRatio, yRatio);
            }

            foreach (Tuple<AnalogStickTracker, Image> tracker in _trackersWithImages)
            {
                if (tracker.Item1.Config != null)
                {
                    tracker.Item2.IsVisible = false;
                    AdjustImage(tracker.Item1.Config, tracker.Item2, xRatio, yRatio);
                    tracker.Item1.TrackingWidth = (uint)(tracker.Item1.OriginalTrackingWidth * xRatio);
                    if (tracker.Item1.TrackingWidth == 0)
                        tracker.Item1.TrackingWidth = 1;
                    tracker.Item1.XRange = (uint)(tracker.Item1.OriginalXRange * xRatio);
                    tracker.Item1.YRange = (uint)(tracker.Item1.OriginalYRange * yRatio);
                }
            }

            foreach (Tuple<AnalogStick, Image> stick in _sticksWithImages)
            {
                if (stick.Item1.Config != null)
                {
                    AdjustImage(stick.Item1.Config, stick.Item2, xRatio, yRatio);
                    stick.Item1.XRange = (uint)(stick.Item1.OriginalXRange * xRatio);
                    stick.Item1.YRange = (uint)(stick.Item1.OriginalYRange * yRatio);
                }
            }

            foreach (Tuple<TouchPad, Image> touchpad in _touchPadWithImages)
            {
                if (touchpad.Item1.Config != null)
                {
                    AdjustImage(touchpad.Item1.Config, touchpad.Item2, xRatio, yRatio);
                    touchpad.Item1.XRange = (uint)(touchpad.Item1.OriginalXRange * xRatio);
                    touchpad.Item1.YRange = (uint)(touchpad.Item1.OriginalYRange * yRatio);
                }
            }

            foreach (Tuple<AnalogText, Label> analogText in _analogTextBoxes)
            {
                uint newX = analogText.Item1.X = (uint)Math.Round(analogText.Item1.OriginalX * xRatio);
                uint newY = analogText.Item1.Y = (uint)Math.Round(analogText.Item1.OriginalY * yRatio);
                analogText.Item2.Margin = new Thickness(newX, newY, 0, 0);
                if (analogText.Item2.Parent != null)
                    ((LayoutTransformControl)analogText.Item2.Parent).LayoutTransform = new ScaleTransform(xRatio, yRatio);
            }
        }

    }

    internal class WindowAspectRatio
    {
        private readonly double _ratio;
        private readonly ViewWindow _window;
        private bool _calculatedMagic;

        private WindowAspectRatio(ViewWindow window)
        {
            _calculatedMagic = false;
            _window = window;
            _ratio = window.Ratio;

            var text = window.GetObservable(TopLevel.ClientSizeProperty);
            text.Subscribe(value =>
            {
                DragHook();
            });
        }

        public static void Register(ViewWindow window)
        {
            _ = new WindowAspectRatio(window);
        }


        private void DragHook()
        {
            if (!_calculatedMagic)
            {
                _window.CalculateMagic();
                _calculatedMagic = true;
            }
            
            double magicWidth = _window.MagicWidth;
            double magicHeight = _window.MagicHeight;

            _window.Width = (int)(magicWidth + ((_window.Height - magicHeight) * _ratio));

            _window.AdjustControllerElements();
        }

    }
}

