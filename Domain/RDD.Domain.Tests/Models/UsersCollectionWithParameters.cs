using RDD.Domain.Models;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using RDD.Domain.Rights;

namespace RDD.Domain.Tests.Models
{
    public class UsersCollectionWithParameters : RestCollection<UserWithParameters, int>
    {
        public UsersCollectionWithParameters(IRepository<UserWithParameters> repository,
            IPatcherProvider patcherProvider, IInstanciator<UserWithParameters> instanciator, QueryContext queryContext)
            : base(repository, patcherProvider, instanciator, queryContext)
        {
        }
    }
}
