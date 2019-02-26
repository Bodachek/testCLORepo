using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLO365
{
    enum LoginStatus
    {
        Success = 1,
        Error = 0,
        InvalidCredentials = 2,
        NetworkError = 3,
        InvalidOTP = 4,
        AccessDenied = 5
    }
}
