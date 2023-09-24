namespace HamedStack.DataShaping;

/// <summary>
/// Represents a structured set of data, organized as a list of lists of <see cref="DataField"/> objects.
/// </summary>
[Serializable]
public class ShapedData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShapedData"/> class.
    /// </summary>
    public ShapedData()
    {
        Values = new List<List<DataField>>();
    }

    /// <summary>
    /// Gets the collection of values represented as a list of lists of <see cref="DataField"/> objects.
    /// </summary>
    public List<List<DataField>> Values { get; }
}