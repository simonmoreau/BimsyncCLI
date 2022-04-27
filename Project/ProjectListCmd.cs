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
using BimsyncCLI.Models.Bimsync;
using BimsyncCLI.Services.HttpServices;
using BimsyncCLI.Services;
using Spectre.Console;

namespace BimsyncCLI.ProjectCmd
{
    [Command(Name = "list", Description = "List available Bimsync projects.")]
    class ProjectListCmd : bimsyncCmdBase
    {
        public ProjectListCmd(ILogger<ProjectCmd> logger, IConsole console, IHttpClientFactory clientFactory, IBimsyncClient bimsyncClient, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _httpClientFactory = clientFactory;
            _bimsyncClient = bimsyncClient;
            _settingsService = settingsService;
        }
        private ProjectCmd Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            // if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            // {
            //     Username = Prompt.GetString("iStrada Username:", Username);
            //     Password = SecureStringToString(Prompt.GetPasswordAsSecureString("iStrada Password:"));
            //     Staging = Prompt.GetYesNo("iStrada Staging?   ", Staging);
            //     Profile = Prompt.GetString("User profile name:", Profile);
            //     OutputFormat = Prompt.GetString("Output format (json|xml|text|table):", OutputFormat);
            // }

            try
            {
                // Asynchronous
                return await AnsiConsole.Status()
                    .StartAsync("Fetching all projects...", async ctx =>
                    {
                        List<Project> projects = await _bimsyncClient.GetProjects(_settingsService.CancellationToken);

                        OutputJson(projects, new[] {"Name","Description","Created At","Updated At","Id"});

                        return 0;
                    });

            }
            catch (Exception ex)
            {
                OnException(ex);
                return 1;
            }
        }
    }
}