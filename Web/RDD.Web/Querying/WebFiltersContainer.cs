﻿using RDD.Domain;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RDD.Web.Querying
{
    public class WebFiltersContainer<TEntity, TKey> : Filter<TEntity>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        private IEnumerable<WebFilter<TEntity>> _filters;

        public WebFiltersContainer(IEnumerable<WebFilter<TEntity>> filters)
        {
            Init(filters);
        }

        private void Init(IEnumerable<WebFilter<TEntity>> filters)
        {
            _filters = filters;

            Expression = new PredicateService<TEntity, TKey>(filters)
                .GetEntityPredicate(new QueryBuilder<TEntity, TKey>());
        }

        public override bool HasFilter(Expression<Func<TEntity, object>> property)
        {
            return _filters.Any(f => f.Property.Contains(property));
        }

        public override void RemoveFilter(Expression<Func<TEntity, object>> property)
        {
            if (HasFilter(property))
            {
                Init(_filters.Where(f => !f.Property.Contains(property)));
            }
        }
    }
}
