using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NESGui.Views;

public class NametableViewer : Window
{
    public NametableViewer()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}