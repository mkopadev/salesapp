using System;
using SalesApp.Core.Enums.Database;

namespace SalesApp.Core.Services.Database.Querying
{
    public class QueryGenerator
    {
       
        public string GetQuery(string queryPrefix, Criterion[] fieldsAndValues,string tableName)
        {
            string query = queryPrefix;
            foreach (var criterion in fieldsAndValues)
            {
                if (query != queryPrefix)
                {
                    query += $" {criterion.Conjunction} ";
                }
                else
                {
                    query = string.Format(queryPrefix, tableName) + " Where ";
                }

                query += string.Format
                    (
                        "{0} {1} {2}"
                        , criterion.Field
                        , OperatorEnumToSqlOperator(criterion.Op)
                        , GetValuePreprocessed(criterion.Value, criterion)
                    );
            }
            
            return query;
        }

        

        private string GetValuePreprocessed(string value, Criterion criterion)
        {
            switch (criterion.Op)
            {
                case Operators.EndLike:
                    value = "%" + value;
                    break;
                case Operators.MiddleLike:
                    value = "%" + value + "%";
                    break;
                case Operators.BeginningLike:
                    value = value + "%";
                    break;
                case Operators.In:
                case Operators.NotIn:
                    value = GetSet(criterion.Values);
                    break;
            }
            if (criterion.Quoted)
            {
                value = "'" + value + "'";
            }
            return value;
        }

        private string GetSet(string[] values)
        {
            string res = "(";

            foreach (var val in values)
            {
                if (!string.IsNullOrEmpty(val))
                {
                    res += $"'{val}',";
                }
            }

            res = res.Substring(0, res.Length - 1);
            res += ")";
            return res;
        }

        private string OperatorEnumToSqlOperator(Operators op)
        {
            switch (op)
            {
                case Operators.EqualTo:
                    return "=";
                case Operators.NotEqualTo:
                    return "!=";
                case Operators.BeginningLike:
                case Operators.MiddleLike:
                case Operators.EndLike:
                    return " like ";
                case Operators.In:
                    return " in ";
                case Operators.LessThan:
                    return " < ";
                case Operators.GreaterThan:
                    return " > ";
                case Operators.LessThanOrEqualTo:
                    return " <= ";
                case Operators.GreaterThanOrEqualTo:
                    return " >= ";
            }
            throw new ArgumentException("Unknown operator '" + op.ToString() + "'");
        }
    }
}