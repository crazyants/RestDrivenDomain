using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Domain.Patchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RDD.Domain.Models
{
    public class RestCollection<TEntity, TKey> : ReadOnlyRestCollection<TEntity, TKey>, IRestCollection<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected IPatcherProvider PatcherProvider { get; private set; }
        protected new IRepository<TEntity> Repository { get; set; }
        protected IInstanciator<TEntity> Instanciator { get; set; }

        public RestCollection(IRepository<TEntity> repository, IPatcherProvider patcherProvider, IInstanciator<TEntity> instanciator)
            : base(repository)
        {
            PatcherProvider = patcherProvider;
            Repository = repository;
            Instanciator = instanciator;
        }

        public virtual Task<TEntity> CreateAsync(ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            TEntity entity = Instanciator.InstanciateNew(candidate);

            GetPatcher().Patch(entity, candidate.JsonValue);

            return CreateAsync(entity, query);
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity, Query<TEntity> query = null)
        {
            ForgeEntity(entity);

            if (!await ValidateEntity(entity, null) || !await OnBeforeCreate(entity))
            {
                return null;
            }

            Repository.Add(entity);

            return entity;
        }

        /// <summary>
        /// Called before the entity is created
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>false if the entity should not be created</returns>
        protected virtual Task<bool> OnBeforeCreate(TEntity entity) => Task.FromResult(true);

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, ICandidate<TEntity, TKey> candidate, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;
            TEntity entity = await GetByIdAsync(id, query);

            return await UpdateAsync(entity, candidate, query);
        }

        public virtual async Task<TEntity> UpdateByIdAsync(TKey id, TEntity entity)
        {
            TEntity oldEntity = await GetByIdAsync(id, new Query<TEntity> { Verb = HttpVerbs.Put });
            if (oldEntity == null)
            {
                return null;
            }

            bool updated = await UpdateEntityCore(id, entity, oldEntity);

            return updated ? entity : oldEntity;
        }

        public virtual async Task<IEnumerable<TEntity>> UpdateByIdsAsync(IDictionary<TKey, ICandidate<TEntity, TKey>> candidatesByIds, Query<TEntity> query = null)
        {
            query = query ?? new Query<TEntity>();
            query.Verb = HttpVerbs.Put;

            var result = new HashSet<TEntity>();

            var ids = candidatesByIds.Select(d => d.Key).ToList();
            var expQuery = new Query<TEntity>(query, e => ids.Contains(e.Id));
            var entities = (await GetAsync(expQuery)).Items.ToDictionary(el => el.Id);

            foreach (KeyValuePair<TKey, ICandidate<TEntity, TKey>> kvp in candidatesByIds)
            {
                TEntity entity = entities[kvp.Key];
                entity = await UpdateAsync(entity, kvp.Value, query);

                result.Add(entity);
            }

            return result;
        }

        protected virtual IPatcher GetPatcher() => new ObjectPatcher(PatcherProvider);

        protected virtual void ForgeEntity(TEntity entity) { }

        protected virtual Task<bool> ValidateEntity(TEntity entity, TEntity oldEntity) => Task.FromResult(true);

        protected virtual Task<bool> OnBeforeUpdateEntity(TEntity entity) => Task.FromResult(true);

        protected virtual Task OnBeforePatchEntity(TEntity entity, ICandidate<TEntity, TKey> candidate) => Task.CompletedTask;

        /// <summary>
        /// Called after entity update
        /// As "oldEntity" is a MemberWiseClone of "entity" before its update, it's a one level deep copy. If you want to go deeper
        /// you can do it by overriding the Clone() method and MemberWiseClone individual sub-properties
        /// </summary>
        protected virtual Task OnAfterPatchEntity(TEntity oldEntity, TEntity entity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query) => Task.CompletedTask;

        private async Task<TEntity> UpdateAsync(TEntity oldEntity, ICandidate<TEntity, TKey> candidate, Query<TEntity> query)
        {
            await OnBeforePatchEntity(oldEntity, candidate);

            TEntity newEntity = oldEntity.Clone();

            GetPatcher().Patch(newEntity, candidate.JsonValue);

            await OnAfterPatchEntity(oldEntity, newEntity, candidate, query);

            bool updated = await UpdateEntityCore((TKey)newEntity.GetId(), newEntity, oldEntity);

            return updated ? newEntity : oldEntity;
        }

        private async Task<bool> UpdateEntityCore(TKey id, TEntity entity, TEntity oldEntity)
        {
            if (!await ValidateEntity(entity, oldEntity) || !await OnBeforeUpdateEntity(entity))
            {
                return false;
            }

            Repository.Update<TEntity, TKey>(id, entity);

            return true;
        }


        public virtual async Task DeleteByIdAsync(TKey id)
        {
            TEntity entity = await GetByIdAsync(id, new Query<TEntity>
            {
                Verb = HttpVerbs.Delete
            });

            Repository.Remove(entity);
        }

        public virtual async Task DeleteByIdsAsync(IList<TKey> ids)
        {
            var expQuery = new Query<TEntity>(e => ids.Contains(e.Id))
            {
                Verb = HttpVerbs.Delete
            };

            var entities = (await GetAsync(expQuery)).Items.ToDictionary(el => el.Id);

            foreach (TKey id in ids)
            {
                var entity = entities[id];

                Repository.Remove(entity);
            }
        }

    }
}