using NclVaultFramework.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NclVaultFramework.Controllers
{
    public class BackendInterface
    {

        #region Costants
        private const string INIT_API_ENDPOINT_URL = "https://192.168.1.216/vault/initvault";
        private const string LOGIN_API_ENDPOINT_URL = "https://192.168.1.216/token/login";
        private const string READ_PASSWORD_API_ENDPOINT_URL = "https://192.168.1.216/vault/password/{0}";
        private const string READ_PASSWORDS_API_ENDPOINT_URL = "https://192.168.1.216/vault/password";
        private const string CREATE_PASSWORD_API_ENDPOINT_URL = "https://192.168.1.216/vault/password";

        #endregion
        #region Members
        private readonly HttpClient _httpClient;
        private static BackendInterface _backendInterface;
        #endregion


        public BackendInterface(bool sslVerificationBypass)
        {
            if (sslVerificationBypass)
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                _httpClient = new HttpClient(clientHandler);
            }
            else
            {
                _httpClient = new HttpClient();
            }
        }

        public static BackendInterface GetInstance(bool sslVerificationBypass)
        {
            if (null == _backendInterface)
            {
                _backendInterface = new BackendInterface(sslVerificationBypass);
            }

            return _backendInterface;
        }

        [Obsolete("UNSECURE Method. Use the Login(NetworkCredential) implementation")]
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

        public async Task<HTTPResponseResult> Init(NetworkCredential credential)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();
            string STRING_SerializedJsonRequestBody = JsonConvert.SerializeObject(new
            {
                credential.UserName,
                credential.Password
            });

            HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(INIT_API_ENDPOINT_URL,
                                   new StringContent(STRING_SerializedJsonRequestBody, Encoding.UTF8, "application/json"));

            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            httpResponseResult.OBJECT_RestResult = JsonConvert.DeserializeObject<InitResponse>(responseContent);

            httpResponseResult.StatusCode = httpResponseMessage.StatusCode;
            httpResponseResult.StatusDescription = httpResponseMessage.ReasonPhrase;

            return httpResponseResult;

        }

        [Obsolete("UNSECURE Method. Use the Login(NetworkCredential, String) implementation")]
        public async Task<HTTPResponseResult> Login(object body, string STRING_InitIdKey)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();
            try
            {

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
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return httpResponseResult;
        }

        public async Task<HTTPResponseResult> Login(NetworkCredential credential, string STRING_InitIdKey)
        {
            HTTPResponseResult httpResponseResult = new HTTPResponseResult();
            try
            {

                string STRING_SerializedJsonRequestBody = JsonConvert.SerializeObject(new { credential.UserName, credential.Password });

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
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
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
            string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            httpResponseResult.OBJECT_RestResult = JsonConvert.DeserializeObject<PasswordEntryReadDto>(responseContent);
            //httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();

            if (httpResponseMessage.StatusCode == HttpStatusCode.Created)
            {
                httpResponseResult.STRING_JwtToken = httpResponseMessage.Headers.GetValues("X-Token").Single();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", httpResponseResult.STRING_JwtToken);
            }

            return httpResponseResult;
        }
    }
}
