using Intentum.Runtime.Resilience.Exceptions;

namespace Intentum.Runtime.Resilience.CircuitBreaker;

public sealed class MemoryCircuitBreaker : ICircuitBreaker
{
    private readonly CircuitBreakerOptions _options;
    private readonly object _lock = new();
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private int _halfOpenAttempts;
    private DateTime _lastFailureTime;

    public MemoryCircuitBreaker(CircuitBreakerOptions options)
    {
        _options = options;
    }

    public CircuitState State
    {
        get
        {
            CheckHalfOpenTransition();
            return _state;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        CheckHalfOpenTransition();

        if (_state == CircuitState.Open)
            throw new CircuitBreakerOpenException();

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch
        {
            OnFailure();
            throw;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _state = CircuitState.Closed;
            _failureCount = 0;
        }
    }

    private void OnSuccess()
    {
        lock (_lock)
        {
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
                _halfOpenAttempts = 0;
            }
        }
    }

    private void OnFailure()
    {
        lock (_lock)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_state == CircuitState.HalfOpen)
            {
                _halfOpenAttempts++;
                if (_halfOpenAttempts >= _options.HalfOpenMaxAttempts)
                    _state = CircuitState.Open;
            }
            else if (_failureCount >= _options.FailureThreshold)
            {
                _state = CircuitState.Open;
            }
        }
    }

    private void CheckHalfOpenTransition()
    {
        if (_state != CircuitState.Open) return;

        lock (_lock)
        {
            if (_state == CircuitState.Open &&
                DateTime.UtcNow - _lastFailureTime >= _options.DurationOfBreak)
            {
                _state = CircuitState.HalfOpen;
            }
        }
    }
}
