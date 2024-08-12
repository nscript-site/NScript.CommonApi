using NScript.CommonApi.Demo.NativeLibrary.Sdk;
using System.Text.Json;

namespace Common;

public class TestCase
{
    public static void Run()
    {
        EchoOutput output = MyLib.Invoke<EchoInput, EchoOutput>("echo", new EchoInput() { message = "hello world!" });
        Console.WriteLine(JsonSerializer.Serialize(output));

        var bytes = new byte[1024];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)(i % 256);
        }

        output = MyLib.Invoke<EchoInput, EchoOutput>("echo-payload", new EchoInput() { message = "hello world with payload!" }, bytes);
        Console.WriteLine(JsonSerializer.Serialize(output));

        output = MyLib.Invoke<EchoInput, EchoOutput>("invalid-route", new EchoInput() { message = "hello world!" });
        Console.WriteLine(JsonSerializer.Serialize(output));
    }
}
