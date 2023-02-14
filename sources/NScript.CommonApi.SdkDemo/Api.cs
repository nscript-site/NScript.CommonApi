using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace NScript.CommonApi.SdkDemo
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(EchoInput))]
    [JsonSerializable(typeof(EchoOutput))]
    internal partial class EchoSerializeOnlyContext : JsonSerializerContext
    {
    }

    public class EchoInput
    {
        public String Message { get; set; }
    }

    public class EchoOutput : BaseResult
    {
        public String Echo { get; set; }
    }

    public class EchoApiHandler : TypedApiHandler<EchoInput, EchoOutput>
    {
        protected override EchoOutput? Handle(EchoInput? input)
        {
            if (input == null) return null;
            EchoOutput output = new EchoOutput();
            output.Echo = input.Message;
            return output;
        }

        protected override (JsonTypeInfo<EchoInput>, JsonTypeInfo<EchoOutput>) GetTypeInfos()
        {
            return (EchoSerializeOnlyContext.Default.EchoInput, EchoSerializeOnlyContext.Default.EchoOutput);
        }
    }

    public class Api : BaseApi
    {
        static Lazy<Api> Instance = new Lazy<Api>(() => {
            var api = new Api();
            // 注册 ApiHandler 到指定路由
            api.Map("echo", new EchoApiHandler());
            return api;
        });

        // 可以修改 EntryPoint 为其它名字
        [UnmanagedCallersOnly(EntryPoint = "sdk_demo_api")]
        public static IntPtr Handle(IntPtr pRoute, IntPtr pJsonParams)
        {
            return Instance.Value.HandleApi(pRoute, pJsonParams);
        }
    }
}