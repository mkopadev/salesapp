using System;
using System.Collections.Generic;
using SalesApp.Core.Enums.Database;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.Services.Database.Querying
{
    public class CriteriaBuilder
    {
        private readonly List<Criterion> _criteria;

        public CriteriaBuilder()
        {
            _criteria = new List<Criterion>();
        }

        public CriteriaBuilder Add(string field, string[] values, ConjunctionsEnum conjunction = ConjunctionsEnum.And,
            Operators op = Operators.In)
        {
            return Add
                (
                    new Criterion(field, values, conjunction, op)
                );
        }

        public CriteriaBuilder AddIfTrue(bool predicate,string field, object value, ConjunctionsEnum conjunction = ConjunctionsEnum.And,
            Operators op = Operators.EqualTo)
        {
            if (!predicate)
            {
                return this;
            }
            return Add(field, value, conjunction, op);
        }

        public CriteriaBuilder Add(string field, object value, ConjunctionsEnum conjunction = ConjunctionsEnum.And,Operators op = Operators.EqualTo)
        {
            return Add(field, value, conjunction, true,op);
        }
        public CriteriaBuilder Add(string field,object value,ConjunctionsEnum conjunction,bool quoted,Operators op)
        {
            _criteria.Add
                (
                    new Criterion(field,value,conjunction,quoted,op)
                );
            return this;
        }

        public CriteriaBuilder AddDateCriterion(string field, DateTime value,
            ConjunctionsEnum conjunction = ConjunctionsEnum.And,Operators op = Operators.EqualTo,bool includeDateComponent = true,bool includeTimeComponent = false)
        {
            
            return Add
                (
                    GetFormattedDateField(field)
                    , GetDateValueFormatedForQuery(value,includeTimeComponent)
                    , conjunction
                    ,op
                );
        }

        public static string GetFormattedDateField(string fieldName)
        {
            
            return string.Format
                (
                    "strftime('{0}', {1})"
                    ,"%s"
                    , fieldName
               ).Trim();
        }

        public static string GetDateValueFormatedForQuery(DateTime date ,bool includeTimeComponent)
        {
            
            if (!includeTimeComponent)
            {
                date = date.ToMidnight();
            }
            return date.ToUnixEpochSeconds().ToString();
        }

        public CriteriaBuilder Add(Criterion criterion)
        {
            _criteria.Add(criterion);
            return this;
        }

        public Criterion[] Criteria
        {
            get { return _criteria.ToArray(); }
        }

        

        public bool HasCriteria
        {
            get { return Criteria != null && Criteria.Length > 0; }
        }

        
    }
}