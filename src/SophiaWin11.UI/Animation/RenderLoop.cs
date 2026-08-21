namespace SophiaWin11.UI.Animation;

public sealed class RenderLoop
{
    private readonly Action _subscribe;
    private readonly Action _unsubscribe;

    public RenderLoop(Action subscribe, Action unsubscribe)
    {
        _subscribe = subscribe;
        _unsubscribe = unsubscribe;
    }

    public bool IsRunning { get; private set; }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        IsRunning = true;
        _subscribe();
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        _unsubscribe();
    }
}
