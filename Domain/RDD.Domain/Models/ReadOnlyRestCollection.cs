using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
    public class ReadOnlyRestCollection<TEntity, TKey> : IReadOnlyRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TKey>
        where TKey : IEquatable<TKey>
    {
        public ReadOnlyRestCollection(IReadOnlyRepository<TEntity> repository)
        {
            _readOnlyRepository = repository;
        }

        private readonly IReadOnlyRepository<TEntity> _readOnlyRepository;

        public async Task<bool> AnyAsync(Query<TEntity> query)
        {
            query.NeedEnumeration = false;
            query.NeedCount = true;

            await GetAsync(query);
            return query.QueryMetadata.TotalCount > 0;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync() => (await GetAsync(new Query<TEntity>()));

        public virtual async Task<IEnumerable<TEntity>> GetAsync(Query<TEntity> query)
        {
            var totalCount = -1;
            IReadOnlyCollection<TEntity> items = null;

            //Dans de rares cas on veut seulement le count des entités
            if (query.NeedCount && !query.NeedEnumeration)
            {
                query.QueryMetadata.TotalCount = totalCount = await _readOnlyRepository.CountAsync(query);
            }

            //En général on veut une énumération des entités
            if (query.NeedEnumeration)
            {
                items = await _readOnlyRepository.GetAsync(query);
                query.QueryMetadata.TotalCount = totalCount != -1 ? totalCount : await _readOnlyRepository.CountAsync(query);
                items = await _readOnlyRepository.PrepareAsync(items, query);
            }

            //Si c'était un PUT/DELETE, on en profite pour affiner la réponse
            if (query.Verb != HttpVerbs.Get && totalCount == 0)
            {
                throw new NotFoundException(string.Format("No item of type {0} matching URL criteria while trying a {1}", typeof(TEntity).Name, query.Verb));
            }

            return items ?? Enumerable.Empty<TEntity>();
        }

        /// <summary>
        /// Si on ne trouve pas l'entité, on renvoie explicitement un NotFound
        /// puisque c'était explicitement cette entité qui était visée
        /// NB : on ne sait pas si l'entité existe mais qu'on n'y a pas accès ou si elle n'existe pas, mais c'est logique
        /// </summary>
        /// <returns></returns>
        public virtual async Task<TEntity> GetByIdAsync(TKey id, Query<TEntity> query)
        {
            TEntity result = (await GetAsync(new Query<TEntity>(query, e => e.Id.Equals(id)))).FirstOrDefault();

            if (result == null)
            {
                throw new NotFoundException($"Resource with ID {id} not found");
            }

            return result;
        }

        protected Task<bool> AnyAsync() => AnyAsync(new Query<TEntity>());

        public async Task<TEntity> TryGetByIdAsync(object id)
        {
            try
            {
                return await GetByIdAsync((TKey)id);
            }
            catch
            {
                return default;
            }
        }

        public Task<TEntity> GetByIdAsync(TKey id, HttpVerbs verb = HttpVerbs.Get)
            => GetByIdAsync(id, new Query<TEntity> { Verb = verb });
    }
}