using System;
using System.Linq;
using SalesApp.Core.Enums.Database;

namespace SalesApp.Core.Services.Database.Querying
{
    public class Criterion
    {
        
        public string Field { get; private set; }
        public string Value { get; private set; }
        public ConjunctionsEnum Conjunction { get; private set; }
        public bool Quoted { get; set; }
        public Operators Op { get; set; }

        public string[] Values { get; private set; }


        /// <summary>
        /// Create a new criteria for your query
        /// </summary>
        /// <param name="field">Name of field to query</param>
        /// <param name="value">Value for the field</param>
        /// <param name="conjunction">The relation of this part of the criteria to the preceding parts of the criteria. Placed in front of this field in the query</param>
        /// <param name="quoted">This specifies if the passed value will be enclosed in quotes when query is built</param>
        /// <param name="op">The operator to be applied to this field in the query</param>
        public Criterion(string field, object value, ConjunctionsEnum conjunction = ConjunctionsEnum.And,bool quoted = true,Operators op = Operators.EqualTo )
        {
            var notExpected = new Operators[]
                    {
                        Operators.In
                        ,Operators.NotIn
                    };

            if (notExpected.Contains(op))
            {
                throw new Exception("Invalid operator '" + op.ToString() + "' passed to criterion");
            }
            
            AssignValues(field,conjunction,quoted,op);
            Value = value.ToString();
        }


        public Criterion(string field, string[] values, ConjunctionsEnum conjunction = ConjunctionsEnum.And,Operators op = Operators.EqualTo)
        {
            var expected = new Operators[]
                    {
                        Operators.In, 
                        Operators.NotIn
                    };

            if (!expected.Contains(op))
            {
                throw new Exception("Invalid operator '" + op.ToString() + "' passed to criterion");
            }
            AssignValues(field,conjunction,false,op);
            Values = values;
        }

        private void AssignValues(string field, ConjunctionsEnum conjunction = ConjunctionsEnum.And,
            bool quoted = true, Operators op = Operators.EqualTo)
        {
            this.Field = field;
            this.Conjunction = conjunction;
            this.Quoted = quoted;
            this.Op = op;
        }
        

       
        
    }
}
