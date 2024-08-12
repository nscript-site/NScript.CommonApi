using NScript.CommonApi.Demo.NativeLibrary.Handlers;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace NScript.CommonApi.Demo.NativeLibrary;

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
    [UnmanagedCallersOnly(EntryPoint = "commonapi_demo_api")]
    public unsafe static IntPtr Handle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return Instance.Value.HandleApi(pRoute, pJsonParams, pDataPayload, payloadLength);
    }

    public static IntPtr JitHandle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return Instance.Value.HandleApi(pRoute, pJsonParams, pDataPayload, payloadLength);
    }

    public static Api GetInstance()
    {
        return Instance.Value;
    }
}