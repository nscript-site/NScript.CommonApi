namespace NScript.CommonApi.Demo.NativeLibrary.Sdk;

public class EchoInput
{
    public String? message { get; set; }
}

public class EchoOutput : BaseResult
{
    public String? echo { get; set; }
    public int sum { get; set; }
}