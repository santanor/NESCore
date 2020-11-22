using System;
using System.ComponentModel;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DynamicData.Binding;
using NESGui.Controls;
using NESGui.ViewModels;
using ReactiveUI;

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
            vm.OnWindowResized += renderer.UpdateSize;
            //vm.SetupObservers();
            DataContext = vm;
            
            nametableWindowWindow = new NametableWindow();
            patterntableWindow = new PatterntableWindow();

            this.WhenAnyValue(x => x.ClientSize).Subscribe(renderer.UpdateSize);
            //this.WhenAnyValue(x => x.ClientSize.Height).Subscribe(w => renderer.UpdateSize(w, vm.Height));
        }

        private void OnNewFrame(ref int[] frame)
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