using BimsyncCLI.Models.BCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BimsyncCLI.Services.HttpServices
{
    public interface IBCFClient
    {
        Task<List<IssueBoard>> GetIssueBoardsAsync(string bimsync_project_id, CancellationToken cancellationToken);
        Task<List<Topic>> GetTopicsAsync(string project_id, CancellationToken cancellationToken);
        Task<Topic> GetTopicAsync(string project_id, string topic_guid, CancellationToken cancellationToken);
        Task<Topic> GetTopicByNumberAsync(string project_id, string topic_guid, CancellationToken cancellationToken);
        Task<Topic> CreateTopicAsync(string project_id, string topic_type, string topic_status, string due_date, string title, string description, string[] labels, Assignation bimsync_assigned_to, Assignation bimsync_requester, CancellationToken cancellationToken);
        Task<Topic> UpdateTopicAsync(string project_id, string topic_guid, string topic_type, string topic_status, string due_date, string title, string description, string[] labels, Assignation bimsync_assigned_to, Assignation bimsync_requester, CancellationToken cancellationToken);
        Task<List<Comment>> GetComments(string project_id, string topic_guid, CancellationToken cancellationToken);
        Task<Comment> CreateCommentAsync(string project_id, string topic_guid, string status, string verbal_status, string comment, string viewpoint_guid, CancellationToken cancellationToken);
        Task<List<Viewpoint>> GetViewpointsAsync(string project_id, string topic_guid, CancellationToken cancellationToken);
        Task<List<IfcObject>> GetObjectsAsync(string project_id, string topic_guid, CancellationToken cancellationToken);
        Task<List<IfcObject>> CreateObjectsAsync(string project_id, string topic_guid, List<string> ifcGuids, CancellationToken cancellationToken);
        Task<Viewpoint> GetViewpointAsync(string project_id, string topic_guid, string viewpoint_guid, CancellationToken cancellationToken);
        Task<Viewpoint> CreateViewpointAsync(string project_id, string topic_guid, Viewpoint viewpoint, CancellationToken cancellationToken);
        Task<IssueBoardExtension> GetIssueBoardExtensionsAsync(string project_id, CancellationToken cancellationToken);
        Task<List<ExtensionStatus>> GetIssueBoardExtensionStatusesAsync(string project_id, CancellationToken cancellationToken);
        Task<List<ExtensionType>> GetIssueBoardExtensionTypesAsync(string project_id, CancellationToken cancellationToken);
        Task<List<ExtensionLabel>> GetIssueBoardExtensionLabelsAsync(string project_id, CancellationToken cancellationToken);
    }
}
