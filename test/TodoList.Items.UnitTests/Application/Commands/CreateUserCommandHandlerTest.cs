using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using TodoList.Items.Domain.Shared;
using Xunit;

namespace TodoList.Items.UnitTests.Application.Commands
{
    public class CreateUserCommandHandlerTest
    {
        private readonly Mock<IUnitOfWork> mockUnitOfWork;
        private readonly Mock<IUserRepository> mockUserRepository;

        private readonly CreateUserCommand command;
        private readonly IRequestHandler<CreateUserCommand> handler;

        public CreateUserCommandHandlerTest()
        {
            this.mockUnitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
            this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);

            this.command = new CreateUserCommand(10);

            this.handler = new CreateUserCommandHandler(
                mockUnitOfWork.Object,
                mockUserRepository.Object);
        }

        [Fact]
        public async Task When_UserExists_Expect_ArgumentException()
        {
            mockUserRepository
                .Setup(r => r.GetUserAsync(command.IdentityId))
                .ReturnsAsync(new User(command.IdentityId));

            // Act
            Func<Task> act = async () => await handler.Handle(command, default);

            await act.Should().ThrowExactlyAsync<ArgumentException>();
        }

        [Fact]
        public async Task Expect_Created()
        {
            mockUserRepository
                .Setup(r => r.GetUserAsync(command.IdentityId))
                .ReturnsAsync((User?)null);

            mockUserRepository
                .Setup(r => r.Create(It.IsAny<User>()))
                .Verifiable();

            mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(default))
                .Returns(Task.CompletedTask);

            // Act
            await handler.Handle(command, default);
        }
    }
}
