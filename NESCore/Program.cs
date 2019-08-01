using System;

namespace NESCore
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ROM.FromFile(@"C:\dev\NESCore\NESCore\rom\nestest.nes");
        }
    }
}
