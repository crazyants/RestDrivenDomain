﻿using RDD.Domain.Models;

namespace RDD.Web.Tests.Models
{
    public class AnotherUser : EntityBase<IUser, int>, IUser
    {
        public override int Id { get; set; }
        public override string Name { get; set; }
    }
}
