using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using CommandServerContracts;
using Microsoft.VisualStudio.Utilities;
using Newtonsoft.Json.Linq;

namespace CommandServer.Commands.Commands;

[Export(typeof(ICommandServerDispatch))]
[ContentType("text")]
public class VisualStudioCommandProcessor:ICommandServerDispatch
{
    public string Name { get; } = "Default Command Handler";

    public bool CanProcess(
        string commandId) => commandId == "VSCommand";

    public async Task<CommandServerResult> Process(
        JObject payload)
    {
        var theData = payload["args"].Children();
        bool result=false;
        if (theData.Any())
        {
            var (cmd, arg) = (theData.First().Value<string>(),theData.ElementAtOrDefault(0)?.Value<string>());
            result=   await VS.Commands.ExecuteAsync(cmd, arg);
        }

        return new CommandServerResult($"Command executed successfully {result}", CommandServerResult.None);
    }
}