// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Dynamic;
using System.Reflection;

namespace HamedStack.DataShaping;

/// <summary>
/// Provides extension methods for data shaping operations.
/// </summary>
public static class DataShapingExtensions
{
    /// <summary>
    /// Converts the given data object into a collection of <see cref="DataField"/> based on the specified fields.
    /// </summary>
    /// <typeparam name="T">The type of the data object.</typeparam>
    /// <param name="dataToShape">The data object to be shaped.</param>
    /// <param name="fields">A comma-separated list of property names to include in the result.</param>
    /// <param name="ignoreCase">Whether to ignore case when matching property names. Default is true.</param>
    /// <param name="converter">An optional converter function to transform property values. 
    /// Parameters of the converter function are:
    /// - T: The current instance of the data type being shaped.
    /// - string: The name of the property currently being processed.
    /// - object?: The value of the property currently being processed.
    /// The function should return the converted value as object?.</param>
    /// <returns>A collection of <see cref="DataField"/> containing the shaped data, or null if the source data is null.</returns>
    public static IEnumerable<DataField>? ToShapedData<T>(this T dataToShape, string fields, bool ignoreCase = true,
        Func<T, string, object?, object?>? converter = null)
    {
        var result = new Dictionary<string, object?>();
        if (dataToShape == null) return null;

        var propertyInfoList = GetPropertyInfos<T>(fields, ignoreCase);

        result.FillDictionary(dataToShape, propertyInfoList, converter);

        return result.Select(x => new DataField { Key = x.Key, Value = x.Value });
    }

    /// <summary>
    /// Converts the given collection of data objects into a <see cref="ShapedData"/> based on the specified fields.
    /// </summary>
    /// <typeparam name="T">The type of the data object in the collection.</typeparam>
    /// <param name="dataToShape">The collection of data objects to be shaped.</param>
    /// <param name="fields">A comma-separated list of property names to include in the result.</param>
    /// <param name="ignoreCase">Whether to ignore case when matching property names. Default is true.</param>
    /// <param name="converter">An optional converter function to transform property values.
    /// Parameters of the converter function are:
    /// - T: The current instance of the data type being shaped.
    /// - string: The name of the property currently being processed.
    /// - object?: The value of the property currently being processed.
    /// The function should return the converted value as object?.</param>
    /// <returns>A <see cref="ShapedData"/> containing the shaped data, or null if the source data collection is null.</returns>
    public static ShapedData? ToShapedData<T>(this IEnumerable<T>? dataToShape, string fields, bool ignoreCase = true,
        Func<T, string, object?, object>? converter = null)
    {
        if (dataToShape == null) return null;

        var propertyInfoList = GetPropertyInfos<T>(fields, ignoreCase);

        var list = dataToShape.Select(line =>
        {
            var data = new Dictionary<string, object?>();
            data.FillDictionary(line, propertyInfoList, converter);
            return data;
        }).Where(d => d.Keys.Count > 0).ToList();

        var dsh = new ShapedData();

        var values = new List<List<DataField>>();
        foreach (var item in list)
        {
            var records = item.Select(x => new DataField { Key = x.Key, Value = x.Value }).ToList();
            values.Add(records);
        }

        dsh.Values.AddRange(values);

        return dsh;
    }

    /// <summary>
    /// Converts a <see cref="ShapedData"/> object to a dynamic object.
    /// </summary>
    /// <param name="shapedData">The <see cref="ShapedData"/> object to convert.</param>
    /// <returns>A dynamic object representing the shaped data.</returns>
    public static dynamic? ToDynamic(this ShapedData? shapedData)
    {
        if (shapedData == null)
            return null;

        dynamic dynamicData = new ExpandoObject();
        var dynamicDict = (IDictionary<string, object?>)dynamicData;

        foreach (var record in shapedData.Values)
        {
            foreach (var field in record)
            {
                if (field.Key != null) dynamicDict[field.Key] = field.Value;
            }
        }

        return dynamicData;
    }
    
    /// <summary>
    /// Converts an <see cref="IEnumerable{DataField}"/> to a dynamic object.
    /// </summary>
    /// <param name="dataFields">The <see cref="IEnumerable{DataField}"/> to convert.</param>
    /// <returns>A dynamic object representing the data fields.</returns>
    public static dynamic? ToDynamic(this IEnumerable<DataField>? dataFields)
    {
        if (dataFields == null)
            return null;

        dynamic dynamicData = new ExpandoObject();
        var dynamicDict = (IDictionary<string, object?>)dynamicData;

        foreach (var dataField in dataFields)
        {
            if (dataField.Key != null) dynamicDict[dataField.Key] = dataField.Value;
        }

        return dynamicData;
    }
    
    /// <summary>
    /// Extracts selected properties' information based on the provided fields.
    /// </summary>
    /// <typeparam name="T">The type to inspect.</typeparam>
    /// <param name="fields">Comma-separated list of property names.</param>
    /// <param name="propertyInfoList">List to store extracted property information.</param>
    /// <param name="ignoreCase">Whether to ignore case when matching property names.</param>
    /// <returns>A collection of <see cref="PropertyInfo"/> matching the provided fields.</returns>
    private static IEnumerable<PropertyInfo> ExtractSelectedPropertiesInfo<T>(string fields,
                ICollection<PropertyInfo> propertyInfoList, bool ignoreCase)
    {
        var fieldsAfterSplit = fields.Split(',');

        foreach (var propertyName in fieldsAfterSplit.Select(f => f.Trim()))
        {
            var propName = ignoreCase ? propertyName.ToLower() : propertyName;
            var propertyInfo = typeof(T).GetRuntimeProperties()
                .FirstOrDefault(x => (ignoreCase ? x.Name.ToLower() : x.Name) == propName);

            if (propertyInfo == null) continue;

            propertyInfoList.Add(propertyInfo);
        }

        return propertyInfoList;
    }

    /// <summary>
    /// Fills a dictionary with the specified fields from the source data.
    /// </summary>
    /// <typeparam name="T">The type of the source data.</typeparam>
    /// <param name="dictionary">The dictionary to fill.</param>
    /// <param name="source">The source data.</param>
    /// <param name="fields">The properties to extract from the source.</param>
    /// <param name="converter">An optional converter function to transform property values.</param>
    private static void FillDictionary<T>(this IDictionary<string, object?> dictionary, T source,
        IEnumerable<PropertyInfo> fields, Func<T, string, object?, object?>? converter = null)
    {
        foreach (var propertyInfo in fields)
        {
            var propertyValue = propertyInfo.GetValue(source);

            var value = converter != null ? converter(source, propertyInfo.Name, propertyValue) : propertyValue;

            dictionary.Add(propertyInfo.Name, value);
        }
    }

    /// <summary>
    /// Retrieves properties' information from a type based on the provided fields.
    /// </summary>
    /// <typeparam name="T">The type to inspect.</typeparam>
    /// <param name="fields">Comma-separated list of property names.</param>
    /// <param name="ignoreCase">Whether to ignore case when matching property names.</param>
    /// <returns>A collection of <see cref="PropertyInfo"/> matching the provided fields.</returns>
    private static IEnumerable<PropertyInfo> GetPropertyInfos<T>(string fields, bool ignoreCase)
    {
        var propertyInfoList = new List<PropertyInfo>();

        if (!string.IsNullOrWhiteSpace(fields))
            return ExtractSelectedPropertiesInfo<T>(fields, propertyInfoList, ignoreCase);

        var propertyInfos = typeof(T).GetRuntimeProperties();
        propertyInfoList.AddRange(propertyInfos);
        return propertyInfoList;
    }
}