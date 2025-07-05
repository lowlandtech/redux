namespace LowlandTech.Redux.Actions;

/// <summary>
/// Represents an action to increment a counter by a specified amount.
/// </summary>
/// <remarks>This action is typically used in scenarios where a counter value needs to be adjusted by a specific
/// amount. The <see cref="Amount"/> property specifies the value by which the counter should be incremented.</remarks>
/// <param name="Amount"></param>
public record IncrementCounterAction(int Amount, IDictionary<string, object>? Metadata = null) : IAction 
{
    /// <summary>
    /// Gets a collection of metadata key-value pairs associated with the object.
    /// </summary>
    /// <remarks>The metadata can be used to store additional information about the object, such as custom
    /// attributes or contextual data. Keys are case-sensitive and must be unique within the dictionary.</remarks>
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
}