using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace NScript.CommonApi;

public class BaseApi
{
    protected unsafe IntPtr HandleApi(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength)
    {
        if (pRoute == IntPtr.Zero || pJsonParams == IntPtr.Zero) return IntPtr.Zero;

        string jsonParams = Marshal.PtrToStringAnsi(pJsonParams)!;
        string route = Marshal.PtrToStringAnsi(pRoute)!;
        var payload = (pDataPayload == IntPtr.Zero || payloadLength <= 0) ? Payload.Empty : new Payload(pDataPayload, payloadLength);

        String result = HandleRoute(route, jsonParams, payload);
        if (result == null) return IntPtr.Zero;
        IntPtr pResult = Marshal.StringToHGlobalAnsi(result);
        return pResult;
    }

    protected Dictionary<String, IApiHandler> ApiHandlers { get; set; } = new();

    protected void Map(String route, IApiHandler handler)
    {
        if (route == null || handler == null) return;
        this.ApiHandlers[route]=handler;
    }

    protected String HandleRoute(String route, String jsonParams, Payload payload)
    {
        IApiHandler? match;
        this.ApiHandlers.TryGetValue(route, out match);

        if (match == null)
            return BaseResult.CreateErrorJsonString(Error.InvalidRoute);

        return match.Handle(jsonParams, payload);
    }

    /// <summary>
    /// 如果是 csharp，在相同模式下（调用方和被调用方同属于 jit 或 aot 编译）可以直接使用本方法来调用，避免序列化和反序列化的开销
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <param name="route"></param>
    /// <param name="input"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public TOutput Invoke<TInput,TOutput>(string route, TInput? input, Payload payload) where TOutput : BaseResult, new()
    {
        IApiHandler? match;
        this.ApiHandlers.TryGetValue(route, out match);

        if (match == null)
            throw new NotImplementedException("route not supported"); 

        var hander = match as ITypedApiHandler<TInput, TOutput>;
        if(hander == null)
            throw new NotImplementedException("route cannot match input/output type");

        return hander.Invoke(input, payload);
    }

    public unsafe TOutput Invoke<TInput, TOutput>(string route, TInput? input, byte[] payloadData) where TOutput : BaseResult, new()
    {
        fixed (byte* pData = payloadData)
        {
            return Invoke<TInput, TOutput>(route, input, new Payload((IntPtr)pData, payloadData.Length));
        }
    }

    public TOutput Invoke<TInput, TOutput>(string route, TInput? input) where TOutput : BaseResult, new()
    {
        return Invoke<TInput, TOutput>(route, input, Payload.Empty);
    }
}