// Intentum Example: Resilience Patterns Demo
// Run: dotnet run --project examples/resilience-demo
// Shows CircuitBreaker, Retry Policy, and Bulkhead patterns.

using Intentum.Runtime.Resilience.Bulkhead;
using Intentum.Runtime.Resilience.CircuitBreaker;
using Intentum.Runtime.Resilience.Retry;

Console.WriteLine("=== Resilience Patterns Demo ===\n");

// 1. Circuit Breaker
Console.WriteLine("--- Circuit Breaker ---");
var cb = new MemoryCircuitBreaker(new CircuitBreakerOptions(
    FailureThreshold: 2,
    DurationOfBreak: TimeSpan.FromSeconds(3)));

for (int i = 0; i < 5; i++)
{
    try
    {
        var result = await cb.ExecuteAsync<int>(() =>
            throw new InvalidOperationException($"Fail #{i + 1}"));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"State: {cb.State}, Exception: {ex.GetType().Name}");
    }
}

await Task.Delay(3500);
Console.WriteLine($"After wait: State = {cb.State}");

try
{
    var result = await cb.ExecuteAsync(() => Task.FromResult(42));
    Console.WriteLine($"Success after recovery: {result}, State: {cb.State}");
}
catch (Exception ex)
{
    Console.WriteLine($"Still failing: {ex.Message}");
}

// 2. Retry Policy
Console.WriteLine("\n--- Retry Policy ---");
var retry = new MemoryRetryPolicy(new RetryOptions(
    MaxRetries: 3,
    BaseDelay: TimeSpan.FromMilliseconds(50),
    Backoff: RetryBackoffType.Exponential));

var attempts = 0;
try
{
    var result = await retry.ExecuteAsync<int>(_ =>
    {
        attempts++;
        return attempts < 3
            ? throw new InvalidOperationException("Transient")
            : Task.FromResult(42);
    });
    Console.WriteLine($"Retry succeeded after {attempts} attempts, result: {result}");
}
catch (Exception ex)
{
    Console.WriteLine($"Retry failed: {ex.Message}");
}

// 3. Bulkhead
Console.WriteLine("\n--- Bulkhead ---");
var bulkhead = new MemoryBulkhead(new BulkheadOptions(
    MaxParallelization: 2,
    QueueTimeout: TimeSpan.FromSeconds(3)));

var tasks = new List<Task<int>>();
for (int i = 0; i < 4; i++)
{
    var index = i;
    tasks.Add(bulkhead.ExecuteAsync(async () =>
    {
        await Task.Delay(500);
        return index;
    }));
}

var results = await Task.WhenAll(tasks);
Console.WriteLine($"Bulkhead completed: {results.Length} tasks, results: [{string.Join(", ", results)}]");

Console.WriteLine("\n=== Demo Complete ===");
