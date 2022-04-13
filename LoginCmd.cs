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

namespace BimsyncCLI
{
    [Command(Name = "login", Description = "login to Bimsync, the login crendentials will be saved locally in the profile")]
    class LoginCmd : bimsyncCmdBase
    {
        public LoginCmd(ILogger<ProjectsCmd> logger, IConsole console, IHttpClientFactory clientFactory,
         IBimsyncClient bimsyncClient, AuthenticationService authenticationService, SettingsService settingsService)
        {
            _logger = logger;
            _console = console;
            _httpClientFactory = clientFactory;
            _bimsyncClient = bimsyncClient;
            _authenticationService = authenticationService;
            _settingsService = settingsService;
        }
        private bimsyncCmd Parent { get; set; }

        protected override async Task<int> OnExecute(CommandLineApplication app)
        {

            try
            {

                _console.WriteLine("+-----------------------+");
                _console.WriteLine("|   Sign in Bimsync     |");
                _console.WriteLine("+-----------------------+");
                _console.WriteLine("");

                bool proceed = Prompt.GetYesNo("Do you want to sign in?",
                        defaultAnswer: true,
                        promptColor: ConsoleColor.Black,
                        promptBgColor: ConsoleColor.White);

                if (!proceed) return 1;

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
            Console.WriteLine($"Start URL: {loginUrl}");

            // open system browser to start authentication
            var ps = new ProcessStartInfo(loginUrl)
            {
                UseShellExecute = true,
                Verb = "open"
            };

            Process.Start(ps);

            // Brings the Console to Focus.
            BringConsoleToFront();

            string code = "";
            code = Prompt.GetString("Please copy here the login code displayed on the web page:", code);

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

                _console.ForegroundColor = ConsoleColor.Green;
                _console.WriteLine("+------------------------------+");
                _console.WriteLine($"| Hello {me.name} |");
                _console.WriteLine("|  Welcome to the Bimsync CLI  |");
                _console.WriteLine("+------------------------------+");
                _console.WriteLine("");
                _console.ResetColor();
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