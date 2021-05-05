using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NESGui.Views
{
    public class PatterntableViewer : Window
    {
        public PatterntableViewer()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
    }
}