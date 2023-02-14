using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace NScript.CommonApi
{
    public abstract class ApiHandler
    {
        public abstract String Handle(String jsonParams);
    }

    public abstract class TypedApiHandler<TInput,TOutput> : ApiHandler where TOutput:BaseResult,new()
    {
        protected abstract ValueTuple<JsonTypeInfo<TInput>, JsonTypeInfo<TOutput>> GetTypeInfos();

        public override string Handle(string jsonParams)
        {
            String outputStr = null;
            String err = null;
            var pair = GetTypeInfos();
            try
            {
                TInput? input = JsonSerializer.Deserialize<TInput>(jsonParams, pair.Item1);
                TOutput? output = Handle(input);
                outputStr = JsonSerializer.Serialize<TOutput>(output, pair.Item2);
            }
            catch(JsonException ex)
            {
                err = BaseResult.CreateErrorJsonString(Error.InvalidInput);
            }
            catch(Exception ex)
            {
                err = BaseResult.CreateErrorJsonString(Error.InternalError, ex.Message + Environment.NewLine + ex.StackTrace);
            }

            if (err != null) return err;
            else return outputStr;
        }

        protected abstract TOutput? Handle(TInput? inpit);
    }
}
