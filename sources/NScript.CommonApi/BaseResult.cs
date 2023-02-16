using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NScript.CommonApi
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
    [JsonSerializable(typeof(BaseResult))]
    internal partial class SerializeOnlyContext : JsonSerializerContext
    {
    }

    public class BaseResult
    {
        public Error code { get; set; }
        public String? message { get; set; }

        public static BaseResult CreateError(Error err, String errMsg = null)
        {
            BaseResult r = new() { code = err };
            if (err != Error.Success)
            {
                if (errMsg != null) r.message = errMsg;
                else r.message = err.ToString();
            }
            return r;
        }

        public static String CreateErrorJsonString(Error err, String errMsg = null)
        {
            BaseResult r = CreateError(err, errMsg);
            return JsonSerializer.Serialize<BaseResult>(r, SerializeOnlyContext.Default.BaseResult);
        }
    }
}
