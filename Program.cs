using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace pngInspector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("pngInspector");
            Sub(args);
            Console.WriteLine();
            Console.WriteLine("Press key to exit");
            Console.ReadKey();
        }


        static private void Sub(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("usage: pngInspector <input image path>");
                return;
            }

            var sourcePath = args[0];

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("The source file you specified does not exist.");
                return;
            }

            var bitmap = new Bitmap(sourcePath);
            int bitmapWidth = bitmap.Width;
            int bitmapHeight = bitmap.Height;

            BitmapData bitmapData = bitmap.LockBits(
                Rectangle.FromLTRB(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            int bitmapStride = bitmapData.Stride;
            int bitmapComponents = GetComponentsNumber(bitmap.PixelFormat);

            var colorDictionary = new Dictionary<Color, int>();
            var indexDictionary = new Dictionary<byte, int>();
            int pixelCountInDic = 0;

            Console.WriteLine();
            Console.WriteLine(sourcePath);

            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                for (int y = 0; y <= bitmapHeight - 1; y++)
                {
                    for (int x = 0; x <= bitmapWidth - 1; x++)
                    {
                        byte index = Marshal.ReadByte(bitmapData.Scan0, (bitmapStride * y) + (bitmapComponents * x));
                        if (indexDictionary.ContainsKey(index))
                            indexDictionary[index] += 1;
                        else
                            indexDictionary[index] = 1;
                    }
                }

                foreach (var key in indexDictionary.Keys)
                {
                    pixelCountInDic += indexDictionary[key];
                }

                Console.WriteLine(indexDictionary.Keys.Count + " keys in indexDictionary");
            }
            else
            {
                for (int y = 0; y < bitmapHeight; y++)
                {
                    for (int x = 0; x < bitmapWidth; x++)
                    {
                        Color color = Color.FromArgb(
                            Marshal.ReadInt32(bitmapData.Scan0, (bitmapStride * y) + (bitmapComponents * x)));
                        if (colorDictionary.ContainsKey(color))
                            colorDictionary[color] += 1;
                        else
                            colorDictionary[color] = 1;
                    }
                }

                foreach (var key in colorDictionary.Keys)
                {
                    pixelCountInDic += colorDictionary[key];
                }

                Console.WriteLine(colorDictionary.Keys.Count + " keys in colorDictionary");
            }

            bitmap.UnlockBits(bitmapData);

            Console.WriteLine(pixelCountInDic + " pixels in dictionary");
            Console.WriteLine(bitmapWidth * bitmapHeight + " pixels in png");

            ColorPalette palette = bitmap.Palette;
            Console.WriteLine(palette.Entries.Length + " colors in bitmap.Palette");
            
        }


        static private int GetComponentsNumber(PixelFormat pixelFormat)
        {
            switch(pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return 1;

                case PixelFormat.Format24bppRgb:
                    return 3;

                case PixelFormat.Format32bppArgb:
                    return 4;

                default:
                    Debug.Assert(false);
                    return 0;
            }
        }
    }
}
