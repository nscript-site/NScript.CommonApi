# NScript.CommonApi

## 介绍

NScript.CommonApi 提供了一套使用 csharp 的 NativeAOT 开发动态链接库，提供基于路由、json格式输入输出的 api 机制，使得动态链接库的调用体验接近于 webapi。

使用 demo：

```csharp

using System.Runtime.InteropServices;
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

    public static Api GetInstance()
    {
        return Instance.Value;
    }
}
```

上例中，通过 api.Map 可以注册路由及对应的 ApiHandler。ApiHandler 分为两类，TypedApiHandler 和 TypedPayloadApiHandler。TypedPayloadApiHandler 的输入，比 TypedApiHandler 多了个 Payload。对于序列化和反序列化代价较大的参数，可以通过 Payload 来传进来。上例中，分别实现了一个 TypedApiHandler 和一个 TypedPayloadApiHandler。

有三种模式，来调用上例中的 API，下面逐一说明。

## 模式1：通过动态链接库来调用

将将上例通过 NativeAOT 编译成动态链接库，供上层各个语言来调用。

每种语言，只需要封装一次，即可实现统一的调用方式。

其中，csharp 版的封装为 [NScript.CommonApi.Wrapper](https://www.nuget.org/packages/NScript.CommonApi.Wrapper)。如此一来，底层可以用 Native AOT 编译，来防止反编译。UI 层，可以用 jit 模式运行，来提高应用的灵活度。

如果是使用其他语言来调用，需要参考 [NScript.CommonApi.Wrapper的实现](https://github.com/nscript-site/NScript.CommonApi/tree/main/wrappers/dotnet/NScript.CommonApi.Wrapper) 自行封装。

使用 NScript.CommonApi.Wrapper 来调用示例如下：

```csharp
using System.Runtime.InteropServices;

namespace NScript.CommonApi.WrapperTest;

public class EchoInput
{
    public String? message { get; set; }
}

public class EchoOutput : BaseResult
{
    public String? echo { get; set; }
    public int sum { get; set; }
}

public class DemoApiWrapper : ApiWrapper
{
    [DllImport("NScript.CommonApi.SdkDemo.dll")]
    static extern IntPtr sdk_demo_api(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength);

    protected override IntPtr InvokeApi(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return sdk_demo_api(pRoute, pJsonParams, pDataPayload, payloadLength);
    }
}

```

Program.cs 代码如下:

```csharp
using NScript.CommonApi.WrapperTest;
using System.Text.Json;

var wrapper = new DemoApiWrapper();
EchoOutput output = wrapper.Invoke<EchoInput,EchoOutput>("echo", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));

var bytes = new byte[1024];
for (int i = 0; i < bytes.Length; i++)
{
    bytes[i] = (byte)(i % 256);
}

output = wrapper.Invoke<EchoInput, EchoOutput>("echo-payload", new EchoInput() { message = "hello world with payload!" }, bytes);
Console.WriteLine(JsonSerializer.Serialize(output));

output = wrapper.Invoke<EchoInput, EchoOutput>("invalid-route", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));
Console.ReadKey();
```

运行一下:

```bash
{"echo":"hello world!","sum":0,"code":0,"message":null}
{"echo":"hello world with payload!, payload: 1024 bytes","sum":130560,"code":0,"message":null}
{"echo":null,"sum":0,"code":-12,"message":"InvalidRoute"}
```

## 模式2：csharp 下直接调用

有时候，不需要通过动态链接库来调用。比如，底层和应用层都使用 csharp 来开发。底层和应用层以相同的模式运行，比如，皆是 jit 模式，或者，皆是 AOT 模式（例如，iOS 应用）。这时，通过 json 来传值，显得有些多余。可以直接进行强类型的传值，示例如下：

```csharp
using NScript.CommonApi.SdkDemo;
using System.Text.Json;

void TestApi()
{
    var api = Api.GetInstance();
    EchoOutput output = api.Invoke<EchoInput, EchoOutput>("echo", new EchoInput() { message = "hello world!" });
    Console.WriteLine(JsonSerializer.Serialize(output));

    var bytes = new byte[1024];
    for (int i = 0; i < bytes.Length; i++)
    {
        bytes[i] = (byte)(i % 256);
    }

    output = api.Invoke<EchoInput, EchoOutput>("echo-payload", new EchoInput() { message = "hello world with payload!" }, bytes!);
    Console.WriteLine(JsonSerializer.Serialize(output));
}

TestApi();

```

## 模式3：调试动态链接库

在模式1中，直接调试动态链接库会比较麻烦。如果应用层语言也是 csharp，提供了个 jit handle，使用 json 传值模式，以 jit 模式运行底层模块，方便各种调试。示例代码如下（需要添加对 NScript.CommonApi.SdkDemo 的引用）：

```csharp
using System.Runtime.InteropServices;

namespace NScript.CommonApi.WrapperJitTest;

public class EchoInput
{
    public String? message { get; set; }
}

public class EchoOutput : BaseResult
{
    public String? echo { get; set; }
    public int sum { get; set; }
}

public class DemoApiWrapper : ApiWrapper
{
    [DllImport("NScript.CommonApi.SdkDemo.dll")]
    static extern IntPtr sdk_demo_api(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength);

    protected override IntPtr InvokeApi(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return sdk_demo_api(pRoute, pJsonParams, pDataPayload, payloadLength);
    }
}
```

Program.cs 代码如下:

```csharp
using NScript.CommonApi.WrapperJitTest;
using System.Text.Json;

var wrapper = new DemoApiWrapper();

// 设置 jit hook。这样一来，所有的 api 调用都会走 jit hook，而不是真正的 pinvoke 调用。
wrapper.SetJitHook(NScript.CommonApi.SdkDemo.Api.JitHandle);

EchoOutput output = wrapper.Invoke<EchoInput, EchoOutput>("echo", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));

var bytes = new byte[1024];
for (int i = 0; i < bytes.Length; i++)
{
    bytes[i] = (byte)(i % 256);
}

output = wrapper.Invoke<EchoInput, EchoOutput>("echo-payload", new EchoInput() { message = "hello world with payload!" }, bytes);
Console.WriteLine(JsonSerializer.Serialize(output));


output = wrapper.Invoke<EchoInput, EchoOutput>("invalid-route", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));
Console.ReadKey();

```

运行一下:

```bash
{"echo":"hello world!","sum":0,"code":0,"message":null}
{"echo":"hello world with payload!, payload: 1024 bytes","sum":130560,"code":0,"message":null}
{"echo":null,"sum":0,"code":-12,"message":"InvalidRoute"}
```