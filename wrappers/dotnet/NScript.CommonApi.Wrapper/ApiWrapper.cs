using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NScript.CommonApi
{
    public abstract class ApiWrapper
    {
        protected abstract IntPtr InvokeApi(IntPtr pRoute, IntPtr pJsonParams);

        public TOutput Invoke<TInput,TOutput>(String route, TInput input) where TOutput:class
        {
            TOutput output = null;

            String inputStr = JsonSerializer.Serialize(input);
            IntPtr pRoute = Marshal.StringToHGlobalAnsi(route);
            IntPtr pInputStr = Marshal.StringToHGlobalAnsi(inputStr);
            IntPtr pResult = IntPtr.Zero;
            try
            {
                pResult = InvokeApi(pRoute, pInputStr);
                if (pResult != IntPtr.Zero)
                {
                    String result = Marshal.PtrToStringAnsi(pResult);
                    if (result != null) output = JsonSerializer.Deserialize<TOutput>(result);
                }
            }
            finally
            {
                if (pResult != IntPtr.Zero) Marshal.FreeHGlobal(pResult);
                if (pRoute != IntPtr.Zero) Marshal.FreeHGlobal(pRoute);
                if (pInputStr != IntPtr.Zero) Marshal.FreeHGlobal(pInputStr);
            }

            return output;
        }
    }
}
