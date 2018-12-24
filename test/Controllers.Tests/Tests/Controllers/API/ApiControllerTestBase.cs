using Controllers.Tests.Fixtures;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Controllers.Tests
{
  public abstract class ApiControllerTestBase : ControllerTestBase
  {
    protected ApiControllerTestBase(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    protected override HttpContent CreateHttpContent(object requestBody)
    {
      return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
    }

    protected T DeserializeResponseBody<T>(HttpResponseMessage response)
    {
      return JsonConvert.DeserializeObject<T>(response.Content.ReadAsStringAsync().Result);
    }
  }
}
