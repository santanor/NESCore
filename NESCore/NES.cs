using System;
using System.Drawing;
using System.IO;
using NESCore.Mappers;
using Serilog;

namespace NESCore
{
    public class NES
    {
        public PPU.FrameEvent OnNewFrame;
        
        public readonly Bus Bus;
        private bool running = true;

        public NES()
        {
            Bus = new Bus();
            Bus.Cpu = new CPU(Bus);
            Bus.Ppu = new PPU();
            Bus.Ram = new RAM();

            Bus.Cpu.PowerUp();
            ConfigureLogger();

            Bus.Ppu.OnNewFrame += (ref int[] frame) => OnNewFrame?.Invoke(ref frame);
        }

        private void ConfigureLogger()
        {
            const string logFileName = "Logs/logfile.log";
            if (File.Exists(logFileName))
            {
                File.Delete(logFileName);
            }
            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}")
                .WriteTo.File(logFileName, outputTemplate: "{Message:lj}{NewLine}")
                .CreateLogger();
        }

        public void Run()
        {
            running = true;

            while (running)
            {
                Step();
            }
        }

        public void Step()
        {
            var cpuCycles = Bus.Cpu.Instruction();
            Bus.Ppu.RunCycles(cpuCycles*3);
        }

        public void Stop()
        {
            running = false;
        }

        public bool LoadCartridge(string fileName)
        {
            var c = Cartridge.FromFile(fileName);
            if (c == null)
            {
                return false;
            }
            Bus.Cartridge = c;
            return true;
        }
    }
}
