using System.Threading;
using Microsoft.VisualStudio.Shell.Interop;

namespace CommandServer.Commands.Commands
{
    [Command(PackageIds.ServerInit)]
    internal sealed class Initialise : BaseCommand<Initialise>
    {
        private IVsOutputWindowPane _pane;
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await _semaphore.WaitAsync();
            if (_pane == null)
            {
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
        }
    }
}
