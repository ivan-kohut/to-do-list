using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Client
{
  public class AppHttpClient : IAppHttpClient
  {
    private readonly string apiUrl;
    private readonly HttpClient httpClient;
    private readonly ILocalStorageService localStorageService;

    public AppHttpClient(HttpClient httpClient, ILocalStorageService localStorageService, NavigationManager navigationManager)
    {
      this.httpClient = httpClient;
      this.localStorageService = localStorageService;

      this.apiUrl = navigationManager.BaseUri.Contains("localhost")
        ? "https://localhost:44388"
        : "https://todo-list-api.azurewebsites.net";
    }

    public async Task<ApiCallResult<T>> GetAsync<T>(string url) where T : class
    {
      await SetAuthorizationHeader();

      return await GenerateApiCallResultAsync<T>(await httpClient.GetAsync($"{apiUrl}{url}"));
    }

    public async Task<ApiCallResult<T>> PostAsync<T>(string url, object? requestBody) where T : class
    {
      await SetAuthorizationHeader();

      using HttpContent httpContent = CreateHttpContent(requestBody);

      return await GenerateApiCallResultAsync<T>(await httpClient.PostAsync($"{apiUrl}{url}", httpContent));
    }

    public async Task<ApiCallResult> PutAsync(string url, object? requestBody)
    {
      await SetAuthorizationHeader();

      using HttpContent httpContent = CreateHttpContent(requestBody);

      return await GenerateApiCallResultAsync(await httpClient.PutAsync($"{apiUrl}{url}", httpContent));
    }

    public async Task<ApiCallResult> DeleteAsync(string url)
    {
      await SetAuthorizationHeader();

      return await GenerateApiCallResultAsync(await httpClient.DeleteAsync($"{apiUrl}{url}"));
    }

    private HttpContent CreateHttpContent(object? requestBody) => new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

    private async Task<ApiCallResult<T>> GenerateApiCallResultAsync<T>(HttpResponseMessage httpResponse) where T : class
    {
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

    private async Task<ApiCallResult> GenerateApiCallResultAsync(HttpResponseMessage httpResponse)
    {
      ApiCallResult apiCallResult = new ApiCallResult
      {
        IsSuccess = httpResponse.IsSuccessStatusCode,
        StatusCode = (int)httpResponse.StatusCode
      };

      if (!apiCallResult.IsSuccess)
      {
        apiCallResult.Errors = JsonConvert.DeserializeObject<ApiCallResult>(await httpResponse.Content.ReadAsStringAsync())
          .Errors;
      }

      return apiCallResult;
    }

    private async Task SetAuthorizationHeader()
    {
      string authToken = await localStorageService.GetItemAsync<string>(AppState.AuthTokenKey);

      if (!string.IsNullOrWhiteSpace(authToken))
      {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
      }
    }
  }
}
