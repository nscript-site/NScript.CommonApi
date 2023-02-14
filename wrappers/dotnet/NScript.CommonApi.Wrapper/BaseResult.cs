using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NScript.CommonApi
{
    public enum Error
    {
        Success = 0,
        InvalidInput = -11,
        InvalidRoute = -12,
        InternalError = -21
    }

    public class BaseResult
    {
        public int Code { get; set; }
        public Error Error { get; set; }
        public String Message { get; set; }
    }
}
