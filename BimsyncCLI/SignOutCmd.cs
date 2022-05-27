using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Web;
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
using System.Net;
using System.Runtime.InteropServices;
using BimsyncCLI.Services;
using Spectre.Console;

namespace BimsyncCLI
{
    [Command(Name = "sign-out", Description = "Sign out of Bimsync.")]
    class SignOutCmd : bimsyncCmdBase
    {
        public SignOutCmd(ILogger<SignInCmd> logger, IConsole console, 
         IBimsyncClient bimsyncClient, AuthenticationService authenticationService, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _bimsyncClient = bimsyncClient;
            _authenticationService = authenticationService;
            _settingsService = settingsService;
        }
        private bimsyncCmd Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {
            try
            {
                _authenticationService.Logout();

                Output("You have been sign out of the Bimsync CLI");

                return await Task.FromResult<int>(0);
            }

            catch (Exception ex)
            {
                OnException(ex);
                return 1;
            }
        }
    }
}