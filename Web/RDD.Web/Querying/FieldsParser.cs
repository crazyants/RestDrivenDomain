using RDD.Domain.Helpers.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RDD.Web.Querying
{
    public class FieldsParser
    {
        public IExpressionSelectorTree<TClass> ParseFields<TClass>(Dictionary<string, string> parameters, bool isCollectionCall)
        {
            if (parameters.ContainsKey(Reserved.fields.ToString()))
            {
                return ParseFields<TClass>(parameters[Reserved.fields.ToString()]);
            }
            else if (!isCollectionCall)
            {
                return ParseAllProperties<TClass>();
            }

            return new ExpressionSelectorTree<TClass>();
        }

        public IExpressionSelectorTree ParseAllProperties(Type classType)
        {
            var fields = string.Join(",", classType.GetProperties().Select(p => p.Name));
            return ParseFields(classType, fields);
        }

        public IExpressionSelectorTree<TClass> ParseAllProperties<TClass>()
        {
            var fields = string.Join(",", typeof(TClass).GetProperties().Select(p => p.Name));
            return ParseFields<TClass>(fields);
        }

        private IExpressionSelectorTree<TClass> ParseFields<TClass>(string fields) => new ExpressionSelectorParser().ParseTree<TClass>(fields);
        private IExpressionSelectorTree ParseFields(Type classType, string fields) => new ExpressionSelectorParser().ParseTree(classType, fields);
    }
}