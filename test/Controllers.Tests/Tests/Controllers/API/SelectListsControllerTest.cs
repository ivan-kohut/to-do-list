using Controllers.Tests.Fixtures;
using FluentAssertions;
using Services;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace Controllers.Tests
{
  [Collection(nameof(IntegrationTestCollection))]
  public class SelectListsControllerTest : ApiControllerTestBase
  {
    private const string url = "/api/v1/select-lists";

    public SelectListsControllerTest(TestServerFixture testServerFixture) : base(testServerFixture)
    {
    }

    public class GetStatusesSelectList : SelectListsControllerTest
    {
      public GetStatusesSelectList(TestServerFixture testServerFixture) : base(testServerFixture)
      {
      }

      [Fact]
      public void Expect_Returned()
      {
        IEnumerable<SelectListItemDTO> expected = new List<SelectListItemDTO>
        {
          new SelectListItemDTO { Value = "1", Text = "Todo" },
          new SelectListItemDTO { Value = "2", Text = "Done" }
        };

        // Act
        HttpResponseMessage response = Get($"{url}/item-statuses");

        response.EnsureSuccessStatusCode();

        IEnumerable<SelectListItemDTO> actual = DeserializeResponseBody<IEnumerable<SelectListItemDTO>>(response);

        actual.ShouldBeEquivalentTo(expected);
      }
    }
  }
}
