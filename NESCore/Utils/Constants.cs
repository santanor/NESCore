namespace NESCore;

public static class Constants
{
    //General emulation constants
    public const int SCREEN_BUFFER_SIZE = 61440;
    public const ushort UNIVERSAL_BACKGROUND = 0x3F00;

    //Patterntable constants
    public const int PATTERNTABLE_BUFFER_SIZE = 16384; //128 x 128 This is per patterntable (There are 2)
    public const int TILE_WIDTH = 8;
    public const int TILE_HEIGHT = 8;
    public const int TILE_BYTE_SIZE = 0x16;
    public const int PATTERNTABLE_WIDTH = 128;
    public const int PATTERNTABLE_HEIGHT = 128;
    public const int TILES_PER_PATTERNTABLE = 256;
    public const ushort PATTERNTABLE_LEFT_ADDR = 0X0000;
    public const ushort PATTERNTABLE_RIGHT_ADDR = 0x1000;
    
    //Nametable constants
    public const int NAMETABLE_TILES_WIDE = 32;
    public const int NAMETABLE_TILES_HEIGHT = 30;
    public const int NAMETABLE_BUFFER_SIZE = 245760;// 512 * 480 
    public const int TILES_PER_NAMETABLE = 960;// 30 x 32 tiles per nametable
    public const ushort NAMETABLE_TOPLEFT_ADDR = 0x2000;
    public const ushort NAMETABLE_TOPRIGHT_ADDR = 0x2400;
    public const ushort NAMETABLE_BOTTOMLEFT_ADDR = 0x2800;
    public const ushort NAMETABLE_BOTTOMRIGHT_ADDR = 0x2C00;
    
    //PPU constants 
    public const ushort PPUCTRL = 0x2000;

    // ROM constants
    public const int CHR_PAGE_SIZE = 8192;
    public const int PRG_PAGE_SIZE = 16384;
    public const int TRAINER_SIZE = 512;
}