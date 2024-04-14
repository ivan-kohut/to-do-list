using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace TodoList.Items.API.Swagger
{
    public class LowercaseDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var newPaths = new Dictionary<string, OpenApiPathItem>();
            var removeKeys = new List<string>();

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
                '/', key.Split('/').Select(x => x.Contains('{') ? x : x.ToLower()).ToList()
            );
        }
    }
}
