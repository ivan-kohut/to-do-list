using Controllers.Tests.Fixtures;
using System.Net.Http;
using System.Threading.Tasks;

namespace Controllers.Tests
{
  public abstract class ControllerTestBase : TestBase
  {
    protected ControllerTestBase(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
      return await Client.GetAsync(url);
    }

    protected async Task<HttpResponseMessage> PostAsync(string url, object requestBody)
    {
      using HttpContent httpContent = CreateHttpContent(requestBody);
      return await Client.PostAsync(url, httpContent);
    }

    protected async Task<HttpResponseMessage> PutAsync(string url, object? requestBody)
    {
      using HttpContent httpContent = CreateHttpContent(requestBody);
      return await Client.PutAsync(url, httpContent);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
      return await Client.DeleteAsync(url);
    }

    protected abstract HttpContent CreateHttpContent(object? requestBody);
  }
}
