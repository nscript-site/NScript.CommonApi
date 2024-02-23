using System.Runtime.InteropServices;

namespace NScript.CommonApi
{
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
    }
}