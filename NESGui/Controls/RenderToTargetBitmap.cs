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
using PixelFormat = Avalonia.Platform.PixelFormat;
using Point = System.Drawing.Point;

namespace NESGui.Controls
{
    public class RenderToTargetBitmap : Control
    {
        private WriteableBitmap bmp;
        
        public int widthh;
        private int height;

        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            bmp = new WriteableBitmap(new PixelSize(256, 240), new Vector(96, 96), PixelFormat.Rgba8888);
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
            
            context.DrawImage(bmp, 1, new Rect(0, 0, 256, 240), new Rect(0, 0, 256, 240));
            Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Render);
        }

        public void UpdateBmp(in byte[] bitmap)
        {
            using (var fb = bmp.Lock())
            {
                Marshal.Copy(bitmap,0, fb.Address, fb.Size.Width * fb.Size.Height);
            }
        }
    }
}