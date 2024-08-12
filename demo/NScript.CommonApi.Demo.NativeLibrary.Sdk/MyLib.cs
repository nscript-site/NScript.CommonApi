using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NScript.CommonApi.Demo.NativeLibrary.Sdk;

public class MyLib
{
    public class MyLibWrapper : ApiWrapper
    {
        [DllImport("NScript.CommonApi.Demo.NativeLibrary")]
        static extern IntPtr commonapi_demo_api(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength);

        protected override IntPtr InvokeApi(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
        {
            return commonapi_demo_api(pRoute, pJsonParams, pDataPayload, payloadLength);
        }
    }

    static Lazy<MyLibWrapper> NativeApi = new Lazy<MyLibWrapper>(() =>
    {
        return new MyLibWrapper();
    });

    /// <summary>
    /// 如果想 jit 模式调用，需要设置 jit handle
    /// </summary>
    /// <param name="jitHandle"></param>
    public static void SetJitHandle(Func<IntPtr, IntPtr, IntPtr, int, IntPtr> jitHandle)
    {
        NativeApi.Value.SetJitHook(jitHandle);
    }

    public static TOutput Invoke<TInput, TOutput>(string route, TInput input, byte[]? payload = null) where TOutput : BaseResult, new()
    {
        return NativeApi.Value.Invoke<TInput, TOutput>(route, input, payload);
    }

    public static TOutput Invoke<TInput, TOutput>(string route, TInput input, IntPtr pPayload, int payloadLength) where TOutput : BaseResult, new()
    {
        return NativeApi.Value.Invoke<TInput, TOutput>(route, input, pPayload, payloadLength);
    }
}
