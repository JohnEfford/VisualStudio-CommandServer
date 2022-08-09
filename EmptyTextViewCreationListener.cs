using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using CommandServer.Commands.Commands;

namespace CommandServer
{
    /// <summary>
    /// Hack
    /// Want to have server start as soon as any text view loaded/vs started and start up
    /// but non of suggestions in https://stackoverflow.com/questions/28881976/automatically-run-extension-code-in-visual-studio-on-startup
    /// worked, so using this which method which creates a empty text view adorner.
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EmptyTextViewCreationListener : IWpfTextViewCreationListener
    {

        public EmptyTextViewCreationListener()
        {
            _startTask =  Task.Run(async () => await InitialiseServerAsync());
        }

        private static async Task InitialiseServerAsync()
        {
            string rendezvous =  System.IO.Path.Combine(System.IO.Path.GetTempPath(), ReceiveCommand.RendezvousDirectory);
            System.IO.Directory.CreateDirectory(rendezvous);
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var output = await VS.Services.GetOutputWindowAsync().ConfigureAwait(false);
            output.CreatePane(PackageGuids.CommandServer, "Command Server", Convert.ToInt32(true),
                Convert.ToInt32(false));
            output.GetPane(PackageGuids.CommandServer, out var pane);
            pane.OutputStringThreadSafe($"Command server started{Environment.NewLine}");
        }
        // Disable "Field is never assigned to..." and "Field is never used" compiler's warnings. Justification: the field is used by MEF.
#pragma warning disable 649, 169

        /// <summary>
        /// Defines the adornment layer for the adornment. This layer is ordered
        /// after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("Empty")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornmentLayer;

        private readonly Task _startTask;

#pragma warning restore 649, 169

        #region IWpfTextViewCreationListener

        /// <summary>
        /// Called when a text view having matching roles is created over a text data model having a matching content type.
        /// Instantiates a Empty manager when the textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            // The adornment will listen to any event that changes the layout (text changes, scrolling, etc)
            new Empty(textView);
        }

        #endregion
    }
}
