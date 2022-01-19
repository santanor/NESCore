namespace NESCore.Mappers;

/// <summary>
///     Mapper 0
///
/// https://wiki.nesdev.org/w/index.php?title=NROM
/// </summary>
public class NROM : IMapper
{
    public const ushort FirstRomPage = 0x8000;
    public const ushort SecondRomPage = 0xC000;
    private readonly ROM rom;
    
    /// <summary>
    /// PRG can be 32K or 16K. If it's 16K, the last 16K are mirrored to the first 16K
    /// </summary>
    private bool mirrorPRG;

    public NROM(ROM rom)
    {
        this.rom = rom;
        if (this.rom.numPRGPages == 1)
        {
            mirrorPRG = true;
        }
    }


    public ushort Map(ushort addr)
    {
        // Determine whether to mirror the first 16K or not
        var mirrorValue = mirrorPRG ? 0x3FFF : 0xFFFF;
        return (ushort) ((addr & mirrorValue)- FirstRomPage);
    }
}