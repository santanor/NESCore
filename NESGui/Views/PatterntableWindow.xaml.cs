using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NESGui.Views
{
    public class PatterntableWindow : Window
    {
        public PatterntableWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}