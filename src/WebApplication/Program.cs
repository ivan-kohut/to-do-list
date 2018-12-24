using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace WebApplication
{
  public class Program
  {
    static void Main(string[] args)
    {
      WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .Build()
        .Run();
    }
  }
}
