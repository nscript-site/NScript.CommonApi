using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace NScript.CommonApi
{
    public class ApiSetting
    {
        public static bool Debug { get; set; }
    }

    public abstract class ApiHandler
    {
        public abstract String Handle(String jsonParams, Payload payload);
    }

    public abstract class TypedApiHandler<TInput,TOutput> : ApiHandler where TOutput:BaseResult,new()
    {
        protected abstract ValueTuple<JsonTypeInfo<TInput>, JsonTypeInfo<TOutput>> GetTypeInfos();

        public override string Handle(string jsonParams, Payload payload)
        {
            String outputStr = null;
            String err = null;
            var pair = GetTypeInfos();
            try
            {
                TInput? input = JsonSerializer.Deserialize<TInput>(jsonParams, pair.Item1);
                TOutput? output = Handle(input, payload);
                outputStr = JsonSerializer.Serialize<TOutput>(output, pair.Item2);
            }
            catch(JsonException ex)
            {
                err = BaseResult.CreateErrorJsonString(Error.InvalidInput);
            }
            catch(Exception ex)
            {
                String errMsg = ApiSetting.Debug == true ? (ex.Message + Environment.NewLine + ex.StackTrace) : (ex.Message);
                err = BaseResult.CreateErrorJsonString(Error.InternalError, ex.Message + Environment.NewLine + ex.StackTrace);
            }

            if (err != null) return err;
            else return outputStr;
        }

        /// <summary>
        /// 处理输入，返回输出
        /// </summary>
        /// <param name="input">输入。input 将会序列化为 json 传输到 common api。</param>
        /// <param name="payload">输入的二进制负载。对于 json 序列化代价比较大的数据，可以通过 payload 直接内存传输。</param>
        /// <returns></returns>
        protected abstract TOutput? Handle(TInput? input, Payload payload);
    }
}
