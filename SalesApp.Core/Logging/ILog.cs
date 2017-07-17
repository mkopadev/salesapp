using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace SalesApp.Core.Logging
{
    public interface ILog
    {
        /// <summary>
        /// Initializes the logger for the specific target.
        /// </summary>
        /// <param name="target">Target of the logger, normally the class name or another identifier to indicate where the logger is used.</param>
        void Initialize(string target);

        void Initialize<T>();

        void Verbose(string v);

        void Verbose(Exception e);

        void Debug(string d);

        void Debug(Exception e);

        void Info(string i);

        void Info(Exception e);

        void Warning(string w, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0);

        void Warning(Exception e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0);

        void Error(string e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0);

        void Error(
            Exception e,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        ObservableCollection<LogFile> GetLogFiles();

        void DeleteFile(LogFile logFile);
    }
}
