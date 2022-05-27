using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using BimsyncCLI.Models.Bimsync;
using BimsyncCLI.Services.HttpServices;
using BimsyncCLI.Services;
using Spectre.Console;

namespace BimsyncCLI.ProjectCmd
{
    [Command(Name = "show", Description = "Get the details of a project.")]
    class ProjectShowCmd : bimsyncCmdBase
    {
        [Option(CommandOptionType.SingleValue, ShortName = "n", LongName = "name", Description = "The name of the project", ValueName = "project name", ShowInHelpText = true)]
        public string Name { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "i", LongName = "id", Description = "The id of the project.", ValueName = "project id", ShowInHelpText = true)]
        public string Id { get; set; }

        public ProjectShowCmd(ILogger<ProjectCmd> logger, IConsole console, IBimsyncClient bimsyncClient, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _bimsyncClient = bimsyncClient;
            _settingsService = settingsService;
        }
        private ProjectCmd Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {

            try
            {
                if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Id))
                {
                    OutputError("Please specify a project name or id.");
                    return 0;
                }

                Project project = null;
                List<Project> projects = await _bimsyncClient.GetProjects(_settingsService.CancellationToken);

                if (!string.IsNullOrEmpty(Name))
                {
                    List<Project> selectedProjects = projects.Where(p => p.name == Name).ToList();

                    if (selectedProjects.Count == 0)
                    {
                        OutputError("No project have been found with this name.");
                        return 0;
                    }
                    else if (selectedProjects.Count == 1)
                    {
                        project = projects.Where(p => p.name == Name).FirstOrDefault();
                    }
                    else
                    {
                        OutputError("There are multiple projects with this name, please use the --id argument instead.");
                        return 0;
                    }
                }

                if (!string.IsNullOrEmpty(Id))
                {
                    project = projects.Where(p => p.id == Id).FirstOrDefault();
                }

                if (project == null)
                {
                    OutputError("No project have been found with this id.");
                    return 0;
                }
                else
                {
                    OutputJson(projects, new[] {"Name","Description","Created At","Updated At","Id"});
                }

                return 0;

            }
            catch (Exception ex)
            {
                OnException(ex);
                return 1;
            }
        }
    }
}