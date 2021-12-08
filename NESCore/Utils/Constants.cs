namespace NESCore;

public static class Constants
{
    //General emulation constants
    public const int SCREEN_BUFFER_SIZE = 61440;

    //Patterntable constants
    public const int PATTERNTABLE_BUFFER_SIZE = 16384;
    public const int TILE_WIDTH = 8;
    public const int TILE_HEIGHT = 8;
    public const int TILE_BYTE_SIZE = 0x16;
    public const int PATTERNTABLE_WIDTH = 128;
    public const int PATTERNTABLE_HEIGHT = 128;
    public const int TILES_PER_PATTERNTABLE = 256;
    public const ushort PATTERNTABLE_LEFT_ADDR = 0X0000;
    public const ushort PATTERNTABLE_RIGHT_ADDR = 0x1000;

    // ROM constants
    public const int CHR_PAGE_SIZE = 8192;
    public const int PRG_PAGE_SIZE = 16384;
    public const int TRAINER_SIZE = 512;
}