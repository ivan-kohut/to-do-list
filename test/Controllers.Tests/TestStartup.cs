using Microsoft.Extensions.Configuration;
using WebApplication;

namespace Controllers.Tests
{
  public class TestStartup : Startup
  {
    protected override string ConnectionStringName { get; } = "DefaultTestConnection";

    public TestStartup(IConfiguration configuration) : base(configuration)
    {
    }
  }
}
