﻿using Controllers.Tests.Extensions;
using Entities;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Repositories;
using System;
using System.IO;
using System.Net.Http;
using Xunit;

namespace Controllers.Tests.Fixtures
{
  [CollectionDefinition(nameof(IntegrationTestCollection))]
  public class IntegrationTestCollection : ICollectionFixture<TestServerFixture> { }

  public class TestServerFixture : IDisposable
  {
    public TestServer Server { get; }
    public HttpClient Client { get; }
    public User User { get; }

    public TestServerFixture()
    {
      string projectRootPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..", "src", "TodoList.API"
      );

      IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder()
        .UseContentRoot(projectRootPath)
        .UseEnvironment(Environments.Development)
        .UseStartup<TestStartup>();

      Server = new TestServer(webHostBuilder);
      Client = Server.CreateClient();

      using AppDbContext appDbContext = Server.GetService<AppDbContext>();

      User = appDbContext.Users
        .AsNoTracking()
        .SingleAsync(u => u.IdentityId == 1)
        .GetAwaiter()
        .GetResult();

      Server.GetService<IOptions<RouteOptions>>().Value.SuppressCheckForUnhandledSecurityMetadata = true;
    }

    public void Dispose()
    {
      Client?.Dispose();
      Server?.Dispose();
    }
  }
}
