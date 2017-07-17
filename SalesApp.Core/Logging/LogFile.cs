using System;
using System.Globalization;
using System.IO;

namespace SalesApp.Core.Logging
{
    public class LogFile : IComparable
    {
        public string FilePath { get; set; }

        public override string ToString()
        {
            string fileName = Path.GetFileNameWithoutExtension(this.FilePath);
            try
            {
                DateTime date = DateTime.ParseExact(fileName, "yyyyMMdd", CultureInfo.InvariantCulture);
                return date.ToString("dd/MM/yyyy");
            }
            catch (Exception)
            {
                return fileName;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            LogFile anotherLogFile = obj as LogFile;

            if (anotherLogFile == null)
            {
                throw new Exception("LogFile.ComapreTo can only compare two objects that are both LogFiles");
            }

            string fileName = Path.GetFileNameWithoutExtension(this.FilePath);
            string thatFileName = Path.GetFileNameWithoutExtension(anotherLogFile.FilePath);
            try
            {
                DateTime thisDateTime = DateTime.ParseExact(fileName, "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime thatDateTime = DateTime.ParseExact(thatFileName, "yyyyMMdd", CultureInfo.InvariantCulture);

                return thisDateTime.CompareTo(thatDateTime);
            }
            catch (Exception)
            {
                return this.ToString().CompareTo(anotherLogFile.ToString());
            }
        }
    }
}