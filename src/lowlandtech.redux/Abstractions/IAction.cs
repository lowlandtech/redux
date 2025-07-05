namespace LowlandTech.Redux.Abstractions;

/// <summary>
/// Represents an action with associated metadata.
/// </summary>
/// <remarks>This interface defines a contract for actions that include a collection of metadata. The metadata is
/// represented as a read-only dictionary of key-value pairs, which can be used to store additional contextual
/// information about the action, such as configuration settings, annotations, or other relevant data.</remarks>
public interface IAction
{
    /// <summary>
    /// Gets a collection of key-value pairs that provide additional metadata about the object.
    /// </summary>
    /// <remarks>The metadata can include custom information relevant to the object, such as configuration
    /// settings, annotations, or other contextual data.  The dictionary is read-only, and its contents cannot be
    /// modified directly.</remarks>
    IDictionary<string, object> Metadata { get; }
}