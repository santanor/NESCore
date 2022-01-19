global using static NESCore.Constants;
global using System;
using System.IO;
using System.Threading;
using Serilog;

namespace NESCore;

public class NES
{
    /// <summary>
    ///     Estimated Nanoseconds per CPU cycle, used to artificially match the NES speed.
    ///     <remarks>If the speed in Hertz is 0, this value will be -1. This will be used to indicate "Go fast AF"</remarks>
    /// </summary>
    private readonly int nanoPerCycle;

    private double cpuTimer;
    private DateTime lastSecond;

    public bool Running;


    private int timer = 0;
    private int uptimeSeconds;

    public NES(int speedInHz = 1789773)
    {
        Bus.Cpu = new CPU(speedInHz);
        Bus.Ppu = new PPU();
        Bus.Ram = new RAM();
        Bus.Vram = new VRAM();
        Bus.Cpu.PowerUp();
        Bus.Ppu.Init();
        ConfigureLogger();

        // A Wildcard to indicate "Go as fast as you can"
        if (speedInHz == 0)
        {
            nanoPerCycle = -1;
        }
        else
        {
            nanoPerCycle = (int) (1.0 / speedInHz * 1000000000);
        }
        
    }

    private void ConfigureLogger()
    {
        const string logFileName = "Logs/logfile.log";
        if (File.Exists(logFileName)) File.Delete(logFileName);
        Log.Logger = new LoggerConfiguration()
            //.WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}")
            .WriteTo.File(logFileName, outputTemplate: "{Message:lj}{NewLine}")
            .CreateLogger();
    }

    public void Run()
    {
        //Already running
        if (Running) return;

        Running = true;

        //Set the first PC
        Bus.Cpu.PC = Bus.Word(0xFFFC);
        while (Running) Step();
    }

    public void Step()
    {
        MatchSpeed();
        var cpuCycles = Bus.Cpu.Instruction();
        Bus.Ppu.RunCycles(cpuCycles * 3); // 3 PPU cycles per CPU cycle
    }

    private void MatchSpeed()
    {
        //Don't try to match the speed all the time, it'll kill the CPU doing all the time calculations
        if (Bus.Cpu.cyclesThisSec % 10000 != 1) return;

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

        if (nanoPerCycle != -1)
        {
            //In milliseconds
            var shouldHaveElapsed = Bus.Cpu.cyclesThisSec * nanoPerCycle / 1000000;

            //The PC is going faster than the emulated cpu. Wait!
            if (shouldHaveElapsed > cpuTimer)
                //Console.WriteLine($"Sleeping for {shouldHaveElapsed - cpuTimer}");
                Thread.Sleep((int) (shouldHaveElapsed - cpuTimer));
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
        Bus.Cartridge = c;
        if (c == null) return false;
        return true;
    }
}