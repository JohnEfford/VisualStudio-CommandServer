using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommandServer
{
    /// <summary>
    /// Empty class to plug into text view creation
    /// </summary>
    internal sealed class Empty
    {
        public Empty(IWpfTextView view)
        {
           
        }
   
    }
}
