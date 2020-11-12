using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using NESGui.ViewModels;

namespace NESGui.Views
{
    public class MainWindow : Window
    {
        private NametableWindow nametableWindowWindow;
        private PatterntableWindow patterntableWindow;
        private NativeMenu menu;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            nametableWindowWindow = new NametableWindow();
            patterntableWindow = new PatterntableWindow();
        }
        
        public void MenuAttached(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (NativeMenu.GetIsNativeMenuExported(this) && sender is Menu mainMenu)
            {
                mainMenu.IsVisible = false;
            }
        }
        
        public void OnCloseClicked(object sender, EventArgs args)
        {
            Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public void OpenNametableWindow(object sender, EventArgs args)
        {
            nametableWindowWindow.Show();
        }

        public void OpenPatterntableWindow(object sender, EventArgs args)
        {
            patterntableWindow.Show();
        }
    }
}