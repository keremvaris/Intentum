namespace Intentum.Runtime.Resilience.Exceptions;

public sealed class BulkheadFullException : InvalidOperationException
{
    public BulkheadFullException()
        : base("Bulkhead is full. No more operations can be queued.") { }
}
