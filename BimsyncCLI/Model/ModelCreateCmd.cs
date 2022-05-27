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

namespace BimsyncCLI.ModelCmd
{
    [Command(Name = "create", Description = "Create a new model.")]
    class ModelCreateCmd : bimsyncCmdBase
    {
        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "project", Description = "The name or the Id of the projet", ValueName = "project name", ShowInHelpText = true)]
        public string ProjectId { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "n", LongName = "name", Description = "The name of the model", ValueName = "model name", ShowInHelpText = true)]
        public string Name { get; set; }

        public ModelCreateCmd(ILogger<ModelCmd> logger, IConsole console, IBimsyncClient bimsyncClient, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _bimsyncClient = bimsyncClient;
            _settingsService = settingsService;
        }
        private ModelCmd Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                if (string.IsNullOrEmpty(ProjectId))
                {
                    OutputError("Please specify a project name or id.");
                    return 0;
                }

                if (string.IsNullOrEmpty(Name))
                {
                    OutputError("Please specify a model name.");
                    return 0;
                }

                List<Project> projects = await _bimsyncClient.GetProjects(_settingsService.CancellationToken);

                // Try to find a project by name
                Project selectedProject = projects.Where(p => p.name == ProjectId).FirstOrDefault();

                if (selectedProject == null)
                {
                    // Try to find a project by id
                    selectedProject = projects.Where(p => p.id == ProjectId).FirstOrDefault();
                }

                if (selectedProject == null)
                {
                    OutputError($"Project {ProjectId} could not be found.");
                    return 0;
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    Model model = await _bimsyncClient.CreateModel(selectedProject.id, Name, _settingsService.CancellationToken);

                    if (model == null)
                    {
                        OutputError("Something went wrong, the model has not been created.");
                        return 0;
                    }
                    else
                    {
                        OutputJson(model, new[] { "Name", "Id" });
                    }
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