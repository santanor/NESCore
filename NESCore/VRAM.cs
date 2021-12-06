namespace NESCore;

public class VRAM : MemoryBase
{
    public const int VRAM_SIZE = 0x3FFF;


    public VRAM() : base(VRAM_SIZE)
    {
    }
}