namespace LowlandTech.Redux.Abstractions;

/// <summary>
/// Represents a generic event in the system.
/// </summary>
/// <remarks>This interface serves as a marker for event types used in event-driven architectures. Implementing
/// this interface allows a class to be recognized as an event, enabling it to be processed by event handlers or
/// published to an event bus.</remarks>
public interface IEvent
{
    /// <summary>
    /// Gets a collection of key-value pairs that provide additional metadata about the object.
    /// </summary>
    /// <remarks>The metadata can include custom information relevant to the object, such as configuration
    /// settings, annotations, or other contextual data.  The dictionary is read-only, and its contents cannot be
    /// modified directly.</remarks>
    IDictionary<string, object>? Metadata { get; }
}
