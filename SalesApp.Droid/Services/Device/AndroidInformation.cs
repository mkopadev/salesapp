using System;
using Android.Content.PM;
using Android.Util;
using SalesApp.Core.Services.Device;

namespace SalesApp.Droid.Services.Device
{
    public class AndroidInformation : Information, IInformation
    {
        public AndroidInformation() : base()
        {
            
        }

        public string DeviceAppVersion
        {
            get
            {

                string version = "unknown";

                try
                {
                    PackageInfo pInfo = SalesApplication
                        .Instance
                        .ApplicationContext
                        .PackageManager
                        .GetPackageInfo(SalesApplication
                            .Instance
                            .ApplicationContext
                            .PackageName, 0);

                    version = pInfo.VersionName;

                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                return version;
            }
        }

        public string DevicePlatform
        {
            get { return "Android"; }
        }

        public string DeviceSoftwareVersion
        {
            get { return Android.OS.Build.VERSION.Codename + Android.OS.Build.VERSION.Sdk; }
        }

        public string ScreenResolution
        {
            get
            {
                if (SalesApplication.Instance != null
                    && SalesApplication.Instance.Resources != null
                    && SalesApplication.Instance.Resources.DisplayMetrics != null)
                {
                    DisplayMetrics metrics = SalesApplication.Instance.Resources.DisplayMetrics;
                    return string.Format("{0}x{1} [{2}]",
                        metrics.WidthPixels,
                        metrics.HeightPixels,
                        metrics.DensityDpi);
                }

                return "unknown";
            }
        }
    }
}