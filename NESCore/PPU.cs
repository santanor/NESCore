using Microsoft.VisualBasic.CompilerServices;

namespace NESCore;

public unsafe struct PPUBufferData
{
    public fixed int backBuffer[SCREEN_BUFFER_SIZE];
    public IntPtr backBufferPtr { get; set; }
}

public unsafe class PPU
{
    private const int width = 256;
    private const int height = 240;

    private readonly Random random = new();
    private PPUBufferData buffer;
    public int CyclesThisFrame;

    public int FrameCount;
    public NametableDebugView NametableDebugView;

    public PatterntableDebugView PatterntableDebugView;
    public int ScanlineThisFrame;

    public PPU()
    {
        //256x240 resolution and 4 bytes per pixel to represent the color
        fixed (PPUBufferData* d = &buffer)
        {
            d->backBufferPtr = new IntPtr(d->backBuffer);
        }

        NametableDebugView = new NametableDebugView(this);
        PatterntableDebugView = new PatterntableDebugView(this);
    }

    public PPUBufferData Buffer
    {
        get => buffer;
        set => buffer = value;
    }

    public void RunCycles(int cpuCycles)
    {
        for (var i = 0; i < cpuCycles; i++) Cycle();
    }

    /// <summary>
    ///     Runs a single PPU cycle
    /// </summary>
    private void Cycle()
    {
        //Noise for now
        SetPixel(ScanlineThisFrame, CyclesThisFrame, (byte) random.Next(0, 255), (byte) random.Next(0, 255),
            (byte) random.Next(0, 255));

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
            buffer.backBuffer[width * row + col] = (255 << 24) + (r << 16) + (g << 8) + b;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public Tile[] EncodeAsTiles(ushort startAddr, int numTiles)
    {
        var tiles = new Tile[numTiles];
        
        // First iterate the number of tiles
        for (var i = 0; i < numTiles; i++)
        {
            tiles[i] = new Tile();
            var tileIndex = 0;
            for (var j = 0; j < TILE_WIDTH; j++)
            {
                for (var k = 0; k < TILE_HEIGHT; k++)
                {
                    //Get the bit j from the current iteration and 8 positions ahead. Then add those two bits so that
                    //you get one of the following: 00, 01, 10, 11.
                    var least_sig_bit = Bit.TestVal(Bus.VByte(startAddr+j + i), k);
                    var most_sig_bit = (byte) ((Bit.TestVal(Bus.VByte(startAddr+j + i + TILE_WIDTH), k)) << 1);
                    
                    //use the abs to flip the tile in the Y component. Otherwise it comes out wrong.
                    //The +1 is because otherwise the tiles wraparound itself. Feel free to change it and brake it :D
                    var yComponent = Math.Abs(k - TILE_HEIGHT + 1);
                    tiles[i].Pattern[tileIndex] = (byte) (most_sig_bit + least_sig_bit);
                    tileIndex++;
                }
            }
        }
        return tiles;
    }
}