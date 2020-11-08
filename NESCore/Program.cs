using System;
using Serilog;

namespace NESCore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var nes = new NES();
            var (success, rom) = ROM.FromFile(@"/Users/jose/Developer/src/github.com/NESCore/Roms/DK.nes");
            if (!success)
            {
                Log.Fatal("Missing ROM. Quitting");
            }
            
            nes.LoadROM(rom);
            nes.Run();
        }
    }
}
