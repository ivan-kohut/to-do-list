using Xunit;

namespace TodoList.Items.IntegrationTests.Infrastructure
{
    [CollectionDefinition(nameof(IntegrationTestCollection))]
    public class IntegrationTestCollection : ICollectionFixture<ItemsWebApplicationFactory>
    {
    }
}
