namespace CommandServer.Commands.Commands
{
    [Command(PackageIds.ReceiveCommand)]
    internal sealed class ReceiveCommand : BaseCommand<ReceiveCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await VS.MessageBox.ShowWarningAsync("CommandServer", "Button clicked");
        }
    }
}
