using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BimsyncCLI.Models.Bimsync;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Diagnostics;

namespace BimsyncCLI.Services.HttpServices
{
    class BimsyncClient : IBimsyncClient
    {
        private HttpClient _client;

        public BimsyncClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri("https://api.bimsync.com/");
            _client.DefaultRequestHeaders.Accept.Clear();
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public async Task<List<Project>> GetProjects(CancellationToken cancellationToken)
        {
            List<Project> projects = new List<Project>();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "v2/projects");

            ReturnValue<List<Project>> returnValueProjects = await SendPaginatedRequest<List<Project>>(request, cancellationToken);
            if (returnValueProjects.Value != null) projects.AddRange(returnValueProjects.Value);

            while (returnValueProjects.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueProjects.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueProjects = await SendPaginatedRequest<List<Project>>(request, cancellationToken);
                if (returnValueProjects.Value != null) projects.AddRange(returnValueProjects.Value);
            }


            return projects;
        }

        public async Task<List<Model>> GetModels(string projectId, CancellationToken cancellationToken)
        {
            List<Model> models = new List<Model>();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"v2/projects/{projectId}/models");

            ReturnValue<List<Model>> returnValueModels = await SendPaginatedRequest<List<Model>>(request, cancellationToken);
            if (returnValueModels.Value != null) models.AddRange(returnValueModels.Value);

            while (returnValueModels.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueModels.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueModels = await SendPaginatedRequest<List<Model>>(request, cancellationToken);
                if (returnValueModels.Value != null) models.AddRange(returnValueModels.Value);
            }

            return models;
        }

        public async Task<Model> CreateModel(string projectId, string name, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"v2/projects/{projectId}/models");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var bodyObject = new { name };

            string serializedBodyToCreate = JsonSerializer.Serialize(bodyObject);

            request.Content = new StringContent(serializedBodyToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Model model = await SendRequest<Model>(request, cancellationToken);
            return model;

        }

        public async Task<List<Member>> GetMembers(string projectId, CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"v2/projects/{projectId}/members");
            List<Member> members = new List<Member>();
            ReturnValue<List<Member>> returnValueMembers = await SendPaginatedRequest<List<Member>>(request, cancellationToken);
            if (returnValueMembers.Value != null) members.AddRange(returnValueMembers.Value);

            while (returnValueMembers.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueMembers.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueMembers = await SendPaginatedRequest<List<Member>>(request, cancellationToken);
                if (returnValueMembers.Value != null) members.AddRange(returnValueMembers.Value);
            }

            return members;
        }

        public async Task<User> GetCurrentUser(CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"v2/user");

            User User = await SendRequest<User>(request, cancellationToken);
            return User;
        }

        private async Task<ReturnValue<T>> SendPaginatedRequest<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine(DateTime.Now.ToString() + " - " + "SendBimsyncPaginatedRequest " + request.RequestUri.ToString() + " - " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            string next = null;

            try
            {
                using (HttpResponseMessage response = await _client.SendAsync(request,
  HttpCompletionOption.ResponseHeadersRead,
  cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // inspect the status code
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // show this to the user
                            Debug.WriteLine("The requested resource cannot be found." + request.RequestUri.ToString());
                            T value = default;
                            return new ReturnValue<T>(value, next);
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    if (response.Headers.Contains("link"))
                    {
                        string linkText = response.Headers.GetValues("link").FirstOrDefault();
                        if (linkText != null)
                        {
                            PaginationLink link = new PaginationLink(linkText);
                            if (link.next != null)
                            {
                                next = link.next;
                            }
                        }
                    }

                    Stream stream = await response.Content.ReadAsStreamAsync();
                    return new ReturnValue<T>(JsonSerializer.Deserialize<T>(stream), next);

                }
            }
            catch (OperationCanceledException ocException)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"An BimsyncClient operation was cancelled with message {ocException.Message}. " + request.RequestUri.ToString());
                T value = default(T);
                return new ReturnValue<T>(value, next);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"Something went wrong: " + ex.Message + " - " + request.RequestUri.ToString());
                throw ex;
            }
        }

        private async Task<T> SendRequest<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine(DateTime.Now.ToString() + " - " + "SendBimsyncRequest " + request.RequestUri.ToString() + " - " + System.Threading.Thread.CurrentThread.ManagedThreadId);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            try
            {
                using (HttpResponseMessage response = await _client.SendAsync(request,
  HttpCompletionOption.ResponseHeadersRead,
  cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // inspect the status code
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // show this to the user
                            Debug.WriteLine("The requested resource cannot be found." + request.RequestUri);
                            T value = default;
                            return value;
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    Stream stream = await response.Content.ReadAsStreamAsync();
                    return JsonSerializer.Deserialize<T>(stream);

                }
            }
            catch (OperationCanceledException ocException)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"An BimsyncClient operation was cancelled with message {ocException.Message}. " + request.RequestUri);
                T value = default(T);
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"Something went wrong: " + ex.Message + " - " + request.RequestUri);
                throw ex;
            }
        }

        public async Task<RevisionStatus> CreateRevisionAsync(string project_id, string model_id, string filename, string comment, string ifcFilePath)
        {
            string path = String.Format("v2/projects/{0}/revisions", project_id);
            byte[] data = File.ReadAllBytes(ifcFilePath);

            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                request.Content = new ByteArrayContent(data);
                request.Headers.Add("Bimsync-Params", "{" +
                "\"callbackUrl\": \"http://127.0.0.1:63842/\"," +
                "\"comment\": \"" + comment + "\"," +
                "\"filename\": \"" + filename + "\"," +
                "\"model\": \"" + model_id + "\"}");

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/ifc");

                using (HttpResponseMessage response = await _client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    Stream stream = await response.Content.ReadAsStreamAsync();
                    return JsonSerializer.Deserialize<RevisionStatus>(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Something went wrong: " + ex.Message + " - " + path);
                throw ex;
            }
        }
    }
}
