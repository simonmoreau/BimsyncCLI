using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Xml;
using System.Collections.Generic;
using BimsyncCLI.Models.Bimsync;
using BimsyncCLI.Services.HttpServices;
using BimsyncCLI.Services;
using Spectre.Console;

namespace BimsyncCLI.ModelCmd
{
    [Command(Name = "list", Description = "List available Bimsync models.")]
    public class ModelListCmd : bimsyncCmdBase
    {
        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "project", Description = "The name or the id of the projet containing the models", ValueName = "project name", ShowInHelpText = true)]
        public string ProjectId { get; set; }
        public ModelListCmd(ILogger<ModelCmd> logger, IConsole console, IBimsyncClient bimsyncClient, SettingsService settingsService)
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

                OutputJson(models, new[] { "Name", "Id" });

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