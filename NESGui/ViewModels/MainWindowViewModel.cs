using System;
using System.Drawing;
using System.Threading.Tasks;
using NESCore;
using NESGui.Controls;

namespace NESGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private RenderToTargetBitmap renderer;
        public MainWindowViewModel()
        {
            // Hardcode a ROM for now
            nes = new NES();
            nes.LoadCartridge("/Users/jose/Developer/src/github.com/NESCore/Roms/DK.nes");
            Task.Run(() => nes.Run());
        }
    }
}