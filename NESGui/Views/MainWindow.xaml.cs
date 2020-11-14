using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using NESGui.Controls;
using NESGui.ViewModels;

namespace NESGui.Views
{
    public class MainWindow : Window
    {
        private NametableWindow nametableWindowWindow;
        private PatterntableWindow patterntableWindow;
        private NativeMenu menu;
        private RenderToTargetBitmap renderer;
        
        public MainWindow()
        {
            InitializeComponent();
            renderer = this.FindControl<RenderToTargetBitmap>("RenderToTargetBitmap");
            var vm = new MainWindowViewModel();
            vm.nes.OnNewFrame += OnNewFrame;
            DataContext = vm;
            
            nametableWindowWindow = new NametableWindow();
            patterntableWindow = new PatterntableWindow();
        }

        private void OnNewFrame(ref byte[] frame)
        {
            renderer.UpdateBmp(frame);
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