using System;

namespace SalesApp.Core.Exceptions.Database
{
    
    public class DuplicateValuesException : Exception
    {
        public string FieldName { get; private set; }
        public string Value { get; private set; }
        public int Count { get; set; }

        public DuplicateValuesException(string fieldName,object value,int count)
        {
            this.FieldName = fieldName;
            this.Value = value.ToString();
            this.Count = count;
        }

        public DuplicateValuesException() : this(string.Empty, string.Empty, default(int))
        {
            
        }

        public string Message
        {
            get
            {
                return string.Format("duplicate record {0} with the same mobile number {1}. Records found {2} ",
                    this.FieldName, this.Value, this.Count);
            }
        }
    }
}
