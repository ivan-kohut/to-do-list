using Entities;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using Repositories;
using Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Services.Tests
{
  public class UserServiceTest
  {
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly IUserService userService;

    public UserServiceTest()
    {
      this.mockUserRepository = new Mock<IUserRepository>(MockBehavior.Strict);
      this.mockTransactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

      this.userService = new UserService(mockUserRepository.Object, mockTransactionManager.Object);
    }

    public class GetByIdAsync : UserServiceTest
    {
      [Fact]
      public async Task When_UserDoesNotExist_Expect_EntityNotFoundException()
      {
        int userId = 10;
        User user = null;

        mockUserRepository
          .Setup(r => r.GetByIdAsync(userId))
          .ReturnsAsync(user);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => userService.GetByIdAsync(userId));

        mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once());
      }

      [Fact]
      public async Task When_UserExists_Expect_Returned()
      {
        int userId = 10;

        User user = new User
        {
          Id = userId,
          UserName = "testUserName",
          Email = "testUserEmail",
          EmailConfirmed = true
        };

        mockUserRepository
          .Setup(r => r.GetByIdAsync(userId))
          .ReturnsAsync(user);

        UserDTO expected = new UserDTO
        {
          Id = userId,
          Name = user.UserName,
          Email = user.Email,
          IsEmailConfirmed = user.EmailConfirmed
        };

        // Act
        UserDTO actual = await userService.GetByIdAsync(userId);

        actual.ShouldBeEquivalentTo(expected);

        mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once());
      }
    }

    public class GetAllAsync : UserServiceTest
    {
      [Fact]
      public async Task When_UsersDoNotExist_Expect_EmptyList()
      {
        mockUserRepository
          .Setup(r => r.GetAll())
          .Returns(Enumerable.Empty<User>().AsQueryable().BuildMock().Object);

        // Act
        Assert.Empty(await userService.GetAllAsync());

        mockUserRepository.Verify(r => r.GetAll(), Times.Once());
      }

      [Fact]
      public async Task When_UsersExist_Expect_Returned()
      {
        User firstUser = new User { Id = 2, UserName = "firstUserName", Email = "firstEmail", EmailConfirmed = true };
        User secondUser = new User { Id = 3, UserName = "secondUserName", Email = "secondEmail", EmailConfirmed = false };

        mockUserRepository
          .Setup(r => r.GetAll())
          .Returns(new List<User> { firstUser, secondUser }.AsQueryable().BuildMock().Object);

        IEnumerable<UserDTO> expected = new List<UserDTO>
        {
          new UserDTO { Id = 2, Name = "firstUserName", Email = "firstEmail", IsEmailConfirmed = true },
          new UserDTO { Id = 3, Name = "secondUserName", Email = "secondEmail", IsEmailConfirmed = false }
        };

        // Act
        IEnumerable<UserDTO> actual = await userService.GetAllAsync();

        actual.ShouldBeEquivalentTo(expected);

        mockUserRepository.Verify(r => r.GetAll(), Times.Once());
      }

      [Fact]
      public async Task Expect_AdminIsNotReturned()
      {
        User firstUser = new User { Id = 1, UserName = "firstUserName", Email = "firstEmail", EmailConfirmed = true };
        User secondUser = new User { Id = 2, UserName = "secondUserName", Email = "secondEmail", EmailConfirmed = false };

        mockUserRepository
          .Setup(r => r.GetAll())
          .Returns(new List<User> { firstUser, secondUser }.AsQueryable().BuildMock().Object);

        IEnumerable<UserDTO> expected = new List<UserDTO>
        {
          new UserDTO { Id = 2, Name = "secondUserName", Email = "secondEmail", IsEmailConfirmed = false }
        };

        // Act
        IEnumerable<UserDTO> actual = await userService.GetAllAsync();

        actual.ShouldBeEquivalentTo(expected);

        mockUserRepository.Verify(r => r.GetAll(), Times.Once());
      }
    }

    public class DeleteAsync : UserServiceTest
    {
      [Fact]
      public async Task When_UserIsAdmin_Expect_ArgumentException()
      {
        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => userService.DeleteAsync(1));
      }

      [Fact]
      public async Task When_UserIsNotFound_Expect_EntityNotFoundException()
      {
        int userId = 10;
        User user = null;

        mockUserRepository
          .Setup(r => r.GetByIdAsync(userId))
          .ReturnsAsync(user);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => userService.DeleteAsync(userId));

        mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once());
      }

      [Fact]
      public async Task When_UserExists_Expect_Deleted()
      {
        int userId = 10;
        User user = new User { Id = userId };

        mockUserRepository
          .Setup(r => r.GetByIdAsync(userId))
          .ReturnsAsync(user);

        mockUserRepository
          .Setup(r => r.Delete(user))
          .Verifiable();

        mockTransactionManager
          .Setup(m => m.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await userService.DeleteAsync(userId);

        mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once());
        mockUserRepository.Verify(r => r.Delete(user), Times.Once());
        mockTransactionManager.Verify(m => m.SaveChangesAsync(), Times.Once());
      }
    }
  }
}
