using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BimsyncCLI.Models.BCF;
using System.Text.Json;

namespace BimsyncCLI.Services.HttpServices
{
    class BimsyncBCFClient : IBCFClient
    {
        private HttpClient _client;
        private const string _version = "2.1";

        public BimsyncBCFClient(HttpClient client)
        {

            _client = client;
            _client.BaseAddress = new Uri("https://bcf.bimsync.com/bcf/");
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public async Task<List<IssueBoard>> GetIssueBoardsAsync(string bimsync_project_id, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects?bimsync_project_id={1}", _version, bimsync_project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<IssueBoard> issueBoards = new List<IssueBoard>();
            ReturnValue<List<IssueBoard>> returnValueIssueBoards = await SendPaginatedRequest<List<IssueBoard>>(request, cancellationToken);
            if (returnValueIssueBoards.Value != null) issueBoards.AddRange(returnValueIssueBoards.Value);

            while (returnValueIssueBoards.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueIssueBoards.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueIssueBoards = await SendPaginatedRequest<List<IssueBoard>>(request, cancellationToken);
                if (returnValueIssueBoards.Value != null) issueBoards.AddRange(returnValueIssueBoards.Value);
            }

            return issueBoards;
        }

        public async Task<List<Topic>> GetTopicsAsync(string project_id, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics?$select=*,bimsync_requester,bimsync_assigned_to", _version, project_id);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Topic> topics = new List<Topic>();
            ReturnValue<List<Topic>> returnValueMembers = await SendPaginatedRequest<List<Topic>>(request, cancellationToken);
            if (returnValueMembers.Value != null) topics.AddRange(returnValueMembers.Value);

            while (returnValueMembers.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueMembers.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueMembers = await SendPaginatedRequest<List<Topic>>(request, cancellationToken);
                if (returnValueMembers.Value != null) topics.AddRange(returnValueMembers.Value);
            }

            return topics;
        }

        public async Task<Topic> GetTopicAsync(string project_id, string topic_guid, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}?$select=*,bimsync_requester,bimsync_assigned_to", _version, project_id, topic_guid);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            Topic topic = await SendRequest<Topic>(request, cancellationToken);
            return topic;
        }

        public async Task<Topic> GetTopicByNumberAsync(string project_id, string topic_number, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics?$select=*,bimsync_requester,bimsync_assigned_to&$filter=bimsync_issue_number eq '{2}'", _version, project_id, topic_number);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Topic> topics = await SendRequest<List<Topic>>(request, cancellationToken);
            return topics.FirstOrDefault();
        }

        public async Task<Topic> CreateTopicAsync(string project_id, string topic_type, string topic_status, string due_date, string title, string description, string[] labels, Assignation bimsync_assigned_to, Assignation bimsync_requester, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics", _version, project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var bodyObject = new { topic_type, topic_status, due_date, title, description, labels, bimsync_assigned_to, bimsync_requester };

            string serializedMovieToCreate = JsonSerializer.Serialize(bodyObject);

            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Topic topic = await SendRequest<Topic>(request, cancellationToken);
            return topic;
        }

        public async Task<Topic> UpdateTopicAsync(string project_id, string topic_guid, string topic_type, string topic_status, string due_date, string title, string description, string[] labels, Assignation bimsync_assigned_to, Assignation bimsync_requester, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var bodyObject = new { topic_type, topic_status, due_date, title, description, labels, bimsync_assigned_to, bimsync_requester };

            string serializedMovieToCreate = JsonSerializer.Serialize(bodyObject);

            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Topic topic = await SendRequest<Topic>(request, cancellationToken);
            return topic;
        }

        public async Task<List<Comment>> GetComments(string project_id, string topic_guid, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/comments", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Comment> comments = new List<Comment>();
            ReturnValue<List<Comment>> returnValueComments = await SendPaginatedRequest<List<Comment>>(request, cancellationToken);
            if (returnValueComments.Value != null) comments.AddRange(returnValueComments.Value);

            while (returnValueComments.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueComments.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueComments = await SendPaginatedRequest<List<Comment>>(request, cancellationToken);
                if (returnValueComments.Value != null) comments.AddRange(returnValueComments.Value);
            }

            return comments;
        }

        public async Task<Comment> CreateCommentAsync(string project_id, string topic_guid, string status, string verbal_status, string comment, string viewpoint_guid, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/comments", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var bodyObject = new { status, verbal_status, comment, viewpoint_guid };

            string serializedBodyToCreate = JsonSerializer.Serialize(bodyObject);

            request.Content = new StringContent(serializedBodyToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Comment createdComment = await SendRequest<Comment>(request, cancellationToken);
            return createdComment;
        }

        public async Task<List<IfcObject>> GetObjectsAsync(string project_id, string topic_guid, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/objects", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<IfcObject> ifcObjects = new List<IfcObject>();
            ReturnValue<List<IfcObject>> returnValueIfcObjects = await SendPaginatedRequest<List<IfcObject>>(request, cancellationToken);
            if (returnValueIfcObjects.Value != null) ifcObjects.AddRange(returnValueIfcObjects.Value);

            while (returnValueIfcObjects.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueIfcObjects.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueIfcObjects = await SendPaginatedRequest<List<IfcObject>>(request, cancellationToken);
                if (returnValueIfcObjects.Value != null) ifcObjects.AddRange(returnValueIfcObjects.Value);
            }

            return ifcObjects;
        }

        public async Task<List<IfcObject>> CreateObjectsAsync(string project_id, string topic_guid, List<string> ifcGuids, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/objects", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var bodyObject = new { ifcGuids };

            string serializedMovieToCreate = JsonSerializer.Serialize(bodyObject);

            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            List<IfcObject> ifcObjects = await SendRequest<List<IfcObject>>(request, cancellationToken);
            return ifcObjects ?? new List<IfcObject>();
        }

        public async Task<List<Viewpoint>> GetViewpointsAsync(string project_id, string topic_guid, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/viewpoints", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<Viewpoint> viewpoints = new List<Viewpoint>();
            ReturnValue<List<Viewpoint>> returnValueViewpoints = await SendPaginatedRequest<List<Viewpoint>>(request, cancellationToken);
            if (returnValueViewpoints.Value != null) viewpoints.AddRange(returnValueViewpoints.Value);

            while (returnValueViewpoints.Next != null)
            {
                request = new HttpRequestMessage(HttpMethod.Get, returnValueViewpoints.Next.Replace(_client.BaseAddress.AbsoluteUri, ""));
                returnValueViewpoints = await SendPaginatedRequest<List<Viewpoint>>(request, cancellationToken);
                if (returnValueViewpoints.Value != null) viewpoints.AddRange(returnValueViewpoints.Value);
            }

            return viewpoints;
        }



        public async Task<Viewpoint> GetViewpointAsync(string project_id, string topic_guid, string viewpoint_guid, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/viewpoints/{3}", _version, project_id, topic_guid, viewpoint_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            Viewpoint viewpoint = await SendRequest<Viewpoint>(request, cancellationToken);
            return viewpoint;
        }

        public async Task<Viewpoint> CreateViewpointAsync(string project_id, string topic_guid, Viewpoint viewpoint, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/topics/{2}/viewpoints", _version, project_id, topic_guid);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string serializedMovieToCreate = JsonSerializer.Serialize(viewpoint);

            request.Content = new StringContent(serializedMovieToCreate);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            Viewpoint createdComment = await SendRequest<Viewpoint>(request, cancellationToken);
            return createdComment;
        }

        public async Task<IssueBoardExtension> GetIssueBoardExtensionsAsync(string project_id, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/extensions", _version, project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            IssueBoardExtension extensions = await SendRequest<IssueBoardExtension>(request, cancellationToken);
            return extensions;
        }

        public async Task<List<ExtensionStatus>> GetIssueBoardExtensionStatusesAsync(string project_id, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/extensions/statuses", _version, project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<ExtensionStatus> extensionStatuses = await SendRequest<List<ExtensionStatus>>(request, cancellationToken);
            return extensionStatuses ?? new List<ExtensionStatus>();
        }

        public async Task<List<ExtensionType>> GetIssueBoardExtensionTypesAsync(string project_id, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/extensions/types", _version, project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<ExtensionType> extensionTypes = await SendRequest<List<ExtensionType>>(request, cancellationToken);
            return extensionTypes ?? new List<ExtensionType>();
        }

        public async Task<List<ExtensionLabel>> GetIssueBoardExtensionLabelsAsync(string project_id, CancellationToken cancellationToken)
        {
            string path = String.Format("{0}/projects/{1}/extensions/labels", _version, project_id);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);

            List<ExtensionLabel> extensionLabels = await SendRequest<List<ExtensionLabel>>(request, cancellationToken);

            return extensionLabels ?? new List<ExtensionLabel>();
        }

        private async Task<ReturnValue<T>> SendPaginatedRequest<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine(DateTime.Now.ToString() + " - " + "SendBCFPaginatedRequest " + request.RequestUri.ToString() + " - " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            string next = null;

            try
            {
                using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        // inspect the status code
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // show this to the user
                            Debug.WriteLine("The requested resource cannot be found." + request.RequestUri);
                            T value = default;
                            return new ReturnValue<T>(value, next);
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    if (response.Headers.Contains("odata.totalCount"))
                    {
                        int totalCount = int.Parse(response.Headers.GetValues("odata.totalCount").FirstOrDefault());

                        string currentRequestCountText = HttpUtility.ParseQueryString(request.RequestUri.Query).Get("skip");
                        int currentRequestCount = 100;
                        if (currentRequestCountText != null)
                        {
                            currentRequestCount = int.Parse(currentRequestCountText);
                        }

                        if (totalCount > currentRequestCount + 100)
                        {
                            UriBuilder uriBuilder = new UriBuilder(request.RequestUri.AbsoluteUri);
                            var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                            if (currentRequestCountText != null)
                            {
                                query["skip"] = (currentRequestCount + 100).ToString();
                                uriBuilder.Query = Uri.EscapeUriString(HttpUtility.UrlDecode(query.ToString()));
                                next = uriBuilder.Uri.AbsoluteUri;
                            }
                            else
                            {
                                query["skip"] = currentRequestCount.ToString();
                                uriBuilder.Query = Uri.EscapeUriString(HttpUtility.UrlDecode(query.ToString()));
                                next = uriBuilder.Uri.AbsoluteUri;
                            }
                        }
                    }

                    Stream stream = await response.Content.ReadAsStreamAsync();
                    return new ReturnValue<T>(JsonSerializer.Deserialize<T>(stream), next);
                }
            }
            catch (OperationCanceledException ocException)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"An BimsyncBCFClient operation was cancelled with message {ocException.Message}. " + request.RequestUri);
                T value = default(T);
                return new ReturnValue<T>(value, next);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"Something went wrong: " + ex.Message + " - " + request.RequestUri);
                throw ex;
            }
        }

        private async Task<T> SendRequest<T>(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Debug.WriteLine(DateTime.Now.ToString() + " - " + "SendBCFRequest " + request.RequestUri.ToString() + " - " + System.Threading.Thread.CurrentThread.ManagedThreadId);
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
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"An BimsyncBCFClient operation was cancelled with message {ocException.Message}. " + request.RequestUri);
                T value = default;
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(DateTime.Now.ToString() + " - " + $"Something went wrong: " + ex.Message + " - " + request.RequestUri);
                throw ex;
            }
        }
    }
}
