using Common;
using NScript.CommonApi.Demo.NativeLibrary;
using NScript.CommonApi.Demo.NativeLibrary.Sdk;

namespace JitInvokeJitTest;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("[Jit 调用 NScript.CommonApi.Demo.NativeLibrary 库测试]");
        MyLib.SetJitHandle(Api.JitHandle);  // 调用前，需要设置 jit handle
        TestCase.Run();
        Console.ReadKey();
    }
}
