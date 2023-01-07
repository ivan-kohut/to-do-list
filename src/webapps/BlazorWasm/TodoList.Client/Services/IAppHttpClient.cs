using System.Threading.Tasks;

namespace TodoList.Client
{
    public interface IAppHttpClient
    {
        Task<ApiCallResult<T>> GetAsync<T>(string url) where T : class;
        Task<ApiCallResult<T>> PostAsync<T>(string url, object? requestBody) where T : class;
        Task<ApiCallResult> PutAsync(string url, object? requestBody);
        Task<ApiCallResult> DeleteAsync(string url);
    }
}
