using Avalonia.LogicalTree;
using NESCore;

namespace NESGui.Controls;

public unsafe class EmulatorRenderer : RenderToTargetBitmap
{
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        backBufferPointer = (void*) Bus.Ppu.Buffer.backBufferPtr;
    }
}