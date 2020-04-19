namespace NESCore
{
    public class PPU
    {
        public int FrameCount { get; private set; }
        public int CyclesThisFrame { get; private set; }

        public void RunCycles(int cpuCycles)
        {
            for (var i = 0; i < cpuCycles; i++)
            {
                Cycle();
            }
            
        }

        private void Cycle()
        {
            CyclesThisFrame++;
            if (CyclesThisFrame == 341)
            {
                CyclesThisFrame = 0;
                FrameCount++;
            }

        }
    }
}