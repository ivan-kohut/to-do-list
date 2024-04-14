﻿using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.API.Application.Models;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;

namespace TodoList.Items.API.Application.Queries
{
    public class GetItemsQueryHandler(
        IItemRepository itemRepository,
        IUserRepository userRepository,
        IMemoryCache memoryCache) : IRequestHandler<GetItemsQuery, IEnumerable<ItemDTO>>
    {
        public async Task<IEnumerable<ItemDTO>> Handle(GetItemsQuery request, CancellationToken cancellationToken)
        {
            if (!memoryCache.TryGetValue(request.IdentityId, out IEnumerable<ItemDTO>? userItems) || userItems is null)
            {
                User currentUser = await userRepository.GetUserAsync(request.IdentityId)
                    ?? throw new EntityNotFoundException($"User with identity id {request.IdentityId} is not found");

                userItems = (await itemRepository
                  .GetAllAsync(currentUser.Id))
                  .Select(i => new ItemDTO(i.Id, i.IsDone, i.Text, i.Priority))
                  .ToList();

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                  .SetSlidingExpiration(TimeSpan.FromMinutes(1));

                memoryCache.Set(request.IdentityId, userItems, cacheEntryOptions);
            }

            return userItems;
        }
    }
}
