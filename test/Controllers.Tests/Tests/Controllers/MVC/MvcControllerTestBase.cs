using Controllers.Tests.Fixtures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Controllers.Tests
{
  public abstract class MvcControllerTestBase : ControllerTestBase
  {
    protected MvcControllerTestBase(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    protected override HttpContent CreateHttpContent(object requestBody)
    {
      return new FormUrlEncodedContent(
        JsonConvert.DeserializeObject<IDictionary<string, string>>(JsonConvert.SerializeObject(requestBody))
      );
    }
  }
}
