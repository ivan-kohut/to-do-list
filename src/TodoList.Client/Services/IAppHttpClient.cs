using System.Threading.Tasks;

namespace TodoList.Client
{
  public interface IAppHttpClient
  {
    Task<ApiCallResult<T>> GetAsync<T>(string url);
    Task<ApiCallResult<T>> PostAsync<T>(string url, object requestBody);
    Task<ApiCallResult> PutAsync(string url, object requestBody);
    Task<ApiCallResult> DeleteAsync(string url);
  }
}
