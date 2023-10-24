using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management.Automation;

namespace PSSimpleConfig.Utilities;

public class PSOutputWrapper
{
    public PSObject? PsObject { get; set; }
    public object[]? Array { get; set; }
    public object? BasicType { get; set; }
    public OutputType?Type { get; set; }

    public enum OutputType
    {
        PSObject,
        Array,
        BasicType
    }
}

/// <summary>
/// Utility class providing methods to convert JSON to PSObjects, Dictionaries,
/// and Arrays. Generally this is called from Get-PSSConfig, and operates with
/// the following requirements:
///
///     - If the target is a JSON Object, then it should be converted to a PSObject.
///     - If the target is a JSON Array, then it should be converted to an Array
///     - If the target is a string/number/null/bool, it should be returned as
///       the corresponding C# type
/// </summary>

public static class JsonConversion
{
    public static PSOutputWrapper ToOutput(JToken token)
    {
        PSOutputWrapper output = new();
        if (token is JObject @object)
        {
            output.PsObject = ToPSObject(@object);
            output.Type = PSOutputWrapper.OutputType.PSObject;
            return output;
        }
        else if (token is JArray array)
        {
            output.Array = ToArray(array);
            output.Type = PSOutputWrapper.OutputType.Array;
            return output;
        }
        else
        {
            output.BasicType = token.ToObject<object>();
            output.Type = PSOutputWrapper.OutputType.BasicType;
            return output;
        }
    }

    /// <summary>
    /// Converts a JObject to a PSObject.
    /// </summary>
    /// <param name="json">Newtonsoft.Json JObject</param>
    /// <returns>System.Management.Automation.PSObject</returns>
    public static PSObject ToPSObject(JObject json)
    {
        if (json == null) return null;

        PSObject output = new PSObject();

        foreach (var property in json.Properties())
        {
            var propertyName = property.Name;
            var value = property.Value;

            if (value == null)
            {
                output.Properties.Add(new PSNoteProperty(propertyName, null));
                continue;
            }

            if (value is JObject)
            {
                output.Properties.Add(new PSNoteProperty(propertyName, ToPSOutput((JObject)value)));
            }
            else if (value is JArray)
            {
                output.Properties.Add(new PSNoteProperty(propertyName, ToArray((JArray)value)));
            }
            else
            {
                output.Properties.Add(new PSNoteProperty(propertyName, value.ToObject<object>()));
            }
        }

        return output;
    }

    /// <summary>
    /// Converts a JToken to output that that is idiomatic to PowerShell.
    ///
    ///     - When converting a JObject, it will be converted to a PSObject.
    ///     - When converting a JArray, it will be converted to an Array
    ///     - When converting a string/number/null/bool, it will be returned as
    ///       an object.
    /// </summary>
    /// <param name="token">Newtonsoft.Json JToken</param>
    /// <returns>System.Object</returns>
    public static object ToPSOutput(JToken token)
    {
        if (token == null) return null;

        if (token is JObject)
        {
            return ToPSObject((JObject)token);
        }
        else if (token is JArray)
        {
            return ToArray((JArray)token);
        }
        else if (token.Type == JTokenType.String ||
                token.Type == JTokenType.Integer ||
                token.Type == JTokenType.Float ||
                token.Type == JTokenType.Boolean ||
                token.Type == JTokenType.Null)
        {
            return token.ToObject<object>();
        }
        else
        {
            return new PSObject(token.ToObject<object>());
        }
    }

    /// <summary>
    /// Converts a JObject to an object array.
    /// </summary>
    /// <param name="array">Newtonsoft.Json JObject</param>
    /// <returns>System.Object[]</returns>
    public static object[] ToArray(this JArray array)
    {
        return array.ToObject<object[]>().Select(ProcessArrayEntry).ToArray();
    }

    /// <summary>
    /// Converts a given array value to the appropriate output type.
    /// </summary>
    /// <param name="value">System.Object</param>
    /// <returns>System.Object</returns>
    private static object ProcessArrayEntry(object value)
    {
        if (value is JObject)
        {
            return ToPSObject((JObject)value);
        }
        if (value is JArray)
        {
            return ToArray((JArray)value);
        }
        return value;
    }
}