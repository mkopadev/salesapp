using System;
using System.Reflection;
using SalesApp.Core.Logging;

namespace SalesApp.Core.Services.Device
{
    public abstract class Information
    {

        protected static readonly ILog Logger = LogManager.Get(typeof(Information));

        protected Information()
        {
            var assembly = typeof(Information).GetTypeInfo().Assembly;
            var assemblyName = new AssemblyName(assembly.FullName);
            Major = assemblyName.Version.Major;
            Minor = assemblyName.Version.Minor;
            Build = assemblyName.Version.Build;
            Revision = assemblyName.Version.Revision;

            
        }
        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Build { get; private set; }

        public int Revision { get; private set; }

        public string CoreVersion
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);
            }
            
        }

        public string BuildTime
        {
            get
            {
                DateTime buildDateTime = new DateTime(2000, 1, 1, 0, 0, 0).AddDays(Build);
                buildDateTime = buildDateTime.AddSeconds(Revision * 2);
                return buildDateTime.ToString("g");
            }

        }
    }
}