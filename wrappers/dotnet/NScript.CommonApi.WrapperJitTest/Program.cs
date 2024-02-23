using NScript.CommonApi.WrapperJitTest;
using System.Text.Json;

var wrapper = new DemoApiWrapper();
wrapper.SetJitHook(NScript.CommonApi.SdkDemo.Api.JitHandle);

EchoOutput output = wrapper.Invoke<EchoInput, EchoOutput>("echo", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));

output = wrapper.Invoke<EchoInput, EchoOutput>("echo-payload", new EchoInput() { message = "hello world with payload!" }, new byte[1024]);
Console.WriteLine(JsonSerializer.Serialize(output));

output = wrapper.Invoke<EchoInput, EchoOutput>("invalid-route", new EchoInput() { message = "hello world!" });
Console.WriteLine(JsonSerializer.Serialize(output));
Console.ReadKey();
