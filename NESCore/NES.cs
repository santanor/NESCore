using System;
using System.Drawing;
using System.IO;
using System.Threading;
using NESCore.Mappers;
using Serilog;

namespace NESCore
{
    public class NES
    {
        public PPU.FrameEvent OnNewFrame;
        
        public readonly Bus Bus;
        public bool Running;
        private int uptimeSeconds;
        private int nanoPerCycle;

        public NES(int speedInHz = 1789773)
        {
            Bus = new Bus();
            Bus.Cpu = new CPU(Bus, speedInHz);
            Bus.Ppu = new PPU();
            Bus.Ram = new RAM();

            Bus.Cpu.PowerUp();
            ConfigureLogger();

            nanoPerCycle = (int) ((1.0 / speedInHz) * 1000000000);

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
            MatchSpeed();
            var cpuCycles = Bus.Cpu.Instruction();
            Bus.Ppu.RunCycles(cpuCycles*3);
        }

        
        private int timer = 0;
        private DateTime lastSecond;
        private double cpuTimer;
        private void MatchSpeed()
        {
            //Don't try to match the speed all the time, it'll kill the CPU doing all the time calculations
            if (Bus.Cpu.cyclesThisSec % 10000 != 1)
            {
                return;
            }
            
            //A second has passed so start over
            if (cpuTimer > 1000)
            {
                Console.WriteLine($"Second: {uptimeSeconds} FPS: {Bus.Ppu.FrameCount}");
                cpuTimer = 0;
                Bus.Ppu.FrameCount = 0;
                Bus.Cpu.cyclesThisSec = 0;
                lastSecond = DateTime.Now;
                uptimeSeconds++;
            }

            //In milliseconds
            var shouldHaveElapsed = (Bus.Cpu.cyclesThisSec * nanoPerCycle)/1000000;
            
            //The PC is going faster than the emulated cpu. Wait!
            if (shouldHaveElapsed > cpuTimer)
            {
                //Console.WriteLine($"Sleeping for {shouldHaveElapsed - cpuTimer}");
                Thread.Sleep((int)(shouldHaveElapsed - cpuTimer));
            }

            cpuTimer = (DateTime.Now - lastSecond).TotalMilliseconds;
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
