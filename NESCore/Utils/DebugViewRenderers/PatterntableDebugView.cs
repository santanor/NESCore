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
        Tile tile;

        //To know which tile to paint, we simply mod the current pixel by the size of a tile
        for (var i = 0; i < TILES_PER_PATTERNTABLE; i++)
        {
            tile = ppu.GetTile(startAddr);
            startAddr += TILE_BYTE_SIZE;
            for (var j = 0; j < TILE_WIDTH; j++)
            {
                for (var k = 0; k < TILE_HEIGHT; k++)
                {
                    var columnIndex = ((i & 0x0F) * TILE_HEIGHT) + k;
                    var rowIndex = ((i >> 4) * TILE_WIDTH) + j;

                    var pixelIndex = PATTERNTABLE_HEIGHT * rowIndex + columnIndex;
                    targetTexture.backBuffer[pixelIndex] = GetPixelColour(tile.Pattern[j,k]);
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
        var addr = UNIVERSAL_BACKGROUND + pixelValue;
        var colourIndex = Bus.VByte(addr);
        return Palette.Colours[colourIndex];
    }
}