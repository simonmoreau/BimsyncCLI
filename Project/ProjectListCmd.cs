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
                await AnsiConsole.Status()
                    .StartAsync("Fetching all projects...", async ctx =>
                    {
                        List<Project> projects = await _bimsyncClient.GetProjects(_settingsService.CancellationToken);

                        // Create a table
                        Table table = new Table();


                        // Add some columns
                        table.AddColumn("Name");
                        table.AddColumn(new TableColumn("Last Updated").Centered());

                        // Add some rows
                        foreach (Project project in projects)
                        {
                            table.AddRow(project.name, project.updatedAt.ToString("MMMM dd, yyyy"));
                        }

                        // Render the table to the console
                        AnsiConsole.Write(table);

                        return 0;
                    });

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