﻿using MediatR;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;

namespace TodoList.Items.API.Application.Commands
{
    public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IItemRepository itemRepository;
        private readonly IUserRepository userRepository;

        public DeleteItemCommandHandler(
            IUnitOfWork unitOfWork,
            IItemRepository itemRepository,
            IUserRepository userRepository)
        {
            this.unitOfWork = unitOfWork;
            this.itemRepository = itemRepository;
            this.userRepository = userRepository;
        }

        public async Task<Unit> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
        {
            User? user = await userRepository.GetUserAsync(request.IdentityId);

            if (user is null)
            {
                throw new EntityNotFoundException($"User with identity id {request.IdentityId} is not found");
            }

            Item? item = await itemRepository.GetByIdAndUserIdAsync(request.ItemId, user.Id);

            if (item is null)
            {
                throw new EntityNotFoundException($"Item with id {request.ItemId} is not found");
            }

            itemRepository.Delete(item);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
