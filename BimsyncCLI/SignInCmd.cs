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
    [Command(Name = "sign-in", Description = "Sign in to Bimsync, the login crendentials will be saved locally in the profile")]
    class SignInCmd : bimsyncCmdBase
    {
        public SignInCmd(ILogger<SignInCmd> logger, IConsole console, 
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

                if (!AnsiConsole.Confirm("Do you want to sign in?"))
                {
                    OutputToConsole("Ok... :(");
                    return 1;
                }

                await SignIn();

                return await Task.FromResult<int>(0);

            }

            catch (Exception ex)
            {
                OnException(ex);
                return 1;
            }
        }

        private async Task SignIn()
        {
            string loginUrl = _authenticationService.GetLoginUrl();

            // open system browser to start authentication
            var ps = new ProcessStartInfo(loginUrl)
            {
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(ps);

            // Brings the Console to Focus.
            BringConsoleToFront();

            string code = AnsiConsole.Ask<string>("Please type here the sign in code displayed on the web page:");

            if (string.IsNullOrEmpty(code)) throw new ArgumentNullException("The login code is not correct");

            _authenticationService.SetAuthorizationCode(code);
            Token token = await _authenticationService.Login();


            if (token == null)
            {
                throw new Exception("The application could not login. Please try again later.");
            }
            else
            {
                User me = await _bimsyncClient.GetCurrentUser(_settingsService.CancellationToken);

                AnsiConsole.Write(new FigletText("Bimsync CLI")
        .LeftAligned()
        .Color(Color.Red));

                AnsiConsole.Write(new Markup($"[bold red]Hello {me.name}[/]"));

            }


        }

        // Hack to bring the Console window to front.
        // ref: http://stackoverflow.com/a/12066376
        [DllImport("kernel32.dll", ExactSpelling = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public void BringConsoleToFront()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream)
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}