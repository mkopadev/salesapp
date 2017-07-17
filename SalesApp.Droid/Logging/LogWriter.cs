using System;
using System.IO;
using System.Runtime.CompilerServices;
using Android.App;
using Newtonsoft.Json;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Settings;
using Environment = Android.OS.Environment;

namespace SalesApp.Droid.Logging
{
    public class LogWriter
    {
        public string LogDirectory
        {
            get
            {
                string dir = Environment.ExternalStorageDirectory.AbsolutePath
                             + @"/" + Application.Context.PackageName + "/Logs/";
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return dir;
            }
        }

        public string BaseFilename
        {
            get
            {
                DateTime now = DateTime.Now;
                return string.Format(
                        @"{0}{1}{2}.txt",
                        GetTwoDigit(now.Year),
                        GetTwoDigit(now.Month),
                        GetTwoDigit(now.Day));
            }
        }

        private string Divider
        {
            get { return "------------------------------"; }
        }

        public void Write(Exception exception, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            string errorDump = JsonConvert.SerializeObject(new {Message = exception.Message, StackTrace = exception.StackTrace},
                    Formatting.Indented)
                    .Trim();
            if (errorDump.IsBlank())
            {
                return;
            }

            Write(errorDump);
            Write(
                    JsonConvert.SerializeObject(
                        new
                        {
                            AdditionalErrorInfo = 
                            new
                            {
                                CallerMemberName = memberName
                                ,
                                SourceFilePath = sourceFilePath
                                ,
                                SourceLineNumber = sourceLineNumber
                            }
                        }));
                        }

        public void Write(string message)
        {
            StreamWriter streamWriter = null;
            try
            {
            string filename = LogDirectory + BaseFilename;
            bool writeHeader = !File.Exists(filename);

                streamWriter = new StreamWriter(filename, true);

                streamWriter.AutoFlush = true;
                if (writeHeader)
                {
                    string appVer =
                    Application.Context.PackageManager.GetPackageInfo(Application.Context.PackageName, 0)
                        .VersionName;
                    streamWriter.WriteLine(GetString(Resource.String.log_app_version, "\t") + appVer);
                    streamWriter.WriteLine(
                        GetString(Resource.String.log_date, "\t\t\t") + DateTime.Now.GetDateStandardFormat());
                    streamWriter.WriteLine(
                        GetString(Resource.String.log_user_number, "\t")
                        + Settings.Instance.DsrPhone);
                    EndSection(streamWriter);
                }

                streamWriter.WriteLine(Divider);
                streamWriter.WriteLine(DateTime.Now.GetTimeStandardFormat());
                streamWriter.WriteLine(Divider);
                
                streamWriter.WriteLine();
                streamWriter.WriteLine("Exception:");
                streamWriter.WriteLine(message);
                EndSection(streamWriter);
            }
            catch (Exception e)
            {
                Android.Util.Log.Warn("LogWriter", e.Message + " - " + e.StackTrace);
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
        }

        private void EndSection(StreamWriter streamWriter)
        {
            streamWriter.WriteLine(Divider);
            streamWriter.WriteLine();
            streamWriter.WriteLine();
            streamWriter.WriteLine();
        }

        private string GetTwoDigit(int value)
        {
            if (value > 9)
            {
                return value.ToString();
            }

            return "0" + value;
        }

        private string GetString(int id, string suffix)
        {
            return Application.Context.GetString(id) + ":" + suffix;
        }
    }
}