using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.Utilities;

namespace CommandServer.Commands.Commands
{
    public interface IDispatcher
    {
        public string Name { get; }
        public void Bunty();
    }

    [Export(typeof(IDispatcher))]
    [ContentType("text")]
    public class Badger:IDispatcher
    {
        public string Name { get; } = "Badger";

        public void Bunty()
        {
            throw new NotImplementedException();
        }
    }

    //[Export(typeof(IDispatcher))]
    //public class Billy : IDispatcher
    //{
    //    public string Name { get; } = "Billy";

    //    public void Bunty()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    [Command(PackageIds.ReceiveCommand)]
    internal sealed class ReceiveCommand : BaseCommand<ReceiveCommand>
    {
        [Import]
        internal IDispatcher Dispatcher { get; set; }
        public const string RendezvousDirectory = "visual-studio-commandServer";
        public string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), RendezvousDirectory, "request.json");
        
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            
            Dispatcher = await VS.GetMefServiceAsync<IDispatcher>();
            var content = "";
            using (var fileStream = new StreamReader(path, Encoding.UTF8))
            {
                content = await fileStream.ReadToEndAsync();
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.GetPane(PackageGuids.CommandServer, out var pane);
            pane.OutputStringThreadSafe($"Command received:{content} {Environment.NewLine}");
        }
    }
}
