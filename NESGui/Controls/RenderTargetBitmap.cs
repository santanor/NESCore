using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;

namespace NESGui.Controls
{
    public class RenderToTargetBitmap : Control
    {
        private WriteableBitmap? bmp;

        private int height => (int) Height;
        private int width => (int) Width;
        
        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            bmp?.Dispose();
            bmp = null;
            base.OnDetachedFromLogicalTree(e);
        }

        private void FillPixels()
        {
            using var fb = bmp!.Lock();
            var copySize = fb.Size.Width * fb.Size.Height;
            Marshal.Copy(NES.Instance.Frame, 0, fb.Address, copySize);
        }
        
        public override void Render(DrawingContext context)
        {
            bmp ??= new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
            FillPixels();
            context.DrawImage(bmp, new Rect(0, 0, width, height), new Rect(0, 0, width, height));
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
            base.Render(context);
        }

    }
}