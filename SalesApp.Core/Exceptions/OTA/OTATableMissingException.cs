using System;

namespace SalesApp.Core.Exceptions.OTA
{
    /// <summary>
    /// Exception to signal that access to OTA was made before initialization of database
    /// </summary>
    public class OtaTableMissingException : Exception
    {
    }
}