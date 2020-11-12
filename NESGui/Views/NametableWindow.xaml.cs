using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NESGui.Views
{
    public class NametableWindow : Window
    {
        public NametableWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}