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
        private NativeMenu menu;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            nametableWindowWindow = new NametableWindow();
            
            //menu = ((NativeMenu.GetMenu(this).Items[0] as NativeMenuItem).Menu.Items[2] as NativeMenuItem).Menu;

            //var mainMenu = this.FindControl<Menu>("MainMenu");
            //mainMenu.AttachedToVisualTree += MenuAttached;
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
    }
}