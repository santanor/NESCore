using System;
using NESCore.Mappers;
using Serilog;

namespace NESCore
{
    public class NES
    {
        public readonly RAM Ram;
        public readonly CPU Cpu;
        private bool running = true;

        public NES()
        {
            Ram = new RAM(RAM.RAM_SIZE);
            Cpu = new CPU
            {
                Ram = Ram
            };
            Ram.Cpu = Cpu;
            Cpu.PowerUp();

            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.File("Logs/logfile.log")
               .CreateLogger();
        }

        public void Run()
        {
            running = true;

            while (running)
            {
                Cpu.Cycle();
            }
        }

        public void Stop()
        {
            running = false;
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
