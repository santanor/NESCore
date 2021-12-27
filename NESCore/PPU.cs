
using System.Diagnostics;

namespace NESCore;

public unsafe struct PPUBufferData
{
    public fixed int backBuffer[SCREEN_BUFFER_SIZE];
    public IntPtr backBufferPtr { get; set; }
}

public unsafe class PPU
{
    private readonly Random random = new();
    private PPUBufferData buffer;
    public int CyclesThisFrame;

    public int FrameCount;
    public NametableDebugView NametableDebugView;

    public PatterntableDebugView PatterntableDebugView;
    public int ScanlineThisFrame;

    private bool nmiOccurred;
    private bool nmiOutput;
    private bool vBlank;

    /// <summary>
    /// Internal latch to distinguish between sequential writes to PPUADDR and PPUSCROLL and all of that
    /// </summary>
    private bool latch;
    
    /// <summary>
    /// Used to hold temporary VRAM info while the two writes are happening with PPUADDR and such. 
    /// </summary>
    private ushort tempVramAddr;
    
    /// <summary>
    /// Current VRAM addr pointer. Whenever something is written to PPUDATA, it'll be stored in this addr
    /// </summary>
    private ushort currentVramAddr;

    /// <summary>
    /// PPU Registers, each is 1 byte and can be read/written from the Bus or internal PPU
    /// </summary>
    private byte Ctrl, Mask, Status;

    public PPU()
    {
        //256x240 resolution and 4 bytes per pixel to represent the color
        fixed (PPUBufferData* d = &buffer)
        {
            d->backBufferPtr = new IntPtr(d->backBuffer);
        }

        NametableDebugView = new NametableDebugView(this);
        PatterntableDebugView = new PatterntableDebugView(this);
    }

    public void Init()
    {
        Bus.WriteByte(PPUCTRL, 0);
        Bus.WriteByte(PPUMASK, 0);
        Bus.WriteByte(PPUSTATUS, 0xA0);
        Bus.WriteByte(OAMADDR, 0);
        Bus.WriteByte(PPUSCROLL, 0);
        Bus.WriteByte(PPUADDR, 0);
        Bus.WriteByte(PPUDATA, 0);
        Bus.WriteByte(OAMDATA, 0); // Undefined default value
        Bus.WriteByte(OAMDMA, 0); // Undefined default value
    }

    public PPUBufferData Buffer
    {
        get => buffer;
        set => buffer = value;
    }

    public void RunCycles(int cpuCycles)
    {
        for (var i = 0; i < cpuCycles; i++) Cycle();
    }

    /// <summary>
    ///     Runs a single PPU cycle
    /// </summary>
    private void Cycle()
    {
        //Noise for now
        SetPixel(ScanlineThisFrame, CyclesThisFrame, (byte) random.Next(0, 255), (byte) random.Next(0, 255),
            (byte) random.Next(0, 255));

        Timings();
        
    }

    /// <summary>
    /// This method keeps all the cycles and scanlines in order. It also will raise the neccesary VBlank and NMI flags
    /// </summary>
    private void Timings()
    {
        CyclesThisFrame++;
        if (CyclesThisFrame > PPU_POINT_PER_SCANLINE)
        {
            CyclesThisFrame = 0;
            ScanlineThisFrame++;
            if (ScanlineThisFrame > PPU_SCANLINES)
            {
                ScanlineThisFrame = PPU_PRERENDER_SCANLINE;
                FrameCount++;
            }
        }

        // The VBlank finishes here. The second cycle of the pre renderer scanline
        if (ScanlineThisFrame == PPU_PRERENDER_SCANLINE && CyclesThisFrame == 1)
        {
            FinishVBlank();
        }
        
        // The VBlank flag of the PPU is set at tick 1 (the second tick) of scanline 241
        if (ScanlineThisFrame == 241 && CyclesThisFrame == 1)
        {
           StartVBlank();
        }

        TryTriggerNMI();
    }
    
    /// <summary>
    /// If all the conditions are met, trigger a NMI
    /// </summary>
    private void TryTriggerNMI() {
        if (nmiOccurred && nmiOutput) {
            //triggering of a NMI can be prevented if bit 7 of PPU Control Register 1 ($2000) is clear.
            var ppuctrl = Bus.Byte(PPUCTRL);
            if(Bit.TestVal(ppuctrl, 7) == 1){
                Bus.Cpu.nmi();
            }

            nmiOccurred = false;
        }
    }

    private void StartVBlank()
    {
        vBlank = true;
        nmiOccurred = true;
    }

    private void FinishVBlank()
    {
        vBlank = true;
        nmiOccurred = true;
    }

    private void SetPixel(int row, int col, byte r, byte g, byte b)
    {
        //Make sure we're painting in the screen
        if (row >= 0 && row < SCREEN_HEIGHT && col >= 0 && col < SCREEN_WIDTH)
            buffer.backBuffer[SCREEN_WIDTH * row + col] = (255 << 24) + (r << 16) + (g << 8) + b;
    }

    /// <summary>
    /// Encodes the next 16 bytes as a patterntable tile, starting at the given address
    /// </summary>
    /// <param name="addr"></param>
    /// <returns></returns>
    public Tile GetTile(ushort addr)
    {
        var tile = new Tile();
        
        for (var i = 0; i < TILE_WIDTH; i++)
        {
            var lowByteAddr = addr + i;
            var highByteAddr = lowByteAddr + TILE_WIDTH;
            for (var j = 0; j < TILE_HEIGHT; j++)
            {
                
                //Get the bit j from the current iteration and 8 positions ahead. Then add those two bits so that
                //you get one of the following: 00, 01, 10, 11.
                var least_sig_bit = Bit.TestVal(Bus.VByte(lowByteAddr), j);
                var most_sig_bit = (byte) ((Bit.TestVal(Bus.VByte(highByteAddr), j)) << 1);
                    
                //use the abs to flip the tile in the Y component. Otherwise it comes out wrong.
                //The +1 is because otherwise the tiles wraparound itself. Feel free to change it and break it :D
                var yComponent = Math.Abs(j - TILE_HEIGHT + 1);
                tile.Pattern[i,yComponent] = (byte) (most_sig_bit + least_sig_bit);
            }
        }

        return tile;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public Tile[] EncodeAsTiles(ushort addr, int numTiles)
    {
        var tiles = new Tile[numTiles];
        
        // First iterate the number of tiles
        for (var i = 0; i < numTiles; i++)
        {
            tiles[i] = GetTile(addr);
            addr += TILE_BYTE_SIZE;
        }
        
        return tiles;
    }

    /// <summary>
    /// Returns the current patterntable address. In order to do that this method will look at bit 3 of PPUTCTRL (0x2000)
    /// </summary>
    /// <returns></returns>
    public ushort GetCurrentPatterntableAddr()
    {
        var ppuctrl = Bus.Byte(PPUCTRL);
        return Bit.Test(ppuctrl, 4) ? PATTERNTABLE_RIGHT_ADDR : PATTERNTABLE_LEFT_ADDR;
    }
    
    #region Register operations

    /// <summary>
    /// Performs the necessary operations when writing to PPU registers
    ///
    /// This method DOES NOT write to memory. The calling method (Bus) takes care of that 
    /// </summary>
    /// <param name="register"></param>
    /// <param name="val"></param>
    /// <exception cref="NotSupportedException"></exception>
    public void WriteRegister(int register, byte val)
    {
        switch (register)
        {
            case PPUCTRL:
                WritePPUCtrl(val);
                break;
            case PPUMASK:
                break;
            case PPUSTATUS:
                break;
            case OAMADDR:
                break;
            case OAMDATA:
                break;
            case PPUSCROLL:
                break;
            case PPUADDR:
                WritePPUADDR(val);
                break;
            case PPUDATA:
                WritePPUDATA(val);
                break;
            case OAMDMA:
                break;
            
            default:
                throw new NotSupportedException($"PPU Register: {register} shouldn't have made it here");
        }
    }

    public byte ReadRegister(int register)
    {
        switch (register)
        {
            case PPUCTRL:
                break;
            case PPUMASK:
                break;
            case PPUSTATUS:
                return ReadPPUStatus();
            case OAMADDR:
                break;
            case OAMDATA:
                break;
            case PPUSCROLL:
                break;
            case PPUADDR:
                break;
            case PPUDATA:
                return ReadPPUData();
            case OAMDMA:
                break;
            
            default:
                throw new NotSupportedException($"PPU Register: {register} shouldn't have made it here");
        }

        return 0x0;
    }

    /// <summary>
    /// Writes to PPUCTRL
    /// </summary>
    /// <param name="val"></param>
    private void WritePPUCtrl(byte val)
    {
        nmiOutput = Bit.Test(val, 7);
        Ctrl = val;
    }

    /// <summary>
    /// Writes to PPUADDR. Writing has to happen in 2 sequential writes (8 bits each) to provide a full address (WORD)
    /// </summary>
    /// <param name="val"></param>
    private void WritePPUADDR(byte val)
    {
        /*
         * t: .FEDCBA ........ = d: ..FEDCBA
	 	 * t: X...... ........ = 0
         * w:                  = 1
	 	*/
        if (!latch) // This writes to high
        {
            tempVramAddr = (ushort) ((val & 0b11111111) << 8);// extract FEDCBA
            tempVramAddr &= 0b0111111111111111;// for the X
            latch = true;
        }
        /*
		 * t: ....... HGFEDCBA = d: HGFEDCBA
		 * v                   = t
		 * w:                  = 0
		 */
        else
        {
            tempVramAddr = (ushort) ((tempVramAddr & ~0b0000000011111111) | (val));
            currentVramAddr = tempVramAddr;
            latch = false;
        }
    }

    /// <summary>
    /// Write data to VRAM
    /// After access, the video memory address will increment by an amount determined by bit 2 of $2000.
    /// </summary>
    /// <param name="data"></param>
    private void WritePPUDATA(byte data)
    {
        Bus.VWriteByte(currentVramAddr, data);
        var ctrl = Bus.Byte(PPUCTRL);
        currentVramAddr += (ushort)(Bit.Test(ctrl, 2) ? 31 : 1);
    }

    /// <summary>
    /// Reads data from VRAM
    /// After access, the video memory address will increment by an amount determined by bit 2 of $2000.
    /// </summary>
    /// <returns></returns>
    private byte ReadPPUData()
    {
        var val = Bus.VByte(currentVramAddr);
        var ctrl = Bus.Byte(PPUCTRL);
        currentVramAddr += (ushort)(Bit.Test(ctrl, 2) ? 31 : 1);

        return val;
    }

    /// <summary>
    /// Reads the contents of PPUSTATUS
    /// </summary>
    /// <returns></returns>
    private byte ReadPPUStatus()
    {
        //Reading STATUS will clear the latch used by PPUSCROLL and PPUADDR
        latch = false;
        // Reading the status register will clear bit 7 
        Bit.Clear(ref Status, 7);
        return Status;
    }
    
    #endregion
}