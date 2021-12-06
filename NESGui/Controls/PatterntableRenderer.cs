using Avalonia.LogicalTree;
using NESCore;

namespace NESGui.Controls;

public unsafe class PatterntableRenderer : RenderToTargetBitmap
{
    public enum PatternTableEnum
    {
        Left,
        Right
    }

    public PatternTableEnum Table { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        switch (Table)
        {
            case PatternTableEnum.Left:
                backBufferPointer = (void*) Bus.Ppu.PatterntableDebugView.LeftBuffer
                    .backBufferPtr;
                break;
            case PatternTableEnum.Right:
                backBufferPointer = (void*) Bus.Ppu.PatterntableDebugView.RightBuffer
                    .backBufferPtr;
                break;
        }

        Bus.Ppu.PatterntableDebugView.Renderer.StartRenderer();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Bus.Ppu.PatterntableDebugView.Renderer.StopRenderer();
    }
}