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
        }

    }
}
