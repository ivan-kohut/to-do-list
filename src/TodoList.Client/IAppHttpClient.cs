using System.Net.Http;
using System.Threading.Tasks;

namespace TodoList.Client
{
  public interface IAppHttpClient
  {
    Task<HttpResponseMessage> GetAsync(string url);
    Task<ApiCallResult<T>> PostAsync<T>(string url, object requestBody);
    Task<HttpResponseMessage> PutAsync(string url, object requestBody);
    Task<HttpResponseMessage> DeleteAsync(string url);
  }
}
