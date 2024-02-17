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
        Unauthorized = -401,
        Forbidden = -403,
        NotFound = -404,
        RequestTimeOut = -408,
        NotImplemented = -501,
        Other = -9999
    }
}
