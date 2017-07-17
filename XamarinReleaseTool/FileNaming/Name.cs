using System;
using System.IO;

namespace XamarinReleaseTool.FileNaming
{
    public class Name
    {

        private string[] _platforms = new[]
        {
            "armeabi"
            , "armeabi-v7a"
            , "x86"
        };

        private string ExtractApkNameComponent(string[] deconstructors)
        {
            foreach (var deconstructor in deconstructors)
            {
                if (ApkName.Contains(deconstructor))
                {
                    ApkName = ApkName.Replace("-" + deconstructor, "");
                    return deconstructor;
                }
            }
            return "";
        }

        public Name(string fileNamePrefix, string buildType, string apkName, string versionCode, string versionName)
        {
            this.BuildType = buildType;
            apkName = apkName.Replace(apkName.Split(new char[] {'-'})[0], fileNamePrefix);
            ApkName = Path.GetFileNameWithoutExtension(apkName);
            Platform = ExtractApkNameComponent(_platforms);
            VersionCode = versionCode;
            VersionName = versionName;

        }

        public string VersionName { get; set; }

        public string VersionCode { get; set; }

        private int YearAndMonth
        {
            get { return int.Parse(DateTime.Now.ToString("yyyyMM")); }
        }

        private uint Day
        {
            get { return uint.Parse(DateTime.Now.ToString("dd")); }
        }

        private string BaseName { get; set; }
        private string Platform { get; set; }

        private string BuildType { get; set; }
        private string ApkName { get; set; }

        public string DirectoryPath
        {
            get
            {
                return string.Format
                    (
                        @"\{0}\{1}\{2}\{3}\"
                        , YearAndMonth
                        , Day
                        ,BuildType
                        , string.IsNullOrEmpty(Platform) ? "Multi Platform" : Platform
                    );
            }
        }

        public string Filename
        {
            get
            {
                return string.Format
                    (
                        "{0}-{1}-{2}-{3}.apk"
                        ,ApkName
                        , VersionName
                        , VersionCode
                        , DateTime.Now.ToString("yyyyMMdd-HHmmss")
                    );
            }
        }
    }
}