using System;
using System.Collections.Generic;
using System.Text;
using NESCore;
using ReactiveUI;

namespace NESGui.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected NES nes;
    }
}