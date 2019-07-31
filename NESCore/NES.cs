namespace NESCore
{
    public class NES
    {
        public RAM Ram;
        public CPU Cpu;

        public NES()
        {
            Cpu = new CPU();
            Ram = new RAM(RAM.RAM_SIZE, Cpu);
        }

    }
}
