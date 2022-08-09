using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;


namespace CommandServer.Commands.Commands
{
    public interface IDispatcher
    {
        public string Name { get; }
        public ValueTask<Boolean> Process(JObject payload);
    }

    [Export(typeof(IDispatcher))]
    [ContentType("text")]
    public class ProcessVsCommand:IDispatcher
    {
        public string Name { get; } = "ProcessVsCommand";

        public ValueTask<bool> Process(
            JObject payload)
        {
            throw new NotImplementedException();
        }
    }

    [Command(PackageIds.ReceiveCommand)]
    internal sealed class ReceiveCommand : BaseCommand<ReceiveCommand>
    {
        [Import]
        internal IDispatcher Dispatcher { get; set; }
        public const string RendezvousDirectory = "visual-studio-commandServer";
        public string requestPath = Path.Combine(System.IO.Path.GetTempPath(), RendezvousDirectory, "request.json");
        public string responsePath = Path.Combine(System.IO.Path.GetTempPath(), RendezvousDirectory, "response.json");
        //public string responsePath = Path.Combine(@"C:", RendezvousDirectory, "response.json");

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var badger = await VS.Services.GetComponentModelAsync();
            var items = badger.GetExtensions<IDispatcher>();
              
            var content = "";
            using (var requestFile = new StreamReader(requestPath, Encoding.UTF8))
            {
                content = await requestFile.ReadToEndAsync();
            }
            JObject json = JObject.Parse(content);
            var uuid = json["uuid"];

            var date = $@"{{""uuid"":""{uuid}""}}{Environment.NewLine}";
            using (var responseFile = new StreamWriter(responsePath))
            {
                await responseFile.WriteAsync($@"{{""uuid"":""{uuid}"",""error"":null,""returnValue"":"""",""warnings"":""""}}{Environment.NewLine}");
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.GetPane(PackageGuids.CommandServer, out var pane);
            pane.OutputStringThreadSafe($"Command received:{content} {Environment.NewLine}");
        }
    }
}
