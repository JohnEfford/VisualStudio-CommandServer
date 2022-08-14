using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandServerContracts;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;


namespace CommandServer.Commands.Commands
{
    [Command(PackageIds.ReceiveCommand)]
    internal sealed class ReceiveCommand : BaseCommand<ReceiveCommand>
    {
        [Import]
        internal ICommandServerDispatch CommandServerDispatch { get; set; }
        public const string RendezvousDirectory = "visual-studio-commandServer";
        public const string SignalDirectory = "signals";
        public string requestPath = Path.Combine(System.IO.Path.GetTempPath(), RendezvousDirectory, "request.json");
        public string responsePath = Path.Combine(System.IO.Path.GetTempPath(), RendezvousDirectory, "response.json");

        public string signalPath =
            Path.Combine(System.IO.Path.GetTempPath(), RendezvousDirectory, SignalDirectory, "prePhrase");

        private IEnumerable<ICommandServerDispatch> _commandServerDispatches;
        //public string responsePath = Path.Combine(@"C:", RendezvousDirectory, "response.json");

        protected override async Task InitializeCompletedAsync()
        {
            var componentModule = await VS.Services.GetComponentModelAsync();
            _commandServerDispatches = componentModule.GetExtensions<ICommandServerDispatch>();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var (content, signalTouched) = await GetContentFromKnowLocation();
            JObject json = JObject.Parse(content);
            var uuid = json["uuid"];
            var result = await PerformCommandAsync(json, uuid);
            await WriteResultsToOutputAsync(signalTouched, json, result);
        }

        private static async ValueTask WriteResultsToOutputAsync(
            DateTime signalTouched,
            JObject content,
            CommandServerResult result)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.CreatePane(PackageGuids.CommandServer, "Command Server", Convert.ToInt32(true),
                Convert.ToInt32(false));
            output.GetPane(PackageGuids.CommandServer, out var pane);
            var commandOutput = result.Message != null ? $"{result.Message}{Environment.NewLine}" : result.Message;
            pane.OutputStringThreadSafe(
                $"Command received:Signal={signalTouched:hh:mm:ss.fff} GetContentFromKnowLocation={content.ToString()} {Environment.NewLine}" +
                commandOutput);
        }

        private async Task<CommandServerResult> PerformCommandAsync(
            JObject json,
            JToken uuid)
        {
            var result = await ExecuteCommand(json, _commandServerDispatches);

            var data = $@"{{""uuid"":""{uuid}""}}{Environment.NewLine}";
            using (var responseFile = new StreamWriter(responsePath))
            {
                await responseFile.WriteAsync(
                    $@"{{""uuid"":""{uuid}"",""error"":null,""returnValue"":""{result.Payload}"",""warnings"":""""}}{Environment.NewLine}");
            }

            return result;
        }

        private async Task<(string content, DateTime signalTouched)> GetContentFromKnowLocation()
        {
            var content = "";
            var signalTouched = File.GetLastWriteTime(signalPath);
            using (var requestFile = new StreamReader(requestPath, Encoding.UTF8))
            {
                content = await requestFile.ReadToEndAsync();
            }

            return (content, signalTouched);
        }

        private async Task<CommandServerResult> ExecuteCommand(
            JObject json,
            IEnumerable<ICommandServerDispatch> commandServerDispatches)
        {
            var id = json["commandId"].ToString();
            var dispatchers = commandServerDispatches
                .Where(d => d.CanProcess(id))
                .OrderBy(d => BestProcessor(d.Name, id));

            CommandServerResult result = new CommandServerResult("No command processor found", CommandServerResult.None);
            if (dispatchers.Any())
            {
                result = await dispatchers.First().Process(json);
            }

            return result;
        }

        private int BestProcessor(
            string argName,
            string id)
        => id.Contains(argName) ? 0 : 100;
        
    }
}
