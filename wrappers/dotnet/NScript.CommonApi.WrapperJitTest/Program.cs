using NScript.CommonApi.WrapperJitTest;
using System.Text.Json;

var wrapper = new DemoApiWrapper();
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
