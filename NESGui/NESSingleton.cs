using System.Threading.Tasks;

namespace NESGui
{
    public class NES
    {
        private static readonly NES instance = new NES();

        private NESCore.NES Emulator;
        public int[] Frame => Emulator.Frame;

        private Task emulatorTask;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static NES() { }

        private NES()
        {
            Emulator = new NESCore.NES();
            Emulator.LoadCartridge("../../../../Roms/DK.nes");
            emulatorTask = Task.Run(() => Emulator.Run());
        }

        public static NES Instance => instance;
    }
}