using FluentAssertions;
using TodoList.Items.Domain.Aggregates.UserAggregate;
using Xunit;

namespace TodoList.Items.UnitTests.Domain.Aggregates
{
    public class UserAggregateTest
    {
        [Fact]
        public void Expect_UserCreated()
        {
            int identityId = 1;

            // Act
            User actualUser = new(identityId);

            actualUser.Id.Should().Be(default);
            actualUser.IdentityId.Should().Be(identityId);
        }

        [Fact]
        public void Expect_IdentityIdUpdated()
        {
            User user = new(1);

            int newIdentityId = 2;

            // Act
            user.SetIdentityId(newIdentityId);

            user.IdentityId.Should().Be(newIdentityId);
        }
    }
}
