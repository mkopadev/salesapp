using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Logging
{
    public sealed class LogManager
    {
        
        /// <summary>
        /// Hiding the constructor. Only Static methods should be used.
        /// </summary>
        private LogManager() {}
        
        /// <summary>
        /// Returns a logger for a specific class. Currently, loggers are not cached to allow disposing them when not needed (save memory).
        /// </summary>
        /// <param name="type">Type to use the logger for</param>
        /// <returns>Specific logger for a class.</returns>
        public static ILog Get(Type type) {
            // if the LogManager is properly configured, continue
            ILog logger = Resolver.Instance.Get<ILog>();

            if (logger == null)
            {
                logger = new DefaultLogger();
            }
            logger.Initialize(type.FullName);
            return logger;
        }

        public class DefaultLogger : ILog
        {
            private string _target;
            public void Initialize(string target)
            {
                _target = target;
                System.Diagnostics.Debug.WriteLine("No logger initialize: using default logger for [{0}]: ", _target);
            }

            public void Initialize<T>()
            {
                this.Initialize(typeof(T).FullName);
            }

            public void Verbose(string v)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1}", _target, v);
            }

            public void Verbose(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1} - {2}", _target, e.Message, e.StackTrace);
            }

            public void Debug(string d)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1}", _target, d);
            }

            public void Debug(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1} - {2}", _target, e.Message, e.StackTrace);
            }

            public void Info(string i)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1}", _target, i);
            }

            public void Info(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1} - {2}", _target, e.Message, e.StackTrace);
            }

            public void Warning(string w, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1}", _target, w);
            }

            public void Warning(Exception e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1} - {2}", _target, e.Message, e.StackTrace);
            }

            public void Error(string e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1}", _target, e);
            }

            public void Error(Exception e, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                System.Diagnostics.Debug.WriteLine("{0} - {1} - {2}", _target, e.Message, e.StackTrace);
                System.Diagnostics.Debug.WriteLine("member name: " + memberName);
                System.Diagnostics.Debug.WriteLine("source file path: " + sourceFilePath);
                System.Diagnostics.Debug.WriteLine("source line number: " + sourceLineNumber);
            }

            public ObservableCollection<LogFile> GetLogFiles()
            {
                throw new NotImplementedException();
            }

            public void DeleteFile(LogFile logFile)
            {
                throw new NotImplementedException();
            }
        }
    }
}
