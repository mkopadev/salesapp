using System;

namespace SalesApp.Core.Services.Settings
{
    public class SettingAttribute : Attribute
    {
        public string EmptySets { get; set; }
    }
}