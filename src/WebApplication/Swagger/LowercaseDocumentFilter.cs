using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Swagger
{
  public class LowercaseDocumentFilter : IDocumentFilter
  {
    public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
    {
      IDictionary<string, PathItem> newPaths = new Dictionary<string, PathItem>();
      IList<string> removeKeys = new List<string>();

      foreach (var path in swaggerDoc.Paths)
      {
        string newKey = LowercaseEverythingButParameters(path.Key);

        if (newKey != path.Key)
        {
          removeKeys.Add(path.Key);
          newPaths.Add(newKey, path.Value);
        }
      }

      foreach (var path in newPaths)
      {
        swaggerDoc.Paths.Add(path.Key, path.Value);
      }

      foreach (var key in removeKeys)
      {
        swaggerDoc.Paths.Remove(key);
      }
    }

    private static string LowercaseEverythingButParameters(string key)
    {
      return string.Join(
        '/', key.Split('/').Select(x => x.Contains("{") ? x : x.ToLower()).ToList()
      );
    }
  }
}
