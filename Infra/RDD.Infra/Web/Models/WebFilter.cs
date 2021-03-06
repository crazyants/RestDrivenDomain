﻿using RDD.Domain.Helpers;
using System.Collections;
using System.Collections.Generic;

namespace RDD.Infra.Web.Models
{
    public class WebFilter<TEntity>
    {
        public PropertySelector<TEntity> Property { get; private set; }
        public WebFilterOperand Operand { get; private set; }
        public IList Values { get; private set; }

        public WebFilter(PropertySelector<TEntity> property, WebFilterOperand operand, IList values)
        {
            Property = property;
            Operand = operand;
            Values = values;
        }
    }

    public class WebFilter<TEntity, TProp> : WebFilter<TEntity>
    {
        public WebFilter(PropertySelector<TEntity> property, WebFilterOperand operand, TProp value)
            : base(property, operand, new List<TProp> { value }) { }
    }
}