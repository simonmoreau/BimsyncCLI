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

namespace BimsyncCLI
{
    [Command(Name = "projects", Description = "List all available Bimsync projects")]
    class ProjectsCmd : bimsyncCmdBase
    {

        [Option(CommandOptionType.SingleValue, ShortName = "u", LongName = "username", Description = "istrada login username", ValueName = "login username", ShowInHelpText = true)]
        public string Username { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "password", Description = "istrada login password", ValueName = "login password", ShowInHelpText = true)]
        public string Password { get; set; }

        [Option(CommandOptionType.NoValue, LongName = "staging", Description = "istrada staging api", ValueName = "staging", ShowInHelpText = true)]
        public bool Staging { get; set; } = false;

        public ProjectsCmd(ILogger<ProjectsCmd> logger, IConsole console, IHttpClientFactory clientFactory, IBimsyncClient bimsyncClient, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _httpClientFactory = clientFactory;
            _bimsyncClient = bimsyncClient;
            _settingsService = settingsService;
        }
        private bimsyncCmd Parent { get; set; }

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