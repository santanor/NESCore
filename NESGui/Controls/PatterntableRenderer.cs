using Avalonia.LogicalTree;

namespace NESGui.Controls
{
    public unsafe class PatterntableRenderer : RenderToTargetBitmap
    {
        protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnAttachedToLogicalTree(e);
            backBufferPointer = (void*) NESSingleton.Instance.Emulator.Bus.Ppu.Buffer.backBufferPtr;
        }
    }
}