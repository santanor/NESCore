using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NESCore
{
    public class PPU
    {
        public delegate void FrameEvent(ref int[] frame);

        public FrameEvent OnNewFrame;
        
        private int[] backBufer;
        private Bitmap lastFrame;
        private Random random = new Random();

        private const int width = 256;
        private const int height = 240;

        public int FrameCount;
        public int CyclesThisFrame;
        public int ScanlineThisFrame;

        public PPU()
        {
            //256x240 resolution and 4 bytes per pixel to represent the color
            backBufer = new int[width * height];
        }

        public void RunCycles(int cpuCycles)
        {
            for (var i = 0; i < cpuCycles; i++)
            {
                Cycle();
            }
        }

        private void Cycle()
        {
            //Noise for now
            SetPixel(ScanlineThisFrame, CyclesThisFrame, (byte) random.Next(0,255),(byte) random.Next(0,255),(byte)random.Next(0,255));
            
            CyclesThisFrame++;
            if (CyclesThisFrame >= 341)
            {
                CyclesThisFrame = 0;
                ScanlineThisFrame++;
                if (ScanlineThisFrame >= 261)
                {
                    ScanlineThisFrame = -1;
                    FrameCount++;
                    OnNewFrame?.Invoke(ref backBufer);
                }
            }
        }

        private void SetPixel(int row, int col, byte r, byte g, byte b)
        {
            //Make sure we're painting in the screen
            if (row >= 0 && row < height && col >= 0 && col < width)
            {
                backBufer[width * row + col] = (255 << 24) + (r << 16) + (g << 8) + b;
            }
        }
    }
}