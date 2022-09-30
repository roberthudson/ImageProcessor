using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ImageProcessor
{
    public class LockBitmap
    {
        IntPtr pointer = IntPtr.Zero;
        Bitmap bmp = null;
        BitmapData bitmapData = null;

        public byte[] Pixels { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }
        public LockBitmap(Bitmap source)
        {
            bmp = source;
        }

        public unsafe void LockBits()
        {
            try
            {
                //dimensions
                Width = bmp.Width;
                Height = bmp.Height;

                var rect = new Rectangle(0, 0, Width, Height);

                int totalPixels = Width * Height;
                
                // get source bitmap pixel format size
                Depth = Image.GetPixelFormatSize(bmp.PixelFormat);

                //lock bits
                bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                var stride = bitmapData.Stride;

                var formatSize = Image.GetPixelFormatSize(bmp.PixelFormat);

                // create byte array to copy pixel values
                int step = formatSize / 8;
                Pixels = new byte[totalPixels * step];

                //wait for it...
                pointer = bitmapData.Scan0;

                //the secret sauce!
                Marshal.Copy(pointer, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void UnlockBits()
        {
            try
            {
                //reverse procedure
                Marshal.Copy(Pixels, 0, pointer, Pixels.Length);

                //unlock
                bmp.UnlockBits(bitmapData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                    bmp.Dispose();
            }
        }

        public bool HasAlpha(int x, int y)
        {
            Color color = Color.Empty;

            //# of color components
            int count = 4; //assume this is a 32-bit image

            //start index
            int i = ((y * Width) + x) * count;

            if (i > Pixels.Length - count)
            {
                throw new IndexOutOfRangeException();
            }

            if (Depth == 32) //RGBA
            {
                return Pixels[i + 3] > 0;
            }
            return false;
        }
   
        public Color GetPixel(int x, int y)
        {
            Color color = Color.Empty;

            //# of color components
            int count = Depth / 8;

            //start index
            int i = ((y * Width) + x) * count;

            if (i > Pixels.Length - count)
            {
                throw new IndexOutOfRangeException();
            }

            if (Depth == 32) //RGBA
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3];
                color = Color.FromArgb(a, r, g, b);
            }
            if (Depth == 24) //RGB
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                color = Color.FromArgb(r, g, b);
            }
            if (Depth == 8) //Monochrome
            {
                byte c = Pixels[i];
                color = Color.FromArgb(c, c, c);
            }
            return color;
        }
    }
}