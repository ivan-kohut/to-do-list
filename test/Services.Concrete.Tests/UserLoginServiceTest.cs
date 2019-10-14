using Entities;
using MockQueryable.Moq;
using Moq;
using Repositories;
using Services.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Services.Tests
{
  public class UserLoginServiceTest
  {
    private readonly Mock<IUserService> mockUserService;
    private readonly Mock<IUserLoginRepository> mockUserLoginRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly IUserLoginService userLoginService;

    public UserLoginServiceTest()
    {
      this.mockUserService = new Mock<IUserService>(MockBehavior.Strict);
      this.mockUserLoginRepository = new Mock<IUserLoginRepository>(MockBehavior.Strict);
      this.mockTransactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

      this.userLoginService = new UserLoginService(mockUserService.Object, mockUserLoginRepository.Object, mockTransactionManager.Object);
    }

    public class CreateAsync : UserLoginServiceTest
    {
      [Fact]
      public async Task When_UserDoesNotExist_Expect_EntityNotFoundException()
      {
        int userId = 10;

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ThrowsAsync(new EntityNotFoundException("exception-message"));

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => userLoginService.CreateAsync(userId, string.Empty, string.Empty));

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
      }

      [Fact]
      public async Task When_UserLoginAlreadyExists_Expect_ArgumentException()
      {
        int userId = 10;

        string loginProvider = "testLoginProvider";
        string providerKey = "testProviderKey";

        UserDTO user = new UserDTO { Id = userId };

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ReturnsAsync(user);

        mockUserLoginRepository
          .Setup(r => r.GetByLoginProviderAndProviderKeyAsync(loginProvider, providerKey))
          .ReturnsAsync(new UserLogin());

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => userLoginService.CreateAsync(userId, loginProvider, providerKey));

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
        mockUserLoginRepository.Verify(r => r.GetByLoginProviderAndProviderKeyAsync(loginProvider, providerKey), Times.Once());
      }

      [Fact]
      public async Task Expect_UserLoginIsCreated()
      {
        int userId = 10;

        string loginProvider = "testLoginProvider";
        string providerKey = "testProviderKey";

        UserDTO user = new UserDTO { Id = userId };
        UserLogin? userLogin = null;

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ReturnsAsync(user);

        mockUserLoginRepository
          .Setup(r => r.GetByLoginProviderAndProviderKeyAsync(loginProvider, providerKey))
          .ReturnsAsync(userLogin);

        mockUserLoginRepository
          .Setup(r => r.CreateAsync(It.Is<UserLogin>(l => l.UserId == userId && l.LoginProvider == loginProvider && l.ProviderDisplayName == loginProvider && l.ProviderKey == providerKey)))
          .Returns(Task.CompletedTask);

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await userLoginService.CreateAsync(userId, loginProvider, providerKey);

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
        mockUserLoginRepository.Verify(r => r.GetByLoginProviderAndProviderKeyAsync(loginProvider, providerKey), Times.Once());
        mockUserLoginRepository.Verify(r => r.CreateAsync(It.Is<UserLogin>(l => l.UserId == userId && l.LoginProvider == loginProvider && l.ProviderDisplayName == loginProvider && l.ProviderKey == providerKey)), Times.Once());
        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }

    public class ExistsWithUserIdAndLoginProviderAsync : UserLoginServiceTest
    {
      [Fact]
      public async Task When_LoginsDoNotExist_Expect_False()
      {
        int userId = 10;
        string loginProvider = "testLoginProvider";

        mockUserLoginRepository
          .Setup(r => r.GetAll())
          .Returns(Enumerable.Empty<UserLogin>().AsQueryable().BuildMock().Object);

        // Act
        Assert.False(await userLoginService.ExistsWithUserIdAndLoginProviderAsync(userId, loginProvider));

        mockUserLoginRepository.Verify(r => r.GetAll(), Times.Once());
      }

      [Fact]
      public async Task When_UserDoesNotHaveSpecifiedLoginProvider_Expect_False()
      {
        int userId = 10;
        string loginProvider = "testLoginProvider";

        UserLogin userLogin = new UserLogin
        {
          UserId = userId,
          LoginProvider = "some-login-provider"
        };

        mockUserLoginRepository
          .Setup(r => r.GetAll())
          .Returns(new[] { userLogin }.AsQueryable().BuildMock().Object);

        // Act
        Assert.False(await userLoginService.ExistsWithUserIdAndLoginProviderAsync(userId, loginProvider));

        mockUserLoginRepository.Verify(r => r.GetAll(), Times.Once());
      }

      [Fact]
      public async Task When_UserHasSpecifiedLoginProvider_Expect_True()
      {
        int userId = 10;
        string loginProvider = "testLoginProvider";

        UserLogin userLogin = new UserLogin
        {
          UserId = userId,
          LoginProvider = loginProvider
        };

        mockUserLoginRepository
          .Setup(r => r.GetAll())
          .Returns(new[] { userLogin }.AsQueryable().BuildMock().Object);

        // Act
        Assert.True(await userLoginService.ExistsWithUserIdAndLoginProviderAsync(userId, loginProvider));

        mockUserLoginRepository.Verify(r => r.GetAll(), Times.Once());
      }
    }
  }
}
