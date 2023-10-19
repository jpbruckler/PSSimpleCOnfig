using Newtonsoft.Json.Linq;
using System.Collections;

namespace PSSimpleConfig.Utilities;

class ConvertJToken
{
    public static object ConvertJTokenToPSObject(JToken token)
    {
        if (token is JObject jObject)
        {
            var table = new Hashtable();
            foreach (var property in jObject.Properties())
            {
                table[property.Name] = ConvertJTokenToPSObject(property.Value);
            }
            return table;
        }
        else if (token is JArray jArray)
        {
            var list = new List<object>();
            foreach (var element in jArray)
            {
                list.Add(ConvertJTokenToPSObject(element));
            }
            return list.ToArray();
        }
        else if (token is JValue jValue)
        {
            return jValue.Value;
        }
        else
        {
            return null;
        }
    }
}