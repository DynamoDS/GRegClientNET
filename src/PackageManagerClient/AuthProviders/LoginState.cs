﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageManagerClient.AuthProviders
{
    public enum LoginState
    {
        LoggedOut, LoggingIn, RequestingUserData, LoggedIn
    }
}
