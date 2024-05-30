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

