﻿using RDD.Application.Controllers;
using RDD.Domain.Models;
using RDD.Infra.Storage;
using RDD.Web.Helpers;
using RDD.Web.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RDD.Web.Tests
{
    public class WebControllerTests
    {
        [Fact]
        public async Task WebControllerShouldWorkOnInterfaces()
        {
            using (var storage = new InMemoryStorageService())
            {
                var repository = new Repository<IUser>(storage, null, null);
                var collection = new ReadOnlyRestCollection<IUser, int>(repository, null, null);
                var appController = new ReadOnlyAppController<IUser, int>(collection);

                repository.Add(new User { Id = 1 });
                repository.Add(new AnotherUser { Id = 2 });

                var controller = new IUserWebController(appController, new ApiHelper<IUser, int>(null, null, null));

                var results = await controller.GetEnumerableAsync(); //Simplified equivalent to GetAsync()

                Assert.Equal(2, results.Count());
            }
        }
    }
}