using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBPemu
{
    /// <summary>
    /// Interaction logic for GameBoyPrinterEmulatorWindow.xaml
    /// </summary>
    public partial class GameBoyPrinterEmulatorWindow : Window
    {
        private readonly int TILE_PIXEL_WIDTH = 8;
        private readonly int TILE_PIXEL_HEIGHT = 8;
        private readonly int TILES_PER_LINE = 20; // Gameboy Printer Tile Constant

        private readonly byte[][][] palettes = {
                                                new byte[][] {
                                                    new byte[] { 0xff, 0xaa, 0x55, 0x00 },
                                                    new byte[] { 0xff, 0xaa, 0x55, 0x00 },
                                                    new byte[]  { 0xff, 0xaa, 0x55, 0x00 }
                                                },   // Grayscale
                                                new byte[][] {
                                                    new byte[] { 0x9b, 0x77, 0x30, 0x0f },
                                                    new byte[] { 0xbc, 0xa1, 0x62, 0x38 },
                                                    new byte[] { 0x0f, 0x12, 0x30, 0x0f }
                                                },   // DMG
                                                new byte[][] {
                                                    new byte[] { 0xc4, 0x8b, 0x4d, 0x1f },
                                                    new byte[] { 0xcf, 0x95, 0x53, 0x1f },
                                                    new byte[] { 0xa1, 0x6d, 0x3c, 0x1f }
                                                },   // GameBoy Pocket
                                                new byte[][] {
                                                    new byte[] { 0xff, 0x7b, 0x01, 0x00 },
                                                    new byte[] { 0xff, 0xff, 0x63, 0x00 },
                                                    new byte[] { 0xff, 0x30, 0xc6, 0x00 }
                                                },   // GameBoy Color EU/US
                                                new byte[][] {
                                                    new byte[] { 0xff, 0xff, 0x83, 0x00 },
                                                    new byte[] { 0xff, 0xad, 0x31, 0x00 },
                                                    new byte[] { 0xff, 0x63, 0x00, 0x00 }
                                                },   // GameBoy Color JP
                                                new byte[][] {
                                                    new byte[] { 0xe0, 0x88, 0x34, 0x08 },
                                                    new byte[] { 0xf8, 0xc0, 0x68, 0x18 },
                                                    new byte[] { 0xd0, 0x70, 0x56, 0x20 }
                                                },   // BGB
                                                new byte[][] {
                                                    new byte[] { 0xe0, 0xa8, 0x70, 0x2b },
                                                    new byte[] { 0xdb, 0x9f, 0x6b, 0x2b },
                                                    new byte[] { 0xcd, 0x94, 0x66, 0x26 }
                                                },   // GraphixKid Gray
                                                new byte[][] {
                                                    new byte[] { 0x7e, 0xab, 0x7b, 0x4c },
                                                    new byte[] { 0x84, 0xc3, 0x92, 0x62 },
                                                    new byte[] { 0xb4, 0x96, 0x78, 0x5a }
                                                },   // GraphixKid Green
                                                new byte[][] {
                                                    new byte[] { 0x7e, 0x57, 0x38, 0x2e },
                                                    new byte[] { 0x84, 0x7b, 0x5d, 0x46 },
                                                    new byte[] { 0x16, 0x46, 0x49, 0x3d }
                                                }    // Black Zero
        };

        private readonly System.Windows.Controls.Image _image;
        private BitmapPixelMaker _imageBuffer;
        private List<byte[]> decompressedTiles;
        private readonly IControllerReader _reader;
        int tile_height_count;
        private int PrintSize;

        private class GamePalette
        {
            public GamePalette(string name, byte[][] colors)
            {
                Name = name;
                Colors = colors;
            }

            public string Name;
            public byte[][] Colors;
        };

        private struct Game
        {
            public Game(string name)
            {
                Name = name;
                Palettes = new List<GamePalette>();
            }

            public string Name;
            public List<GamePalette> Palettes;
        }

        private List<Game> games;

        private void ParseGamePalettes()
        {
            bool getMaxRGBValue = false;
            games = new List<Game>();
            int currentGame = 0;
            byte maxRGBValue = 255;
            List<GamePalette> newPalettes = new List<GamePalette>();
            bool lookingForGame = true;

            foreach (string line in System.IO.File.ReadLines(@"game_palettes.cfg"))
            {
                if (lookingForGame && line.StartsWith("Game:", System.StringComparison.Ordinal))
                {
                    var gameName = line.Split(':')[1];
                    Game g = new Game(gameName);
                    games.Add(g);
                    getMaxRGBValue = true;
                    lookingForGame = false;
                    continue;
                }

                if (lookingForGame)
                    break;

                if (lookingForGame == false && line.StartsWith("EndGame", System.StringComparison.Ordinal))
                {
                    currentGame++;
                    lookingForGame = true;
                    continue;
                }

                if (getMaxRGBValue)
                {
                    maxRGBValue = byte.Parse(line, CultureInfo.CurrentCulture);
                    getMaxRGBValue = false;
                    continue;
                }

                byte[][] colorValues = new byte[3][];
                colorValues[0] = new byte[4];
                colorValues[1] = new byte[4];
                colorValues[2] = new byte[4];

                var colors = line.Split(',');
                var paletteName = colors[0];
                for (int i = 1; i < 5; ++i)
                {
                    var comps = colors[i].Split(' ');
                    colorValues[0][i-1] = (byte)(((byte.Parse(comps[0], CultureInfo.CurrentCulture) - 0.0) / (maxRGBValue - 0.0)) * (255.0 - 0.0) + 0.0);
                    colorValues[1][i-1] = (byte)(((byte.Parse(comps[1], CultureInfo.CurrentCulture) - 0.0) / (maxRGBValue - 0.0)) * (255.0 - 0.0) + 0.0);
                    colorValues[2][i-1] = (byte)(((byte.Parse(comps[2], CultureInfo.CurrentCulture) - 0.0) / (maxRGBValue - 0.0)) * (255.0 - 0.0) + 0.0);
                }

                games[currentGame].Palettes.Add(new GamePalette(paletteName, colorValues));
            }


            for (int i = 0; i < currentGame; ++i)
            {
                var gameMenu = new System.Windows.Controls.MenuItem
                {
                    Header = games[i].Name
                };

                for (int j = 0; j < games[i].Palettes.Count; ++j)
                {
                    var paletteMenu = new System.Windows.Controls.MenuItem
                    {
                        Header = games[i].Palettes[j].Name,
                        IsCheckable = true
                    };
                    paletteMenu.Click += Game_Palette_Click;
                    gameMenu.Items.Add(paletteMenu);
                }

                Palette_Games.Items.Add(gameMenu);
            }
           
        }

        void ClearGamePalette(System.Windows.Controls.MenuItem menuItem)
        {
            foreach(System.Windows.Controls.MenuItem game in Palette_Games.Items)
            {
                foreach(System.Windows.Controls.MenuItem palette in game.Items)
                {
                    if (palette == menuItem)
                        palette.IsChecked = true;
                    else
                        palette.IsChecked = false;
                }
            }
        }

        private void Game_Palette_Click(object sender, EventArgs e)
        {
            var menuItem  = (System.Windows.Controls.MenuItem)sender;

            //Clear Checks
            CheckPalette(9);
            ClearGamePalette(menuItem);

            GamePalette newPalette = null;

            string gameName = (string)(((System.Windows.Controls.MenuItem)menuItem.Parent).Header);
            foreach(Game game in games)
            {
                if (gameName == game.Name)
                {
                    foreach(GamePalette palette in game.Palettes)
                    {
                        if (palette.Name == (string)menuItem.Header)
                        {
                            newPalette = palette;
                        }
                    }    
                }
            }

            SelectedPalette = -1;
            SelectedGamePalette = newPalette;
            
            DisplayImage(PrintSize, PrintSize);
        }

        private void Size_Click(object sender, EventArgs e)
        {
            if (sender == Size_1x)
            {
                PrintSize = 1;
            }
            else if (sender == Size_2x)
            {
                PrintSize = 2;
            }
            else if(sender == Size_3x)
            {
                PrintSize = 3;
            }
            else if (sender == Size_4x)
            {
                PrintSize = 4;
            }
            else if (sender == Size_5x)
            {
                PrintSize = 5;
            }
            else if (sender == Size_6x)
            {
                PrintSize = 6;
            }
            else if (sender == Size_7x)
            {
                PrintSize = 7;
            }
            else if (sender == Size_8x)
            {
                PrintSize = 8;
            }

            CheckSize(PrintSize);

            Properties.Settings.Default.PrintSize = PrintSize;
        }

        private void CheckSize(int sizeId)
        {
            Size_1x.IsChecked = sizeId == 1;
            Size_2x.IsChecked = sizeId == 2;
            Size_3x.IsChecked = sizeId == 3;
            Size_4x.IsChecked = sizeId == 4;
            Size_5x.IsChecked = sizeId == 5;
            Size_6x.IsChecked = sizeId == 6;
            Size_7x.IsChecked = sizeId == 7;
            Size_8x.IsChecked = sizeId == 8;
        }

        private int SelectedPalette;
        private GamePalette SelectedGamePalette;

        private void CheckPalette(int paletteId)
        {
            Palette_Grayscale.IsChecked = paletteId == 0;
            Palette_DMG.IsChecked = paletteId == 1;
            Palette_GBPocket.IsChecked = paletteId == 2;
            Palette_GBCUS.IsChecked = paletteId == 3;
            Palette_GBCJP.IsChecked = paletteId == 4;
            Palette_BGB.IsChecked = paletteId == 5;
            Palette_GKGray.IsChecked = paletteId == 6;
            Palette_GKGreen.IsChecked = paletteId == 7;
            Palette_BZ.IsChecked = paletteId == 8;
        }

        private void Palette_Click(object sender, EventArgs e)
        {
            int newPalette = 0;

            if (sender == Palette_DMG)
            {
                newPalette = 1;
            }
            else if (sender == Palette_GBPocket)
            {
                newPalette = 2;
            }
            else if (sender == Palette_GBCUS)
            {
                newPalette = 3;
            }
            else if (sender == Palette_GBCJP)
            {
                newPalette = 4;
            }
            else if (sender == Palette_BGB)
            {
                newPalette = 5;
            }
            else if (sender == Palette_GKGray)
            {
                newPalette = 6;
            }
            else if (sender == Palette_GKGreen)
            {
                newPalette = 7;
            }
            else if (sender == Palette_BZ)
            {
                newPalette = 8;
            }

            CheckPalette(newPalette);
            ClearGamePalette(null);

            SelectedPalette = newPalette;
            Properties.Settings.Default.SelectedPalette = SelectedPalette;
            
            DisplayImage(PrintSize, PrintSize);
 

        }

        private void DisplayError()
        {
            SelectedPalette = Properties.Settings.Default.SelectedPalette;
            PrintSize = Properties.Settings.Default.PrintSize;

            using (Bitmap bmp = new Bitmap(Properties.Resources.ErrorImage))
            {
                _imageBuffer = new BitmapPixelMaker(480, 432);

                _imageBuffer.SetColor(palettes[SelectedPalette == -1 ? 0 : SelectedPalette][0][3], palettes[SelectedPalette == -1 ? 0 : SelectedPalette][1][3], palettes[SelectedPalette == -1 ? 0 : SelectedPalette][2][3]);

                for (int i = 0; i < bmp.Width; ++i)
                {
                    for (int j = 0; j < bmp.Height; ++j)
                    {
                        System.Drawing.Color pixel = bmp.GetPixel(i, j);
                        if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255)
                        {
                            _imageBuffer.SetRed(i, j, palettes[SelectedPalette == -1 ? 0 : SelectedPalette][0][0]);
                            _imageBuffer.SetGreen(i, j, palettes[SelectedPalette == -1 ? 0 : SelectedPalette][1][0]);
                            _imageBuffer.SetBlue(i, j, palettes[SelectedPalette == -1 ? 0 : SelectedPalette][2][0]);
                        }
                    }
                }


                // imageBuffer.SetColor(0, 0, 0);
                // Convert the pixel data into a WriteableBitmap.
                WriteableBitmap wbitmap = _imageBuffer.MakeBitmap(96, 96);

                // Set the Image source.
                _image.Source = wbitmap;

                GameBoyPrinterEmulatorWindowGrid.Height = 432;
                GameBoyPrinterEmulatorWindowGrid.Width = 480;
                Height = 432;
                Width = 480;
            }
        }

        public GameBoyPrinterEmulatorWindow(IControllerReader reader)
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            InitializeComponent();
            DataContext = this;

            ParseGamePalettes();

            _reader = reader ?? throw new ArgumentNullException(nameof(reader));

            SelectedPalette = Properties.Settings.Default.SelectedPalette;
            if (SelectedPalette == -1)  // This shouldn't happen, but it probably can in certain corner cases
                SelectedPalette = 1;

            PrintSize = Properties.Settings.Default.PrintSize;

            using (Bitmap bmp = new Bitmap(Properties.Resources.PrintImage))
            {
                _imageBuffer = new BitmapPixelMaker(480, 432);

                _imageBuffer.SetColor(palettes[SelectedPalette][0][3], palettes[SelectedPalette][1][3], palettes[SelectedPalette][2][3]);

                for (int i = 0; i < bmp.Width; ++i)
                {
                    for (int j = 0; j < bmp.Height; ++j)
                    {
                        System.Drawing.Color pixel = bmp.GetPixel(i, j);
                        if (pixel.R == 255 && pixel.G == 255 && pixel.B == 255)
                        {
                            _imageBuffer.SetRed(i, j, palettes[SelectedPalette][0][0]);
                            _imageBuffer.SetGreen(i, j, palettes[SelectedPalette][1][0]);
                            _imageBuffer.SetBlue(i, j, palettes[SelectedPalette][2][0]);
                        }
                    }
                }

                WriteableBitmap wbitmap = _imageBuffer.MakeBitmap(96, 96);

                // Create an Image to display the bitmap.
                _image = new System.Windows.Controls.Image
                {
                    Stretch = Stretch.None,
                    Margin = new Thickness(0)
                };

                _ = GameBoyPrinterEmulatorWindowGrid.Children.Add(_image);
                _image.Source = wbitmap;

                _reader.ControllerStateChanged += Reader_ControllerStateChanged;
                _reader.ControllerDisconnected += Reader_ControllerDisconnected;

                CheckPalette(SelectedPalette);
                CheckSize(PrintSize);

            }
        }

        private void Reader_ControllerDisconnected(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                Hide();
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Hide();
                });
            }
        }

        bool rawPacketParse = false;

        private void Reader_ControllerStateChanged(object reader, ControllerStateEventArgs e)
        {

            if (e?.RawPrinterData?.StartsWith("// GAMEBOY PRINTER Packet Capture V3.2.1 (Copyright (C) 2022 Brian Khuu)") == true
                || e?.RawPrinterData?.StartsWith("88 33 ") == true)
            {
                rawPacketParse = true;
            }

            string processedString;
            if (rawPacketParse)
                processedString = ProcessRawBuffer(e?.RawPrinterData);
            else
                processedString = e?.RawPrinterData ?? string.Empty;

            _imageBuffer.SetColor(0, 0, 0, 255);

            int square_width = PrintSize;// 480 / (TILE_PIXEL_WIDTH * TILES_PER_LINE);
            int square_height = square_width;

            string[] tiles_rawBytes_array = processedString.Split('\n');

            decompressedTiles = new List<byte[]>();

            int total_tile_count = Decompress(tiles_rawBytes_array, decompressedTiles);

            tile_height_count = total_tile_count / TILES_PER_LINE;

            if (tile_height_count == 0)
            {
                DisplayError();
                return;
            }

            _imageBuffer = new BitmapPixelMaker(square_width * TILE_PIXEL_WIDTH * TILES_PER_LINE, square_height * TILE_PIXEL_HEIGHT * tile_height_count);

            _image.Height = square_height * TILE_PIXEL_HEIGHT * tile_height_count;
            _image.Width = square_width * TILE_PIXEL_WIDTH * TILES_PER_LINE;
            GameBoyPrinterEmulatorWindowGrid.Height = square_height * TILE_PIXEL_HEIGHT * tile_height_count; ;
            GameBoyPrinterEmulatorWindowGrid.Width = square_width * TILE_PIXEL_WIDTH * TILES_PER_LINE;
            Height = square_height * TILE_PIXEL_HEIGHT * tile_height_count;
            Width = square_width * TILE_PIXEL_WIDTH * TILES_PER_LINE;

            DisplayImage(square_width, square_height);
        }

        private static string ProcessRawBuffer(string rawPrinterData)
        {
            if (rawPrinterData == null)
                return string.Empty;

            string retVal = string.Empty;

            string[] lines = rawPrinterData.Split('\n');
            foreach (var line in lines)
            {
                if (line.Length < 5 || !(line[0] == '8' && line[1] == '8' && line[2] == ' ' && line[3] == '3' && line[4] == '3'))
                    continue;

                if (line[6] == '0' && line[7] == '4')  // We have a data line
                {
                    if (line[10] == '1') // We have compression
                    {
                        retVal += "{ \"command\":\"DATA\", \"compressed\":1, \"more\":1}\n";
                    }

                    char[] packet_length = new char[5];
                    packet_length[2] = line[12];
                    packet_length[3] = line[13];
                    packet_length[0] = line[15];
                    packet_length[1] = line[16];
                    packet_length[4] = (char)0;
                    string packet_length_str = string.Join("", packet_length);
                    int length = Int32.Parse(packet_length_str, System.Globalization.NumberStyles.HexNumber);
                    if (length == 0)
                        continue;
                    string packet = line.Substring(18, length * 3 - 1);
                    string[] bytes = packet.Split(' ');

                    int current_position = 0;
                    string newline = string.Empty;
                    while (current_position != bytes.Length)
                    {
                        if (current_position + 16 < bytes.Length)
                        {
                            newline = string.Empty;
                            for (int i = 0; i < 16; ++i)
                            {
                                newline += bytes[current_position++];
                                if (i != 16)
                                    newline += " ";
                            }

                        }
                        else
                        {
                            newline = string.Empty;
                            int bytesLeft = length - current_position;

                            for (int i = 0; i < bytesLeft; ++i)
                            {
                                newline += bytes[current_position++];
                                if (i != length - current_position)
                                    newline += " ";
                            }
                        }
                        newline += "\n";
                        retVal += newline;
                    }
                }
            }
            return retVal;
        }

        private void DisplayImage(int square_width, int square_height)
        {
            for (int i = 0; i < decompressedTiles.Count; i++)
            {
                int tile_x_offset = i % TILES_PER_LINE;
                int tile_y_offset = i / TILES_PER_LINE;

                byte[] pixels = Decode(decompressedTiles[i]);

                Paint(_imageBuffer, pixels, square_width, square_height, tile_x_offset, tile_y_offset);
            }

            //imageBuffer.SetColor(0, 0, 0);
            // Convert the pixel data into a WriteableBitmap.
            WriteableBitmap wbitmap = _imageBuffer.MakeBitmap(96, 96);

            // Set the Image source.
            _image.Source = wbitmap;
        }

        private class Tile
        {
            public byte[] tile_bytes = new byte[16];
            private int tile_bytes_idx;

            public bool Add(byte b)
            {
                tile_bytes[tile_bytes_idx++] = b;
                return tile_bytes_idx == 16;
            }
        }

        private class DecompressState
        {
            public DecompressState(bool _isCompressed)
            {

                loopRunLength = 0;
                compressedRun = false;
                repeatByteGet = false;
                buffIndex = 0;
                isCompressed = _isCompressed;
            }

            public int loopRunLength;
            public bool compressedRun;
            public bool repeatByteGet;
            public int buffIndex;
            public byte repeatByte;
            public bool isCompressed;
        }

        private static int Decompress(string[] tiles_rawBytes_array, List<byte[]> decompressedTiles)
        {
            List<byte[]> compressedBytes = new List<byte[]>();
            bool isCompressed = false;

            for (int tile_i = 0; tile_i < tiles_rawBytes_array.Length; tile_i++)
            {
                string tile_element = tiles_rawBytes_array[tile_i];

                // Check for invalid raw lines
                if (tile_element.Length == 0)
                {   // Skip lines with no bytes (can happen with .split() )
                    continue;
                }
                else if (tile_element.StartsWith("!", StringComparison.Ordinal))
                {   // Skip lines used for comments
                    continue;
                }
                else if (tile_element.StartsWith("#", StringComparison.Ordinal))
                {   // Skip lines used for comments
                    continue;
                }
                else if (tile_element.StartsWith("//", StringComparison.Ordinal))
                {   // Skip lines used for comments
                    continue;
                }
                else if (tile_element.StartsWith("{", StringComparison.Ordinal))
                {   // Skip lines used for comments
                    if (tile_element.Contains("\"compressed\":1"))
                    {
                        isCompressed = true;
                    }
                    continue;
                }

                string bytes = tile_element.Replace(" ", "").Replace("\r", "");

                byte[] byteArray = new byte[bytes.Length/2];
                for (int i = 0; i < byteArray.Length; i++)
                {
                    byteArray[i] = byte.Parse(bytes.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.CurrentCulture);
                }

                compressedBytes.Add(byteArray);
            }

            int tile_count = 0;

            DecompressState state = new DecompressState(isCompressed);
            Tile t = new Tile();
            for (int i = 0; i < compressedBytes.Count; i++)
            {
                bool done;
                do
                {
                    done = !ProcessBuffer(state, compressedBytes[i], t);
                    if (!done) // Filled a tile, so need a new one
                    {
                        decompressedTiles.Add(t.tile_bytes);
                        tile_count++;
                        t = new Tile();
                    }

                } while (!done);

            }

            return tile_count;
        }

        private static bool ProcessBuffer(DecompressState state, byte[] buffer, Tile tile)
        {
            if (!state.isCompressed)
            {
                while (true)
                {
                    if (state.buffIndex < buffer.Length)
                    {
                        if(tile.Add(buffer[state.buffIndex++]))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        state.buffIndex = 0;
                        return false;
                    }
                }
            }
            else
            {
                while (true)
                {
                    if ((state.buffIndex < buffer.Length) || (state.compressedRun && !state.repeatByteGet && (state.loopRunLength != 0)))
                    {
                        if (state.loopRunLength == 0)
                        {
                            byte b = buffer[state.buffIndex++];
                            if (b < 128)
                            {
                                state.loopRunLength = b + 1;
                                state.compressedRun = false;
                            }
                            else if (b >= 128)
                            {
                                state.loopRunLength = b - 128 + 2;
                                state.compressedRun = true;
                                state.repeatByteGet = true;
                            }
                        }
                        else if (state.repeatByteGet)
                        {
                            state.repeatByte = buffer[state.buffIndex++];
                            state.repeatByteGet = false;
                        }
                        else
                        {
                            byte b = (state.compressedRun) ? state.repeatByte : buffer[state.buffIndex++];
                            state.loopRunLength--;

                            if (tile.Add(b))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        state.buffIndex = 0;
                        return false;
                    }
                }
            }
        }

        private void Paint(BitmapPixelMaker canvas, byte[] pixels, int pixel_width, int pixel_height, int tile_x_offset, int tile_y_offset)
        {   // This paints the tile with a specified offset and pixel width

            int pixel_x_offset = TILE_PIXEL_WIDTH * tile_x_offset * pixel_width;
            int pixel_y_offset = TILE_PIXEL_HEIGHT * tile_y_offset * pixel_height;


            for (int i = 0; i < TILE_PIXEL_WIDTH; i++)
            {   // pixels along the tile's x axis
                for (int j = 0; j < TILE_PIXEL_HEIGHT; j++)
                {   // pixels along the tile's y axis

                    if (SelectedPalette != -1)
                    {
                        canvas.SetRect(pixel_x_offset + (i * pixel_width),
                                pixel_y_offset + (j * pixel_height),
                                pixel_width,
                                pixel_height,
                                palettes[SelectedPalette][0][pixels[(j * TILE_PIXEL_WIDTH) + i]],
                                palettes[SelectedPalette][1][pixels[(j * TILE_PIXEL_WIDTH) + i]],
                                palettes[SelectedPalette][2][pixels[(j * TILE_PIXEL_WIDTH) + i]]);
                    }
                    else
                    {
                        canvas.SetRect(pixel_x_offset + (i * pixel_width),
                                pixel_y_offset + (j * pixel_height),
                                pixel_width,
                                pixel_height,
                                SelectedGamePalette.Colors[0][pixels[(j * TILE_PIXEL_WIDTH) + i]],
                                SelectedGamePalette.Colors[1][pixels[(j * TILE_PIXEL_WIDTH) + i]],
                                SelectedGamePalette.Colors[2][pixels[(j * TILE_PIXEL_WIDTH) + i]]);
                    }
                }
            }
        }

        private byte[] Decode(byte[] byteArray)
        {
            byte[] pixels = new byte[TILE_PIXEL_WIDTH * TILE_PIXEL_HEIGHT];
            for (int j = 0; j < TILE_PIXEL_HEIGHT; j++)
            {
                for (int i = 0; i < TILE_PIXEL_WIDTH; i++)
                {
                    byte hiBit = (byte)((byteArray[(j * 2) + 1] >> (7 - i)) & 1);
                    byte loBit = (byte)((byteArray[j * 2] >> (7 - i)) & 1);
                    pixels[(j * TILE_PIXEL_WIDTH) + i] = (byte)((hiBit << 1) | loBit);
                }
            }

            return pixels;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            _reader.Finish();
            Environment.Exit(0);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                Filter = Properties.Resources.ResourceManager.GetString("PNGFilter", CultureInfo.CurrentUICulture)
            };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                SaveUsingEncoder(_image, saveFileDialog.FileName, encoder);
            }
            saveFileDialog.Dispose();
        }

        private static void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (FileStream stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

    }
}
