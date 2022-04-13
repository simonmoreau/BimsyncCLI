using BimsyncCLI.Models.Bimsync;
using BimsyncCLI.Models.BCF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Text.Json;

namespace BimsyncCLI.Services
{
    public class AuthenticationService
    {
        private string loginUrl;
        private string client_id;
        private string client_secret;
        private string redirect_uri;
        private DateTime _lastTokenRefresh;
        private string _authorizationCode;
        private HttpClient _client;
        private SettingsService _settingsService;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        TimeSpan timeBetweenRefresh = new TimeSpan(0,59,0);


        public AuthenticationService(IWebProxy proxy, SettingsService settingsService)
        {
            _settingsService = settingsService;

            // Create a client handler which uses that proxy
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                AutomaticDecompression = System.Net.DecompressionMethods.GZip
            };

            _client = new HttpClient(httpClientHandler, true);
            _client.BaseAddress = new Uri("https://api.bimsync.com/");
            _client.DefaultRequestHeaders.Accept.Clear();
            
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            client_id = "bDaDxEqY4icOBdp";
            client_secret = "pUM59IePpjCGY94";
            redirect_uri = "https://catenda.com/products/bimsync-arena/";
            loginUrl = string.Format(
                "https://api.bimsync.com/oauth2/authorize?client_id={0}&response_type=code&state=api&redirect_uri={1}&prompt=none",
                client_id,
                HttpUtility.UrlEncode(redirect_uri));
        }

        public string GetLoginUrl()
        {
            return loginUrl;
        }

        public string GetRedirectUri()
        {
            return redirect_uri.ToLower();
        }

        public bool SetRedirectUri(string callbackUrl)
        {
            Uri callbackUri = new Uri(callbackUrl);
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(callbackUri.Query);

            if (nameValueCollection.AllKeys.Contains("error"))
            {
                return false;
            }
            
            if (nameValueCollection.AllKeys.Contains("code"))
            {
                _authorizationCode = HttpUtility.ParseQueryString(callbackUri.Query).Get("code");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetAuthorizationCode(string code)
        {
            _authorizationCode = code;
            return true;
        }

        public async Task<Token> Login()
        {
            
            // if the token exist in memory
            if (_settingsService.Token != null)
            {
                if (DateTime.Now > _lastTokenRefresh + timeBetweenRefresh)
                {
                    //going to request refresh token: enter or start wait
                    await _semaphoreSlim.WaitAsync();
                    Debug.WriteLine(DateTime.Now.ToString() + " - " + "await _semaphoreSlim.WaitAsync();" + System.Threading.Thread.CurrentThread.ManagedThreadId);

                    // Test again to see if a refreshed token is now available
                    if (DateTime.Now < _lastTokenRefresh + timeBetweenRefresh)
                    {
                        _semaphoreSlim.Release();
                        Debug.WriteLine(DateTime.Now.ToString() + " - " + "A token has been refreshed previously;" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                        return _settingsService.Token;
                    }

                    // Refresh token if necessary
                    _settingsService.SetToken(await RefreshAccessTokenAsync(client_id, client_secret, _settingsService.Token.refresh_token, _cancellationTokenSource.Token));
                    _lastTokenRefresh = DateTime.Now;
                    //token is set set, so release:
                    _semaphoreSlim.Release();
                    Debug.WriteLine(DateTime.Now.ToString() + " - " + "//token is set set, so release:;" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                    return _settingsService.Token;
                }
                else
                {
                    Debug.WriteLine(DateTime.Now.ToString() + " - " + "else" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                    return _settingsService.Token;
                }
            }
            else // We don't have any existing token, we must get one
            {
                if (_authorizationCode == null) // We must first have an authorization code
                {
                    throw new Exception("You are not logged to Bimsync. Please sign in first by running the command \"bimsync sign-in\"");
                }
                else // We have an authorization code, we use it
                {
                    _settingsService.SetToken(await GetAccessTokenAsync(_authorizationCode, client_id, client_secret, redirect_uri, _cancellationTokenSource.Token));
                    // Once the authorization code have been used, we nullify it
                    _authorizationCode = null;
                    _lastTokenRefresh = DateTime.Now;
                    return _settingsService.Token;
                }
            }
        }

        
        public void Logout()
        {
            _settingsService.ClearToken();
        }

        private async Task<Token> GetAccessTokenAsync(string code, string client_id, string client_secret, string redirect_uri, CancellationToken cancellationToken)
        {
            try
            {

                string path = String.Format("oauth2/token");

                List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "authorization_code"));
                keyValues.Add(new KeyValuePair<string, string>("code", code));
                keyValues.Add(new KeyValuePair<string, string>("client_id", client_id));
                keyValues.Add(new KeyValuePair<string, string>("client_secret", client_secret));
                keyValues.Add(new KeyValuePair<string, string>("redirect_uri", redirect_uri));

                HttpContent httpContent = new FormUrlEncodedContent(keyValues);

                HttpResponseMessage response = await _client.PostAsync(path, httpContent, cancellationToken); // _client.GetAsync(path);
                response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync();
                Debug.WriteLine(DateTime.Now.ToString() + " - " + "A token is fetched." + System.Threading.Thread.CurrentThread.ManagedThreadId);
                return JsonSerializer.Deserialize<Token>(stream);

            }
            catch (OperationCanceledException ocException)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"GetAccessTokenAsync was cancelled with message {ocException.Message}.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"Something went wrong, the token has not been fetched.  " + ex.Message);
                throw ex;
            }

        }

        private async Task<Token> RefreshAccessTokenAsync(string client_id, string client_secret, string refresh_token, CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + "A token is refreshing" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                string path = String.Format("oauth2/token");

                List<KeyValuePair<string, string>> keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("grant_type", "refresh_token"));
                keyValues.Add(new KeyValuePair<string, string>("refresh_token", refresh_token));
                keyValues.Add(new KeyValuePair<string, string>("client_id", client_id));
                keyValues.Add(new KeyValuePair<string, string>("client_secret", client_secret));

                HttpContent httpContent = new FormUrlEncodedContent(keyValues); // = new StringContent(jsonString, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpResponseMessage response = await _client.PostAsync(path, httpContent, cancellationToken); // _client.GetAsync(path);
                response.EnsureSuccessStatusCode();

                Stream stream = await response.Content.ReadAsStreamAsync();
                Debug.WriteLine(DateTime.Now.ToString() + " - " + "A token is refreshed" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                return JsonSerializer.Deserialize<Token>(stream);

            }
            catch (OperationCanceledException ocException)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"RefreshAccessTokenAsync was cancelled with message {ocException.Message}.");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"Something went wrong, the token has not been refreshed.  " + ex.Message);
                throw ex;
            }
        }
    }
}
