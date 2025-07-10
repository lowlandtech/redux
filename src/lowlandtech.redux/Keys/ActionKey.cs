namespace LowlandTech.Redux.Keys;

/// <summary>
/// Represents a unique key for identifying an action, consisting of a plugin identifier and an action name.
/// </summary>
/// <param name="PluginId">The plugin's deterministic Guid identifier.</param>
/// <param name="ActionName">The logical action name (e.g., "MarkOrderAsProcessed").</param>
public readonly record struct ActionKey(Guid PluginId, string ActionName)
{
    /// <summary>
    /// Returns a string representation of the current ActionKey.
    /// </summary>
    /// <example>
    /// "4e54ed77-3750-48a0-ace6-20c4c7b3d2dc|MarkOrderAsProcessed"
    /// </example>
    public override string ToString() => $"{PluginId}|{ActionName}";

    /// <summary>
    /// Parses a raw string into an ActionKey.
    /// </summary>
    /// <param name="raw">The string to parse, containing two components separated by a pipe.</param>
    public static ActionKey Parse(string raw)
    {
        var parts = raw.Split('|');
        if (parts.Length != 2)
            throw new FormatException("Invalid ActionKey format. Expected 'PluginId|ActionName'.");

        return new ActionKey(
            Guid.Parse(parts[0]),
            parts[1]);
    }

    /// <summary>
    /// Creates an <see cref="ActionKey"/> instance for the specified action type and plugin identifier.
    /// </summary>
    /// <typeparam name="TAction">The type of the action for which the key is being created. The name of this type will be used as the action
    /// name.</typeparam>
    /// <param name="pluginIdString">The string representation of the plugin's unique identifier. Must be a valid GUID.</param>
    /// <returns>An <see cref="ActionKey"/> representing the specified action and plugin.</returns>
    public static ActionKey For<TAction>(string pluginIdString)
    {
        var pluginId = Guid.Parse(pluginIdString);
        var actionName = typeof(TAction).Name;
        return new ActionKey(pluginId, actionName);
    }

    /// <summary>
    /// Gets the unique identifier for the action, generated as a version 5 UUID based on the action name.
    /// </summary>
    public Guid Id => ActionKeyExtensions.ToGuidv5(PluginId, ActionName);

    public static ValueConverter<ActionKey, string> GetValueConverter()
    {
        return new ValueConverter<ActionKey, string>(
            v => v.ToString(),
            v => Parse(v)
        );
    }
}