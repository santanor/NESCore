using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace NESGui.Controls
{
    public abstract unsafe class RenderToTargetBitmap : Control
    {
        private WriteableBitmap? bmp;
        protected void* backBufferPointer;

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
            var copySize = fb.Size.Width * fb.Size.Height * 4;
            Buffer.MemoryCopy(backBufferPointer, (void*)fb.Address, copySize, copySize);
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