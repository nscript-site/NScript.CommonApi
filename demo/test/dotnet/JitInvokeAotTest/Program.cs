using Common;

namespace JitInvokeAotTest;

internal class Program
{
    static void Main(string[] args)
    {
        // 运行测试前，需要先运行 NScript.CommonApi.Demo.NativeLibrary 项目下的 publish.ps1 脚本，生成 nativeaot dll 文件
        Console.WriteLine("Jit 调用 NScript.CommonApi.Demo.NativeLibrary 库通过 NativeAot 生成的 dll 测试");
        TestCase.Run();
        Console.ReadKey();
    }
}
