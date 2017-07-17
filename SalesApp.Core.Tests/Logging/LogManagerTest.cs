using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Tests.Logging
{
    // [TestFixture] todo One or more tests in the fixture are broken
    public class LogManagerTest //: BaseTest
    {
        [SetUp]
        public void Setup() { }


        [TearDown]
        public void Tear() { }

        // [Test] Todo Fix failing test
        public void TestInitializeAndGetLogManager()
        {
            // first check Get when LogManager is not Initialized
            Assert.Throws<Exception>(delegate { LogManager.Get(typeof (LogManagerTest)); });
            
            Resolver.Instance.Register<ILog, LogTest>();

            var log = LogManager.Get(typeof (LogManagerTest));

            Assert.That(log, Is.AssignableTo(typeof(ILog)));
            Assert.That(log, Is.InstanceOf(typeof(LogTest)));
            log.Debug("Test the logger.");

            
        }



        /// <summary>
        /// This class represents an invalid logger class and using it will throw an Exception.
        /// </summary>
        internal class InvalidLogger
        {
            
        }

        internal class LogTest : ILog
        {
            public void Initialize(string target)
            {
                
            }

            public void Initialize<T>()
            {
                
            }

            public void Verbose(string v)
            {

            }

            public void Verbose(Exception e)
            {
                
            }

            public void Debug(string d)
            {
                
            }

            public void Debug(Exception e)
            {
                
            }

            public void Info(string i)
            {
                
            }

            public void Info(Exception e)
            {
                
            }

            public void Warning(string w, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
            {
                throw new NotImplementedException();
            }

            public void Warning(Exception e, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
            {
                throw new NotImplementedException();
            }

            public void Error(string e, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
            {
                throw new NotImplementedException();
            }

            public void Warning(string w)
            {
                
            }

            public void Warning(Exception e)
            {
                throw new NotImplementedException();
            }

            public void Error(string e)
            {
                throw new NotImplementedException();
            }

            public void Error(Exception e, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
            {
                throw new NotImplementedException();
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