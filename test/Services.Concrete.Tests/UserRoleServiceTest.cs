using Entities;
using Moq;
using Repositories;
using Services.Exceptions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Services.Tests
{
  public class UserRoleServiceTest
  {
    private readonly Mock<IUserService> mockUserService;
    private readonly Mock<IRoleRepository> mockRoleRepository;
    private readonly Mock<IUserRoleRepository> mockUserRoleRepository;
    private readonly Mock<ITransactionManager> mockTransactionManager;

    private readonly IUserRoleService userRoleService;

    public UserRoleServiceTest()
    {
      this.mockUserService = new Mock<IUserService>(MockBehavior.Strict);
      this.mockRoleRepository = new Mock<IRoleRepository>(MockBehavior.Strict);
      this.mockUserRoleRepository = new Mock<IUserRoleRepository>(MockBehavior.Strict);
      this.mockTransactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

      this.userRoleService = new UserRoleService(mockUserService.Object, mockRoleRepository.Object, mockUserRoleRepository.Object, mockTransactionManager.Object);
    }

    public class CreateAsync : UserRoleServiceTest
    {
      [Fact]
      public async Task When_UserDoesNotExist_Expect_EntityNotFoundException()
      {
        int userId = 10;

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ThrowsAsync(new EntityNotFoundException("exception message"));

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => userRoleService.CreateAsync(userId, string.Empty));

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
      }

      [Fact]
      public async Task When_RoleDoesNotExist_Expect_EntityNotFoundException()
      {
        int userId = 10;
        string roleName = "user";

        UserDTO user = new UserDTO();
        Role role = null;

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ReturnsAsync(user);

        mockRoleRepository
          .Setup(r => r.GetByNameAsync(roleName))
          .ReturnsAsync(role);

        // Act
        await Assert.ThrowsAsync<EntityNotFoundException>(() => userRoleService.CreateAsync(userId, roleName));

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
        mockRoleRepository.Verify(r => r.GetByNameAsync("user"), Times.Once());
      }

      [Fact]
      public async Task When_UserAlreadyHasRole_Expect_ArgumentException()
      {
        int userId = 10;
        int roleId = 20;
        string roleName = "user";

        UserDTO user = new UserDTO();
        Role role = new Role { Id = roleId };

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ReturnsAsync(user);

        mockRoleRepository
          .Setup(r => r.GetByNameAsync(roleName))
          .ReturnsAsync(role);

        mockUserRoleRepository
          .Setup(r => r.GetByUserIdAndRoleIdAsync(userId, roleId))
          .ReturnsAsync(new UserRole());

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => userRoleService.CreateAsync(userId, roleName));

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
        mockRoleRepository.Verify(r => r.GetByNameAsync("user"), Times.Once());
        mockUserRoleRepository.Verify(r => r.GetByUserIdAndRoleIdAsync(userId, roleId), Times.Once());
      }

      [Fact]
      public async Task Expect_RoleIsAddedToUser()
      {
        int userId = 10;
        int roleId = 20;
        string roleName = "user";

        UserDTO user = new UserDTO { Id = userId };
        Role role = new Role { Id = roleId };
        UserRole userRole = null;

        mockUserService
          .Setup(s => s.GetByIdAsync(userId))
          .ReturnsAsync(user);

        mockRoleRepository
          .Setup(r => r.GetByNameAsync(roleName))
          .ReturnsAsync(role);

        mockUserRoleRepository
          .Setup(r => r.GetByUserIdAndRoleIdAsync(userId, roleId))
          .ReturnsAsync(userRole);

        mockUserRoleRepository
          .Setup(r => r.CreateAsync(It.Is<UserRole>(u => u.UserId == userId && u.RoleId == roleId)))
          .Returns(Task.CompletedTask);

        mockTransactionManager
          .Setup(t => t.SaveChangesAsync())
          .Returns(Task.CompletedTask);

        // Act
        await userRoleService.CreateAsync(userId, roleName);

        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once());
        mockRoleRepository.Verify(r => r.GetByNameAsync("user"), Times.Once());
        mockUserRoleRepository.Verify(r => r.GetByUserIdAndRoleIdAsync(userId, roleId), Times.Once());
        mockUserRoleRepository.Verify(r => r.CreateAsync(It.Is<UserRole>(u => u.UserId == userId && u.RoleId == roleId)), Times.Once());
        mockTransactionManager.Verify(t => t.SaveChangesAsync(), Times.Once());
      }
    }
  }
}
