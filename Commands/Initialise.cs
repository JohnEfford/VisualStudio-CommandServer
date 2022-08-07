using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace CommandServer.Commands.Commands
{

    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EmptyAdornmentListener : IWpfTextViewCreationListener
    {
        public EmptyAdornmentListener()
        {
            Initialise().GetAwaiter().GetResult();
        }

        private static async Task Initialise()
        {
            IVsOutputWindowPane _pane;
            var temp = System.IO.Path.GetTempPath();
            var directory = System.IO.Path.Combine(temp, "visual-studio-commandServer");
            System.IO.Directory.CreateDirectory(directory);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.CreatePane(PackageGuids.CommandServer, "Command Server", Convert.ToInt32(true),
                Convert.ToInt32(false));
            output.GetPane(PackageGuids.CommandServer, out _pane);
            _pane.OutputStringThreadSafe($"Command server started{Environment.NewLine}");
        }

        public void TextViewCreated(
            IWpfTextView textView)
        {
            //   No op         
        }

        [Export(typeof(AdornmentLayerDefinition))]
        [Name("Empty")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornmentLayer;

    }

    [Command(PackageIds.ServerInit)]
    internal sealed class Initialise : BaseCommand<Initialise>
    {
        private IVsOutputWindowPane _pane;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
           
        }
    }
}
