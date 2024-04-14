using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;
using Xunit;

namespace TodoList.Items.UnitTests.Application.Commands
{
    public class DeleteItemCommandHandlerTest
    {
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<IItemRepository> mockItemRepository;
        private readonly Mock<IUserRepository> mockUserRepository;

        private readonly DeleteItemCommand command;
        private readonly DeleteItemCommandHandler handler;

        public DeleteItemCommandHandlerTest()
        {
            this.mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            this.command = new DeleteItemCommand(5, 10);

            this.handler = new DeleteItemCommandHandler(
                mockUnitOfWork.Object,
                mockItemRepository.Object,
                mockUserRepository.Object);
        }

        [Fact]
        public async Task When_UserIsNull_Expect_EntityNotFoundException()
        {
            mockUserRepository
                .Setup(r => r.GetUserAsync(command.IdentityId))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = async () => await handler.Handle(command, default);

            await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task When_ItemIsNull_Expect_EntityNotFoundException()
        {
            User user = new(command.IdentityId);

            mockUserRepository
                .Setup(r => r.GetUserAsync(command.IdentityId))
                .ReturnsAsync(user);

            mockItemRepository
                .Setup(r => r.GetByIdAndUserIdAsync(command.ItemId, user.Id))
                .ReturnsAsync((Item?)null);

            // Act
            Func<Task> act = async () => await handler.Handle(command, default);

            await act.Should().ThrowExactlyAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task Expect_Deleted()
        {
            User user = new(command.IdentityId);

            mockUserRepository
                .Setup(r => r.GetUserAsync(command.IdentityId))
                .ReturnsAsync(user);

            Item item = new(user.Id, "test_text", 1);

            mockItemRepository
                .Setup(r => r.GetByIdAndUserIdAsync(command.ItemId, user.Id))
                .ReturnsAsync(item);

            mockItemRepository
                .Setup(r => r.Delete(item))
                .Verifiable();

            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(default))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(command, default);
        }
    }
}
