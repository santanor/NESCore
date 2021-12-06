using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NESGui.Views;

public class Emulator : Window
{
    public Emulator()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        //Force the load
        var n = NESSingleton.Instance;
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        NESSingleton.Instance.Emulator.Stop();
        Close();
    }

    private void OpenNametableWindow(object? sender, EventArgs e)
    {
        var nametableViewer = new NametableViewer();
        nametableViewer.Show();
    }

    private void OpenPatterntableWindow(object? sender, EventArgs e)
    {
        var patterntableViewer = new PatterntableViewer();
        patterntableViewer.Show();
    }
}