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

namespace BimsyncCLI
{
    [Command(Name = "projects", Description = "login to istrada, the login crendentials will be saved locally in the profile")]
    class ProjectsCmd : bimsyncCmdBase
    {

        [Option(CommandOptionType.SingleValue, ShortName = "u", LongName = "username", Description = "istrada login username", ValueName = "login username", ShowInHelpText = true)]       
        public string Username { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "password", Description = "istrada login password", ValueName = "login password", ShowInHelpText = true)]        
        public string Password { get; set; }

        [Option(CommandOptionType.NoValue, LongName = "staging", Description = "istrada staging api", ValueName = "staging", ShowInHelpText = true)]
        public bool Staging { get; set; } = false;
        
        public ProjectsCmd(ILogger<ProjectsCmd> logger, IConsole console, IHttpClientFactory clientFactory, IBimsyncClient bimsyncClient)
        {            
            _logger = logger;
            _console = console;
            _httpClientFactory = clientFactory;
            _bimsyncClient = bimsyncClient;
        }
        private bimsyncCmd Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                Username = Prompt.GetString("iStrada Username:", Username);
                Password = SecureStringToString(Prompt.GetPasswordAsSecureString("iStrada Password:"));
                Staging = Prompt.GetYesNo("iStrada Staging?   ", Staging);
                Profile = Prompt.GetString("User profile name:", Profile);
                OutputFormat = Prompt.GetString("Output format (json|xml|text|table):", OutputFormat);
            }

            try
            {      
                List<Project> projects = await _bimsyncClient.GetProjects(new System.Threading.CancellationToken());

                

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