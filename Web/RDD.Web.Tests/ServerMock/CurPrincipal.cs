using System.Collections.Generic;
using System.Linq;
using RDD.Domain;
using RDD.Domain.Helpers;

namespace RDD.Web.Tests.ServerMock
{
    public class CurPrincipal : IPrincipal
    {
        public int Id { get; }
        public string Token { get; set; }
        public string Name { get; }
        public Culture Culture { get; }
        public bool HasOperation(int operation) => true;

        public bool HasAnyOperations(HashSet<int> operations) => true;

        public HashSet<int> GetOperations(HashSet<int> operations) => operations;

        public IQueryable<TEntity> ApplyRights<TEntity>(IQueryable<TEntity> entities, HashSet<int> operations) => entities;
    }
}