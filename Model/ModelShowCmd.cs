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
    [Command(Name = "show", Description = "Get the details of a model.")]
    class ModelShowCmd : bimsyncCmdBase
    {
        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "project", Description = "The name or the Id of the projet", ValueName = "project name", ShowInHelpText = true)]
        public string ProjectId { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "n", LongName = "name", Description = "The name of the model", ValueName = "model name", ShowInHelpText = true)]
        public string Name { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "i", LongName = "id", Description = "The id of the model.", ValueName = "model id", ShowInHelpText = true)]
        public string Id { get; set; }

        public ModelShowCmd(ILogger<ModelCmd> logger, IConsole console, IHttpClientFactory clientFactory, IBimsyncClient bimsyncClient, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _httpClientFactory = clientFactory;
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

                if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Id))
                {
                    OutputError("Please specify a model name or id.");
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

                List<Model> models = await _bimsyncClient.GetModels(selectedProject.id, _settingsService.CancellationToken);

                Model model = null;

                if (!string.IsNullOrEmpty(Name))
                {
                    List<Model> selectedModels = models.Where(p => p.name == Name).ToList();

                    if (selectedModels.Count == 0)
                    {
                        OutputError("No models have been found with this name.");
                        return 0;
                    }
                    else if (selectedModels.Count == 1)
                    {
                        model = selectedModels.Where(p => p.name == Name).FirstOrDefault();
                    }
                    else
                    {
                        OutputError("There are multiple models with this name, please use the --id argument instead.");
                        return 0;
                    }
                }

                if (!string.IsNullOrEmpty(Id))
                {
                    model = models.Where(p => p.id == Id).FirstOrDefault();
                }

                if (model == null)
                {
                    OutputError("No project have been found with this id.");
                    return 0;
                }
                else
                {
                    OutputJson(models, new[] { "Name", "Id" });
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