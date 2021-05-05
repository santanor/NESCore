using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace NESCore
{
    public unsafe struct PPUBufferData
    {
        public fixed int backBuffer[256 * 240];
        public IntPtr backBufferPtr { get; set; }
    }
    public unsafe class PPU
    {
        private PPUBufferData buffer = new PPUBufferData();
        public PPUBufferData Buffer
        {
            get => buffer;
            set => buffer = value;
        }
        
        private Random random = new Random();

        private const int width = 256;
        private const int height = 240;

        public int FrameCount;
        public int CyclesThisFrame;
        public int ScanlineThisFrame;

        public PPU()
        {
            //256x240 resolution and 4 bytes per pixel to represent the color
            fixed (PPUBufferData* d = &buffer)
            {
                d->backBufferPtr = new IntPtr(d->backBuffer);
            }
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
                }
            }
        }

        private void SetPixel(int row, int col, byte r, byte g, byte b)
        {
            //Make sure we're painting in the screen
            if (row >= 0 && row < height && col >= 0 && col < width)
            {
                buffer.backBuffer[width * row + col] = (255 << 24) + (r << 16) + (g << 8) + b;
            }
        }
    }
}