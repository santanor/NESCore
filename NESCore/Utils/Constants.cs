namespace NESCore;

public static class Constants
{
    //General emulation constants
    public const int SCREEN_BUFFER_SIZE = 61440;
    public const ushort UNIVERSAL_BACKGROUND = 0x3F00;
    public const int SCREEN_HEIGHT = 256;
    public const int SCREEN_WIDTH = 240;

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
    public const ushort PPUCTRL = 0x2000; //VPHB SINN - NMI enable (V), PPU master/slave (P), sprite height (H), background tile select (B), sprite tile select (S), increment mode (I), nametable select (NN)
    public const ushort PPUMASK = 0x2001; // BGRs bMmG - color emphasis (BGR), sprite enable (s), background enable (b), sprite left column enable (M), background left column enable (m), greyscale (G)
    public const ushort PPUSTATUS = 0x2002; // VSO- ---- - vblank (V), sprite 0 hit (S), sprite overflow (O); read resets write pair for $2005/$2006
    public const ushort OAMADDR = 0x2003; // aaaa aaaa - OAM read/write address
    public const ushort OAMDATA = 0x2004; // dddd dddd - OAM data read/write
    public const ushort PPUSCROLL = 0x2005; // xxxx xxxx - fine scroll position (two writes: X scroll, Y scroll)
    public const ushort PPUADDR = 0x2006; // aaaa aaaa - PPU read/write address (two writes: most significant byte, least significant byte)
    public const ushort PPUDATA = 0x2007; // dddd dddd - PPU data read/write
    public const ushort OAMDMA = 0x4014; // aaaa aaaa - OAM DMA high address
    public const int PPU_VISIBLE_SCANLINES = 240;
    public const int PPU_POINT_PER_SCANLINE = 340;
    public const int PPU_SCANLINES = 260;
    public const int PPU_PRERENDER_SCANLINE = -1;
    

    // ROM constants
    public const int CHR_PAGE_SIZE = 8192;
    public const int PRG_PAGE_SIZE = 16384;
    public const int TRAINER_SIZE = 512;
}