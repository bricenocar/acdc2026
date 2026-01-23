using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftFunctions.Helpers
{
    public class JsonHelper
    {
        public static int GetInt(JObject obj, string field)
        {
            return obj.TryGetValue(field, out JToken? token) &&
                   token != null &&
                   token.Type != JTokenType.Null
                ? token.Value<int>()
                : 0;
        }

    }
}
