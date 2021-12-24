namespace NESCore;

/// <summary>
/// Data structure that holds the render texture and a pointer to it. Used to communicate with the UI
/// </summary>
public unsafe struct PPUNametableBufferData
{
    public fixed int  backBuffer[NAMETABLE_BUFFER_SIZE];
    public IntPtr backBufferPtr { get; set; }
}

public unsafe class NametableDebugView
{
    public PPUNametableBufferData Nametable;

    public ushort[] startAddresses =
        {NAMETABLE_TOPRIGHT_ADDR, NAMETABLE_TOPLEFT_ADDR, NAMETABLE_BOTTOMLEFT_ADDR, NAMETABLE_BOTTOMRIGHT_ADDR};
    private PPU ppu;

    public DebugViewRenderer Renderer;

    public NametableDebugView(PPU ppu)
    {
        Nametable = new PPUNametableBufferData();
        fixed (PPUNametableBufferData* d = &Nametable)
        {
            d->backBufferPtr = new IntPtr(d->backBuffer);
        }

        this.ppu = ppu;
        Renderer = new DebugViewRenderer(10, UpdateNametables);
    }

    /// <summary>
    /// Updates all 4 nametables
    /// </summary>
    public void UpdateNametables()
    {
        for (int i = 0; i < NAMETABLE_BUFFER_SIZE; i++)
        {
            Nametable.backBuffer[i] = i % 255;
        }
    }

    /// <summary>
    /// Updates a single nametable, given a start address and a target render texture 
    /// </summary>
    /// <param name="startAddr"></param>
    /// <param name="targetTexture"></param>
    private void RenderNametable(ushort startAddr, ref PPUNametableBufferData targetTexture)
    {
        //Retrieve current patterntable base addr. Based on what's written into PPUCTRL 
        var patterntableAddr = ppu.GetCurrentPatterntableAddr();

       

    }
}