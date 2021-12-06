using Avalonia.LogicalTree;
using NESCore;

namespace NESGui.Controls;

public unsafe class NametableRenderer : RenderToTargetBitmap
{
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        backBufferPointer = (void*) Bus.Ppu.NametableDebugView.Buffer.backBufferPtr;
        Bus.Ppu.NametableDebugView.Renderer.StartRenderer();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Bus.Ppu.NametableDebugView.Renderer.StopRenderer();
    }
}