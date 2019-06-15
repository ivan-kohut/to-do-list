using Controllers.Tests.Extensions;
using Controllers.Tests.Fixtures;
using Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API.Models;
using Repositories;
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

        IEnumerable<UserListApiModel> expected = await Task.WhenAll(users.Select(u => SaveUserAsync(u)));

        // Act
        HttpResponseMessage response = await GetAsync(url);

        response.EnsureSuccessStatusCode();

        IEnumerable<UserListApiModel> actual = await DeserializeResponseBodyAsync<IEnumerable<UserListApiModel>>(response);

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
        UserListApiModel user = await SaveUserAsync(new User { UserName = "userName", Email = "email", EmailConfirmed = true });

        IEnumerable<Item> items = new List<Item>
        {
          new Item { UserId = user.Id, Text = "firstItemText", Priority = 2, Status = ItemStatus.Todo },
          new Item { UserId = user.Id, Text = "secondItemText", Priority = 1, Status = ItemStatus.Done }
        };

        IEnumerable<ItemApiModel> expected = (await Task.WhenAll(items.Select(i => SaveItemAsync(i))))
          .OrderBy(i => i.Priority)
          .ToList();

        // Act
        HttpResponseMessage response = await GetAsync($"{url}/{user.Id}/items");

        response.EnsureSuccessStatusCode();

        IEnumerable<ItemApiModel> actual = await DeserializeResponseBodyAsync<IEnumerable<ItemApiModel>>(response);

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
        UserListApiModel userApiModel = await SaveUserAsync(new User { UserName = "userName", Email = "email", EmailConfirmed = true });

        // Act
        HttpResponseMessage response = await DeleteAsync($"{url}/{userApiModel.Id}");

        response.EnsureSuccessStatusCode();

        using (AppDbContext appDbContext = Server.GetService<AppDbContext>())
        {
          User user = await appDbContext
            .Users
            .SingleOrDefaultAsync(i => i.Id == userApiModel.Id);

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

        UserListApiModel userApiModel = await SaveUserAsync(user);

        object body = new
        {
          userApiModel.Email,
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

        UserListApiModel userApiModel = await SaveUserAsync(user);

        object body = new
        {
          userApiModel.Email,
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

        UserListApiModel userApiModel = await SaveUserAsync(user);

        object body = new
        {
          userApiModel.Email,
          Password = userPassword
        };

        // Act
        HttpResponseMessage response = await PostAsync(loginAPI, body);

        response.EnsureSuccessStatusCode();

        Assert.True(!string.IsNullOrWhiteSpace(await DeserializeResponseBodyAsync<string>(response)));
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

        UserListApiModel userApiModel = await SaveUserAsync(user);

        object body = new
        {
          userApiModel.Email,
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

        UserListApiModel userApiModel = await SaveUserAsync(user);

        object body = new
        {
          userApiModel.Email,
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

        UserListApiModel userApiModel = await SaveUserAsync(user);

        object body = new
        {
          userApiModel.Email,
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

        UserListApiModel userApiModel = await SaveUserAsync(user);

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

    public class ConfirmEmailAsync : UsersControllerTest
    {
      public ConfirmEmailAsync(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_CodeIsNull_Expect_BadRequest()
      {
        // Act
        HttpResponseMessage response = await GetAsync($"{url}/10/email/confirm");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserIsNotFound_Expect_BadRequest()
      {
        // Act
        HttpResponseMessage response = await GetAsync($"{url}/10/email/confirm?code=some-code");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_UserEmailIsAlreadyConfirmed_Expect_BadRequest()
      {
        UserListApiModel user = await SaveUserAsync(new User { EmailConfirmed = true });

        // Act
        HttpResponseMessage response = await GetAsync($"{url}/{user.Id}/email/confirm?code=some-code");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_CodeIsNotValid_Expect_BadRequest()
      {
        UserListApiModel user = await SaveUserAsync(new User { EmailConfirmed = false });

        // Act
        HttpResponseMessage response = await GetAsync($"{url}/{user.Id}/email/confirm?code=not-valid-code");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_CodeIsValid_Expect_EmailIsConfirmed()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User user = new User { UserName = "test-user-name", Email = "test@test.test" };

          await userManager.CreateAsync(user, "hrbEgerGER534.tf");

          string code = Uri.EscapeDataString(await userManager.GenerateEmailConfirmationTokenAsync(user));

          // Act
          HttpResponseMessage response = await GetAsync($"{url}/{user.Id}/email/confirm?code={code}");

          response.EnsureSuccessStatusCode();

          Assert.True((await dbContext.Users.SingleAsync(u => u.Id == user.Id)).EmailConfirmed);
        }
      }
    }

    public class RecoverPassword : UsersControllerTest
    {
      public RecoverPassword(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_UserIsNotFound_Expect_NotFound()
      {
        // Act
        HttpResponseMessage response = await PostAsync($"{url}/password", new { Email = "test@test.test" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
      }

      [Fact]
      public async Task When_UserIsFound_NewPasswordIsGenerated()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User user = new User { UserName = "test-user-name", Email = "test@test.test" };

          await userManager.CreateAsync(user, "hrbEgerGER534.tf");

          // Act
          HttpResponseMessage response = await PostAsync($"{url}/password", new { user.Email });

          response.EnsureSuccessStatusCode();

          Assert.NotEqual(user.PasswordHash, (await dbContext.Users.SingleAsync(u => u.Id == user.Id)).PasswordHash);
        }
      }
    }

    public class ChangePassword : UsersControllerTest
    {
      public ChangePassword(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_InputModelIsNotValid_Expect_BadRequest()
      {
        // Act
        HttpResponseMessage response = await PutAsync($"{url}/password", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_NewPasswordAndConfirmationPasswordDoNotEqual_Expect_BadRequest()
      {
        object body = new
        {
          OldPassword = "abcABC123.",
          NewPassword = "new-password",
          ConfirmNewPassword = "some-wrong-password"
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/password", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_OldPasswordIsWrong_Expect_BadRequest()
      {
        object body = new
        {
          OldPassword = "wrong-password",
          NewPassword = "abcABC123.",
          ConfirmNewPassword = "abcABC123."
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/password", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_NewPasswordIsNotValid_Expect_BadRequest()
      {
        object body = new
        {
          OldPassword = "abcABC123.",
          NewPassword = "password-without-digits",
          ConfirmNewPassword = "password-without-digits"
        };

        // Act
        HttpResponseMessage response = await PutAsync($"{url}/password", body);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task Expect_Changed()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          var body = new
          {
            OldPassword = "abcABC123.",
            NewPassword = "abcABC123.123",
            ConfirmNewPassword = "abcABC123.123"
          };

          User user = await dbContext.Users.SingleAsync(u => u.Id == UserId);

          // Act
          HttpResponseMessage response = await PutAsync($"{url}/password", body);

          response.EnsureSuccessStatusCode();

          Assert.NotEqual(user.PasswordHash, (await dbContext.Users.AsNoTracking().SingleAsync(u => u.Id == UserId)).PasswordHash);

          body = new
          {
            OldPassword = "abcABC123.123",
            NewPassword = "abcABC123.",
            ConfirmNewPassword = "abcABC123."
          };

          await PutAsync($"{url}/password", body); // change password back
        }
      }
    }

    public class GetAuthenticatorUri : UsersControllerTest
    {
      public GetAuthenticatorUri(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_AuthenticatorKeyIsNull_Expect_AuthenticatorKeyIsGeneratedAndAuthenticatorUriIsReturned()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          UserToken authenticatorKey = await dbContext
            .UserTokens
            .SingleOrDefaultAsync(t => t.UserId == UserId && t.LoginProvider == "[AspNetUserStore]" && t.Name == "AuthenticatorKey");

          Assert.Null(authenticatorKey);

          // Act
          HttpResponseMessage response = await GetAsync($"{url}/authenticator-uri");

          response.EnsureSuccessStatusCode();

          authenticatorKey = await dbContext
            .UserTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.UserId == UserId && t.LoginProvider == "[AspNetUserStore]" && t.Name == "AuthenticatorKey");

          Assert.NotNull(authenticatorKey);

          string expected = $"otpauth://totp/ToDoList:admin?secret={authenticatorKey.Value}&issuer=ToDoList&digits=6";
          string actual = await DeserializeResponseBodyAsync<string>(response);

          Assert.Equal(expected, actual);

          dbContext.Rollback<UserToken>();
          dbContext.SaveChanges();
        }
      }

      [Fact]
      public async Task When_AuthenticatorKeyIsNotNull_Expect_AuthenticatorUriIsReturned()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User admin = await userManager.FindByIdAsync(UserId.ToString());

          await userManager.ResetAuthenticatorKeyAsync(admin);

          UserToken authenticatorKey = await dbContext
            .UserTokens
            .SingleOrDefaultAsync(t => t.UserId == UserId && t.LoginProvider == "[AspNetUserStore]" && t.Name == "AuthenticatorKey");

          Assert.NotNull(authenticatorKey);

          // Act
          HttpResponseMessage response = await GetAsync($"{url}/authenticator-uri");

          response.EnsureSuccessStatusCode();

          string expected = $"otpauth://totp/ToDoList:admin?secret={authenticatorKey.Value}&issuer=ToDoList&digits=6";
          string actual = await DeserializeResponseBodyAsync<string>(response);

          Assert.Equal(expected, actual);

          dbContext.Rollback<UserToken>();
          dbContext.SaveChanges();
        }
      }
    }

    public class EnableAuthenticator : UsersControllerTest
    {
      public EnableAuthenticator(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_TwoFactorIsEnabledAlready_Expect_BadRequest()
      {
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User admin = await userManager.FindByIdAsync(UserId.ToString());

          admin.TwoFactorEnabled = true;

          await userManager.UpdateAsync(admin);

          // Act
          HttpResponseMessage response = await PutAsync($"{url}/two-factor-authentication/enable", new { Code = 111111 });

          Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

          admin.TwoFactorEnabled = false;

          await userManager.UpdateAsync(admin);
        }
      }

      [Fact]
      public async Task When_CodeIsNotValid_Expect_BadRequest()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User admin = await userManager.FindByIdAsync(UserId.ToString());

          await userManager.ResetAuthenticatorKeyAsync(admin);

          // Act
          HttpResponseMessage response = await PutAsync($"{url}/two-factor-authentication/enable", new { Code = 111111 });

          Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

          dbContext.Rollback<UserToken>();
          dbContext.SaveChanges();
        }
      }
    }

    public class DisableTwoFactorAuthentication : UsersControllerTest
    {
      public DisableTwoFactorAuthentication(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_TwoFactorIsNotEnabled_Expect_BadRequest()
      {
        // Act
        HttpResponseMessage response = await PutAsync($"{url}/two-factor-authentication/disable", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      }

      [Fact]
      public async Task When_TwoFactorIsEnabled_Expect_TwoFactorIsDisabledAndAuthenticatorKeyIsChanged()
      {
        using (AppDbContext dbContext = Server.GetService<AppDbContext>())
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User admin = await userManager.FindByIdAsync(UserId.ToString());

          admin.TwoFactorEnabled = true;

          await userManager.UpdateAsync(admin);
          await userManager.ResetAuthenticatorKeyAsync(admin);

          UserToken authenticatorKey = await dbContext
            .UserTokens
            .SingleOrDefaultAsync(t => t.UserId == UserId && t.LoginProvider == "[AspNetUserStore]" && t.Name == "AuthenticatorKey");

          Assert.True(admin.TwoFactorEnabled);

          // Act
          HttpResponseMessage response = await PutAsync($"{url}/two-factor-authentication/disable", null);

          response.EnsureSuccessStatusCode();

          User adminAfterDisabling = await dbContext
            .Users
            .AsNoTracking()
            .SingleAsync(u => u.Id == UserId);

          Assert.False(adminAfterDisabling.TwoFactorEnabled);

          UserToken authenticatorKeyAfterDisabling = await dbContext
            .UserTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.UserId == UserId && t.LoginProvider == "[AspNetUserStore]" && t.Name == "AuthenticatorKey");

          Assert.NotEqual(authenticatorKey.Value, authenticatorKeyAfterDisabling.Value);

          dbContext.Rollback<UserToken>();
          dbContext.SaveChanges();

          admin.TwoFactorEnabled = false;

          await userManager.UpdateAsync(admin);
        }
      }
    }

    public class IsTwoFactorAuthenticationEnabled : UsersControllerTest
    {
      public IsTwoFactorAuthenticationEnabled(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public async Task When_TwoFactorIsNotEnabled_Expect_False()
      {
        // Act
        HttpResponseMessage response = await GetAsync($"{url}/two-factor-authentication/is-enabled");

        response.EnsureSuccessStatusCode();

        Assert.False(await DeserializeResponseBodyAsync<bool>(response));
      }

      [Fact]
      public async Task When_TwoFactorIsEnabled_Expect_True()
      {
        using (UserManager<User> userManager = Server.GetService<UserManager<User>>())
        {
          User admin = await userManager.FindByIdAsync(UserId.ToString());

          admin.TwoFactorEnabled = true;

          await userManager.UpdateAsync(admin);

          // Act
          HttpResponseMessage response = await GetAsync($"{url}/two-factor-authentication/is-enabled");

          response.EnsureSuccessStatusCode();

          Assert.True(await DeserializeResponseBodyAsync<bool>(response));

          admin.TwoFactorEnabled = false;

          await userManager.UpdateAsync(admin);
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
