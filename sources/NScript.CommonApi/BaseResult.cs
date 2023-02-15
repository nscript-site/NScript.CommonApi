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
        public Error Code { get; set; }
        public String Message { get; set; }

        public static BaseResult CreateError(Error err, String errMsg = null)
        {
            BaseResult r = new() { Code = err };
            if (err != Error.Success)
            {
                if (errMsg != null) r.Message = errMsg;
                else r.Message = err.ToString();
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
