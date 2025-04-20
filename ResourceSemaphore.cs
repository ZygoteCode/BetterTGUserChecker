public class ResourceSemaphore
{
    private volatile int state = 1;

    public bool IsResourceAvailable() => state == 1;

    public bool LockResource()
    {
        return Interlocked.CompareExchange(ref state, 0, 1) == 1;
    }

    public void UnlockResource()
    {
        Interlocked.Exchange(ref state, 1);
    }
}