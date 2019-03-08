using Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Services.Tests
{
  public class SelectListServiceTest
  {
    private readonly ISelectListService selectListService;

    public SelectListServiceTest()
    {
      this.selectListService = new SelectListService();
    }

    public class BuildSelectList : SelectListServiceTest
    {
      [Fact]
      public void When_InputStructIsNotEnum_Expect_ArgumentExeption()
      {
        // Act
        Assert.Throws<ArgumentException>(() => selectListService.BuildSelectList<int>());
        Assert.Throws<ArgumentException>(() => selectListService.BuildSelectList<long>());
        Assert.Throws<ArgumentException>(() => selectListService.BuildSelectList<double>());
        Assert.Throws<ArgumentException>(() => selectListService.BuildSelectList<decimal>());
      }

      [Fact]
      public void When_InputStructIsEnum_Expect_SelectListIsReturned()
      {
        IEnumerable<SelectListItemDTO> expected = new List<SelectListItemDTO>
      {
        new SelectListItemDTO { Value = "1", Text = "Todo" },
        new SelectListItemDTO { Value = "2", Text = "Done" }
      };

        // Act
        IEnumerable<SelectListItemDTO> actual = selectListService.BuildSelectList<ItemStatus>();

        actual.ShouldBeEquivalentTo(expected);
      }
    }
  }
}
