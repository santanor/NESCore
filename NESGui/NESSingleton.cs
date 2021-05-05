using System.Threading.Tasks;

namespace NESGui
{
    public class NESSingleton
    {
        private static readonly NESSingleton instance = new();

        public NESCore.NES Emulator;

        private Task emulatorTask;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static NESSingleton() { }

        private NESSingleton()
        {
            Emulator = new NESCore.NES();
            Emulator.LoadCartridge("../../../../Roms/DK.nes");
            emulatorTask = Task.Run(() => Emulator.Run());
        }

        public static NESSingleton Instance => instance;
    }
}