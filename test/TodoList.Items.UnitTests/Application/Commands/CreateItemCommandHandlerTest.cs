using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.API.Application.Exceptions;
using TodoList.Items.API.Application.Models;
using TodoList.Items.Domain.Aggregates.ItemAggregate;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;
using Xunit;

namespace TodoList.Items.UnitTests.Application.Commands
{
    public class CreateItemCommandHandlerTest
    {
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<IItemRepository> mockItemRepository;
        private readonly Mock<IUserRepository> mockUserRepository;

        private readonly CreateItemCommand command;
        private readonly IRequestHandler<CreateItemCommand, ItemDTO> handler;

        public CreateItemCommandHandlerTest()
        {
            this.mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            this.mockItemRepository = new Mock<IItemRepository>(MockBehavior.Strict);
            this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            this.command = new CreateItemCommand("test_text", 10);

            this.handler = new CreateItemCommandHandler(
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
        public async Task Expect_Created()
        {
            User user = new(command.IdentityId);

            mockUserRepository
                .Setup(r => r.GetUserAsync(command.IdentityId))
                .ReturnsAsync(user);

            mockItemRepository
                .Setup(r => r.GetMaxItemPriorityAsync(user.Id))
                .ReturnsAsync(3);

            mockItemRepository
                .Setup(r => r.Create(It.IsAny<Item>()))
                .Verifiable();

            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(default))
                .Returns(Task.CompletedTask);

            // Act
            ItemDTO actualItem = await handler.Handle(command, default);

            ItemDTO expectedItem = new(default, false, command.Text, 4);

            actualItem.Should().BeEquivalentTo(expectedItem);
        }
    }
}
