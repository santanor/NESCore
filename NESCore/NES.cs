using System;
using NESCore.Mappers;
using Serilog;

namespace NESCore
{
    public class NES
    {
        public RAM Ram;
        public CPU Cpu;

        public NES()
        {
            Cpu = new CPU();
            Ram = Cpu.Ram;
            Cpu.PowerUp();

            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
                        .WriteTo.File("Logs/logfile.log",outputTemplate:"[{Level:u3}] {Message:lj}{NewLine}{Exception}")
               .CreateLogger();
        }

        /// <summary>
        /// Loads the ROM into memory, applying the mapper and copying the necessary bits to where they should go
        /// </summary>
        public void LoadROM(ROM rom)
        {
            //TODO move this crap to a different file
            switch (rom.mapper)
            {
                case 0:
                    var nrom = new NROM(this);
                    nrom.Apply(rom);
                    break;
            }
        }

    }
}
