using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Controllers.Tests
{
  [Collection(nameof(IntegrationTestCollection))]
  public class UsersControllerTest : ApiControllerTestBase, IDisposable
  {
    private const string url = "/api/v1/users";

    public UsersControllerTest(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    public class GetAllAsync : UsersControllerTest
    {
      public GetAllAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_UsersExist_Expect_Returned()
      {
        IEnumerable<User> users = new List<User>
        {
          new User { UserName = "firstUserName", Email = "firstEmail", EmailConfirmed = true },
          new User { UserName = "secondUserName", Email = "secondEmail", EmailConfirmed = false }
        };

        IEnumerable<UserDTO> expected = await Task.WhenAll(users.Select(u => SaveUserAsync(u)));

        // Act
        HttpResponseMessage response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        IEnumerable<UserDTO> actual = DeserializeResponseBody<IEnumerable<UserDTO>>(response);

        actual.ShouldBeEquivalentTo(expected);
      }
    }

    public class GetUserItemsAsync : UsersControllerTest
    {
      public GetUserItemsAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_UserItemsExist_Expect_Returned()
      {
        UserDTO userDTO = await SaveUserAsync(new User { UserName = "userName", Email = "email", EmailConfirmed = true });

        IEnumerable<Item> items = new List<Item>
        {
          new Item { UserId = userDTO.Id, Text = "firstItemText", Priority = 2, Status = ItemStatus.Todo },
          new Item { UserId = userDTO.Id, Text = "secondItemText", Priority = 1, Status = ItemStatus.Done }
        };

        IEnumerable<ItemDTO> expected = (await Task.WhenAll(items.Select(i => SaveItemAsync(i))))
          .OrderBy(i => i.Priority)
          .ToList();

        // Act
        HttpResponseMessage response = await GetAsync($"{url}/{userDTO.Id}/items");

        response.EnsureSuccessStatusCode();

        IEnumerable<ItemDTO> actual = DeserializeResponseBody<IEnumerable<ItemDTO>>(response);

        actual.ShouldBeEquivalentTo(expected);
      }
    }

    public class DeleteAsync : UsersControllerTest
    {
      public DeleteAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_UserDoesNotExist_Expect_NotFound()
      {
        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{10}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }

      [Fact]
      public async Task When_UserExists_Expect_Deleted()
      {
        UserDTO userDTO = await SaveUserAsync(new User { UserName = "userName", Email = "email", EmailConfirmed = true });

        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{userDTO.Id}");

        response.EnsureSuccessStatusCode();

        using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
        {
          User user = await appDbContext
            .Users
            .SingleOrDefaultAsync(i => i.Id == userDTO.Id);

          Assert.Null(user);
        }
      }
    }

    public class LoginAsync : UsersControllerTest
    {
      private const string loginAPI = url + "/login";

      public LoginAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_InputModelIsNotValid_Expect_BadRequest()
      {
        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserIsNotFound_Expect_NotFound()
      {
        object body = new
        {
          Email = "test@test.test",
          Password = "test password"
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }

      [Fact]
      public async Task When_PasswordIsNotValid_Expect_BadRequest()
      {
        string userEmail = "user@user.user";

        User user = new User { UserName = "userName", Email = userEmail, NormalizedEmail = userEmail.ToUpper() };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, "user-password");
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          user.Email,
          Password = "wrong-user-password"
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_EmailIsNotConfirmed_Expect_BadRequest()
      {
        string userEmail = "user@user.user";
        string userPassword = "user-password";

        User user = new User { UserName = "userName", Email = userEmail, NormalizedEmail = userEmail.ToUpper() };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPassword);
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          user.Email,
          Password = userPassword
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_TwoFactorIsNotEnabled_Expect_AccessTokenIsReturned()
      {
        string userEmail = "user@user.user";
        string userPassword = "user-password";

        User user = new User { UserName = "userName", Email = userEmail, NormalizedEmail = userEmail.ToUpper(), EmailConfirmed = true };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPassword);
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          user.Email,
          Password = userPassword
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        response.EnsureSuccessStatusCode();

        Assert.True(!string.IsNullOrWhiteSpace(DeserializeResponseBody<string>(response)));
      }

      [Fact]
      public async Task When_TwoFactorTokenIsNotPresented_Expect_CustomHttpStatusCodeIsReturned()
      {
        string userEmail = "user@user.user";
        string userPassword = "user-password";

        User user = new User
        {
          UserName = "userName",
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper(),
          EmailConfirmed = true,
          TwoFactorEnabled = true
        };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPassword);
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          user.Email,
          Password = userPassword
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        Assert.Equal(427, (int)response.StatusCode);
      }

      [Fact]
      public async Task When_TwoFactorTokenIsNotValid_Expect_BadRequest()
      {
        string userEmail = "user@user.user";
        string userPassword = "user-password";

        User user = new User
        {
          UserName = "userName",
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper(),
          EmailConfirmed = true,
          TwoFactorEnabled = true,
          UserTokens = new List<UserToken>
          {
            new UserToken
            {
              LoginProvider = "[AspNetUserStore]",
              Name = "AuthenticatorKey",
              Value = new string('a', 15)
            }
          }
        };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPassword);
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          user.Email,
          Password = userPassword,
          TwoFactorToken = "555555"
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }
    }

    public class CreateUserAsync : UsersControllerTest
    {
      public CreateUserAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_InputModelIsNotValid_Expect_BadRequest()
      {
        object body = new
        {
          Email = string.Empty,
          Name = string.Empty,
          Password = string.Empty,
          ConfirmPassword = string.Empty
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserAlreadyExistsWithSpecifiedEmail_Expect_BadRequest()
      {
        string userEmail = "user@user.user";
        string userPassword = "user-password";

        User user = new User
        {
          UserName = "userName",
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper()
        };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPassword);
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          user.Email,
          Name = user.UserName,
          Password = userPassword,
          ConfirmPassword = userPassword
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserAlreadyExistsWithSpecifiedName_Expect_BadRequest()
      {
        string userName = "userName";
        string userEmail = "user@user.user";
        string userPassword = "user-password";

        User user = new User
        {
          UserName = userName,
          NormalizedUserName = userName.ToUpper(),
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper()
        };

        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          user.PasswordHash = userManager.PasswordHasher.HashPassword(user, userPassword);
        }

        UserDTO userDTO = await SaveUserAsync(user);

        object body = new
        {
          Email = "test-email.test@test",
          Name = user.UserName,
          Password = "abtRHh54rh.cAK",
          ConfirmPassword = "abtRHh54rh.cAK"
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserDoesNotExistInDataBase_Expect_Created()
      {
        var body = new
        {
          Email = "test-email.test@test",
          Name = "userName",
          Password = "abtRHh54rh.cAK",
          ConfirmPassword = "abtRHh54rh.cAK"
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        response.EnsureSuccessStatusCode();

        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        {
          User user = await dbContext
            .Users
            .Include(u => u.UserRoles)
            .ThenInclude(u => u.Role)
            .SingleOrDefaultAsync(u => u.Email == body.Email);

          Assert.NotNull(user);

          Assert.Equal(body.Name, user.UserName);

          Assert.Equal(1, user.UserRoles.Count);
          Assert.Equal("user", user.UserRoles.First().Role.Name);
        }
      }

      [Fact]
      public async Task When_UserExistsWithoutPassword_Expect_BadRequestRelatedToUserName()
      {
        string userName = "userName";

        await SaveUserAsync(new User
        {
          UserName = userName,
          NormalizedUserName = userName.ToUpper()
        });

        string userEmail = "user@user.user";

        User user = new User
        {
          UserName = userEmail,
          NormalizedUserName = userEmail.ToUpper(),
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper()
        };

        await SaveUserAsync(user);

        object body = new
        {
          Email = userEmail,
          Name = userName,
          Password = "abtRHh54rh.cAK",
          ConfirmPassword = "abtRHh54rh.cAK"
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserExistsWithoutPassword_Expect_BadRequestRelatedToUserPassword()
      {
        string userEmail = "user@user.user";

        User user = new User
        {
          UserName = userEmail,
          NormalizedUserName = userEmail.ToUpper(),
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper()
        };

        await SaveUserAsync(user);

        object body = new
        {
          Email = userEmail,
          Name = "userName",
          Password = "abcABC",
          ConfirmPassword = "abcABC"
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserExistsWithoutPassword_Expect_UserNameAndPasswordIsUpdated()
      {
        string userEmail = "user@user.user";

        User user = new User
        {
          UserName = userEmail,
          NormalizedUserName = userEmail.ToUpper(),
          Email = userEmail,
          NormalizedEmail = userEmail.ToUpper()
        };

        await SaveUserAsync(user);

        var body = new
        {
          Email = userEmail,
          Name = "userName",
          Password = "abtRHh54rh.cAK",
          ConfirmPassword = "abtRHh54rh.cAK"
        };

        // Act
        HttpResponseMessage response = await PostAsync(url, body);

        response.EnsureSuccessStatusCode();

        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        {
          user = await dbContext
            .Users
            .SingleOrDefaultAsync(u => u.Email == body.Email);

          Assert.Equal(body.Name, user.UserName);
        }
      }
    }

    public void Dispose()
    {
      using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
      {
        DbSet<User> users = appDbContext.Set<User>();
        users.RemoveRange(users.Where(u => u.Id != 1));

        appDbContext.SaveChanges();
      }
    }
  }
}
