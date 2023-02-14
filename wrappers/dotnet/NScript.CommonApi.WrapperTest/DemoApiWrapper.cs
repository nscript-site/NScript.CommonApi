using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NScript.CommonApi.WrapperTest
{
    public class EchoInput
    {
        public String Message { get; set; }
    }

    public class EchoOutput : BaseResult
    {
        public String Echo { get; set; }
    }

    public class DemoApiWrapper : ApiWrapper
    {
        [DllImport("NScript.CommonApi.SdkDemo.dll")]
        static extern IntPtr sdk_demo_api(IntPtr pRoute, IntPtr pJsonParams);

        protected override IntPtr InvokeApi(IntPtr pRoute, IntPtr pJsonParams)
        {
            return sdk_demo_api(pRoute, pJsonParams);
        }
    }
}
