namespace NESCore
{
    public class NES
    {
        public RAM Ram;

        public NES()
        {
            Ram = new RAM(RAM.RAM_SIZE);
        }

    }
}
