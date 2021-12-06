namespace NESCore;

public unsafe struct PPUNametableBufferData
{
    public fixed int backBuffer[512 * 480];
    public IntPtr backBufferPtr { get; set; }
}

public unsafe class NametableDebugView
{
    private PPUNametableBufferData buffer;
    private PPU ppu;

    public DebugViewRenderer Renderer;

    public NametableDebugView(PPU ppu)
    {
        //512x480 resolution and 4 bytes per pixel to represent the color
        fixed (PPUNametableBufferData* d = &buffer)
        {
            d->backBufferPtr = new IntPtr(d->backBuffer);
        }

        this.ppu = ppu;
        Renderer = new DebugViewRenderer(10, UpdateNametable);
    }

    public PPUNametableBufferData Buffer
    {
        get => buffer;
        set => buffer = value;
    }

    public void UpdateNametable()
    {
        var rand = new Random();
        var nametableSize = 512 * 480;
        for (var i = 0; i < nametableSize; i++)
            buffer.backBuffer[i] = (255 << 24) + (rand.Next(0, 255) << 16) + ((i / 2) << 8) + i / 3;
    }
}