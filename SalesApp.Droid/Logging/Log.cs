using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Com.Crashlytics.Android;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using Xamarin;
using AndroidLog = Android.Util.Log;

namespace SalesApp.Droid.Logging
{
    public class Log : ILog
    {
        private string _target = "Log";
        private readonly string _defaultLogString = "NO LOG MESSAGE AVAILABLE!";
        private LogWriter _writer;
        private bool _fileExtensiveLogs;

        private bool _xamarinInsights = true;
        TraceEventCache _traceEventCache = new TraceEventCache();

        private bool FileExtensiveLogs
        {
            get
            {
                return this.LogSettings.FileExtensiveLogs;
            }
        }

        private LogWriter Writer
        {
            get
            {
                return _writer = _writer ?? new LogWriter();
            }
        }

        private ILogSettings LogSettings
        {
            get
            {
                return Resolver.Instance.Get<ILogSettings>();
            }
        }

        public void Initialize(string target)
        {
            _target = target;
        }

        public void Initialize<T>()
        {
            Initialize(typeof(T).FullName);
        }

        public void Verbose(string v)
        {
            if (v == null)
            {
                v = _defaultLogString;
            }

            AndroidLog.Verbose(_target, v);
            CrashlyticsLog(v);

            if (FileExtensiveLogs)
            {
                Writer.Write(v);
            }
        }

        public void Verbose(Exception e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                AndroidLog.Verbose(_target, e.Message);
            }

            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                AndroidLog.Verbose(_target, e.StackTrace);
            }

            CrashlyticsLog(e);
            Writer.Write(e);
        }

        public void Debug(string d, bool writeToFile = false)
        {
            throw new NotImplementedException();
        }

        public void Debug(string d)
        {
            if (d == null)
            {
                d = _defaultLogString;
            }

            AndroidLog.Debug(_target, d);
            CrashlyticsLog(d);

            if (FileExtensiveLogs)
            {
                Writer.Write(d);
            }
        }

        public void Debug(Exception e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                AndroidLog.Debug(_target, e.Message);
            }

            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                AndroidLog.Debug(_target, e.StackTrace);
            }

            CrashlyticsLog(e);
            Writer.Write(e);
        }

        public void Info(string i)
        {
            if (i == null)
            {
                i = _defaultLogString;
            }

            AndroidLog.Info(_target, i);
            CrashlyticsLog(i);

            if (FileExtensiveLogs)
            {
                Writer.Write(i);
            }
        }

        public void Info(Exception e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                AndroidLog.Info(_target, e.Message);
            }

            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                AndroidLog.Info(_target, e.StackTrace);
            }

            CrashlyticsLog(e);
            Writer.Write(e);
        }

        public void Warning(string w, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (w == null)
            {
                w = _defaultLogString;
            }

            AndroidLog.Warn(_target, w);
            CrashlyticsLog(w);

            if (FileExtensiveLogs)
            {
                Writer.Write(w);
            }

            if (_xamarinInsights)
            {
                Dictionary<string, string> additionalInfo = new Dictionary<string, string>
                {
                    {"Member Name", memberName},
                    {"Source Filepath", sourceFilePath},
                    {"Source Line Number", sourceLineNumber.ToString()},
                    {"CallStack", _traceEventCache.Callstack }
                };

                Insights.Report(new Exception(w), additionalInfo);
            }
        }

        public void Warning(Exception e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                AndroidLog.Warn(_target, e.Message);
            }

            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                AndroidLog.Warn(_target, e.StackTrace);
            }

            CrashlyticsLog(e);
            Writer.Write(e);

            if (_xamarinInsights)
            {
                Dictionary<string, string> additionalInfo = new Dictionary<string, string>
                {
                    {"Member Name", memberName},
                    {"Source Filepath", sourceFilePath},
                    {"Source Line Number", sourceLineNumber.ToString()},
                    {"CallStack", _traceEventCache.Callstack }
                };

                Insights.Report(e, additionalInfo);
            }
        }

        public void Error(string e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (e == null)
            {
                e = _defaultLogString;
            }

            AndroidLog.Error(_target, e);
            CrashlyticsLog(e);
            Writer.Write(e);

            if (_xamarinInsights)
            {
                Dictionary<string, string> additionalInfo = new Dictionary<string, string>
                {
                    {"Member Name", memberName},
                    {"Source Filepath", sourceFilePath},
                    {"Source Line Number", sourceLineNumber.ToString()},
                    {"CallStack", _traceEventCache.Callstack }
                };

                Insights.Report(new Exception(e), additionalInfo);
            }
        }

        public void Error(Exception e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(e.Message))
            {
                return;
            }

            AndroidLog.Error(_target, "member name: " + memberName);
            AndroidLog.Error(_target, "source file path: " + sourceFilePath);
            AndroidLog.Error(_target, "source line number: " + sourceLineNumber);
            AndroidLog.Error(_target, e.Message);

            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                AndroidLog.Error(_target, e.StackTrace);
            }

            CrashlyticsLog(e);
            Writer.Write(e);

            if (_xamarinInsights)
            {
                Dictionary<string, string> additionalInfo = new Dictionary<string, string>
                {
                    {"Member Name", memberName},
                    {"Source Filepath", sourceFilePath},
                    {"Source Line Number", sourceLineNumber.ToString()},
                    {"CallStack", _traceEventCache.Callstack }
                };

                Insights.Report(e, additionalInfo);
            }
        }

        public ObservableCollection<LogFile> GetLogFiles()
        {
            ObservableCollection<LogFile> files = new ObservableCollection<LogFile>();
            List<string> paths = Directory.GetFiles(Writer.LogDirectory).ToList();

            foreach (var path in paths)
            {
                LogFile logFile = new LogFile();
                logFile.FilePath = path;
                files.Add(logFile);
            }

            // files.Sort((a, b) => -1 * a.CompareTo(b));
            files.Sort(i => i.FilePath, false);
            return files;
        }

        public void DeleteFile(LogFile logFile)
        {
            File.Delete(logFile.FilePath);
        }

        public void CrashlyticsLog(string log)
        {
            if (log == null)
            {
                return;
            }

            try
            {
                Crashlytics.Log(log);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(e.Message))
                {
                    AndroidLog.Info(_target, e.Message);
                }

                if (!string.IsNullOrEmpty(e.StackTrace))
                {
                    AndroidLog.Info(_target, e.StackTrace);
                }
            }
        }

        public void CrashlyticsLog(Exception e)
        {
            if (e == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(e.Message))
            {
                CrashlyticsLog(e.Message);
            }

            if (!string.IsNullOrEmpty(e.StackTrace))
            {
                CrashlyticsLog(e.StackTrace);
            }
        }
    }
}