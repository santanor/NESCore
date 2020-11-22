using System;
using System.ComponentModel;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NESCore;
using NESGui.Controls;
using ReactiveUI;

namespace NESGui.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public delegate void ResizeEvent(double width, double height);

        public ResizeEvent OnWindowResized;

        public MainWindowViewModel()
        {
            // Hardcode a ROM for now
            nes = new NES();
            nes.LoadCartridge("/Users/jose/Developer/src/github.com/NESCore/Roms/DK.nes");
            Task.Run(() => nes.Run());
        }
    }
}