namespace CommandServer.Commands.Commands
{
    [Command(PackageIds.ReceiveCommand)]
    internal sealed class ReceiveCommand : BaseCommand<ReceiveCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.GetPane(PackageGuids.CommandServer, out var pane);
            pane.OutputStringThreadSafe($"Command received {Environment.NewLine}");
        }
    }
}
