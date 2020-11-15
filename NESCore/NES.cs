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
        public bool Running;
        private int lastSecond;
        private int uptimeSeconds;

        public NES(int speedInHz = 1789773)
        {
            Bus = new Bus();
            Bus.Cpu = new CPU(Bus, speedInHz);
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
                .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}")
                .WriteTo.File(logFileName, outputTemplate: "{Message:lj}{NewLine}")
                .CreateLogger();
        }

        public void Run()
        {
            //Already running
            if (Running)
            {
                return;
            }
            
            Running = true;

            while (Running)
            {
                Step();
            }
        }

        public void Step()
        {
            if(Bus.Cpu.cyclesThisSec % 10000 == 1)
            {
                var s = DateTime.Now.Second;
                if (s != lastSecond)
                {
                    lastSecond = s;
                    uptimeSeconds++;
                    Log.Information($"Second: {uptimeSeconds} FPS: {Bus.Ppu.FrameCount}");
                    //Console.WriteLine($"Second: {uptimeSeconds} FPS: {Bus.Ppu.FrameCount}");
                    Bus.Ppu.FrameCount = 0;
                    Bus.Cpu.cyclesThisSec = 0;
                }
            }

            var cpuCycles = Bus.Cpu.Instruction();
            Bus.Ppu.RunCycles(cpuCycles*3);
        }

        public void Stop()
        {
            Running = false;
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
