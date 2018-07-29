﻿using Microsoft.AspNetCore.Mvc;
using RDD.Application;
using RDD.Domain;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models.Querying;
using RDD.Web.Helpers;
using RDD.Web.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RDD.Web.Querying;

namespace RDD.Web.Controllers
{
    public abstract class WebController<TEntity, TKey> : WebController<IAppController<TEntity, TKey>, TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        protected WebController(IAppController<TEntity, TKey> appController, ICandidateFactory<TEntity, TKey> helper, IQueryFactory queryFactory) 
            : base(appController, helper, queryFactory)
        {
        }
    }

    public abstract class WebController<TAppController, TEntity, TKey> : ReadOnlyWebController<TAppController, TEntity, TKey>
        where TAppController : IAppController<TEntity, TKey>
        where TEntity : class, IEntityBase<TEntity, TKey>
        where TKey : IEquatable<TKey>
    {
        
        protected WebController(TAppController appController, ICandidateFactory<TEntity, TKey> helper, IQueryFactory queryFactory)
            : base(appController, helper, queryFactory)
        {
        }

        public Task<ActionResult<TEntity>> PostAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Post))
            {
                return ProtectedPostAsync();
            }
            return Task.FromResult((ActionResult<TEntity>)NotFound());
        }

        public Task<ActionResult<TEntity>> PutByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return ProtectedPutAsync(id);
            }
            return Task.FromResult((ActionResult<TEntity>)NotFound());
        }

        public Task<ActionResult<IEnumerable<TEntity>>> PutAsync()
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Put))
            {
                return ProtectedPutAsync();
            }
            return Task.FromResult((ActionResult<IEnumerable<TEntity>>)NotFound());
        }

        public Task<ActionResult> DeleteByIdAsync(TKey id)
        {
            if (AllowedHttpVerbs.HasVerb(HttpVerbs.Delete))
            {
                return ProtectedDeleteAsync(id);
            }
            return Task.FromResult((ActionResult)NotFound());
        }

        protected virtual async Task<ActionResult<TEntity>> ProtectedPostAsync()
        {
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Post);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.CreateAsync(candidate, query);

            return Ok(entity);
        }

        protected virtual async Task<ActionResult<TEntity>> ProtectedPutAsync(TKey id)
        {
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Put);
            ICandidate<TEntity, TKey> candidate = Helper.CreateCandidate();

            TEntity entity = await AppController.UpdateByIdAsync(id, candidate, query);

            return Ok(entity);
        }

        protected virtual async Task<ActionResult<IEnumerable<TEntity>>> ProtectedPutAsync()
        {
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Put);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            //Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
            if (candidates.Any(c => !c.HasId()))
            {
                throw new BadRequestException("PUT on collection implies that you provide an array of objets each of which with an id attribute");
            }

            var candidatesByIds = candidates.ToDictionary(c => c.Id);

            IEnumerable<TEntity> entities = await AppController.UpdateByIdsAsync(candidatesByIds, query);

            return Ok(entities);
        }

        protected virtual async Task<ActionResult> ProtectedDeleteAsync(TKey id)
        {
            await AppController.DeleteByIdAsync(id);

            return Ok();
        }

        protected virtual async Task<IActionResult> ProtectedDeleteAsync()
        {
            Query<TEntity> query = QueryFactory.NewFromHttpRequest<TEntity, TKey>(HttpVerbs.Delete);
            IEnumerable<ICandidate<TEntity, TKey>> candidates = Helper.CreateCandidates();

            //Datas est censé contenir un tableau d'objet ayant une prop "id" qui permet de les identifier individuellement
            if (candidates.Any(c => !c.HasId()))
            {
                throw new BadRequestException("DELETE on collection implies that you provide an array of objets each of which with an id attribute");
            }

            var ids = candidates.Select(c => c.Id).ToList();

            await AppController.DeleteByIdsAsync(ids);

            return Ok();
        }
    }
}