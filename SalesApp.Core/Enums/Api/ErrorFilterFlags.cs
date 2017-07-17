using System;

namespace SalesApp.Core.Enums.Api
{
    [Flags]
    public enum ErrorFilterFlags
    {
        EnableErrorHandling = 1,
        DisableErrorHandling = 2,
        AllowEmptyResponses = 4,
        Ignore400Family = 8,
        Ignore500Family = 16,
        IgnoreTimeOut = 32,
        IgnoreJsonParseError = 64,
        IgnoreNoInternetError = 128,
        Ignore204 = 256
    }
}