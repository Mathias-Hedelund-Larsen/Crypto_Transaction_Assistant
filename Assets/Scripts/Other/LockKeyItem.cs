using System.Threading;

public sealed class LockKeyItem 
{
    public string LockId { get; }
    public bool InUse { get; set; }
    public SemaphoreSlim Semaphore { get; }

    public LockKeyItem(bool inUse, string lockId, SemaphoreSlim semaphore)
    {
        InUse = inUse;
        LockId = lockId;
        Semaphore = semaphore;
    }
}
