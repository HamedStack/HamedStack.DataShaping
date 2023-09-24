namespace HamedStack.DataShaping;

/// <summary>
/// Represents a single data field with a key and a value.
/// </summary>
[Serializable]
public class DataField
{
    /// <summary>
    /// Gets or sets the key associated with this data field.
    /// </summary>
    /// <value>The key for this data field, or null if no key is set.</value>
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets the value associated with this data field.
    /// </summary>
    /// <value>The value for this data field, or null if no value is set.</value>
    public object? Value { get; set; }
}