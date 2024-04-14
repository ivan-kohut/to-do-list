using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Client
{
    public class AppHttpClient(HttpClient httpClient) : IAppHttpClient
    {
        public async Task<ApiCallResult<T>> GetAsync<T>(string url) where T : class =>
            await GenerateApiCallResultAsync<T>(await httpClient.GetAsync(url));

        public async Task<ApiCallResult<T>> PostAsync<T>(string url, object? requestBody) where T : class
        {
            using HttpContent content = CreateContent(requestBody);

            return await GenerateApiCallResultAsync<T>(await httpClient.PostAsync(url, content));
        }

        public async Task<ApiCallResult> PutAsync(string url, object? requestBody)
        {
            using HttpContent content = CreateContent(requestBody);

            return await GenerateApiCallResultAsync(await httpClient.PutAsync(url, content));
        }

        public async Task<ApiCallResult> DeleteAsync(string url) =>
            await GenerateApiCallResultAsync(await httpClient.DeleteAsync(url));

        private static StringContent CreateContent(object? requestBody) =>
            new(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        private static async Task<ApiCallResult<T>> GenerateApiCallResultAsync<T>(HttpResponseMessage httpResponse) where T : class
        {
            ApiCallResult<T> apiCallResult = new()
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
                apiCallResult.Errors = JsonConvert.DeserializeObject<ApiCallResult<T>>(httpResponseContent)?.Errors;
            }

            return apiCallResult;
        }

        private static async Task<ApiCallResult> GenerateApiCallResultAsync(HttpResponseMessage httpResponse)
        {
            ApiCallResult apiCallResult = new()
            {
                IsSuccess = httpResponse.IsSuccessStatusCode,
                StatusCode = (int)httpResponse.StatusCode
            };

            if (!apiCallResult.IsSuccess)
            {
                apiCallResult.Errors = JsonConvert
                    .DeserializeObject<ApiCallResult>(await httpResponse.Content.ReadAsStringAsync())?.Errors;
            }

            return apiCallResult;
        }
    }
}
