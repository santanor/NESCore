namespace NESCore.Mappers;

/// <summary>
///     Mapper 0
/// </summary>
public class NROM : IMapper
{
    public const ushort FirstRomPage = 0x8000;
    public const ushort SecondRomPage = 0xC000;
    private readonly ROM rom;

    public NROM(ROM rom)
    {
        this.rom = rom;
    }


    public ushort Map(ushort addr)
    {
        return addr;
    }
}