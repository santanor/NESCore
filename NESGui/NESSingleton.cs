using System.Threading.Tasks;
using NESCore;

namespace NESGui;

public class NESSingleton
{
    public NES Emulator;

    private Task emulatorTask;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static NESSingleton()
    {
    }

    private NESSingleton()
    {
        Emulator = new NES();
        Emulator.LoadCartridge("../../../../Roms/DK.nes");
        
    }

    public void Run()
    {
        emulatorTask = Task.Run(() => Emulator.Run());
    }

    public static NESSingleton Instance { get; } = new();
}