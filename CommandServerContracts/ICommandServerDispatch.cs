using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CommandServerContracts
{
    public class CommandServerResult
    {
        public readonly string Message;
        public readonly string Payload;

        /// <summary>
        /// Nothing to return to command server
        /// </summary>
        public const string None = "";

        public CommandServerResult(
            string message,
            string payload)
        {
            Message = message;
            Payload = payload;
        }
    }

    /// <summary>
    /// Command dispatch which plugs into the Command Server plug in
    /// </summary>
    public interface ICommandServerDispatch
    {
        
        string Name { get; }

        /// <summary>
        /// Can this implementation process the command. Implementations are selected using the following rule
        /// 1 Select the implementation which name forms part of the command and can handle the the command
        /// 2 Select he first implementation in the pipeline which says it can handle the command
        /// </summary>
        /// <param name="commandId">The command id to be handled</param>
        /// <returns>True if the CommandServerDispatch can handle the payload.</returns>
        Boolean CanProcess(string  commandId);
        
        /// <summary>
        /// Process the command and return any results. Most commands will not return any results.
        /// </summary>
        /// <param name="payload">JSON representation of the command</param>
        /// <returns>JSON representation of the result</returns>
        Task<CommandServerResult> Process(JObject payload);

    }
}
