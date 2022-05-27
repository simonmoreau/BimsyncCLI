using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BimsyncCLI.Tests.Utils;
using McMaster.Extensions.CommandLineUtils;
using Xunit;
using Xunit.Abstractions;

namespace BimsyncCLI.Tests
{
    public class ProjectTests
    {
        private readonly ITestOutputHelper _output;

        public ProjectTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CommandLineAppCanBeCalledTwice()
        {
            var app = new CommandLineApplication(new TestConsole(_output));
            var helpOption = app.HelpOption(inherited: true);
            var verboseOption = app.VerboseOption();
            var subcmd = app.Command("test", _ => { });

            app.Execute("test", "--help");
            Assert.True(app.IsShowingInformation);
            Assert.True(subcmd.IsShowingInformation);
            Assert.True(helpOption.HasValue());

            app.Execute("-vvv");
            Assert.False(app.IsShowingInformation);
            Assert.False(subcmd.IsShowingInformation);
            Assert.False(helpOption.HasValue());
            Assert.Equal(3, verboseOption.Values.Count);

            app.Execute("test");
            Assert.Empty(verboseOption.Values);
        }
    }
}