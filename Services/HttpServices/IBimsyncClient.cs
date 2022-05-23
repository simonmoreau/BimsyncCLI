using BimsyncCLI.Models.Bimsync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BimsyncCLI.Services.HttpServices
{
    public interface IBimsyncClient
    {
        Task<List<Project>> GetProjects(CancellationToken cancellationToken);
        Task<List<Member>> GetMembers(string projectId, CancellationToken cancellationToken);
        Task<List<Model>> GetModels(string projectId, CancellationToken cancellationToken);
        Task<Model> CreateModel(string projectId, string name, CancellationToken cancellationToken);
        Task<List<Revision>> GetRevisions(string projectId, string modelId, CancellationToken cancellationToken);
        Task<User> GetCurrentUser(CancellationToken cancellationToken);
        Task<RevisionStatus> CreateRevisionAsync(string project_id, string model_id, string filename, string comment, string ifcFilePath);
    }
}
