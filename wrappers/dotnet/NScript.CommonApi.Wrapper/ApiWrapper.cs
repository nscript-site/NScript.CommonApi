using System.Runtime.InteropServices;
using System.Text.Json;

namespace NScript.CommonApi
{
    public abstract class ApiWrapper
    {
        private Func<IntPtr, IntPtr, IntPtr, int, IntPtr>? _jitHandle;

        /// <summary>
        /// 为这个接口设置 jit hook。方便开发时采用 jit 模式，直接绕开 pinvoke 调用，
        /// </summary>
        /// <param name="jitHandle"></param>
        public void SetJitHook(Func<IntPtr, IntPtr, IntPtr, int, IntPtr> jitHandle)
        {
            this._jitHandle = jitHandle;
        }

        public bool HasJitHook => _jitHandle != null;

        protected abstract IntPtr InvokeApi(IntPtr pRoute, IntPtr pJsonParams, IntPtr pDataPayload, int payloadLength);

        public TOutput Invoke<TInput, TOutput>(String route, TInput input, byte[]? payload = null) where TOutput : BaseResult, new()
        {
            if(payload == null)
            {
                return Invoke<TInput, TOutput>(route, input, IntPtr.Zero, 0);
            }
            else
            {
                unsafe
                {
                    fixed(byte* pData = payload)
                    {
                        return Invoke<TInput, TOutput>(route, input, (IntPtr)pData, payload.Length);
                    }
                }
            }
        }

        public TOutput Invoke<TInput,TOutput>(String route, TInput input, IntPtr pDataPayload, int payloadLength) where TOutput:BaseResult,new()
        {
            TOutput? output = null;

            String inputStr = JsonSerializer.Serialize(input);
            IntPtr pRoute = Marshal.StringToHGlobalAnsi(route);
            IntPtr pInputStr = Marshal.StringToHGlobalAnsi(inputStr);
            IntPtr pResult = IntPtr.Zero;
            try
            {
                if (_jitHandle != null)
                    pResult = _jitHandle(pRoute, pInputStr, pDataPayload, payloadLength);
                else
                    pResult = InvokeApi(pRoute, pInputStr, pDataPayload, payloadLength);
                if (pResult != IntPtr.Zero)
                {
                    String? result = Marshal.PtrToStringAnsi(pResult);
                    if (result != null) output = JsonSerializer.Deserialize<TOutput>(result);
                }
            }
            finally
            {
                if (pResult != IntPtr.Zero) Marshal.FreeHGlobal(pResult);
                if (pRoute != IntPtr.Zero) Marshal.FreeHGlobal(pRoute);
                if (pInputStr != IntPtr.Zero) Marshal.FreeHGlobal(pInputStr);
            }

            return output ?? new TOutput() { code = Error.Other, message = "null output" };
        }
    }
}
