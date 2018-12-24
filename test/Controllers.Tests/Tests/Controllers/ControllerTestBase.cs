using Controllers.Tests.Fixtures;
using System.Net.Http;

namespace Controllers.Tests
{
  public abstract class ControllerTestBase : TestBase
  {
    protected ControllerTestBase(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    protected HttpResponseMessage Get(string url)
    {
      return Client.GetAsync(url).Result;
    }

    protected HttpResponseMessage Post(string url, object requestBody)
    {
      using (HttpContent httpContent = CreateHttpContent(requestBody))
      {
        return Client.PostAsync(url, httpContent).Result;
      }
    }

    protected HttpResponseMessage Put(string url, object requestBody)
    {
      using (HttpContent httpContent = CreateHttpContent(requestBody))
      {
        return Client.PutAsync(url, httpContent).Result;
      }
    }

    protected HttpResponseMessage Delete(string url)
    {
      return Client.DeleteAsync(url).Result;
    }

    protected abstract HttpContent CreateHttpContent(object requestBody);
  }
}
