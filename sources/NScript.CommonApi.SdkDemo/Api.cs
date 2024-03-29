﻿using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NScript.CommonApi.SdkDemo;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(EchoInput))]
[JsonSerializable(typeof(EchoOutput))]
internal partial class EchoSerializeOnlyContext : JsonSerializerContext
{
}

public class EchoInput
{
    public String? message { get; set; }
}

public class EchoOutput : BaseResult
{
    public String? echo { get; set; }
    public int sum { get; set; }
}

public class EchoApiHandler : TypedApiHandler<EchoInput, EchoOutput>
{
    protected override EchoOutput? Handle(EchoInput? input)
    {
        if (input == null) return null;
        EchoOutput output = new EchoOutput();
        var msg = input.message ?? String.Empty;
        output.echo = $"{msg}";
        return output;
    }

    protected override (JsonTypeInfo<EchoInput>, JsonTypeInfo<EchoOutput>) GetTypeInfos()
    {
        return (EchoSerializeOnlyContext.Default.EchoInput, EchoSerializeOnlyContext.Default.EchoOutput);
    }
}

public class EchoPayloadApiHandler : TypedPayloadApiHandler<EchoInput, EchoOutput>
{
    protected override EchoOutput? Handle(EchoInput? input, Payload payload)
    {
        if (input == null) return null;
        EchoOutput output = new EchoOutput();
        var msg = input.message ?? String.Empty;
        output.echo = $"{msg}, payload: {payload.Length} bytes";
        var bytes = payload.ToArray();
        for (int i = 0; i < bytes.Length; i++)
        {
            output.sum += bytes[i];
        }
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
        // api.Map("your-route1", new YourRoute1ApiHandler());
        // api.Map("your-route2", new YourRoute2ApiHandler());
        api.Map("echo", new EchoApiHandler());
        api.Map("echo-payload", new EchoPayloadApiHandler());
        return api;
    });

    // 可以修改 EntryPoint 为其它名字
    [UnmanagedCallersOnly(EntryPoint = "sdk_demo_api")]
    public unsafe static IntPtr Handle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return Instance.Value.HandleApi(pRoute, pJsonParams, pDataPayload, payloadLength);
    }

    public static IntPtr JitHandle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return Instance.Value.HandleApi(pRoute, pJsonParams, pDataPayload, payloadLength);
    }
}