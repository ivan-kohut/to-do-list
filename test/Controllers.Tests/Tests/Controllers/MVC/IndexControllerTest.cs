using Controllers.Tests.Fixtures;
using System.Net.Http;
using Xunit;

namespace Controllers.Tests
{
  [Collection(nameof(IntegrationTestCollection))]
  public class IndexControllerTest : MvcControllerTestBase
  {
    private const string url = "/";

    public IndexControllerTest(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    [Fact]
    public void GetIndexPage_Expect_Ok()
    {
      // Act
      HttpResponseMessage response = Get(url);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }
  }
}
