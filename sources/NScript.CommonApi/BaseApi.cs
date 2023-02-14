using System.Runtime.InteropServices;

namespace NScript.CommonApi
{
    public class BaseApi
    {
        protected IntPtr HandleApi(IntPtr pRoute, IntPtr pJsonParams)
        {
            if (pRoute == IntPtr.Zero || pJsonParams == IntPtr.Zero) return IntPtr.Zero;
            string jsonParams = Marshal.PtrToStringAnsi(pJsonParams)!;
            string route = Marshal.PtrToStringAnsi(pRoute)!;
            String result = HandleRoute(route, jsonParams);
            if (result == null) return IntPtr.Zero;
            IntPtr pResult = Marshal.StringToHGlobalAnsi(result);
            return pResult;
        }

        protected Dictionary<String, ApiHandler> ApiHandlers { get; set; } = new();

        protected void Map(String route, ApiHandler handler)
        {
            if (route == null || handler == null) return;
            this.ApiHandlers[route]=handler;
        }

        protected String HandleRoute(String route, String jsonParams)
        {
            ApiHandler? match;
            this.ApiHandlers.TryGetValue(route, out match);

            if (match == null)
                return BaseResult.CreateErrorJsonString(Error.InvalidRoute);

            return match.Handle(jsonParams);
        }
    }
}