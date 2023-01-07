using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.API.Application.Models;
using TodoList.Items.API.Application.Queries;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using Xunit;

namespace TodoList.Items.UnitTests.Application.Queries
{
    public class GetItemsQueryHandlerTest
    {
        private readonly Mock<IItemRepository> mockItemRepository;
        private readonly Mock<IUserRepository> mockUserRepository;
        private readonly Mock<IMemoryCache> mockMemoryCache;
        private readonly Mock<ICacheEntry> mockCacheEntry;

        private readonly GetItemsQuery query;
        private readonly IRequestHandler<GetItemsQuery, IEnumerable<ItemDTO>> handler;

        public GetItemsQueryHandlerTest()
        {
            this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
            this.mockMemoryCache = new Mock<IMemoryCache>(MockBehavior.Strict);
            this.mockCacheEntry = new Mock<ICacheEntry>(MockBehavior.Strict);

            this.query = new GetItemsQuery(10);

            this.handler = new GetItemsQueryHandler(
                mockItemRepository.Object,
                mockUserRepository.Object,
                mockMemoryCache.Object);
        }

        [Fact]
        public async Task When_ItemsExistInCache_Expect_ReturnedFromCache()
        {
            IEnumerable<ItemDTO> expectedItems = new[]
            {
                new ItemDTO(1, false, "test_text1", 1),
                new ItemDTO(5, true, "test_text2", 2)
            };

            object? value = expectedItems;

            mockMemoryCache
                .Setup(c => c.TryGetValue(query.IdentityId, out value))
                .Returns(true);

            // Act
            IEnumerable<ItemDTO> actualItems = await handler.Handle(query, default);

            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public async Task When_UserIsNull_Expect_EntityNotFoundException()
        {
            object? value = null;

            mockMemoryCache
                .Setup(c => c.TryGetValue(query.IdentityId, out value))
                .Returns(false);

            mockUserRepository
                .Setup(r => r.GetUserAsync(query.IdentityId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await handler.Handle(query, default);

            await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task When_ItemsDoNotExistInCache_Expect_CachedAndReturned()
        {
            object? value = null;

            mockMemoryCache
                .Setup(c => c.TryGetValue(query.IdentityId, out value))
                .Returns(false);

            User user = new(query.IdentityId);

            mockUserRepository
                .Setup(r => r.GetUserAsync(query.IdentityId))
                .ReturnsAsync(user);

            IEnumerable<Item> items = new[]
            {
                new Item(user.Id, "test_text1", 1, ItemStatus.Todo),
                new Item(user.Id, "test_text2", 2, ItemStatus.Done)
            };

            mockItemRepository
                .Setup(r => r.GetAllAsync(user.Id))
                .ReturnsAsync(items);

            mockMemoryCache
                .Setup(c => c.CreateEntry(query.IdentityId))
                .Returns(mockCacheEntry.Object);

            mockCacheEntry
                .Setup(c => c.Dispose())
                .Verifiable();

            mockCacheEntry.SetupAllProperties();

            // Act
            IEnumerable<ItemDTO> actualItems = await handler.Handle(query, default);

            IEnumerable<ItemDTO> expectedItems = items
                .Select(i => new ItemDTO(i.Id, i.IsDone, i.Text, i.Priority))
                .ToList();

            actualItems.Should().BeEquivalentTo(expectedItems);
        }
    }
}
