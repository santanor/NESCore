using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;
using PixelFormat = Avalonia.Platform.PixelFormat;
using Point = System.Drawing.Point;
using Size = Avalonia.Size;

namespace NESGui.Controls
{
    public class RenderToTargetBitmap : Control
    {
        private WriteableBitmap bmp;
        
        public int width = 1920;
        public int height = 1080;

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            bmp = new WriteableBitmap(new PixelSize(width, height), new Vector(72, 72), PixelFormat.Bgra8888);
            base.OnAttachedToLogicalTree(e);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);

            bmp?.Dispose();
            bmp = null;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
            
            context.DrawImage(bmp, 1, new Rect(0, 0, width, height), new Rect(0, 0, width, height));
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
        }

        public void UpdateSize(Size size)
        {
            UpdateSize(size.Width, size.Height);
        }
        public void UpdateSize(double w, double h)
        {
            UpdateSize((int)w, (int)h);
        }
        public void UpdateSize(int w, int h)
        {
            if (w <= 0 || h <= 0)
            {
                return;
            }
            width = w;
            height = h;
            bmp = new WriteableBitmap(new PixelSize(width, height), new Vector(72, 72), PixelFormat.Bgra8888);
        }

        public void UpdateBmp(in int[] bitmap)
        {
            if (bmp == null )
            {
                return;
            }
            using var fb = bmp.Lock();
            var bytesToCopy = Math.Min((256 * 240), bmp.PixelSize.Width * bmp.PixelSize.Height);
            Marshal.Copy(bitmap,0, fb.Address, bytesToCopy);
        }
    }
}