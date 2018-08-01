using System;
using System.Linq.Expressions;
using RDD.Domain.Models.Querying;
using RDD.Domain.Rights;

namespace RDD.Web.Helpers
{
    /// <summary>
    /// Default right helper : everything is allowed.
    /// With AddRddRights<>() you can register specific ICombinationsHolder or IRightExpressionsHelper tu implement custom rights
    /// </summary>
    public class DefaultRightExpressionsHelper : IRightExpressionsHelper
    {
        public Expression<Func<T, bool>> GetFilter<T>(Query<T> query) where T : class
        {
            return t => true;
        }
    }
}
