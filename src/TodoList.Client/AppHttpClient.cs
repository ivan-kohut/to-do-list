using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Client
{
  public class AppHttpClient : IAppHttpClient
  {
    private readonly HttpClient httpClient;

    public AppHttpClient(HttpClient httpClient)
    {
      this.httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
      return await httpClient.GetAsync(url);
    }

    public async Task<ApiCallResult<T>> PostAsync<T>(string url, object requestBody)
    {
      using (HttpContent httpContent = CreateHttpContent(requestBody))
      {
        HttpResponseMessage httpResponse = await httpClient.PostAsync(url, httpContent);

        ApiCallResult<T> apiCallResult = new ApiCallResult<T>
        {
          IsSuccess = httpResponse.IsSuccessStatusCode,
          StatusCode = (int)httpResponse.StatusCode
        };

        string httpResponseContent = await httpResponse.Content.ReadAsStringAsync();

        if (apiCallResult.IsSuccess)
        {
          apiCallResult.Value = JsonConvert.DeserializeObject<T>(httpResponseContent);
        }
        else
        {
          apiCallResult.Errors = JsonConvert.DeserializeObject<ApiCallResult<T>>(httpResponseContent).Errors;
        }

        return apiCallResult;
      }
    }

    public async Task<HttpResponseMessage> PutAsync(string url, object requestBody)
    {
      using (HttpContent httpContent = CreateHttpContent(requestBody))
      {
        return await httpClient.PutAsync(url, httpContent);
      }
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
      return await httpClient.DeleteAsync(url);
    }

    private HttpContent CreateHttpContent(object requestBody)
    {
      return new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
    }
  }
}
