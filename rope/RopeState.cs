/// <summary>
/// Represents the possible states of the rope.
/// </summary>
public enum RopeState
{
    /// <summary>
    /// The rope is hidden.
    /// </summary>
    Hidden,

    /// <summary>
    /// The hook is being shot.
    /// </summary>
    Shot,

    /// <summary>
    /// The hook is attached to something.
    /// </summary>
    Hooked,

    /// <summary>
    /// The rope is retracting.
    /// </summary>
    Retracting,

    /// <summary>
    /// The rope is slack.
    /// </summary>
    Slack
}
