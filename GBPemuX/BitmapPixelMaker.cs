using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using Image = SixLabors.ImageSharp.Image;

namespace GBPemu
{
    public struct Pixel : IEquatable<Pixel>
    {
        public Pixel(byte? red, byte? green, byte? blue, byte? alpha)
        {
            Red = red ?? 0;
            Green = green ?? 0;
            Blue = blue ?? 0;
            Alpha = alpha ?? 0;
        }

        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Alpha { get; set; }

        public override bool Equals(object? obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Pixel p = (Pixel)obj;
                return (Red == p.Red) && (Green == p.Green) && (Blue == p.Blue) && (Alpha == p.Alpha);
            }
        }

        public override int GetHashCode()
        {
            return (Red.GetHashCode() * 17) + (Green.GetHashCode() * 17) + (Blue.GetHashCode() * 17) + (Alpha.GetHashCode() * 17);
        }

        public static bool operator ==(Pixel left, Pixel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pixel left, Pixel right)
        {
            return !(left == right);
        }

        public bool Equals(Pixel other)
        {
            return (Red == other.Red) && (Green == other.Green) && (Blue == other.Blue) && (Alpha == other.Alpha);
        }
    }

    public class BitmapPixelMaker
    {
        // The raw pixels.
        private Image<Bgra32> Pixels;
        public BitmapPixelMaker(int width, int height)
        {
            Pixels = new Image<Bgra32>(width, height);
        }

        // Get a pixel's value.
        public Pixel GetPixel(int x, int y)
        {
            Bgra32 pixel = Pixels[x, y];

            Pixel retVal = new()
            {
                Blue = pixel.B,
                Green = pixel.G,
                Red = pixel.R,
                Alpha = pixel.A
            };

            return retVal;
        }

        public byte GetBlue(int x, int y)
        {
            return Pixels[x, y].B;
        }
        public byte GetGreen(int x, int y)
        {
            return Pixels[x, y].G;
        }
        public byte GetRed(int x, int y)
        {
            return Pixels[x, y].R;
        }
        public byte GetAlpha(int x, int y)
        {
            return Pixels[x, y].A;
        }

        // Set a pixel's value.
        public void SetPixel(int x, int y, Pixel color)
        {
            Pixels[x, y] = new Bgra32(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public void SetBlue(int x, int y, byte blue)
        {
            Bgra32 pixel = Pixels[x, y];
            Pixels[x, y] = new Bgra32(pixel.R, pixel.G, blue, pixel.A);
        }
        public void SetGreen(int x, int y, byte green)
        {
            Bgra32 pixel = Pixels[x, y];
            Pixels[x, y] = new Bgra32(pixel.R, green, pixel.B, pixel.A);
        }
        public void SetRed(int x, int y, byte red)
        {
            Bgra32 pixel = Pixels[x, y];
            Pixels[x, y] = new Bgra32(red, pixel.G, pixel.B, pixel.A);
        }
        public void SetAlpha(int x, int y, byte alpha)
        {
            Bgra32 pixel = Pixels[x, y];
            Pixels[x, y] = new Bgra32(pixel.R, pixel.G, pixel.B, alpha);
        }

        public void SetRect(int x1, int y1, int width, int height, byte? red, byte? green, byte? blue)
        {
            for (int i = x1; i < width + x1; ++i)
            {
                for (int j = y1; j < height + y1; ++j)
                {
                    SetAlpha(i, j, 255);
                    SetRed(i, j, red ?? 0);
                    SetGreen(i, j, green ?? 0);
                    SetBlue(i, j, blue ?? 0);

                }
            }
        }

        // Set all pixels to a specific color.
        public void SetColor(byte red, byte green, byte blue, byte alpha)
        {
            for (int i = 0; i < Pixels.Width; ++i)
            {
                for (int j = 0; j < Pixels.Height; ++j)
                {
                    Bgra32.FromAbgr32(new Abgr32(blue, green, red, alpha));

                }
            }
        }

        // Set all pixels to a specific opaque color.
        public void SetColor(byte red, byte green, byte blue)
        {
            SetColor(red, green, blue, 255);
        }

        public struct GBImage
        {
            public Image _rawImage;
            public Bitmap _bitmap;
        }

        public void ReplaceColor(Pixel oldColor, Pixel newColor)
        {
            for (int i = 0; i < Pixels.Width; ++i)
            {
                for (int j = 0; j < Pixels.Height; ++j)
                {
                    Pixel pixel = GetPixel(i, j);
                    if (pixel == oldColor)
                    {
                        SetPixel(i, j, newColor);
                    }
                }
            }
        }

        public void SetRawImage(string path)
        {

            Pixels = SixLabors.ImageSharp.Image.Load<Bgra32>(path);
        }

        // Use the pixel data to create a WriteableBitmap.
        public GBImage MakeBitmap()
        {
            GBImage retVal = new()
            {
                _rawImage = Pixels
            };

            using (MemoryStream ms = new())
            {
                retVal._rawImage.Save(ms, BmpFormat.Instance);
                ms.Position = 0;
                retVal._bitmap = new Bitmap(ms);
            }

            return retVal;
        }

    }

}
