using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageProcessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //TODO - change this path
            Bitmap bmp = (Bitmap)Image.FromFile(@"c:\users\rob\downloads\raw image.png");

            //benchmark
            Stopwatch sw = Stopwatch.StartNew();
            
            //instantiate a LockBitmap and load it with image
            var lockBitmap = new LockBitmap(bmp);
            
            //lockdown
            lockBitmap.LockBits();

            //original slow code sped up a wee bit
            //initialize 
            int top = lockBitmap.Height;
            int bottom = 0;
            int left = lockBitmap.Width;
            int right = 0;

            Parallel.For(0, lockBitmap.Width, x =>
            {
                for (int y = 0; y < lockBitmap.Height; y++) //non-parallel inner loop, the overhead isn't worth it
                {
                    //Trace.WriteLine($"X:{x} Y:{y}"); //<< this will slow things down considerably                    

                    //don't need to actually get the color components - too time consuming
                    //we only need to test if the Alpha channel portion of the array is non-zero
                    if (lockBitmap.HasAlpha(x, y))
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
            });


            //release lock
            lockBitmap.UnlockBits();

            //the final result
            var activeArea = new OverlayActiveArea(top, bottom, left, right);

            sw.Stop();

            //how did we do?
            var elapsedMilliseconds = sw.ElapsedMilliseconds;
            Console.WriteLine(string.Format("Elapsed time: {0} milliseconds", elapsedMilliseconds));

            Console.WriteLine(String.Format("\nOverlay coordinates (top, bottom, left, right): {0}, {1}, {2}, {3}", top, bottom, left, right));
            Console.WriteLine("\nHit any key to close window");
            Console.ReadKey();
        }
    }
}
