using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace BimsyncCLI
{
    [Command(Name = "bimsync", OptionsComparison = System.StringComparison.InvariantCultureIgnoreCase )]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(ProjectsCmd),
        typeof(SignInCmd))]
    class bimsyncCmd : bimsyncCmdBase
    {
        public bimsyncCmd(ILogger<bimsyncCmd> logger, IConsole console)
        {
            _logger = logger;
            _console = console;
        }

        protected override Task<int> OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return Task.FromResult(0);
        }

        private static string GetVersion()
            => typeof(bimsyncCmd).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}