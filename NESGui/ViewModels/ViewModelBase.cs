using System;
using System.Collections.Generic;
using System.Text;
using NESCore;
using ReactiveUI;

namespace NESGui.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public NES nes;
        
        private double width = 256;
        private double height = 240;
        
        public double Width
        {
            get => width;
            set => this.RaiseAndSetIfChanged(ref width, value);
        }

        public double Height
        {
            get => height;
            set => this.RaiseAndSetIfChanged(ref height, value);
        }
    }
}