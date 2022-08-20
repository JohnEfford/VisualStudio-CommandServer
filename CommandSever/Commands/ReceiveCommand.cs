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
        public const int maxStatlenessInSeconds = 3;

        protected override async Task InitializeCompletedAsync()
        {
            var componentModule = await VS.Services.GetComponentModelAsync();
            _commandServerDispatches = componentModule.GetExtensions<ICommandServerDispatch>();
        }

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Executing command");
            var (contentLoaded, content, signalTouched) = await GetContentFromKnowLocationAsync();
            string outputString;
            if (contentLoaded)
            {
                try
                {
                    JObject json = JObject.Parse(content);
                    var uuid = json["uuid"];
                    var result = await PerformCommandAsync(json, uuid);
                    outputString = CreateProcessedString(signalTouched, json, result);
                }
                catch (Exception exception)
                {
                    outputString = $"message:{exception.Message} {Environment.NewLine} stack:{exception.StackTrace} {Environment.NewLine}content: {content}";
                }
            }
            else
            {
                outputString = content;
            }
            await WriteResultsToOutputAsync(outputString);

        }

        private static async ValueTask WriteResultsToOutputAsync(
           string outputString)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.CreatePane(PackageGuids.CommandServer, "Command Server", Convert.ToInt32(true),
                Convert.ToInt32(false));
            output.GetPane(PackageGuids.CommandServer, out var pane);
            pane.OutputStringThreadSafe(outputString);
        }

        private static string CreateProcessedString(
            DateTime signalTouched,
            JObject content,
            CommandServerResult result)
        {
            var commandOutput = result.Message != null ? $"{result.Message}{Environment.NewLine}" : result.Message;
            var outputString =
                $"Command received:Signal={signalTouched:hh:mm:ss.fff} GetContentFromKnowLocationAsync={content.ToString()} {Environment.NewLine}" +
                commandOutput;
            return outputString;
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

        private async Task<(bool contentLoaded, string content, DateTime signalTouched)> GetContentFromKnowLocationAsync()
        {
            var contentLoaded = false;
            string content;
            var signalTouched = File.GetLastWriteTime(signalPath);
            var requestTouched = File.GetLastWriteTime(requestPath);
            var requestTooOld = RequestToOld(requestTouched);
            if (!requestTooOld)
            {
                using (var requestFile = new StreamReader(requestPath, Encoding.UTF8))
                {
                    content = await requestFile.ReadToEndAsync();
                    contentLoaded = true;
                }
            }
            else
            {
                content = $"RequestToOld:{requestTooOld}";
            }
            return (contentLoaded, content, signalTouched);
        }

        private bool RequestToOld(
            DateTime requstTouched) => requstTouched.AddSeconds(maxStatlenessInSeconds) < DateTime.Now;
    

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
