using System.Threading;

namespace NESCore;

public class DebugViewRenderer
{
    private readonly int millisecondSpeed;
    private readonly Action renderAction;
    private Thread renderThread;
    private bool running;
    private object lockObj = new();

    /// <summary>
    /// </summary>
    /// <param name="fps">Refresh speed. measured in frames per second</param>
    public DebugViewRenderer(int fps, Action renderAct)
    {
        millisecondSpeed = (int) (1.0 / fps);
        renderAction = renderAct;
    }


    public void StartRenderer()
    {
        running = true;
        renderThread = new Thread(RenderLoop);
        renderThread.Start();
    }

    private void RenderLoop()
    {
        while (running)
        {
            lock (lockObj)
            {
                renderAction?.Invoke();
                Thread.Sleep(millisecondSpeed);
            }
        }
    }

    public void StopRenderer()
    {
        running = false;
        renderThread.Join();
    }
}