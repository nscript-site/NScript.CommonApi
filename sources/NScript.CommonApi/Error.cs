using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NScript.CommonApi
{
    public enum Error
    {
        Success = 0,
        InvalidInput = -11,
        InvalidRoute = -12,
        InternalError = -21,
        Other = -9999
    }
}
