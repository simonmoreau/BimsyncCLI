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

namespace BimsyncCLI.ModelCmd
{
    [Command(Name = "model", Description = "Manage available Bimsync models")]
    [Subcommand(
        typeof(ModelListCmd),
        typeof(ModelShowCmd),
        typeof(ModelCreateCmd))]
    class ModelCmd : bimsyncCmdBase
    {
        public ModelCmd(ILogger<ModelCmd> logger, IConsole console, IHttpClientFactory clientFactory, IBimsyncClient bimsyncClient, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _httpClientFactory = clientFactory;
            _bimsyncClient = bimsyncClient;
            _settingsService = settingsService;
        }
        private bimsyncCmd Parent { get; set; }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return Task.FromResult(0);
        }
    }
}