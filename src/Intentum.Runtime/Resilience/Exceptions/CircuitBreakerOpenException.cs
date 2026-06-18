namespace Intentum.Runtime.Resilience.Exceptions;

public sealed class CircuitBreakerOpenException : InvalidOperationException
{
    public CircuitBreakerOpenException()
        : base("Circuit breaker is open. Operation cannot be executed.") { }

    public CircuitBreakerOpenException(string message) : base(message) { }
}
