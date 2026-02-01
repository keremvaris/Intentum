namespace Intentum.Versioning;

/// <summary>
/// Tracks policy versions for rollback and comparison.
/// </summary>
public sealed class PolicyVersionTracker
{
    private readonly List<IVersionedPolicy> _versions = [];
    private int _currentIndex = -1;

    /// <summary>
    /// Registers a versioned policy. Latest is used as current.
    /// </summary>
    public PolicyVersionTracker Add(IVersionedPolicy versionedPolicy)
    {
        _versions.Add(versionedPolicy ?? throw new ArgumentNullException(nameof(versionedPolicy)));
        _currentIndex = _versions.Count - 1;
        return this;
    }

    /// <summary>
    /// Gets the current (active) versioned policy.
    /// </summary>
    public IVersionedPolicy? Current => _currentIndex >= 0 && _currentIndex < _versions.Count ? _versions[_currentIndex] : null;

    /// <summary>
    /// Gets all registered versions (oldest to newest).
    /// </summary>
    public IReadOnlyList<IVersionedPolicy> Versions => _versions;

    /// <summary>
    /// Rolls back to the previous version. Returns true if rollback was performed.
    /// </summary>
    public bool Rollback()
    {
        if (_currentIndex <= 0) return false;
        _currentIndex--;
        return true;
    }

    /// <summary>
    /// Rolls forward to the next version (after a rollback). Returns true if rollforward was performed.
    /// </summary>
    public bool Rollforward()
    {
        if (_currentIndex < 0 || _currentIndex >= _versions.Count - 1) return false;
        _currentIndex++;
        return true;
    }

    /// <summary>
    /// Sets the current version by index (0 = oldest).
    /// </summary>
    public void SetCurrent(int index)
    {
        if (index < 0 || index >= _versions.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _currentIndex = index;
    }

    /// <summary>
    /// Compares two version identifiers (e.g. semantic or string).
    /// </summary>
    public static int CompareVersions(string a, string b)
    {
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
