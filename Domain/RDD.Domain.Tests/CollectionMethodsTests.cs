﻿using Moq;
using RDD.Domain.Exceptions;
using RDD.Domain.Helpers;
using RDD.Domain.Models;
using RDD.Domain.Tests.Models;
using RDD.Domain.Tests.Templates;
using RDD.Domain.WebServices;
using System.Collections.Generic;
using Xunit;

namespace RDD.Domain.Tests
{
	public class CollectionMethodsTests : SingleContextTests
	{
		[Fact]
		public async void GetById_SHOULD_throw_exception_WHEN_id_does_not_exist()
		{
			var user = new User { Id = 1 };
			var users = new UsersCollection(_storage, _execution, null, _newStorage);

			await users.CreateAsync(user);

			await _storage.CommitAsync();

			await Assert.ThrowsAsync<NotFoundException>(() => users.GetByIdAsync(0));
		}

		[Fact]
		public async void TryGetById_SHOULD_not_throw_exception_and_return_null_WHEN_id_does_not_exist()
		{
			var user = new User { Id = 2 };
			var users = new UsersCollection(_storage, _execution, null, _newStorage);

			await users.CreateAsync(user);

			await _storage.CommitAsync();

			Assert.Null(await users.TryGetByIdAsync(0));
		}

		[Fact]
		public async void Put_SHOULD_throw_notfound_exception_WHEN_unexisting_entity_()
		{
			_execution.curPrincipal = new WebService { Id = 1, AppOperations = new HashSet<int>() { 1 } };
			
			var mock = new Mock<ICombinationsHolder>();
			mock.Setup(h => h.Combinations)
			.Returns(new HashSet<Combination>() {
				new Combination { Operation = new Operation { Id = 1 }, Subject = typeof(User), Verb = HttpVerb.PUT }
			});
			var combinationsHolder = mock.Object;

			var user = new User { Id = 3 };
			var users = new UsersCollection(_storage, _execution, combinationsHolder, _newStorage);

			await users.CreateAsync(user);

			await _storage.CommitAsync();

			await Assert.ThrowsAsync<NotFoundException>(() => users.UpdateAsync(0, new { name = "new name" }));
		}
	}
}
