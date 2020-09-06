using NclVaultCLIClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace NclVaultCLIClient.Controllers
{
    public class BackendInterface
    {

        #region Costants
        private const string INIT_API_ENDPOINT_URL = "https://localhost:5001/vault/initvault";
        private const string LOGIN_API_ENDPOINT_URL = "https://localhost:5001/token/login";
        private const string READ_PASSWORD_API_ENDPOINT_URL = "https://localhost:5001/vault/read/password/{0}";
        private const string READ_PASSWORDS_API_ENDPOINT_URL = "https://localhost:5001/vault/read/password";
        private const string CREATE_PASSWORD_API_ENDPOINT_URL = "https://localhost:5001/vault/create/password";

        #endregion
        #region Members
        private HttpClient _httpClient;
        private static BackendInterface _backendInterface;
        #endregion


        public BackendInterface()
        {
            _httpClient = new HttpClient();
        }

        public static BackendInterface GetInstance()
        {
            if(null == _backendInterface)
            {
                _backendInterface = new BackendInterface();
            }

            return _backendInterface;
        }


        public async Task<HTTPResponseResult> Init(object body)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();
            string STRING_SerializedJsonRequestBody = JsonConvert.SerializeObject(body);

            HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(INIT_API_ENDPOINT_URL,
                                   new StringContent(STRING_SerializedJsonRequestBody, Encoding.UTF8, "application/json"));

            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            httpResponseResult.OBJECT_RestResult = JsonConvert.DeserializeObject<InitResponse>(responseContent);

            httpResponseResult.StatusCode = httpResponseMessage.StatusCode;
            httpResponseResult.StatusDescription = httpResponseMessage.ReasonPhrase;

            return httpResponseResult;

        }

        public async Task<HTTPResponseResult> Login(object body, string STRING_InitIdKey)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();
            string STRING_SerializedJsonRequestBody = JsonConvert.SerializeObject(body);

            _httpClient.DefaultRequestHeaders.Add("InitId", STRING_InitIdKey);
            

            HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(LOGIN_API_ENDPOINT_URL,
                                   new StringContent(STRING_SerializedJsonRequestBody, Encoding.UTF8, "application/json"));

            httpResponseResult.StatusCode = httpResponseMessage.StatusCode;
            httpResponseResult.StatusDescription = httpResponseMessage.ReasonPhrase;
            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", httpResponseResult.STRING_JwtToken);

                
            }
            
            return httpResponseResult;
        }

        public async Task<HTTPResponseResult> ReadPassword(int INT32_Id)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();


            

            
            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(String.Format(READ_PASSWORD_API_ENDPOINT_URL, INT32_Id));

            httpResponseResult.StatusCode = httpResponseMessage.StatusCode;
            httpResponseResult.StatusDescription = httpResponseMessage.ReasonPhrase;
            
            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            httpResponseResult.OBJECT_RestResult = JsonConvert.DeserializeObject<PasswordEntryReadDto>(responseContent);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", httpResponseResult.STRING_JwtToken);
            }

            return httpResponseResult;
        }

        public async Task<HTTPResponseResult> ReadPasswords()
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();


            HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(READ_PASSWORDS_API_ENDPOINT_URL);

            httpResponseResult.StatusCode = httpResponseMessage.StatusCode;
            httpResponseResult.StatusDescription = httpResponseMessage.ReasonPhrase;
            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            httpResponseResult.OBJECT_RestResult = JsonConvert.DeserializeObject<List<PasswordEntryReadDto>>(responseContent);

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", httpResponseResult.STRING_JwtToken);
            }

            return httpResponseResult;
        }

        public async Task<HTTPResponseResult> CreatePassword(PasswordEntryCreateDto newPassword)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();
            string STRING_SerializedJsonRequestBody = JsonConvert.SerializeObject(newPassword);


            HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(CREATE_PASSWORD_API_ENDPOINT_URL,
                                   new StringContent(STRING_SerializedJsonRequestBody, Encoding.UTF8, "application/json"));

            httpResponseResult.StatusCode = httpResponseMessage.StatusCode;
            httpResponseResult.StatusDescription = httpResponseMessage.ReasonPhrase;
            httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", httpResponseResult.STRING_JwtToken);
            }

            return httpResponseResult;
        }
    }
}
