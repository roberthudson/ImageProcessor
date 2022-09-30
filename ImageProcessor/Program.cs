using System.Diagnostics;
using System.Drawing;

namespace ImageProcessor
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            //TODO - change this path
            Bitmap bmp = (Bitmap)Image.FromFile(@"c:\users\rob\downloads\Raw Image.png");

            //benchmark
            Stopwatch sw = Stopwatch.StartNew();
            
            //instantiate a LockBitmap and load it with image
            var lockBitmap = new LockBitmap(bmp);
            
            //lockdown
            lockBitmap.LockBits();

            //slow code sped up a "wee bit"
            //initialize 
            int top = lockBitmap.Height;
            int bottom = 0;
            int left = lockBitmap.Width;
            int right = 0;
            for (int x = 0; x < lockBitmap.Width; x++)
            {
                for (int y = 0; y < lockBitmap.Height; y++)
                {
                    //Trace.WriteLine($"X:{x} Y:{y}"); //<< this will slow things down considerably
                    
                    //don't need to actually get the color components - too time consuming
                    //we only need to test if the Alpha channel in the array is non-zero
                    //var c = lockBitmap.GetPixel(x, y);
                    bool hasAlpha = lockBitmap.HasAlpha(x, y);
                    if (hasAlpha) // (c.A != 0)
                    {
                        if (y < top)
                            top = y;
                        if (x < left)
                            left = x;
                        if (y > bottom)
                            bottom = y;
                        if (x > right)
                            right = x;
                    }
                }
            }

            //the final result
            var activeArea = new OverlayActiveArea(top, bottom, left, right);

            //release lock
            lockBitmap.UnlockBits();
          
            sw.Stop();

            //how did we do?
            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            Debug.WriteLine(string.Format("Elapsed time: {0}", elapsedMilliseconds));
        }
    }
}
