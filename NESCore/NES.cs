using System;
using System.IO;
using NESCore.Mappers;
using Serilog;

namespace NESCore
{
    public class NES
    {
        public readonly RAM Ram;
        public readonly CPU Cpu;
        public readonly PPU Ppu;
        private bool running = true;

        public NES()
        {
            Ram = new RAM(RAM.RAM_SIZE);
            Cpu = new CPU
            {
                Ram = Ram
            };
            Ram.Cpu = Cpu;
            Ppu = new PPU();
            Cpu.Ppu = Ppu;
            Cpu.PowerUp();
            ConfigureLogger();
        }

        private void ConfigureLogger()
        {
            const string logFileName = "Logs/logfile.log";
            if (File.Exists(logFileName))
            {
                File.Delete(logFileName);
            }
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}")
                .WriteTo.File(logFileName, outputTemplate: "{Message:lj}{NewLine}")
                .CreateLogger();
            
        }

        public void Run()
        {
            running = true;

            while (running)
            {
                var cpuCycles = Cpu.Instruction();
                Ppu.RunCycles(cpuCycles*3);
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
