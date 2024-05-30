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
