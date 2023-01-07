using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Threading.Tasks;
using TodoList.Items.API.Application.Commands;
using TodoList.Items.API.Application.Models;
using Xunit;

namespace TodoList.Items.UnitTests.Application.Commands
{
    public class RemoveCachedItemsCommandHandlerTest
    {
        private readonly Mock<IMediator> mockMediator;
        private readonly Mock<IMemoryCache> mockMemoryCache;

        private readonly RemoveCachedItemsCommand<CreateItemCommand, ItemDTO> command;
        private readonly IRequestHandler<RemoveCachedItemsCommand<CreateItemCommand, ItemDTO>, ItemDTO> handler;

        public RemoveCachedItemsCommandHandlerTest()
        {
            this.mockMediator = new Mock<IMediator>(MockBehavior.Strict);
            this.mockMemoryCache = new Mock<IMemoryCache>(MockBehavior.Strict);

            this.command = new RemoveCachedItemsCommand<CreateItemCommand, ItemDTO>(new CreateItemCommand("test_text", 10));

            this.handler = new RemoveCachedItemsCommandHandler<CreateItemCommand, ItemDTO>(
                mockMediator.Object,
                mockMemoryCache.Object);
        }

        [Fact]
        public async Task Expect_Removed()
        {
            ItemDTO expectedItem = new(default, false, command.Command.Text, 1);

            mockMediator
                .Setup(m => m.Send(command.Command, default))
                .ReturnsAsync(expectedItem);

            mockMemoryCache
                .Setup(c => c.Remove(command.Command.IdentityId))
                .Verifiable();

            // Act
            ItemDTO actualItem = await handler.Handle(command, default);

            actualItem.Should().BeEquivalentTo(expectedItem);
        }
    }
}
