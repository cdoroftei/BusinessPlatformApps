using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Common.Helpers
{
    public class RestClient
    {
        private readonly HttpClient _client;
        private readonly string _mediaType;

        public RestClient(string baseUri, AuthenticationHeaderValue authenticationInfo = null, string mediaType = "application/json")
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = true;
            _client = new HttpClient { BaseAddress = new Uri(baseUri) };
            _mediaType = mediaType;

            // Set authorization 
            if (authenticationInfo != null)
                _client.DefaultRequestHeaders.Authorization = authenticationInfo;
        }

        private string SanitizeBody(string body)
        {
            return string.IsNullOrEmpty(body) ? string.Empty :
                                                Regex.Replace(body, "\\s*\"password\"\\s*:\\s*\".*?\"\\s*", "password\": \"XXXXXXX\"", RegexOptions.IgnoreCase);
        }

        public async Task<string> HandleRequest(HttpMethod method, string relativeUri, Dictionary<string, string> headers, string body)
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType));

            HttpRequestMessage message = new HttpRequestMessage(method, relativeUri);

            if (headers != null)
            {
                _client.DefaultRequestHeaders.Clear();
                headers.Keys.ToList().ForEach(p => _client.DefaultRequestHeaders.Add(p, headers[p]));
            }

            if (body != null)
            {
                message.Content = new StringContent(body, Encoding.UTF8, _mediaType);
            }

            var response = await _client.SendAsync(message);
            string responseMessage = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
                return responseMessage;

            throw new HttpRequestException(responseMessage);
        }

        public async Task<string> Get(string relativeUri, string parameters = null, Dictionary<string, string> headers = null)
        {
            return parameters == null
                ? await HandleRequest(HttpMethod.Get, relativeUri, headers, null)
                : await HandleRequest(HttpMethod.Get, string.Concat(relativeUri, '?', parameters), headers, null);
        }

        public async Task<string> Post(string relativeUri, string body, string parameters = null, Dictionary<string, string> headers = null)
        {
            return parameters == null
                ? await HandleRequest(HttpMethod.Post, relativeUri, headers, body)
                : await HandleRequest(HttpMethod.Post, string.Concat(relativeUri, '?', parameters), headers, body);
        }

        public async Task<string> Put(string relativeUri, string body, Dictionary<string, string> headers = null)
        {
            return await HandleRequest(HttpMethod.Put, relativeUri, headers, body);
        }

        public async Task<string> Delete(string relativeUri, string parameters = null, Dictionary<string, string> headers = null)
        {
            return parameters == null
                ? await HandleRequest(HttpMethod.Delete, relativeUri, headers, null)
                : await HandleRequest(HttpMethod.Delete, string.Concat(relativeUri, '?', parameters), headers, null);
        }
    }
}