using Entities;
using Moq;
using Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Services.Tests
{
  public class UserServiceTest
  {
    private const int identityId = 10;

    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly IUserService userService;

    public UserServiceTest()
    {
      this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
      this.mockTransactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

      this.userService = new UserService(mockUserRepository.Object, mockTransactionManager.Object);
    }

    public class SaveAsync : UserServiceTest
    {
      [Fact]
      public async Task When_UserExists_Expect_ArgumentException()
      {
        mockUserRepository
          .Setup(r => r.GetUserAsync(identityId))
          .ReturnsAsync(new User { IdentityId = identityId });

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => userService.SaveAsync(identityId));

        mockUserRepository.Verify(r => r.GetUserAsync(identityId), Times.Once);
      }

      [Fact]
      public async Task Expect_Saved()
      {
        mockUserRepository
          .Setup(r => r.GetUserAsync(identityId))
          .ReturnsAsync((User?)null);

        mockUserRepository
          .Setup(r => r.Create(It.Is<User>(u => u.IdentityId == identityId)))
          .Verifiable();

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await userService.SaveAsync(identityId);

        mockUserRepository.Verify(r => r.GetUserAsync(identityId), Times.Once());
        mockUserRepository.Verify(r => r.Create(It.IsAny<User>()), Times.Once());
        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }
  }
}
