namespace NESCore;

public unsafe struct PPUPatterntableBufferData
{
    public fixed int backBuffer[PATTERNTABLE_BUFFER_SIZE];
    public IntPtr backBufferPtr { get; set; }
}

public unsafe class PatterntableDebugView
{
    public PPUPatterntableBufferData LeftBuffer;

    private PPU ppu;

    public DebugViewRenderer Renderer;
    public PPUPatterntableBufferData RightBuffer;

    public PatterntableDebugView(PPU ppu)
    {
        //256x128 resolution and 4 bytes per pixel to represent the color
        fixed (PPUPatterntableBufferData* d = &LeftBuffer)
        {
            d->backBufferPtr = new IntPtr(d->backBuffer);
        }

        fixed (PPUPatterntableBufferData* d = &RightBuffer)
        {
            d->backBufferPtr = new IntPtr(d->backBuffer);
        }

        this.ppu = ppu;
        Renderer = new DebugViewRenderer(10, UpdatePatterntable);
    }

    /// <summary>
    ///     Updates both patterntables
    /// </summary>
    public void UpdatePatterntable()
    {
        RenderPatterntable(PATTERNTABLE_LEFT_ADDR, ref LeftBuffer);
        RenderPatterntable(PATTERNTABLE_RIGHT_ADDR, ref RightBuffer);
    }

    /// <summary>
    ///     Renders a patterntable starting at a given address to a target texture.
    /// </summary>
    /// <param name="startAddr">Memory address where the patterntable starts</param>
    /// <param name="targetTexture">Pointer to render texture</param>
    private void RenderPatterntable(ushort startAddr, ref PPUPatterntableBufferData targetTexture)
    {
        var tiles = ppu.EncodeAsTiles(startAddr, TILES_PER_PATTERNTABLE);
        
        var pixelCounter = 0; //To keep track of the pixel on the screen

        //To know which tile to paint, we simply mod the current pixel by the size of a tile
        for (var i = 0; i < TILES_PER_PATTERNTABLE; i++)
        {
            var tileCounter = 0; //To keep track of the pixel in the tile
            for (var j = 0; j < TILE_WIDTH; j++)
            {
                for (var k = 0; k < TILE_HEIGHT; k++)
                {
                    targetTexture.backBuffer[pixelCounter] = GetPixelColour(tiles[i].Pattern[tileCounter]);
                    tileCounter++;
                    pixelCounter++;
                }
            }
        }

        
    }
    
    /// <summary>
    /// Returns a colour based on 
    /// </summary>
    /// <param name="pixelValue"></param>
    /// <returns></returns>
    int GetPixelColour(int pixelValue)
    {
        switch (pixelValue)
        {
            case 0:
                return (255 << 24) + (0 << 16) + (0 << 8) + 0;
            case 1:
                return (255 << 24) + (255 << 16) + (0 << 8) + 0;
            case 2:
                return (255 << 24) + (0 << 16) + (255 << 8) + 0;
            case 3:
                return (255 << 24) + (0 << 16) + (0 << 8) + 255;
            default:
                throw new NotImplementedException();
                return (255 << 24) + (0 << 16) + (0 << 8) + 0;
        }
    }
}