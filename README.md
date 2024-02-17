# NScript.CommonApi

## 简介

NativeAOT 推出后，csharp 可以方便的撰写各种库或sdk，暴露 cstyle api，供其它语言调用。它的优势是，开发速度快，运行速度也不弱，生成的动态链接库的尺寸也不大，很适合应用软件的底层开发。

然而，cstyle api 调用很不方便，为了简化 api 的提供和调用，CommonApi 的目的是制定一套 NativeAOT 开发 api 的规范，并提供相关封装，使得sdk的开发者，可以用类似开发 webapi 的方式，来提供 sdk api。调用层，也可以用类似 webapi 的方式，来进行调用。

这样一来，对于上层语言，我们只需要针对该语言，编写一套公共调用库和文档即可，而不需要，每一个新项目，每一个新接口，都需要独立的文档及示例代码。

## 约定

### 底层接口

Sdk 开发者，只对外公开一个接口，所有上层语言，对sdk的调用，均转发到该接口执行：
```c
char* your_api_name(char* route, char* pJsonParams, void* pPayload, int payloadLength)
```
在 csharp 侧，该接口表现为：
```csharp
[UnmanagedCallersOnly(EntryPoint = "your_api_name")]
public static IntPtr Handle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pPayload, int payloadLength)
```
约定：
- 对 api 的调用，需要传入路由字符串和 json 格式的入参，以 json 格式返回结果。
- 对于 json 序列化成本较高的数据，可通过 payload 直接传入，以减少序列化和反序列化的计算成本
- 输入参数中字符串、payload 所在的内存，由调用方负责管理。输出字符串虽然由底层库产生，但也由调用方管理。

### Sdk 开发

NScript.CommonApi 对 api 的提供进行了封装，可通过 nuget 安装。NScript.CommonApi 提供了 BaseApi、TypedApiHandler、BaseResult 三个基类。基于这些基类，程序员即可以开发 webapi 的类似体验，进行 sdk 开发。

下面是一个开发示例。Sdk 的整体代码框架如下：

```csharp
public class Api : BaseApi
{
    static Lazy<Api> Instance = new Lazy<Api>(() => {
        var api = new Api();
        // 注册 ApiHandler 到指定路由
        // api.Map("your-route1", new YourRoute1ApiHandler());
        // api.Map("your-route2", new YourRoute2ApiHandler());
        // ...
        return api;
    });

    // 可以修改 EntryPoint 为其它名字
    [UnmanagedCallersOnly(EntryPoint = "sdk_demo_api")]
    public unsafe static IntPtr Handle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return Instance.Value.HandleApi(pRoute, pJsonParams, pDataPayload, payloadLength);
    }
}
```

在 Api 类里注册路由，进来的调用，将通过路由，找到对应的 ApiHandler，进行处理。

我们试着添加一个 `EchoApiHandler`，这个 ApiHandler 实现的逻辑是，接收一个 EchoInput 输入，产生一个 EchoOutput 输出，输出携带 EchoInput 的 message 以及 payload 信息 ：

```csharp
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
}

public class EchoApiHandler : TypedApiHandler<EchoInput, EchoOutput>
{
    protected override EchoOutput? Handle(EchoInput? input, Payload payload)
    {
        if (input == null) return null;
        EchoOutput output = new EchoOutput();
        var msg = input.message ?? String.Empty;
        output.echo = $"{msg}, payload: {payload.Length} bytes";
        return output;
    }

    protected override (JsonTypeInfo<EchoInput>, JsonTypeInfo<EchoOutput>) GetTypeInfos()
    {
        return (EchoSerializeOnlyContext.Default.EchoInput, EchoSerializeOnlyContext.Default.EchoOutput);
    }
}
```

这里注意，上面的 JsonSourceGenerationOptions 部分代码很重要，因为 NativeAOT对反射支持的不好，这里通过 dotnet 的 SourceGeneration 特性，对NativeAOT下相关类型的序列化和反序列化提供支持。

完整的代码如下:

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
}

public class EchoApiHandler : TypedApiHandler<EchoInput, EchoOutput>
{
    protected override EchoOutput? Handle(EchoInput? input, Payload payload)
    {
        if (input == null) return null;
        EchoOutput output = new EchoOutput();
        var msg = input.message ?? String.Empty;
        output.echo = $"{msg}, payload: {payload.Length} bytes";
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
        return api;
    });

    // 可以修改 EntryPoint 为其它名字
    [UnmanagedCallersOnly(EntryPoint = "sdk_demo_api")]
    public unsafe static IntPtr Handle(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        return Instance.Value.HandleApi(pRoute, pJsonParams, pDataPayload, payloadLength);
    }
}
```

以 NativeAOT 模式 publish，得到 dll，这个 dll 我们暂且命名为 NScript.CommonApi.SdkDemo.dll，大小为 3.6M。

### sdk 调用

每个语言可以对 api 进行自己的封装。这里以 csharp 为例子。NScript.CommonApi.Wrapper 是我进行的 csharp 版本的封装，可通过 nuget 安装。调用时，只需要简单的继承下 ApiWrapper，设置好 dllimport，做个转发即可：

```chsarp

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

把 Sdk 里的实体类复制过来：

```csharp
public class EchoInput
{
    public String? message { get; set; }
}

public class EchoOutput : BaseResult
{
    public String? echo { get; set; }
}
```

开始调用：

```csharp
var wrapper = new DemoApiWrapper();
EchoOutput output = wrapper.Invoke<EchoInput,EchoOutput>("echo", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));

output = wrapper.Invoke<EchoInput, EchoOutput>("echo", new EchoInput() { message = "hello world with payload!" }, new byte[1024]);
Console.WriteLine(JsonSerializer.Serialize(output));

output = wrapper.Invoke<EchoInput, EchoOutput>("invalid-route", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));
```

输出：

```json
{"echo":"hello world!, payload: 0 bytes","code":0,"message":null}
{"echo":"hello world with payload!, payload: 1024 bytes","code":0,"message":null}
{"echo":null,"code":-12,"message":"InvalidRoute"}
```